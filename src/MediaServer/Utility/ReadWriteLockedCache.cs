using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MediaServer.Utility
{
	/// <summary>
	/// <para>
	/// ReadWriteLockedCache provides a cache which is guarded by a readerwriter lock.
	/// Its interface is patterened after memcached.  Its largely like a dictionary.
	/// Operations that need to modify the internal state acquire a write lock, operations
	/// that only need to read the state will acquire a read lock.
	/// </para>
	/// <para>
	/// Details of exactly how the readwrite lock is acquired and released, who gets what when, 
	/// etc, can be foudn at:
	/// </para>
	/// <para>
	/// http://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim.aspx
	/// </para>
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	class ReadWriteLockedCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		#region Data

		private uint _versionId = 0;
		private readonly Dictionary<TKey, TValue> _storage = new Dictionary<TKey, TValue>();
		private readonly ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();


		// Only access from inside write lock
		private void UpdateVersionId()
		{
			unchecked
			{
				_versionId += 1;
			}
		}

		public uint VersionId
		{
			get
			{
				_readWriteLock.EnterReadLock();
				try
				{
					return _versionId;
				}
				finally
				{
					_readWriteLock.ExitReadLock();
				}
			}
		}

		#endregion

		#region Getters

		/// <summary>
		/// Gets the specified key.  Will throw KeyNotFoundException if the key not present
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The value.</returns>
		public TValue Get(TKey key)
		{
			_readWriteLock.EnterReadLock();
			try
			{
				return _storage[key];
			}
			finally
			{
				_readWriteLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Tries the get value for the given key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>true when the key is found, false if not.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			_readWriteLock.EnterReadLock();
			try
			{
				return _storage.TryGetValue(key, out value);
			}
			finally
			{
				_readWriteLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns an enumerable of all the values that match the set of given keys.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns>the matching values.</returns>
		public IEnumerable<TValue> GetMulti(IEnumerable<TKey> keys)
		{
			if (keys == null || keys.Count() == 0) return new TValue[0];

			var values = new List<TValue>();
			_readWriteLock.EnterReadLock();
			try
			{
				foreach(var item in keys)
				{
					TValue val;
					if(_storage.TryGetValue(item, out val))
					{
						values.Add(val);
					}
				}
			}
			finally
			{
				_readWriteLock.ExitReadLock();
			}
			return values;
		}

		#endregion

		#region Setters

		/// <summary>
		/// Sets the specified key.  If the key already exists in the cache, it will be overwritten.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Set(TKey key, TValue value)
		{
			_readWriteLock.EnterWriteLock();
			try
			{
				UpdateVersionId();
				_storage[key] = value;
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Will add or overwrite a set of key/value pairs defined by the input iterator.  Preexisting keys will be overwritten.
		/// </summary>
		/// <param name="valuePairs">The value pairs.</param>
		public void SetMulti(IEnumerable<KeyValuePair<TKey,TValue>> valuePairs)
		{
			if (valuePairs == null || valuePairs.Count() == 0) return;

			_readWriteLock.EnterWriteLock();
			try
			{
				UpdateVersionId();
				foreach(var pair in valuePairs)
				{
					_storage[pair.Key] = pair.Value;
				}
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
		}

		#endregion

		#region Delete

		/// <summary>
		/// Deletes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		public void Delete(TKey key)
		{
			_readWriteLock.EnterWriteLock();
			try
			{
				UpdateVersionId();
				_storage.Remove(key);
			}
			finally			
			{
				_readWriteLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Deletes all entries in the cache that have keys contained in the input iterator.
		/// </summary>
		/// <param name="keys">The keys.</param>
		public void DeleteMulti(IEnumerable<TKey> keys)
		{
			if (keys == null || keys.Count() == 0) return;
			_readWriteLock.EnterWriteLock();
			try
			{
				UpdateVersionId();
				foreach (var key in keys)
				{
					_storage.Remove(key);
				}
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
		}

		#endregion

		#region Add

		/// <summary>
		/// Adds the specified key if it doesnt already exist.  Unlike set, preexisting keys will not be overwritten.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Add(TKey key, TValue value)
		{
			_readWriteLock.EnterUpgradeableReadLock();
				
			try
			{
				if (_storage.ContainsKey(key)) return;
				_readWriteLock.EnterWriteLock();
				try
				{
					UpdateVersionId();
					_storage.Add(key, value);
				}
				finally
				{
					_readWriteLock.ExitWriteLock();
				}
				
			}
			finally
			{
				_readWriteLock.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		/// Adds more than one key/value pair, as defined by the input enumerable.  Like single add, this
		/// will not replace any preexisting pairs.
		/// </summary>
		/// <param name="valuePairs">The value pairs.</param>
		public void AddMulti(IEnumerable<KeyValuePair<TKey,TValue>> valuePairs)
		{
			if (valuePairs == null || valuePairs.Count() == 0) return;

			_readWriteLock.EnterUpgradeableReadLock();
			
			try
			{
				var toAdd = new List<KeyValuePair<TKey, TValue>>();
				foreach(var item in valuePairs)
				{
					if (!_storage.ContainsKey(item.Key))
					{
						toAdd.Add(item);
					}
				}
				
				if (toAdd.Count == 0) return;

				_readWriteLock.EnterWriteLock();
				try
				{
					UpdateVersionId();
					foreach (var item in toAdd)
					{
						_storage.Add(item.Key, item.Value);
					}
				}
				finally
				{
					_readWriteLock.ExitWriteLock();
				}

			}
			finally
			{
				_readWriteLock.ExitUpgradeableReadLock();
			}
		}

		#endregion

		#region Replace

		/// <summary>
		/// Replaces the specified key.  Unlike set, this will not add new values.  This will only replace
		/// a key if it already exists.  
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Replace(TKey key, TValue value)
		{
			_readWriteLock.EnterUpgradeableReadLock();

			try
			{
				if (!_storage.ContainsKey(key)) return;
				_readWriteLock.EnterWriteLock();
				try
				{
					UpdateVersionId();
					_storage[key] = value;
				}
				finally
				{
					_readWriteLock.ExitWriteLock();
				}

			}
			finally
			{
				_readWriteLock.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		/// Replaces any keys in the cache with new values provided in the input enumerable.  This will not add
		/// new entries, only replace ones with keys that already exist.
		/// </summary>
		/// <param name="valuePairs">The value pairs.</param>
		public void ReplaceMulti(IEnumerable<KeyValuePair<TKey,TValue>> valuePairs)
		{
			_readWriteLock.EnterUpgradeableReadLock();

			try
			{
				var toRemove = new List<KeyValuePair<TKey, TValue>>();

				foreach(var item in valuePairs)
				{
					if (_storage.ContainsKey(item.Key))
					{
						toRemove.Add(item);
					}
				}

				if (toRemove.Count == 0) return;

				_readWriteLock.EnterWriteLock();
				try
				{
					UpdateVersionId();
					foreach (var item in toRemove)
					{
						_storage[item.Key] = item.Value;
					}
				}
				finally
				{
					_readWriteLock.ExitWriteLock();
				}

			}
			finally
			{
				_readWriteLock.ExitUpgradeableReadLock();
			}
		}

		#endregion

		#region Clear

		/// <summary>
		/// Clears the cache
		/// </summary>
		public void Clear()
		{
			_readWriteLock.EnterWriteLock();
			try
			{
				_versionId = 0;
				_storage.Clear();
			}
			finally
			{
				_readWriteLock.ExitWriteLock();
			}
		}

		#endregion
        
		#region Internal Enumerator

		/// <summary>
		/// This class provides a mechanism to iterate across the cache while preserving the 
		/// readwrite locks usage.  This can only be used for reading the cache
		/// and while it is doing so, a read lock will be held.  This is intended to be used from within
		/// foreach statements.  the IEnumerator needs to be disposed once finished, to make sure
		/// the lock is released, and this is done by foreach automatically.  if you need to use it
		/// directly, make sure you Dispose it when your done.
		/// </summary>
		private sealed class Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>
		{
			private readonly ReadWriteLockedCache<TKey, TValue> _node;
			private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

			/// <summary>
			/// Initializes a new instance of the <see cref="ReadWriteLockedCache&lt;TKey, TValue&gt;.Enumerator"/> class.
			/// This will acquire a readlock on the cache before returning, so can be blocked for a while.
			/// </summary>
			/// <param name="node">The node.</param>
			public Enumerator(ReadWriteLockedCache<TKey,TValue> node)
			{
				_node = node;
				if (node == null) return;

				_node._readWriteLock.EnterReadLock();
				_enumerator = _node._storage.GetEnumerator();
			}

			~Enumerator()
			{
				Dispose(false);
			}

			#region Implementation of IDisposable

			private volatile bool _disposed;

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources.  This will release the readlock that we hold on the cache.
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			private void Dispose(bool disposing)
			{
				lock (this)
				{
					if (!disposing || _disposed) return;

					if (_node != null)
					{
						_enumerator.Dispose();
						_node._readWriteLock.ExitReadLock();
					}
					_disposed = true;
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			#region Implementation of IEnumerator

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}

			public KeyValuePair<TKey,TValue> Current
			{
				get { return _enumerator.Current; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			#endregion
		}

		#endregion

		#region Implementation of IEnumerable

		public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}

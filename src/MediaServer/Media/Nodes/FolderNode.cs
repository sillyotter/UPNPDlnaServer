using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Threading;

namespace MediaServer.Media.Nodes
{
	public class FolderNode : MediaNode, IList<MediaNode>
	{
		private readonly string _title;
		private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
		private readonly List<MediaNode> _children = new List<MediaNode>();
		private uint _containerUpdateId = 0;
		
		public FolderNode(FolderNode parentNode, string title)
			: base(parentNode)
		{
			_title = title;
		}

		public override string Title 
		{ 
			get { return _title; }	
		}

		public override string Class
		{
			get { return UpnpTypeLookup.FolderType; }
		}
		
		public uint ContainerUpdateId
		{
			get 
			{ 
				_readerWriterLock.EnterReadLock();
				try
				{
<<<<<<< local
					if (!_containerUpdateId.HasValue) 
					{
						_readerWriterLock.EnterWriteLock();
						try
						{
							unchecked
							{
								_containerUpdateId = (uint)(_children.Sum(item => (uint)item.GetHashCode()) 
										+ (uint)GetHashCode());
							}
						}
						finally
						{
							_readerWriterLock.ExitWriteLock();
						}
					}
					return _containerUpdateId.Value;
=======
					return _containerUpdateId;
>>>>>>> other
				}
				finally
				{
					_readerWriterLock.ExitReadLock();
				}
			}
		}

		private void UpdateContainerUpdateId()
		{
			// Only ever call fro inside a write lock.
			// I dont costrut them to support recursion, so wont grab it here
			unchecked
			{
				//_containerUpdateId = (uint)(_children.Sum(item => (uint)item.GetHashCode()) + (uint)GetHashCode());
				_containerUpdateId += 1;
			}
		}

		public override void RemoveFromIndexes()
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				base.RemoveFromIndexes();
				foreach (var item in _children)
				{
					item.RemoveFromIndexes();
				}
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		// this little guy was the root of a nasty deadlock.
		// I was passing in Ienumerables that hadnt been evaulated
		// yet, and inside the evaluation nodes were created.  These 
		// created nodes wanted to add something to the main resourceCache
		// Normally this wouldnt have been a big deal.  It would have gotten
		// the write lock on this, then add on cache would have been called
		// which would have gotten its write lock, and then both exited.
		// The problem was that at the same time that was going on
		// the client was trying to get the system update id.  that requires
		// getting a read lock on this, and is done from with in side a readlock
		// on the cache.  The readlock on this node, for containerupdate id
		// wouldnt be acquired until this write lock exited, but this 
		// write lock couldnt exit until I got a write lock on the cache, which 
		// was tied up on this, and so on.  loop.  deadlock.  
		// So, to fix, remember that what ever you hadn to this addrange will
		// be evaluated in the context of a lock.  thats bad in this case, so 
		// to fix it, just .toList() the tings before they get in here.  
		// To see that it always happened, I changed this IEnum to an IList
		public void AddRange(IList<MediaNode> nodes)
		{
			if (nodes == null) return;
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.AddRange(nodes);
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		public void RemoveRange(IList<MediaNode> nodes)
		{
			if (nodes == null) return;
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.RemoveAll(nodes.Contains);
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}
		
		private string GetParentId()
		{
			return Id == Guid.Empty ? "-1" : (ParentId == Guid.Empty ? "0" : ParentId.ToString());
		}

		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			_readerWriterLock.EnterReadLock();
			try
			{
				return
					new XElement(
						Didl + "container",
						new XAttribute("id", Id == Guid.Empty ? "0" : Id.ToString()),
						new XAttribute("parentID", GetParentId()),
						new XAttribute("childCount", _children.Count),
						new XAttribute("restricted", true),
						new XElement(Dc + "title", new XText(Title)),
						new XElement(Upnp + "class", new XText(Class)));
			}
			finally
			{
				_readerWriterLock.ExitReadLock();
			}
		}

		public override IEnumerable<XElement> RenderDirectChildren(uint startingIndex, uint requestedCount, IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			_readerWriterLock.EnterReadLock();
			try
			{
				return _children.Skip((int)startingIndex)
					.Take((int)requestedCount)
					.Select(item => item.RenderMetadata(queryEndpoint, mediaEndpoint))
					.ToList();
			}
			finally
			{
				_readerWriterLock.ExitReadLock();
			}
		}

		public class Enumerator : IEnumerator<MediaNode>
		{
			private readonly FolderNode _node;
			private readonly IEnumerator<MediaNode> _enumerator;
			
			public Enumerator(FolderNode node)
			{
				_node = node;
				if (node == null) return;

				_enumerator = _node._children.GetEnumerator();
				_node._readerWriterLock.EnterReadLock();
			}

			~Enumerator()
			{
				Dispose(false);
			}

			#region Implementation of IDisposable

			private volatile bool _disposed;

			protected virtual void Dispose(bool disposing)
			{
				lock (this)
				{
					if (!disposing || _disposed) return;
					if (_node != null)
					{
						_node._readerWriterLock.ExitReadLock();
						_enumerator.Dispose();
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

			public MediaNode Current
			{
				get { return _enumerator.Current; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			#endregion
		}

		#region Implementation of IEnumerable

		public IEnumerator<MediaNode> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICollection<MediaNode>

		public void Add(MediaNode item)
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.Add(item);
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}

		}

		public void Clear()
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.Clear();
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		public bool Contains(MediaNode item)
		{
			_readerWriterLock.EnterReadLock();
			try
			{
				return _children.Contains(item);
			}
			finally
			{
				_readerWriterLock.ExitReadLock();
			}
		}

		public void CopyTo(MediaNode[] array, int arrayIndex)
		{
			_readerWriterLock.EnterReadLock();
			try
			{
				_children.CopyTo(array, arrayIndex);
			}
			finally
			{
				_readerWriterLock.ExitReadLock();
			}
		}

		public bool Remove(MediaNode item)
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				var x = _children.Remove(item);
				UpdateContainerUpdateId();
				return x;
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		public int Count
		{
			get
			{
				_readerWriterLock.EnterReadLock();
				try
				{
					return _children.Count;
				}
				finally
				{
					_readerWriterLock.ExitReadLock();
				}
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Implementation of IList<MediaNode>

		public int IndexOf(MediaNode item)
		{
			_readerWriterLock.EnterReadLock();
			try
			{
				return _children.IndexOf(item);
			}
			finally
			{
				_readerWriterLock.ExitReadLock();
			}
		}

		public void Insert(int index, MediaNode item)
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.Insert(index, item);
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		public void RemoveAt(int index)
		{
			_readerWriterLock.EnterWriteLock();
			try
			{
				_children.RemoveAt(index);
				UpdateContainerUpdateId();
			}
			finally
			{
				_readerWriterLock.ExitWriteLock();
			}
		}

		public MediaNode this[int index]
		{
			get 
			{ 
				_readerWriterLock.EnterReadLock();
				try
				{
					return _children[index];
				}
				finally
				{
					_readerWriterLock.ExitReadLock();
				}
			}
			set
			{
				_readerWriterLock.EnterWriteLock();
				try
				{
					_children[index] = value;
					UpdateContainerUpdateId();
				}
				finally
				{
					_readerWriterLock.ExitWriteLock();
				}
			}
		}

		#endregion
	}
}

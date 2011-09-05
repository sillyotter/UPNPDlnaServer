using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace MediaServer.Utility
{
	[Serializable]
	public class QueueDisabledException : Exception
	{
		public QueueDisabledException()
		{    
		}

		public QueueDisabledException(String msg) : base(msg)
		{
		}

		public QueueDisabledException(string message, Exception inner) : base(message, inner)
		{
		}

		protected QueueDisabledException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	public class BlockingQueue<T>
	{
		#region Data

		private readonly int _maxCapacity;
		private readonly Queue<T> _queue = new Queue<T>();

		#endregion

		#region Constructors

		public BlockingQueue()
			: this(int.MaxValue)
		{
		}

		public BlockingQueue(int maxCapacity)
		{
			_maxCapacity = maxCapacity;
		}

		#endregion

		#region Properties

		private bool _enabled = true;
		public bool Enabled 
		{
			get
			{
				lock (this)
				{
					return _enabled;
				}
			}
			set
			{
				lock (this)
				{
					_enabled = value;
					if (_enabled == false)
					{
						Monitor.PulseAll(this);
					}
				}
			}
		}

		public int Length
		{
			get
			{
				lock (this)
				{
					return _queue.Count;
				}
			}
		}

		public bool IsEmpty
		{
			get
			{
				lock (this)
				{
					return (_queue.Count == 0);
				}
			}
		}

		public bool IsFull
		{
			get
			{
				lock (this)
				{
					return (_queue.Count >= _maxCapacity);
				}
			}
		}

		#endregion

		#region Enqueue

		public void Enqueue(T item)
		{
			Enqueue(item, Timeout.Infinite);
		}

		public void Enqueue(T item, int timeoutMilliseconds)
		{
			lock (this)
			{
				if (!_enabled) throw new QueueDisabledException();

				while (_queue.Count >= _maxCapacity)
				{
					try
					{
						if (!Monitor.Wait(this, timeoutMilliseconds))
						{
							throw new TimeoutException();
						}
					}
					catch
					{
						Monitor.PulseAll(this);
						throw;
					}

					if (!_enabled) throw new QueueDisabledException();
				}

			   
				_queue.Enqueue(item);

				if (_queue.Count == 1)
				{
					Monitor.PulseAll(this);
				}
			}
		}

		public bool TryEnqueue(T item)
		{
			lock (this)
			{
				if (!IsFull)
				{
					Enqueue(item);
					return true;
				}
				return false;
			}
		}

		#endregion

		#region Dequeue

		public T Dequeue()
		{
			return Dequeue(Timeout.Infinite);
		}

		public T Dequeue(int timeoutMilliseconds)
		{
			lock (this)
			{
				if (!_enabled) throw new QueueDisabledException();

				while (_queue.Count <= 0)
				{
					try
					{
						if (!Monitor.Wait(this, timeoutMilliseconds))
						{
							throw new TimeoutException();
						}
					}
					catch
					{
						Monitor.PulseAll(this);
						throw;
					}

					if (!_enabled) throw new QueueDisabledException();
				}

				
				T item = _queue.Dequeue();

				if (_queue.Count == (_maxCapacity - 1))
				{
					Monitor.PulseAll(this);
				}

				return item;
			}
		}

		public bool TryDequeue(out T item)
		{
			lock (this)
			{
				if (!IsEmpty)
				{
					item = Dequeue();
					return true;
				}
				item = default(T);
				return false;
			}
		}

		#endregion
	}
}
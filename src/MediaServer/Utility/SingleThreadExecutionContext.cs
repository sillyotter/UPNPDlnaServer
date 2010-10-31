using System;
using System.Threading;

namespace MediaServer.Utility
{
	public class SingleThreadExecutionContext
	{
		private class TaskDescription
		{
			public ManualResetEvent IsDone;
			public Action Delegate;
			public Exception Exception;
		}

		private readonly BlockingQueue<TaskDescription> _queue = new BlockingQueue<TaskDescription>(1);
		private readonly Thread _t;

		public SingleThreadExecutionContext()
		{
			_t = new Thread(Run) {IsBackground = true};
			_t.Start();
		}

		~SingleThreadExecutionContext()
		{
			_queue.Enabled = false;
			_t.Join();
		}

		private void Run()
		{
			try
			{
				while (true)
				{
					TaskDescription t = _queue.Dequeue();

					try
					{

						t.Delegate();
						t.IsDone.Set();
					}
					catch (Exception ex)
					{
						t.Exception = ex;
						t.IsDone.Set();
					}
				}
			}
			catch (QueueDisabledException)
			{
			}
		}

		public void PostDelegateToThread(Action action)
		{
			var isDone = new ManualResetEvent(false);
			var tuple = new TaskDescription
			            	{
			            		IsDone = isDone,
			            		Delegate = action
			            	};
			_queue.Enqueue(tuple);
		}

		public void SendDelegateToThread(Action action)
		{
			var isDone = new ManualResetEvent(false);
			var tuple = new TaskDescription
			            	{
			            		IsDone = isDone,
			            		Delegate = action
			            	};
			_queue.Enqueue(tuple);

			isDone.WaitOne();

			if (tuple.Exception != null)
				throw tuple.Exception;
		}
	}
}
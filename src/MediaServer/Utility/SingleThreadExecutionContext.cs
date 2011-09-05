using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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

		private readonly BlockingCollection<TaskDescription> _queue 
			= new BlockingCollection<TaskDescription>(new ConcurrentQueue<TaskDescription>());
		private readonly Task _t;
		private readonly CancellationTokenSource _tokenSource  = new CancellationTokenSource();

		public SingleThreadExecutionContext()
		{
			_t = Task.Factory.StartNew(Run, _tokenSource.Token, TaskCreationOptions.LongRunning);
		}

		~SingleThreadExecutionContext()
		{
			_queue.CompleteAdding();
			_t.Wait();
		}

		private void Run(object data)
		{
			var token = (CancellationToken) data;

			while (!_queue.IsCompleted && !token.IsCancellationRequested)
			{
				var t = _queue.Take(token);

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
			token.ThrowIfCancellationRequested();
		}

		public void PostDelegateToThread(Action action)
		{
			var isDone = new ManualResetEvent(false);
			var tuple = new TaskDescription
			            	{
			            		IsDone = isDone,
			            		Delegate = action
			            	};
			_queue.Add(tuple);
		}

		public void SendDelegateToThread(Action action)
		{
			var isDone = new ManualResetEvent(false);
			var tuple = new TaskDescription
			            	{
			            		IsDone = isDone,
			            		Delegate = action
			            	};
			_queue.Add(tuple);

			isDone.WaitOne();

			if (tuple.Exception != null)
				throw tuple.Exception;
		}
	}
}
using System;
using System.Threading;

namespace MediaServer.Media.Nodes
{
	internal abstract class WebResourceFolderBase : FolderNode
	{
		private readonly Timer _timer;

		protected WebResourceFolderBase(FolderNode parentNode, string title)
			: base(parentNode, title)
		{
			var rand = new Random();
			_timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(rand.Next(0, 3)), TimeSpan.FromMinutes(rand.Next(50,70)));
		}

		~WebResourceFolderBase()
		{
			_timer.Change(-1, -1);
		}

		private void TimerCallback(object state)
		{
			lock(this)
			{
				foreach(var item in this)
				{
					item.RemoveFromIndexes();
				}
				Clear();

				FetchImageInformation();
			}
		}

		protected abstract void FetchImageInformation();
	}
}

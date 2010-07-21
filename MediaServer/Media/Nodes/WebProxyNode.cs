using System;
using System.Net;

namespace MediaServer.Media.Nodes
{
	class WebProxyNode : ResourceNode
	{
		private readonly string _title;
		private readonly Uri _targetUri;
		private readonly Uri _thumbnailUri;
		private readonly string _mimeType;
		private readonly long   _contentLength;

		public WebProxyNode(FolderNode parentNode, string title, Uri targetUri, Uri thumbnailUri) 
			: base(parentNode)
		{
			_title = title;
			_targetUri = targetUri;
			_thumbnailUri = thumbnailUri;

			var req = WebRequest.Create(targetUri);
			req.Method = "HEAD";

			using (var resp = req.GetResponse())
			{
				_mimeType = resp.ContentType;
				_contentLength = resp.ContentLength;
			}
		}

		#region Overrides of MediaNode

		public override string Title
		{
			get { return _title; }
		}

		public override string Class
		{
			get { return UpnpTypeLookup.GetUpnpTypeForMime(_mimeType); }
		}

		#endregion

		#region Overrides of ResourceNode

	    public override Uri GetRequestUrl(IPEndPoint baseAddress)
	    {
	        return _targetUri;
	    }

	    public override Uri GetIconUrl(IPEndPoint baseAddr)
	    {
	        return _thumbnailUri;
	    }

	    public override string MimeType
		{
			get { return _mimeType; }
		}

		public override ulong Size 
		{
			get { return (ulong)_contentLength; }
		}

		#endregion
	}
}

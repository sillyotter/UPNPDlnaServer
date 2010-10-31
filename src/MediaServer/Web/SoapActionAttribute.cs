using System;
using System.Linq;
using System.Xml.Linq;

namespace MediaServer.Web
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	sealed class SoapActionAttribute : Attribute
	{
		public string Action { get; private set; }

		public SoapActionAttribute(string action)
		{
			Action = action;
		}

		public XNamespace Namespace
		{
			get
			{
				return Action.Split('#').FirstOrDefault();
			}
		}

		public string Method
		{
			get
			{
				return Action.Split('#').LastOrDefault();
			}
		}
	}
}
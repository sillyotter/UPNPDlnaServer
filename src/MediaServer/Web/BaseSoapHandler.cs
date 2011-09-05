using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using MediaServer.Configuration;
using MediaServer.Utility;

namespace MediaServer.Web
{
	class BaseSoapHandler : BaseRequestHandler
	{
		private readonly Dictionary<string, MethodInfo> _soapActions = new Dictionary<string, MethodInfo>();
		
		public BaseSoapHandler()
		{
			var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
			foreach (var method in methods)
			{
				var attributes = method.GetCustomAttributes(typeof(SoapActionAttribute), true).Cast<SoapActionAttribute>();
				var attr = attributes.FirstOrDefault();
				if (attr != null) _soapActions.Add(attr.Action, method);
			}
		}

		private XElement InvokeMethod(string soapAction, XContainer invokeData)
		{
			soapAction = soapAction.Replace('"', ' ').Trim();

			var methodInfo = _soapActions[soapAction];
	
			var parametersInfo = methodInfo.GetParameters();
			var parameters = new List<object>();
			var names = new List<string>();
			var pieces = soapAction.Split('#');
			var methodName = pieces[1];
			XNamespace u = pieces[0];
			XNamespace s = "http://schemas.xmlsoap.org/soap/envelope/";

			foreach (var item in parametersInfo)
			{
				var attributes = item.GetCustomAttributes(typeof(SoapParameterAttribute), true).Cast<SoapParameterAttribute>();
				var attribute = attributes.FirstOrDefault();
				if (attribute != null)
				{
					var name = attribute.Name;
					names.Add(name);

					if (item.IsOut)
					{
						parameters.Add(item.ParameterType.GetElementType() == typeof (string)
										   ? String.Empty
										   : Activator.CreateInstance(item.ParameterType.GetElementType()));
					}
					else
					{
						if (invokeData != null)
						{
							var firstOrDefault = invokeData.Descendants(u + methodName).Elements(name).FirstOrDefault();
							if (firstOrDefault != null)
							{
								var val = firstOrDefault.Value;
								if (item.ParameterType == typeof(string))
								{
									parameters.Add(val);
								}
								else if (item.ParameterType.IsPrimitive)
								{
									var data = Convert.ChangeType(val, item.ParameterType);
									parameters.Add(data);
								}
								else if (item.ParameterType.IsEnum)
								{
									var data = Enum.Parse(item.ParameterType, val);
									parameters.Add(data);
								}
							}
						}
					}
				}
			}

			try
			{
				var ps = parameters.ToArray();
				methodInfo.Invoke(this, ps);

				var outs = names
					.Zip(ps, (a, b) => new { Name = a, Value = b.ToString() })
					.Zip(parametersInfo, (a, b) => new { a.Name, a.Value, b.IsOut })
					.Where(item => item.IsOut);

				var root = new XElement(s + "Envelope",
										new XAttribute(XNamespace.Xmlns + "s", s.ToString()),
										new XAttribute(s + "encodingStyle", "http://schemas.xmlsoap.org/soap/encoding/"),
										new XElement(s + "Body",
													 new XElement(u + methodName + "Response",
																  new XAttribute(XNamespace.Xmlns + "u", u.ToString()),
																  from element in outs select new XElement(element.Name, element.Value)))
					);
				return root;
			}
			catch (Exception ex)
			{
				Logger.Instance.Exception("Failed Soap Invocation", ex);
			}
			return null;
		}

		public override void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp)
		{
			if (req.HttpMethod.ToUpper() == "POST")
			{
				var action = req.Headers["SOAPACTION"];
				if (!String.IsNullOrEmpty(action))
				{
					var inputLength = (int)req.ContentLength64;
					var buffer = new byte[inputLength];
					
					req.InputStream.Read(buffer, 0, inputLength);

					XElement postData;
					using (var ms = new MemoryStream(buffer))
					{
						postData = XElement.Load(XmlReader.Create(ms));
					}
					
					var localData = Thread.GetNamedDataSlot("localEndPoint");
					Thread.SetData(localData, req.LocalEndPoint);
					var result = InvokeMethod(action, postData);
					Thread.FreeNamedDataSlot("localEndPoint");

					if (result != null)
					{
						var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), result);
						using (var s = new MemoryStream())
						using (var tw = new StreamWriter(s))
						{
							doc.Save(tw);
							var len = s.Length;
							var data = s.GetBuffer();

							resp.StatusCode = (int)HttpStatusCode.OK;
							resp.ContentLength64 = len;
							resp.ContentType = "text/xml";
							resp.Headers.Add("Server", Settings.Instance.ServerName);
							resp.Headers.Add("Date", DateTime.Now.ToUniversalTime().ToString("R"));		
							resp.Headers.Add("EXT", "");

							resp.OutputStream.Write(data, 0, (int)len);
							resp.OutputStream.Close();
						}
						return;
					}
				}
			}
			resp.StatusCode = (int)HttpStatusCode.NotFound;
		}
	}
}

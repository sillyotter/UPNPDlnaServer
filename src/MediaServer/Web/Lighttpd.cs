using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Text;
using System.IO;
using MediaServer.Media;

namespace MediaServer.Web
{
	internal class Lighttpd
	{
		private Process _proc;

		#region Singleton
		private static readonly Lazy<Lighttpd> SingletonInstance = new Lazy<Lighttpd>(() => new Lighttpd());

		public static Lighttpd Instance
		{
			get
			{
				return SingletonInstance.Value;
			}
		}

		#endregion

		private Lighttpd()
		{
			UrlMapping = new Dictionary<string,string>();
			_proc = null;
		}

		public string DocRoot { get; set; }
		public int Port { get; set; }
		public IDictionary<string,string> UrlMapping { get; private set;}

		private void CreateConfigurationFile(string tfn)
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (path != null)
			{
				var tmplPath = Path.Combine(path, "lighttpd.conf.tmpl");
				var tmplText = File.ReadAllText(tmplPath);

				var msb = new StringBuilder();
				var asb = new StringBuilder();

				foreach(var item in MimeTypeLookup.GetAllMappings())
				{
					msb.AppendFormat("\".{0}\" => \"{1}\",", item.Key, item.Value).AppendLine();
				}

				foreach (var item in UrlMapping)
				{
					asb.AppendFormat("alias.url += ( \"{0}\" => \"{1}\" )", item.Key, item.Value ).AppendLine();
				}

				tmplText = tmplText.Replace("{{docroot}}", DocRoot);
				tmplText = tmplText.Replace("{{port}}", Port.ToString());
				tmplText = tmplText.Replace("{{mimetypes}}", msb.ToString());
				tmplText = tmplText.Replace("{{aliases}}", asb.ToString());

				File.WriteAllText(tfn, tmplText);
			}
		}

		public void Start()
		{
			lock(this)
			{
				if (_proc == null)
				{
					var cf = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

					CreateConfigurationFile(cf);

					var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					if (path != null)
					{
						var exepath =
							Path.Combine(
								Path.Combine(
									Path.Combine(
										path,
										"lighttpd"),
									"sbin"),
								"lighttpd");

						var modpath =
							Path.Combine(
								Path.Combine(
									path,
									"lighttpd"),
								"lib");



						var cmdLine = String.Format("-D -m {0} -f {1}", modpath, cf);

						_proc = Process.Start(exepath, cmdLine);
					}
				}
			}
		}

		public void Stop()
		{
			lock(this)
			{
				if (_proc != null)
				{
					if (!_proc.HasExited)
					{
						_proc.Kill();
					}
					_proc.WaitForExit();
					_proc = null;
				}
			}
		}

	}
}

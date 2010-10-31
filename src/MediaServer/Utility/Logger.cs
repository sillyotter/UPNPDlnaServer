using System;
using System.IO;
using System.Text;

namespace MediaServer.Utility
{
	public enum LogLevel { Exception = 0, Error = 1, Warning = 2, Info = 3, Debug = 4 }

	public class Logger
	{
		private readonly SingleThreadExecutionContext _context = new SingleThreadExecutionContext();
		private TextWriter _output = Console.Out;
		
		private Logger()
		{
			LogLogLevel = LogLevel.Error;
		}

		~Logger()
		{
			if (_output == null) return;
			_output.Flush();
			_output.Close();
		}

		#region Singleton

		private static readonly Logger SingletonInstance = new Logger();

		public static Logger Instance
		{
			get
			{
				return SingletonInstance;
			}
		}
        
		static Logger() { }

		#endregion

		public void Initialize(string path, LogLevel logLevel)
		{
			LogLogLevel = logLevel;
			_output = new StreamWriter(path, true, Encoding.ASCII, 8192);
		}

		public LogLevel LogLogLevel { get; set; }

		public void Debug(string format, params object[] args)
		{
			Debug(String.Format(format, args));
		}

		public void Debug(string message)
		{
			LogMessage(LogLevel.Debug, message);
		}

		public void Info(string format, params object[] args)
		{
			Info(String.Format(format, args));
		}

		public void Info(string message)
		{
			LogMessage(LogLevel.Info, message);
		}

		public void Warn(string format, params object[] args)
		{
			Warn(String.Format(format, args));
		}

		public void Warn(string message)
		{
			LogMessage(LogLevel.Warning, message);
		}

		public void Error(string format, params object[] args)
		{
			Error(String.Format(format, args));
		}

		public void Error(string message)
		{
			LogMessage(LogLevel.Error, message);
		}

		public void Exception(string message, Exception ex)
		{
			LogMessage(LogLevel.Exception, message + "\n" + ex);
		}

		public void Exception(Exception ex)
		{
			LogMessage(LogLevel.Exception, ex.ToString());
		}

		private void LogMessage(LogLevel msgType, string message)
		{
			if (msgType > LogLogLevel) return;
			var now = DateTime.Now;
			_context.PostDelegateToThread( () => 
			                               	{
			                               		_output.WriteLine(String.Format("<event time=\"{0}\" type=\"{1}\">\n{2}\n</event>", 
			                               		                                now.ToString("O"), msgType, message));
			                               		_output.Flush();
			                               	});
		}

	}
}
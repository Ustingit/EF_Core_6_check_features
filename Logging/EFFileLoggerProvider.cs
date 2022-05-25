using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Logging
{
	public sealed class EFFileLoggerProvider : ILoggerProvider
	{
		public void Dispose()
		{
			//possible disposing here
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new MyLogger();
		}

		private class MyLogger : ILogger, IDisposable
		{
			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				File.AppendAllText("logs.txt", formatter(state, exception));
				Console.WriteLine(formatter(state, exception));
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true; //usually some cheks on log-level based on which we decide it's available or not
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return this;
			}

			public void Dispose()
			{
				//do disposing
			}
		}
	}
}

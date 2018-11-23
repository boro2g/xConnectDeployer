using System;

namespace xConnectDeployer
{
	internal class OperationLogger : IDisposable
	{
		private readonly string _message;
		private DateTime _startTime;

		public OperationLogger(string message)
		{
			_message = message;
			_startTime = DateTime.UtcNow;

			Console.WriteLine($"{message} starting");
		}

		public void Log(string message)
		{
			Console.WriteLine(" " + message);
		}

		public void Dispose()
		{
			var timespan = DateTime.UtcNow - _startTime;

			Console.WriteLine($"{_message} complete in {timespan}");
			Console.Write("-----------------------------------");
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PingQRS.Qlik.Sense.RestClient;

namespace PingQRS
{
	class Program
	{
		private static readonly StreamWriter LogFile = new StreamWriter("Logfile.txt");

		static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if (args.Length < 1)
			{
				PrintUsage();
			}

			var url = args[0];
			Log("Connecting to " + url);
			while (true)
			{
				using (var client = new RestClient(url))
				{
					var d0 = DateTime.Now;
					client.AsNtlmUserViaProxy();
					client.Get("/qrs/about");
					var d1 = DateTime.Now;
					var dt = d1 - d0;
					Log(d1 + " - Connection successfully established. dt=" + dt);
					Thread.Sleep(5000);
				}
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage:   PingQRS.exe <url>");
			Console.WriteLine("Example: PingQRS.exe https://my.server.url");
			Environment.Exit(1);
		}

		private static void Log(string msg)
		{
			LogFile.WriteLine(msg);
			LogFile.Flush();
			Console.WriteLine(msg);
		}

	}
}

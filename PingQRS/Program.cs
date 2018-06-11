using System;
using System.Collections.Generic;
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
		static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if (args.Length < 1)
			{
				PrintUsage();
			}

			var url = args[0];
			Console.WriteLine("Connecting to " + url);
			while (true)
			{
				using (var client = new RestClient(url))
				{
					var d0 = DateTime.Now;
					client.AsNtlmUserViaProxy();
					client.Get("/qrs/about");
					var d1 = DateTime.Now;
					var dt = d1 - d0;
					Console.WriteLine(d1 + " - Connection successfully established. t=" + dt);
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
	}
}

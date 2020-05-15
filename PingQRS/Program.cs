using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Qlik.Sense.RestClient;
using Qlik.Sense.RestClient.Qrs;

namespace PingQRS
{
	class Program
	{
		private static readonly StreamWriter LogFile = new StreamWriter("Logfile.txt");

		class ArgDef<T>
		{
			public readonly string Flag;
			public readonly string Description;
			public Action<string, T> Process { get; }

			public ArgDef(string flag, string description, Action<string, T> process)
			{
				Flag = flag;
				Description = description;
				Process = process;
			}
		}

		class Args
		{
			public int interval = 5000;
			public string url;
			public string endpoint;
			public User user = new User{ Directory = "INTERNAL", Id = "sa_repository"};
		}

		private static T ProcessArguments<T>(IEnumerable<ArgDef<T>> argDefs, string[] args) where T : new()
		{
			var argInfo = BuildArgDictionary(args);
			var dict = argInfo.Item1;
			var rest = argInfo.Item2;
			if (rest.Any())
				PrintUsage();

			var t = new T();
			foreach (var argDef in argDefs)
			{
				if (dict.ContainsKey(argDef.Flag))
				{
					try
					{
						argDef.Process(dict[argDef.Flag], t);
					}
					catch (Exception e)
					{
						Log(e.Message);
						PrintUsage();
					}
				}
			}

			return t;
		}

		private static Tuple<Dictionary<string, string>, List<string>> BuildArgDictionary(string[] args)
		{
			var dict = new Dictionary<string, string>();
			var rest = new List<string>();

			var i = 0;
			while (i < args.Length)
			{
				if (args[i].StartsWith("-"))
				{
					var flag = args[i].TrimStart('-');
					i++;
					dict[flag] = args[i];
				}
				else
				{
					rest.Add(args[i]);
				}

				i++;
			}

			return Tuple.Create(dict, rest);
		}

		static void Main(string[] args)
		{
			var argDefs = new []
			{
				new ArgDef<Args>("i", "interval", (str, t) => t.interval = int.Parse(str)),
				new ArgDef<Args>("url", "url", (str, t) => t.url = str),
				new ArgDef<Args>("e", "endpoint", (str, t) => t.endpoint = str),
				new ArgDef<Args>("u", "user", (str, t) =>
				{
					var uInfo = str.Split('\\');
					t.user = new User {Directory = uInfo[0], Id = uInfo[1]};
				})
			};
			var arg = ProcessArguments(argDefs, args);

			var certs = RestClient.LoadCertificateFromStore();
			var factory = new ClientFactory(arg.url, certs, true);
			var client = factory.GetClient(arg.user);

			Log($"Connecting to {client.Url} as {client.UserDirectory}\\{client.UserId}");
			LogTime(client, "/qrs/about");
			client.Get("/qrs/about");
			Log("Connection successfully established.");
			while (true)
			{
				LogTime(client, arg.endpoint);
				Thread.Sleep(arg.interval);
			}
		}

		private static void LogTime(IRestClient client, string endpoint)
		{
			var sw = new Stopwatch();
			sw.Start();
			client.Get(endpoint);
			sw.Stop();
			Log($"Call GET {endpoint}, dt={sw.Elapsed}");
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage:   PingQRS.exe -url <url> -e <endpoint> -u <user> [-i <interval>]");
			Console.WriteLine("Example: PingQRS.exe https://my.server.url -e /qrs/app -u MYDOMAIN\\MyUser -i 5000");
			Environment.Exit(1);
		}

		private static void Log(string msg)
		{
			var fullMessage = DateTime.Now.ToString("O") + " : " + msg;
			LogFile.WriteLine(fullMessage);
			LogFile.Flush();
			Console.WriteLine(fullMessage);
		}
	}
}

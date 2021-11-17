using System;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;

namespace Server
{
	public class Server
	{
		/// <summary>
		/// Configure logging via NLog.
		/// </summary>
		public static void ConfigureNLog()
		{
			var config = new NLog.Config.LoggingConfiguration();

			var console =
				new NLog.Targets.ConsoleTarget("console")
				{
					Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception:format=tostring}"
				};
			config.AddTarget(console);
			config.AddRuleForAllLevels(console);

			LogManager.Configuration = config;
		}
		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			ConfigureNLog();

			var builder = CreateWebHostBuilder(args);
			builder.Build().RunAsync();

			Console.WriteLine("Holding main thread.");
			while (true)
			{
				Thread.Sleep(1000);
			}
		}

		/// <summary>
		/// Create and configure web host builder.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <returns>Builder created</returns>
		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			var builder =
				WebHost
					.CreateDefaultBuilder(args)
					.UseKestrel(options => {
						options.Listen(IPAddress.Loopback, 5000);
					})
					.UseStartup<Startup>();

			return builder;
		}
	}
}

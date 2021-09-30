using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using System.Threading;
namespace Server
{
    public class Program
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configure loggin subsystem.
        /// </summary>
        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var console =
                new NLog.Targets.ConsoleTarget("console")
                {
                    Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
                };
            config.AddTarget(console);
            config.AddRuleForAllLevels(console);

            LogManager.Configuration = config;
        }

        /// <summary>
        /// Program body.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        void Run(string[] args)
        {
            //configure logging
            ConfigureLogging();

            //configure and start the server
            CreateHostBuilder(args).Build().RunAsync();

            //suspend the main thread
            log.Info("Server has started.");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            var self = new Program();
            self.Run(args);
        }
    }

}

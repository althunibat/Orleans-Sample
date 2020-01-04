﻿using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Godwit.Grains;
using Godwit.Silo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Statistics;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Godwit.Silo {
    internal static class Program {
        public static async Task<int> Main() {
            try {
                InitializeLogs();
                await CreateHostBuilder()
                    .RunConsoleAsync();
                return 0;
            }
            catch (Exception ex) {
                const string str = "Host terminated unexpectedly";
                Log.Fatal(ex, str);
                await Console.Out.WriteLineAsync(ex.ToString());
                return 1;
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureSilo(ISiloBuilder builder) {
            builder
                .UseSiloUnobservedExceptionsHandler()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences())
                .AddCustomStorageBasedLogConsistencyProviderAsDefault()
                .UseIf(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                    x => x.UseLinuxEnvironmentStatistics())
                .UseIf(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                    x => x.UsePerfCounterEnvironmentStatistics())
                .UseDashboard();
        }

        private static IHostBuilder CreateHostBuilder() {
            return new HostBuilder()
                .UseSerilog()
                .ConfigureServices((ctx, services) => {
                    var config = ctx.Configuration;
                    services.AddSingleton(c => {
                        var conn = EventStoreConnection.Create(new Uri(config["ENVENTSTORE_URI"]));
                        conn.ConnectAsync().Wait();
                        return conn;
                    });
                })
                .ConfigureAppConfiguration((ctx, config) => { config.AddEnvironmentVariables(); })
                .UseOrleans(ConfigureSilo);
        }


        private static void InitializeLogs() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.WithProperty("Application", GetAssemblyProductName())
                .Enrich.FromLogContext()
                .Enrich.With(new TraceIdEnricher())
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static string GetAssemblyProductName() {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        }
    }
}
using System;
using System.Reflection;
using System.Threading.Tasks;
using Godwit.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Godwit.Client {
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

        private static IHostBuilder CreateHostBuilder() {
            return new HostBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) => services.AddClusterService())
                .ConfigureAppConfiguration((ctx, config) => { config.AddEnvironmentVariables(); });
        }


        private static void InitializeLogs() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.WithProperty("Application", GetAssemblyProductName())
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static string GetAssemblyProductName() {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        }
    }
}
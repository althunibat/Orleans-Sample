using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Godwit.WebApi {
    public static class Program {
        public static async Task<int> Main() {
            try {
                InitializeLogs();
                await CreateHostBuilder()
                    .Build()
                    .RunAsync();
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
                .ConfigureWebHost(cfg => {
                cfg
                    .UseKestrel(opt => opt.AddServerHeader = false)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((ctx, config) => { config.AddEnvironmentVariables(); })
                    .UseStartup<Startup>()
                    .UseSerilog();
            });
        }

        private static void InitializeLogs() {
            const string format =
                "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} [{RequestId}] {Message:lj} {Properties:lj}{NewLine}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(outputTemplate: format)
                .CreateLogger();
        }
    }
}
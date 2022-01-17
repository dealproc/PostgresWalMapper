namespace FluentConfiguration {
    using System;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program {
        static void Main(string[] args) {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Example app = serviceProvider.GetService<Example>();

            try {
                app.Start();
                Console.ReadLine();
            }
            catch (Exception exc) {
                app.HandleError(exc);
            }
            finally {
                app.Stop();
            }
        }

        private static void ConfigureServices(ServiceCollection services) {
            services.AddLogging(cfg => cfg.AddConsole())
                .AddTransient<Example>();
        }
    }
}
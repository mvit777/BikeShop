using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MV.Framework.providers;
using BikeDistributor.Infrastructure.core;
using MV.Framework.interfaces;
using BikeDistributor.Infrastructure.services;
using System.IO;

namespace BikeShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // var configuration = new ConfigurationBuilder()
            //.SetBasePath(Directory.GetCurrentDirectory()) // This is the line you would change if your configuration files were somewhere else
            //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            //builder.Configuration.AddConfiguration(configuration.Build());

            

            await builder.Build().RunAsync();
          
        }

        /// <summary>
        /// dont use
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureBikeDistributorServices(WebAssemblyHostBuilder builder)
        {
            //Console.WriteLine(builder.Configuration["Zio"].ToString());
            var mongoUrl = "mongodb+srv://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/BikeDb?retryWrites=true&w=majority";//MongoSettings["url"];
            //var mongoUrl = "mongodb://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/BikeDb";//MongoSettings["url"];
            var mongoDb = "BikeDb";//MongoSettings["dbName"];
            var mongoServicesNs = "BikeDistributor.Infrastructure.services";//MongoSettings["servicesNamespace"];
                                                                            //var BS = (MongoBikeService)MongoServiceFactory
                                                                            //                                    .GetMongoService(mongoUrl, mongoDb, mongoServicesNs, "MongoBikeService");
            var mongoContext = new MongoDBContext(mongoUrl);
            var BS = new MongoBikeService(mongoContext);//exception list of servers should not be empty
            builder.Services.AddSingleton<MongoBikeService>(bs => BS);


        }

    }
}

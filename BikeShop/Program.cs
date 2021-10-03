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

            //=======================
            var mongoUrl = "mongodb://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/?authSource=admin";
            var mongoDb = "BikeDb";
            var mongoContext = new MongoDBContext(mongoUrl, mongoDb);
            var BS = new MongoBikeService(mongoContext);
            builder.Services.AddSingleton(bs=>BS);
            //=======================
            await builder.Build().RunAsync();
          
        }

        

    }
}

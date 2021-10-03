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
            var mongoUrl = "";
            var mongoDb = "";

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(sp => http);

            using var response = await http.GetAsync("appsettings.json");
            using var stream = await response.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream);
            mongoUrl = builder.Configuration.GetValue<string>("Mongo:url");
            mongoDb = builder.Configuration.GetValue<string>("Mongo:dbName");
            
            //=======================
            var mongoContext = new MongoDBContext(mongoUrl, mongoDb);
            var BS = new MongoBikeService(mongoContext);
            builder.Services.AddSingleton<IMongoService>(bs=>BS);
            //=======================
            await builder.Build().RunAsync();
        }

        //public void ConfigureServices(IServiceCollection services)
        //{
            
        //}


    }
}

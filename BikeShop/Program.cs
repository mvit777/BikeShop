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
using BikeShop.Services;
using Newtonsoft.Json;

namespace BikeShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var mongoUrl = "";
            //var mongoDb = "";

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(localClient => http);
            using var response = await http.GetAsync("appsettings.json");
            using var stream = await response.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream);
            //mongoUrl = builder.Configuration.GetSection("Mongo").GetValue<string>("url","");
            //mongoDb = builder.Configuration.GetSection("Mongo").GetValue<string>("dbName", "");

            ////=======================cannot use in blazor wasm
            //var mongoContext = new MongoDBContext(mongoUrl, mongoDb);
            //var BS = new MongoBikeService(mongoContext);
            //builder.Services.AddSingleton<IMongoService>(bs=>BS);
            ////=======================
            var restBaseUrl = builder.Configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl", "");
            var restClient = new HttpClient { BaseAddress = new Uri(restBaseUrl) };
            builder.Services.AddScoped(RestClient => restClient);

            /****************just some shit to pretend we have a login system in place*******/
            var users = builder.Configuration.GetSection("Users").GetChildren();
            
            var userInfos = new List<BikeShopUserInfo>();
            foreach (var user in users)
            {
                var userInfo = new BikeShopUserInfo();
                user.Bind(userInfo);
                userInfos.Add(userInfo);
            }
            var UserService = new BikeShopUserService(userInfos);
            //builder.Services.AddScoped<IUserService>(us=>UserService);
            builder.Services.AddSingleton(UserService);

            /*****************end of shit*************************************************/

            //=================cannot use RestSharp in blazor wasm==============
            //var restClient = new BaseRestClient(restBaseUrl);
            //builder.Services.AddSingleton<BaseRestClient>(RC => restClient);
            //==========================================================

            await builder.Build().RunAsync();
        }

        //public void ConfigureServices(IServiceCollection services)
        //{
            
        //}


    }
}

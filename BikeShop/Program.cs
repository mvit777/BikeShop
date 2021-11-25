using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BikeShop.Services;
using Grpc.Net.Client.Web;
using BikeShop.Protos;
using AutoMapper;
using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.interfaces;
using BikeShop.Helpers;
using BikeDistributor.Infrastructure.factories;
using System.Linq;

namespace BikeShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var mongoUrl = "";
            //var mongoDb = "";
            var configFile = "appsettings.json";
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(localClient => http);
            using var response = await http.GetAsync(configFile);
            using var stream = await response.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream);
            //mongoUrl = builder.Configuration.GetSection("Mongo").GetValue<string>("url","");
            //mongoDb = builder.Configuration.GetSection("Mongo").GetValue<string>("dbName", "");
            var ConfigService = new ConfigService(configFile, http);
            await ConfigService.LoadAsync();
            builder.Services.AddSingleton(ConfigService);
            ////=======================cannot use tcp in blazor wasm
            //var mongoContext = new MongoDBContext(mongoUrl, mongoDb);
            //var BS = new MongoBikeService(mongoContext);
            //builder.Services.AddSingleton<IMongoService>(bs=>BS);
            ////=======================
            var baseUrl = builder.Configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl", "");
            var restClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
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

            builder.Services
            .AddGrpcClient<Bikes.BikesClient> (options =>
                {
                    options.Address = new Uri(baseUrl);
                }).ConfigurePrimaryHttpMessageHandler(
                () => new GrpcWebHandler(new HttpClientHandler())
            );

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MongoEntityBike, EntityMongoBike>();
                
                cfg.CreateMap<BikeOption, EntityBikeOption>().ReverseMap();
                cfg.CreateMap<IBike, EntityBike>();
                //cfg.CreateMap<EntityBike, IBike>().As<BikeVariant>();
                cfg.CreateMap<EntityMongoBike, MongoEntityBike>()
                .ConstructUsing(d=>new MongoEntityBike(
                                        BikeFactory.Create(d.Bike.Brand,d.Bike.Model, d.Bike.Price, d.IsStandard, (List<BikeOption>)d.SelectedOptions.AsEnumerable())
                                                    .GetBike())
                                );


            });
            var mapper = mapperConfiguration.CreateMapper();
            builder.Services.AddSingleton(m=>mapper);

            await builder.Build().RunAsync();
        }

        //public void ConfigureServices(IServiceCollection services)
        //{
            
        //}


    }
}

using AutoMapper;
using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.core;
using BikeDistributor.Infrastructure.factories;
using BikeDistributor.Infrastructure.interfaces;
using BikeDistributor.Infrastructure.services;
using BikeShop.Protos;
using BikeShop.Services;
using BikeShopWS.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MV.Framework.providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BikeShop.Test
{
    public class UnitTest1
    {
        private WsConfig _configWS;
        private Config _blazorConfig;
        private Config _testConfig;
        private HttpClient _restClient;
        private bool _BsonTypesRegistered = false;
        private string _baseUrl = "http://localhost:8021";
        public UnitTest1()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Bike)))
            {
                BsonClassMap.RegisterClassMap<Bike>();
                BsonClassMap.RegisterClassMap<BikeVariant>();
                BsonClassMap.RegisterClassMap<BikeOption>();
                BsonClassMap.RegisterClassMap<MongoEntityBike>();
            }
            _restClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            _configWS = GetWSConfig();
        }
        private WsConfig GetWSConfig()
        {
            var file = @"C:\inetpub\wwwroot\sites\bikeapi\appsettings.json";
            return new WsConfig(file);
        }
        [Fact]
        public async void TestMongoBikeService()
        {
            var mongoSetts = _configWS.GetClassObject<MongoSettings>("Mongo");
            var MongoCtx = new MongoDBContext(mongoSetts);
            var BS = (MongoBikeService)MongoServiceFactory
                                                .GetMongoService(MongoCtx, "MongoBikeService");

            var bikes = await BS.Get();
            bikes.Count.Should().BeOneOf(new int[] {2});

        }

        [Fact]
        public async void TestMongoBikeOptionService()
        {
            var mongoSetts = _configWS.GetClassObject<MongoSettings>("Mongo");//_configWS.LoadMongoSettings(0);
            var MongoCtx = new MongoDBContext(mongoSetts);
            var bos = (MongoBikeOptionService)MongoServiceFactory.GetMongoService(MongoCtx, "MongoBikeOptionService");

            var options = await bos.Get();
            options.Count.Should().BeOneOf(new int[] { 1 });
        }

        [Fact]
        public void TestBikeWS()
        {
            string baseurl = _baseUrl;
            string action = "/bikes";
            var restClient = new RestClient(baseurl);
            var request = new RestRequest(action, DataFormat.Json);
            var response = restClient.Get(request);
            //throw new Exception(response.Content);
            List<MongoEntityBike> mebs = JsonUtils.DeserializeMongoEntityBikeList(response.Content);
            mebs.Count.Should().BeOneOf(new int[] { 2 });
            var MebVariant = mebs.Find(x => x.IsStandard == false);
            MebVariant.SelectedOptions.Count.Should().BeOneOf(new int[] { 5 });
        }

        /// <summary>
        /// https://newbedev.com/populate-iconfiguration-for-unit-tests
        /// </summary>
        [Fact]
        public void TestUservice()
        {
            var file = @"C:\Users\Marcello\source\repos\Blazor\BikeShop\wwwroot\appsettings.json";
            // var WasmConfig = new Config(file);
            // var jUsers = WasmConfig.GetJObject("Users");
            // var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jUsers.ToString());

            //var configuration = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            var configuration = new ConfigurationBuilder().AddJsonFile(file).AddInMemoryCollection().Build();
            var url = configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl");
            //url.Should().Be("http://localhost:8021");
            url.Should().Be("https://localhost:5001");
            var users = configuration.GetSection("Users").GetChildren();
            //users.Should().Be("caz");
            users.Count().Should().BeOneOf(new int[] { 4 });
            var userInfos = new List<BikeShopUserInfo>();
            foreach(var user in users)
            {
                var userInfo = new BikeShopUserInfo();
                user.Bind(userInfo);
                userInfos.Add(userInfo);
            }
            var admin = userInfos.Where(x => x.Username == "admin").SingleOrDefault();
            admin.Role.Should().Be("Admin");
           
        }
        [Fact]
        public async Task TestAddBikeWsAsync()
        {
            var file = @"C:\Users\Marcello\source\repos\Blazor\BikeShop\wwwroot\appsettings.json";
            
            var configuration = new ConfigurationBuilder().AddJsonFile(file).AddInMemoryCollection().Build();
            var url = configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl");      
            url.Should().Be(_baseUrl);

            var bike = JsonConvert.SerializeObject((IBike)BikeFactory.Create("Bianchi", "X25", 1000, true).GetBike());
            var data = new StringContent(bike, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            var response = httpClient.PostAsync("/bikes/create", data);
            var result = await response.Result.Content.ReadAsStringAsync();
            var meb = JsonConvert.DeserializeObject<MongoEntityBike>(result);
            meb.Bike.Model.Should().Be("X25");
        }

        private Mapper GetMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MongoEntityBike, EntityMongoBike>();

                cfg.CreateMap<BikeOption, EntityBikeOption>().ReverseMap();
                cfg.CreateMap<IBike, EntityBike>();
                //cfg.CreateMap<EntityBike, IBike>().As<BikeVariant>();
                //cfg.CreateMap<EntityMongoBike, MongoEntityBike>()
                //.ConstructUsing(d => new MongoEntityBike(
                //                        BikeFactory.Create(d.Bike.Brand, d.Bike.Model, d.Bike.Price, d.IsStandard, (List<BikeOption>)d.SelectedOptions.AsEnumerable())
                //                                    .GetBike())
                //                );


            });
            var mapper = mapperConfiguration.CreateMapper();

            return (Mapper)mapper;
        }

        [Fact]
        public void TestProtos()
        {
            var mapper = GetMapper();

            var jibike = new JObject
            {
                {"Brand", "Giant" },
                {"Model","Defy 1" },
                {"Price",Bike.OneThousand },
                {"Description","some description" },
                {"isStandard",true },
                {"options", "" }
            };
            var IBike = BikeFactory.Create(jibike).GetBike();
            var _MongoBikeEntity = new MongoEntityBike(IBike);
            _MongoBikeEntity.Bike.Brand.Should().Be("Giant");
            var gprcMongoEntity = new EntityMongoBike();
            gprcMongoEntity = mapper.Map<EntityMongoBike>(_MongoBikeEntity);
            gprcMongoEntity.Bike.Brand.Should().Be("Giant");

        }

    }
}

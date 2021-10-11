using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.core;
using BikeDistributor.Infrastructure.services;
using BikeShopWS.Infrastructure;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MV.Framework.providers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            string action = "/bike";
            var restClient = new RestClient(baseurl);
            var request = new RestRequest(action, DataFormat.Json);
            var response = restClient.Get(request);
            //throw new Exception(response.Content);
            List<MongoEntityBike> mebs = JsonUtils.DeserializeMongoEntityBikeList(response.Content);
            mebs.Count.Should().BeOneOf(new int[] { 2 });
            var MebVariant = mebs.Find(x => x.IsStandard == false);
            MebVariant.SelectedOptions.Count.Should().BeOneOf(new int[] { 5 });
        }
    }
}

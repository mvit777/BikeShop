using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.core;
using BikeDistributor.Infrastructure.services;
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MV.Framework.providers;
using RestSharp;
using System;
using System.Collections.Generic;
using Xunit;

namespace BikeShop.Test
{
    public class UnitTest1
    {
        public UnitTest1()
        {
            BsonClassMap.RegisterClassMap<Bike>();
            BsonClassMap.RegisterClassMap<BikeVariant>();
        }
        [Fact]
        public async void TestMongoBikeService()
        {
            var mongoUrl = "mongodb+srv://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/BikeDb?retryWrites=true&w=majority";//MongoSettings["url"];
            var mongoDb = "BikeDb";//MongoSettings["dbName"];
            var mongoServicesNs = "BikeDistributor.Infrastructure.services";//MongoSettings["servicesNamespace"];
            var BS = (MongoBikeService)MongoServiceFactory
                                                .GetMongoService(mongoUrl, mongoDb, mongoServicesNs, "MongoBikeService");

            var bikes = (List<MongoEntityBike>) await BS.Get();
            bikes.Count.Should().BeOneOf(new int[] {2});

        }
        [Fact]
        public async void TestMongoBikeServiceOverrideMethod()
        {
            //var mongoUrl = "mongodb+srv://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/BikeDb?retryWrites=true&w=majority";//MongoSettings["url"];
            var mongoUrl = "mongodb+srv://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/?authSource=admin";
            var mongoDb = "BikeDb";//MongoSettings["dbName"];
            var mongoServicesNs = "BikeDistributor.Infrastructure.services";//MongoSettings["servicesNamespace"];
            var mongoContext = new MongoDBContext(mongoUrl, mongoDb);
            var BS = new MongoBikeService(mongoContext);

            var bikes = (List<MongoEntityBike>)await BS.Get();
            bikes.Count.Should().BeOneOf(new int[] { 2 });

        }

        [Fact]
        public void TestBikeWS()
        {
            string baseurl = "http://localhost:8021";
            string action = "/bike";
            var restClient = new RestClient(baseurl);
            var request = new RestRequest(action, DataFormat.Json);
            var response = restClient.Get(request);
            response.Should().BeNull();//find how to map response.Content
        }
    }
}

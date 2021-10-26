using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.interfaces;
using BikeDistributor.Infrastructure.services;
using BikeShopWS.Infrastructure;
using BikeShopWS.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MV.Framework.providers;

namespace GrpcBike
{
    #region snippet
    public class BikesService : Bikes.BikesBase
    {
        private readonly ILogger<BikesService> _logger;
        private MongoServiceInstanceRegister _register;
        private readonly MongoBikeService _bikeService;
        private WsConfig _config;
        private IMapper _mapper;
        public BikesService(ILogger<BikesService> logger, MongoServiceInstanceRegister register,WsConfig config)
        {
            _logger = logger;
            _register = register;
            _config = config;
            _bikeService = (MongoBikeService)_register.GetServiceInstance("MongoBikeService", _config.GetMongoServiceIdentity("MongoBikeService"));
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MongoEntityBike, EntityMongoBike>();
                cfg.CreateMap<BikeOption, EntityBikeOption>();
                cfg.CreateMap<IBike, EntityBike>();
            });
            _mapper = mapperConfiguration.CreateMapper();
        }

        public override Task<BikesHelloReply> SayHello(BikesHelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BikesHelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task<GetBikesResponse> GetBikes(Empty request, ServerCallContext context)
        {
            
            List<MongoEntityBike> mebs = await _bikeService.Get();
            var response = new GetBikesResponse();
            foreach (MongoEntityBike meb in mebs)
            {
                var eb = _mapper.Map<EntityMongoBike>(meb);
                Console.WriteLine(eb.Id);
                response.BikeEntities.Add(eb);
            }
            
            
            //return Task.FromResult(response);
            return response;
        }
    }
    #endregion
}
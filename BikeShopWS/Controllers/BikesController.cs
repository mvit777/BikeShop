using BikeDistributor.Domain.Entities;
using MV.Framework.interfaces;
using BikeDistributor.Infrastructure.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using BikeDistributor.Domain.Models;
using Microsoft.AspNetCore.Cors;
using MV.Framework.providers;
using BikeDistributor.Infrastructure.core;
using BikeShopWS.Infrastructure;

namespace BikeShopWS.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("[controller]")]
    public class BikesController : ControllerBase
    {

        private readonly ILogger<BikesController> _logger;
        private MongoServiceInstanceRegister _register;
        private readonly MongoBikeService _bikeService;
        private WsConfig _config;
        public BikesController(ILogger<BikesController> logger, MongoServiceInstanceRegister register, WsConfig config)
        {
            _logger = logger;
            _register = register;
            _config = config;
            _bikeService = (MongoBikeService)_register.GetServiceInstance("MongoBikeService", _config.GetMongoServiceIdentity("MongoBikeService")); //"BikeDistributor.Infrastructure.services.MongoBikeService, BikeDistributor"
        }
        [HttpGet]
        public async Task<List<MongoEntityBike>> Get()
        {
            try
            {
                return await _bikeService.Get();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Warning, ex.Message, Array.Empty<object>());
                return null;
            }
            
        }
    }
}

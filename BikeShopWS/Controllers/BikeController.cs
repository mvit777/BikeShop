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

namespace BikeShopWS.Controllers
{
    [EnableCors("PolicyName")]
    [ApiController]
    [Route("[controller]")]
    public class BikeController : ControllerBase
    {

        private readonly ILogger<BikeController> _logger;
        private readonly MongoBikeService _bikeService;
        public BikeController(ILogger<BikeController> logger, IMongoService bikeService)
        {
            _logger = logger;
            _bikeService = (MongoBikeService)bikeService;
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
                _logger.Log(LogLevel.Critical, ex.Message, Array.Empty<object>());
                return null;
            }
            
        }
    }
}

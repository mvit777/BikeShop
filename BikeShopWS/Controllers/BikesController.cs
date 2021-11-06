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
using BikeDistributor.Infrastructure.interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BikeDistributor.Infrastructure.factories;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

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

        
        [HttpPost]
        [Route("/Bikes/create")]
        public async Task<MongoEntityBike> Create(string bike)
        {
            MongoEntityBike meb = null;
            try
            {
                var b = JsonUtils.DeserializeIBikeModel(bike);
                meb = await _bikeService.AddBikeAsync(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.InnerException);
            }
            return meb;
        }
       
        [HttpPost]
        [Route("/Bikes/update")]
        public MongoEntityBike Update(string entity)
        {
            var meb = JsonUtils.DeserializeIBikeEntity(entity);
            meb = _bikeService.Update(meb);
            return meb;
        }
        //[HttpPost]
        //public async Task<MongoEntityBike> Create(HttpContext context)
        //{
        //    string bike = "";
        //    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
        //    {
        //        bike = await reader.ReadToEndAsync();
        //    }
        //    MongoEntityBike meb = null;
        //    try
        //    {
        //        var b = JsonUtils.DeserializeIBikeModel(bike);
        //        meb = await _bikeService.AddBikeAsync(b);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message + " " + ex.InnerException);
        //    }
        //    return meb;
        //}
    }
}

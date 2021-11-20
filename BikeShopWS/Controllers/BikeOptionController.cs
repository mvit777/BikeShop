using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.services;
using BikeShopWS.Infrastructure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MV.Framework.providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShopWS.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("[controller]")]
    public class BikeOptionController : Controller
    {
        private readonly ILogger<BikeOptionController> _logger;
        private MongoServiceInstanceRegister _register;
        private readonly MongoBikeOptionService _bikeOptionService;
        private WsConfig _config;

        public BikeOptionController(ILogger<BikeOptionController> logger, MongoServiceInstanceRegister register, WsConfig config)
        {
            _logger = logger;
            _register = register;
            _config = config;
            var optionService = "MongoBikeOptionService";
            _bikeOptionService = (MongoBikeOptionService)_register.GetServiceInstance(optionService, _config.GetMongoServiceIdentity(optionService));
        }
        [HttpGet]
        public async Task<List<MongoEntityBikeOption>> Get()
        {
            try
            {
                return await _bikeOptionService.Get();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, ex.Message, Array.Empty<object>());
                return null;
            }

        }
        [HttpPost]
        [Route("/BikeOption/create")]
        public async Task<MongoEntityBikeOption> Create(string bikeOption)
        {
            MongoEntityBikeOption mebo = null;
            try
            {
                var obj = JsonConvert.DeserializeObject<BikeOption>(bikeOption);
                mebo = await _bikeOptionService.AddBikeOptionAsync(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.InnerException);
            }
            return mebo;
        }

        [HttpPost]
        [Route("/BikeOption/update")]
        public MongoEntityBikeOption Update(MongoEntityBikeOption bikeOption)
        {
            
            var meb = _bikeOptionService.Update(bikeOption);
            return meb;
        }
    }
}

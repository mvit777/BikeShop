using MV.Framework.providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShopWS.Infrastructure
{
    public class WsConfig : BikeDistributor.Infrastructure.core.Config
    {
        //public MongoSettings DefaultMongoSettings { get; set; } //moved to BikeDistributor lib for testing purposes
        public WsConfig(string jsonFilePath) : base(jsonFilePath)
        {
            
        }

        //public string GetMongoServiceIdentity(string serviceName)
        //{
        //    return DefaultMongoSettings.servicesNameSpace + "." + serviceName + ", " + DefaultMongoSettings.servicesDll;
        //}

        
    }
}

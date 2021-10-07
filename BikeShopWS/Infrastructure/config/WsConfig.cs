using MV.Framework.providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShopWS.Infrastructure
{
    public class WsConfig : BikeDistributor.Infrastructure.core.Config
    {
        
        public WsConfig(string jsonFilePath) : base(jsonFilePath)
        {
            
        }

        
    }
}

using AutoMapper;
using BikeDistributor.Domain.Entities;
using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.interfaces;
using BikeShop.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BikeShop.Helpers
{
    
    /// <summary>
    /// Not in use at the moment
    /// </summary>
    public class MappingHelper
    {
        public static IBike IBikeConverter(EntityBike source)
        {
            if (source.IsStandard)
            {
                return new Bike(source.Brand, source.Model, source.BasePrice);
            }
            return new BikeVariant(source.Brand, source.Model, source.BasePrice);
        }
    }

    public class MongoEntityBikeExt : MongoEntityBike
    {
        public MongoEntityBikeExt(EntityMongoBike bike)
        {

        }

        public MongoEntityBikeExt(EntityBike bike)
        {
            this.Bike.Model = bike.Model;
            this.Bike.BasePrice = bike.BasePrice;
            this.Bike.Brand = bike.Brand;
            this.Bike.SelectedOptions = (List<BikeOption>)bike.SelectedOptions.AsEnumerable();
        }
    }
}

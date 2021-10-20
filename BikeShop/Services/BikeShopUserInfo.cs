using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShop.Services
{
    public class BikeShopUserInfo
    {
        [BsonId]
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public string ShoppingBehavior { get; set; }
        public string AboutMe { get; set; }
        public string Role { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShop.Helpers
{
    public class StringHelper
    {
        public static string NormaliseStringId(string dirtyId, string tokenToReplace)
        {
            string cleanId = dirtyId.Replace(tokenToReplace,"").Replace("_"," ").Trim();

            return cleanId;
        }
    }
}

using AKSoftware.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using MV.Framework.providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShop.Services
{
    /// <summary>
    /// Just pretending we have a login system in place
    /// </summary>
    public class BikeShopUserService : IUserService
    {
        protected BikeShopUserInfo _CurrentUser { get; set; }
        protected string _AdminUsername = "admin";
        protected List<BikeShopUserInfo> _Users;
        //public NavigationManager NavigationManager { get; set; }

      

        public BikeShopUserService(List<BikeShopUserInfo>users)
        {
            _Users = users;
        }
        public bool LogIn(string username, string password)
        {
            if (username == "")
            {
                return false;
            }
            var users = _Users.AsQueryable();
            _CurrentUser = users.Where(x => x.Username == username).SingleOrDefault();
            if (_CurrentUser == null)
            {
                return false;
            }
            NotifyUserChanged();

            return true;
        }

        protected void NotifyUserChanged()
        {
            string valueToSend = _CurrentUser.Username;
            MessagingCenter.Send(this, "OnUserChanged", valueToSend);
        }
        public void LogOut()
        {
            Console.WriteLine(_CurrentUser + " has logged out");
        }

        public BikeShopUserInfo GetCurrentUser()
        {
            return _CurrentUser;
        }

        public bool IsAdmin()
        {
            if(_CurrentUser.Username == _AdminUsername)
            {
                return true;
            }
            return false;
        }
        public List<BikeShopUserInfo> GetUsers()
        {
            return _Users;
        }
    }
}

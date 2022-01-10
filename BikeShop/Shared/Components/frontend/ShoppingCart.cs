using AKSoftware.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using MV.Framework.interfaces;
using MV.Framework.providers.Database.MongoDb;
using System.Collections.Generic;
using System.Linq;

namespace frontend.Components
{
    public partial class ShoppingCart
    {

        private List<ICartItem> _cartItems = new List<ICartItem>();
        private List<MongoEntityCartItemBase> _allItems = new(); 


        //protected override void OnInitialized()
        //{
        //    _cartItems = GetCartListFromCart();

        //    //Subscribe to the item_added message sent from the ProductsList and fire a callback whenever a new item added to the cart
        //    MessagingCenter.Subscribe<ProductsList, Item>(this, "item_added", (sender, args) =>
        //    {
        //        //Recaulcate the items in the cart
        //        _cartItems = GetCartListFromCart();

        //        //Notify the UI about the change
        //        StateHasChanged();
        //    });
        //}

        /// <summary>
        /// Remove a specific Item from Cart
        /// </summary>
        /// <param name = "itemId" ></ param >
        private void RemoveItemFromCart(string itemId)
        {
            //Get the item id
            var item = _allItems.SingleOrDefault(i => i.Id == itemId);

            // Remove the item from the cart
            RemoveItemFromCart(itemId);

           //remove the item from the list of the UI
           var cartItem = _cartItems.SingleOrDefault(i => i.Id == itemId);
            _cartItems.Remove(cartItem);

            //Send a message to notify other components about the removing process and the total amount that has been removed
            MessagingCenter.Send(this, "cartitem_removed", cartItem);
        }

        //private List<ICartItem> GetCartListFromCart()
        //{
        //    var allItems = _allItems;
        //    var groupedItems = ItemsService.GetInCartItemIds().GroupBy(i => i).Select(i => new
        //    {
        //        Item = allItems.SingleOrDefault(item => item.Id == i.Key),
        //        Quantity = i.Count()
        //    });

        //    return groupedItems.Select(i => new MongoEntityCartItemBase
        //    {
        //        Id = i.Item.Id,
        //        Quantity = i.Quantity,
        //        ItemPrice = i.Item.Price,
        //        Name = i.Item.Name,
        //        TotalPrice = i.Quantity * i.Item.Price
        //    }).ToList();
        //}

    }

}
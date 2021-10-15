using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKSoftware.Blazor.Utilities;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Button
    {
        /// <summary>
        /// If used in table, HTMLId might identify the row something like @rowId_ButtonEdit
        /// </summary>
        [Parameter]
        public virtual string HTMLId { get; set; }
        [Parameter]
        public virtual string HTMLCssClass { get; set; } = "btn-primary";
        [Parameter]
        public virtual string Label { get; set; }
        [Parameter]
        public virtual string ClickEventName { get; set; }


        //[Parameter]
        //public MessagingCenter MC { get; set; }

        public virtual void SendMessage()
        {
            string valueToSend = "Hi from Component instance " + HTMLId;
            MessagingCenter.Send(this, ClickEventName, valueToSend);
        }

    }
}

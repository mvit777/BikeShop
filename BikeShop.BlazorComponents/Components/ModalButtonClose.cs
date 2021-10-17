using AKSoftware.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class ModalButtonClose
    {
        [Parameter]
        public string TargetHTMLId { get; set; }
        [Parameter]
        public virtual bool PreventDefault { get; set; } = true;
        [Parameter]
        public EventCallback<MouseEventArgs> OnClickCallback { get; set; }
       
    }
    
}

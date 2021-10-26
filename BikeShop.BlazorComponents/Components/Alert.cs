using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Alert
    {
        [Parameter]
        public virtual string HTMLId {get; set;}
        [Parameter]
        public virtual string HTMLCssClass { get; set; } = "alert-primary";
        [Parameter]
        public virtual RenderFragment ChildContent { get; set; }
        [Parameter]
        public virtual bool Visible { get; set; } = false;
        public void ChangeVisible(bool visible, bool executeStateHasChanged = false)
        {
            Visible = visible;
            //if (executeStateHasChanged)
            //{
            //    StateHasChanged();
            //}
            
        }
        public void ChangeCssClass(string cssClass, bool executeStateHasChanged = false)
        {
            HTMLCssClass = cssClass;
            //if (executeStateHasChanged)
            //{
            //    StateHasChanged();
            //}
        }
    }
}

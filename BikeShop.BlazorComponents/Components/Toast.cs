using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Toast
    {
        [Parameter]
        public string HTMLId { get; set; }

        [Parameter]
        public string HTMLCss { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Message { get; set; }

        [Parameter]
        public bool Visible { get; set; } = true;

        [Parameter]
        public string Icon { get; set; } = "...";

        public void SetTitle(string title)
        {
            Title = title;
        }
        public void SetMessage(string message)
        {
            Message = message;
        }
        public void SetCssClass(string cssClass)
        {
            HTMLCss = cssClass;
        }
        public void ChangeVisible(bool visible)
        {
            Visible = visible;
        }

        public void SetIcon(string icon)
        {
            Icon = icon;
        }

        public void RefreshComponent(string title, string message, string icon, string cssClass)
        {
            SetTitle(title);
            SetMessage(message);
            SetIcon(icon);
            SetCssClass(cssClass);
            StateHasChanged();
        }
    }
}

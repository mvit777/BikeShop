using AKSoftware.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Modal
    {
        [Parameter]
        public virtual string HTMLId { get; set; }
        [Parameter]
        public virtual string HTMLCssClass { get; set; }
        [Parameter]
        public virtual bool ShowFooter { get; set; } = true;
        [Parameter]
        public virtual string HeaderTitle { get; set; }

        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }

        public void SetHeaderTitle(string title)
        {
            HeaderTitle = title;
        }
        public void NotifyModalClosing()
        {
            string valueToSend = HTMLId;
            MessagingCenter.Send(this, "ModalClosing", "#" + valueToSend);
        }
    }
}

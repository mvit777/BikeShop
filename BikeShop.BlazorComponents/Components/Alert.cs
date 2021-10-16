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
        public virtual string HTMLCssClass { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        
    }
}

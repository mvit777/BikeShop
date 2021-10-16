using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Spinner
    {
        [Parameter]
        public string HTMLId { get; set; }
        [Parameter]
        public string HTMLCssClass { get; set; } = "spinner-border";
    }
}

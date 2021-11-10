using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BikeShop.BlazorComponents.Components
{
    public partial class Alert
    {
        [Parameter]
        public virtual string HTMLId {get; set;}
        [Parameter]
        public virtual string HTMLCssClass { get; set; } = "alert-primary";
        [Parameter]
        public virtual double AutoFade { get; set; } = 0;
        [Parameter]
        public virtual RenderFragment ChildContent { get; set; }
        [Parameter]
        public virtual bool Visible { get; set; } = false;

        //see https://wellsb.com/csharp/aspnet/blazor-timer-navigate-programmatically/
        private System.Timers.Timer _timer;
        public void ChangeVisible(bool visible, bool executeStateHasChanged = false)
        {
            Visible = visible;
            if (Visible)
            {
                if(AutoFade > 0)
                {
                    _timer = new System.Timers.Timer(AutoFade);
                    _timer.Elapsed += NotifyTimerElapsed;
                    _timer.Enabled = true;
                }
            }
            
        }
        public event Action OnElapsed;
        private void NotifyTimerElapsed(Object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();
            Visible = false;
            _timer.Dispose();
            StateHasChanged();
        }
        public void ChangeCssClass(string cssClass, bool executeStateHasChanged = false)
        {
            HTMLCssClass = cssClass;
            //if (executeStateHasChanged)
            //{
            //    StateHasChanged();
            //}
        }
        public void SetAutoFade(double autofade)
        {
            AutoFade = autofade;
        }
    }
}

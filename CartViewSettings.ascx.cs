using System;
using DotNetNuke.Services.Exceptions;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class CartViewSettings : NBrightBuySettingBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.CtrlTypeCode = "CartView";
            base.OnInit(e);
        }

        public override void UpdateSettings()
        {
            try
            {
                base.UpdateSettings();
                UpdateData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


    }

}


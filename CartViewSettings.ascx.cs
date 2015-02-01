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
            // cart type is not a setting, so use the controlanme
            if (ControlName == "NBS_MiniCart") base.CtrlTypeCode = "MiniCart";
            if (ControlName == "NBS_FullCart") base.CtrlTypeCode = "FullCart";
            if (ControlName == "NBS_Checkout") base.CtrlTypeCode = "Checkout";

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // if we don't have module settings update so we pickup the default.
            if (ModSettings.Get("themefolder") == "") UpdateSettings();
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


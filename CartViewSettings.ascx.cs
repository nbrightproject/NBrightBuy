using System;
using System.Reflection;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
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
            base.CtrlTypeCode = ModuleConfiguration.DesktopModule.ModuleName.Replace("NBS_", "");

            if (ModuleConfiguration.DesktopModule.ModuleName == "NBS_Cart") base.CtrlTypeCode = "CartView"; // backward compatiblity

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


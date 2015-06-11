using System;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Exceptions;
using NBrightCore.common;
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
    public partial class CategoryMenuSettings : NBrightBuySettingBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.CtrlTypeCode = "CategoryMenu";
            base.OnInit(e);
        }

        public override void UpdateSettings()
        {
            try
            {
                base.UpdateSettings();
                NBrightBuyUtils.RemoveModCache(ModuleId);
                UpdateData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override NBrightInfo EventBeforeRender(NBrightInfo objInfo)
        {
            return NBrightBuyUtils.ModuleSettingsResetCatIdFromRef(objInfo);
        }

        public override NBrightInfo EventBeforeUpdate(Repeater rpData, NBrightInfo objInfo)
        {
            return NBrightBuyUtils.ModuleSettingsSaveCatRefFromId(objInfo);
        }
    }

}


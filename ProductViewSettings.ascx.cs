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
    public partial class ProductViewSettings : NBrightBuySettingBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.CtrlTypeCode = "ProductView";
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

        public override NBrightInfo EventBeforeRender(NBrightInfo objInfo)
        {
            // clear the checkbox for defualt list, if it has been changed.
            var objGlobal = NBrightBuyUtils.GetGlobalSettings(PortalId);
            if (objInfo.GetXmlProperty("genxml/checkbox/chkdefaultlistmodule") == "True")
            {
                if (objGlobal.GetXmlProperty("genxml/hidden/defaultlistmoduleid") != ModuleId.ToString(""))
                {
                    objInfo.SetXmlProperty("genxml/checkbox/chkdefaultlistmodule","False");
                }
            }
            return objInfo;
        }

        public override NBrightInfo EventBeforeUpdate(System.Web.UI.WebControls.Repeater rpData, NBrightDNN.NBrightInfo objInfo)
        {
            // check we have a unique modulekey and then return the new key for updating.
            var dbKey = "";
            var objDb = ModCtrl.Get(objInfo.ItemID);
            if (objDb != null) dbKey = objDb.GetXmlProperty("genxml/textbox/modulekey");
            var newKey = objInfo.GetXmlProperty("genxml/textbox/modulekey");
            if (newKey != dbKey)
            {
                newKey = NBrightBuyUtils.GetUniqueKeyRef(PortalId, ModuleId, newKey, 0);
                objInfo.SetXmlProperty("genxml/textbox/modulekey",newKey);
                objInfo.GUIDKey = newKey;
            }
            return objInfo;
        }

        public override void EventAfterUpdate(System.Web.UI.WebControls.Repeater rpData, NBrightDNN.NBrightInfo objInfo)
        {
            // the global setting tell the DDRMenu to redirect to a certain page, so that the category menu works from multiple pages.
            var objGlobal = NBrightBuyUtils.GetGlobalSettings(PortalId);
            if (objInfo.GetXmlProperty("genxml/checkbox/chkdefaultlistmodule") == "True")
            {
                // set the global default values
                objGlobal.SetXmlProperty("genxml/hidden/defaultlistpage",TabId.ToString(""));
                objGlobal.SetXmlProperty("genxml/hidden/defaultlistmodule", objInfo.GetXmlProperty("genxml/textbox/modulekey"));
                objGlobal.SetXmlProperty("genxml/hidden/defaultlistmoduleid", ModuleId.ToString(""));
                ModCtrl.Update(objGlobal);
            }
            else
            {
                // The default has been unchecked so remove the global defaults if they are matching this module
                if (objGlobal.GetXmlProperty("genxml/hidden/defaultlistmoduleid") == ModuleId.ToString(""))
                {
                    objGlobal.SetXmlProperty("genxml/hidden/defaultlistpage", "");
                    objGlobal.SetXmlProperty("genxml/hidden/defaultlistmodule", "");
                    objGlobal.SetXmlProperty("genxml/hidden/defaultlistmoduleid", "");
                    ModCtrl.Update(objGlobal);
                }
            }
            // set page size so we pick it up in the product view.
            var navigationdata = new NavigationData(PortalId, objInfo.GetXmlProperty("genxml/textbox/modulekey"), StoreSettings.Current.Get("storagetype"));
            navigationdata.PageSize = objInfo.GetXmlProperty("genxml/textbox/pagesize");
            navigationdata.Save();
        }

    }

}


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
            // set page size so we pick it up in the product view.
            var navigationdata = new NavigationData(PortalId, objInfo.GetXmlProperty("genxml/textbox/modulekey"));
            navigationdata.PageSize = objInfo.GetXmlProperty("genxml/textbox/pagesize");
            navigationdata.Save();
        }

    }

}


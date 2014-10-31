using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditNBrightIndex class is used to manage content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class PropertyGroups : NBrightBuyAdminBase
    {

        private const string NotifyRef = "groupaction";

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);

            var t1 = "grouplistheader.html";
            var t2 = "grouplistbody.html";
            var t3 = "grouplistfooter.html";

            // Get Display Header
            var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
            rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            // Get Display Body
            var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            // Get Display Footer
            var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
            rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            #region "Data Repeater"

            if (UserId > 0) // only logged in users can see data on this module.
            {
                rpData.DataSource = NBrightBuyUtils.GetCategoryGroups(EditLanguage,true);
                rpData.DataBind();
            }

            #endregion

            // display header (Do header after the data return so the productcount works)
            base.DoDetail(rpDataH);

            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region "Event handlers"

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "addnew":
                    var groupData = new GroupData(-1, EditLanguage);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "delete":
                    if (Utils.IsNumeric(cArg))
                    {
                        ModCtrl.Delete(Convert.ToInt32(cArg));
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "saveall":
                    SaveAll();
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + "save", NotifyCode.ok);
                    NBrightBuyUtils.RemoveModCache(-1); //clear any cache
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "move":
                    if (Utils.IsNumeric(cArg))
                    {
                        MoveRecord(Convert.ToInt32(cArg));
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }


        #endregion

        private void SaveAll()
        {
            foreach (RepeaterItem rtnItem in rpData.Items)
            {
                var isdirty = GenXmlFunctions.GetField(rtnItem, "isdirty");
                var itemid = GenXmlFunctions.GetField(rtnItem, "itemid");
                if (isdirty == "true" && Utils.IsNumeric(itemid))
                {
                    var grpData = new GroupData(Convert.ToInt32(itemid), StoreSettings.Current.EditLanguage);
                    if (grpData.Exists)
                    {
                        var grpname = GenXmlFunctions.GetField(rtnItem, "groupname");
                        var grpref = GenXmlFunctions.GetField(rtnItem, "groupref");
                        grpData.Name = grpname;
                        grpData.Ref = grpref;
                        grpData.Save();
                    }
                }
            }

        }

        private void MoveRecord(int itemId)
        {

            var selecteditemid = GenXmlFunctions.GetField(rpDataH, "selecteditemid");
            if (Utils.IsNumeric(selecteditemid))
            {
                var movData = new CategoryData(itemId, StoreSettings.Current.EditLanguage);
                var selData = new CategoryData(Convert.ToInt32(selecteditemid), StoreSettings.Current.EditLanguage);
                var strneworder = movData.DataRecord.GetXmlProperty("genxml/hidden/recordsortorder");

                var selorder = selData.DataRecord.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(strneworder)) strneworder = "1";
                if (!Utils.IsNumeric(selorder)) selorder = "1";
                var neworder = Convert.ToDouble(strneworder);
                if (Convert.ToDouble(strneworder) < Convert.ToDouble(selorder))
                    neworder = neworder - 0.5;
                else
                    neworder = neworder + 0.5;                    
                selData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder",neworder.ToString(""),TypeCode.Double);
                ModCtrl.Update(selData.DataRecord);
                FixRecordSortOrder();
            }
        }

        private void FixRecordSortOrder()
        {
            // fix any incorrect sort orders
            Double lp = 1;
            var levelList = NBrightBuyUtils.GetCategoryGroups(EditLanguage,true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var recordsortorder = catinfo.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(recordsortorder) || Convert.ToDouble(recordsortorder) != lp)
                {
                    var catData = new CategoryData(catinfo.ItemID, StoreSettings.Current.EditLanguage);
                    catData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", lp.ToString(""));
                    ModCtrl.Update(catData.DataRecord);
                }
                lp += 1;
            }
        }


    }

}
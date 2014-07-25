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
    public partial class Groups : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch;
        private String _entryid = "";
        private Boolean _displayentrypage = false;

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

                    //Default orderby if not set
                var grpCats = GetGroupList();

                    rpData.DataSource = grpCats;
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
                    var categoryData = new CategoryData(-1, EditLanguage);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "delete":
                    if (Utils.IsNumeric(cArg))
                    {
                        ModCtrl.Delete(Convert.ToInt32(cArg));
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "saveall":
                    var grp = ModCtrl.GetByType(PortalId, -1, "GROUP", "", "GROUPLANG", StoreSettings.Current.EditLanguage);
                    var grpData = ModCtrl.GetData(grp.ItemID);
                    var grpDataLang = ModCtrl.GetData(grp.ItemID, StoreSettings.Current.EditLanguage);
                    var lp = 1;
                    foreach (RepeaterItem rtnItem in rpData.Items)
                    {
                        var isdirty = GenXmlFunctions.GetField(rtnItem, "isdirty");
                        var itemid = GenXmlFunctions.GetField(rtnItem, "itemid");
                        if (isdirty == "true" && Utils.IsNumeric(itemid))
                        {
                            var grpref = GenXmlFunctions.GetField(rtnItem, "ref");
                            var name = GenXmlFunctions.GetField(rtnItem, "name");
                            grp.SetXmlProperty("genxml/items[" + lp + "]/ref", grpref);
                            grp.SetXmlProperty("genxml/lang/genxml/items[" + lp + "]/name", name);
                        }
                        lp += 1;
                    }
                    var nodlang = grp.XMLDoc.SelectSingleNode("genxml/lang");
                    if (nodlang != null)
                    {
                        grpDataLang.XMLData = nodlang.InnerXml;
                        ModCtrl.Update(grpDataLang);
                    }
                    grp.RemoveXmlNode("genxml/lang");
                    grpData.XMLData = grp.XMLData;
                    ModCtrl.Update(grpData);

                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + "save", NotifyCode.ok);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "move":
                    if (Utils.IsNumeric(cArg))
                    {
                        MoveRecord(Convert.ToInt32(cArg));
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }


        #endregion

        private void MoveRecord(int itemId)
        {

            var selecteditemid = GenXmlFunctions.GetField(rpDataH, "selecteditemid");
            if (Utils.IsNumeric(selecteditemid))
            {
                var movData = new CategoryData(itemId, StoreSettings.Current.EditLanguage);
                var selData = new CategoryData(Convert.ToInt32(selecteditemid), StoreSettings.Current.EditLanguage);
                selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid",movData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlparentcatid"));
                selData.DataRecord.ParentItemId = movData.DataRecord.ParentItemId;
                selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype",movData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));
                var strneworder = movData.DataRecord.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(strneworder)) strneworder = "1";
                var neworder = Convert.ToDouble(strneworder) - 0.5;
                selData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder",neworder.ToString(""),TypeCode.Double);
                ModCtrl.Update(selData.DataRecord);

            }
        }


        private List<NBrightInfo> GetGroupList()
        {
            var rtnList = new List<NBrightInfo>();
            var grp = ModCtrl.GetByType(PortalId, -1, "GROUP","","GROUPLANG",StoreSettings.Current.EditLanguage);
            var nodLang = grp.XMLDoc.SelectNodes("genxml/items/*");

            if (nodLang != null)
            {
                var lp = 1;
                foreach (var nod in nodLang)
                {
                    var nbi = new NBrightInfo();
                    nbi.ItemID = lp;
                    nbi.TypeCode = grp.GetXmlProperty("genxml/items[" + lp + "]/ref");
                    nbi.GUIDKey = grp.GetXmlProperty("genxml/lang/genxml/items[" + lp + "]/name");
                    rtnList.Add(nbi);
                    lp += 1;
                }      
            }
            return rtnList;
        }


    }

}
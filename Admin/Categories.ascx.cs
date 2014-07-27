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
    public partial class Categories : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch;
        private String _entryid = "";
        private String _templatType = "list";

        private const string NotifyRef = "categoryaction";

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);

            _entryid = Utils.RequestParam(Context, "eid");
            if (_entryid != "") _templatType = "detail";

            // Get Search
            var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "categorysearch.html", Utils.GetCurrentCulture(), DebugMode);
            _templSearch = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            rpSearch.ItemTemplate = _templSearch;

            var t1 = "category" + _templatType + "header.html";
            var t2 = "category" + _templatType + "body.html";
            var t3 = "category" + _templatType + "footer.html";

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

                if (_templatType == "detail")
                {
                    if (Utils.IsNumeric(_entryid) && _entryid != "0")
                    {
                        var categoryData = new CategoryData(Convert.ToInt32(_entryid), EditLanguage);
                        base.DoDetail(rpData, categoryData.Info);
                    }
                }
                else
                {
                    var navigationData = new NavigationData(PortalId, "CategoryOrders");

                    // get search data
                    var sInfo = new NBrightInfo();
                    sInfo.XMLData = navigationData.XmlData;

                    // display search
                    base.DoDetail(rpSearch, sInfo);

                    var strFilter = navigationData.Criteria;

                    //Default orderby if not set
                    const string strOrder = " ";
                    var grpCats = new List<NBrightInfo>();
                    grpCats = GetTreeCatList(grpCats);

                    rpData.DataSource = grpCats;
                    rpData.DataBind();

                }
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
            var navigationData = new NavigationData(PortalId, "CategoryAdmin");

            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "return":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "search":
                    navigationData.XmlData = GenXmlFunctions.GetGenXml(rpSearch, "", "");
                    navigationData.Save();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
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
                    foreach (RepeaterItem rtnItem in rpData.Items)
                    {
                        var isdirty = GenXmlFunctions.GetField(rtnItem, "isdirty");
                        var itemid = GenXmlFunctions.GetField(rtnItem, "itemid");
                        if (isdirty == "true" && Utils.IsNumeric(itemid))
                        {
                            var catname = GenXmlFunctions.GetField(rtnItem, "txtcategoryname");
                            var catData = new CategoryData(Convert.ToInt32(itemid), StoreSettings.Current.EditLanguage);
                            if (catData.Exists)
                            {
                                catData.DataLangRecord.SetXmlProperty("genxml/textbox/txtcategoryname", catname);
                                ModCtrl.Update(catData.DataLangRecord);
                            }
                        }
                    }
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + "save", NotifyCode.ok);
                    NBrightBuyUtils.RemoveModCache(-1); //clear any cache
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

                FixRecordSortOrder(selData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlparentcatid"));
            }
        }

        private void FixRecordSortOrder(String parentid)
        {
            if (!Utils.IsNumeric(parentid)) parentid = "0";
            // fix any incorrect sort orders
            Double lp = 1;
            var strFilter = " and NB1.ParentItemId = " + parentid + " ";
            var levelList = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLanguage, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
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


        private List<NBrightInfo> GetTreeCatList(List<NBrightInfo> rtnList, int level = 0, int parentid = 0, string groupref = "", int displaylevels = 50)
        {
            if (level > displaylevels) return rtnList; // stop infinate loop

            var strFilter = " and NB1.ParentItemId = " + parentid + " ";
            if (groupref != "") strFilter += " and [XMLData].value('(genxml/hidden/ddlgrouptype)[1]','nvarchar(max)') = '" + groupref + "' ";

            var levelList = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLanguage, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var str = new string('.',level);
                str = str.Replace(".", "&nbsp;");
                catinfo.SetXmlProperty("genxml/hidden/levelprefix",str);
                rtnList.Add(catinfo);
                rtnList = GetTreeCatList(rtnList, level + 1, catinfo.ItemID, groupref, displaylevels);
            }

            return rtnList;
        }

    }

}
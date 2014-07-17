// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Orders : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch; 
        private String _entryid = "";
        private Boolean _displayentrypage = false;
        private String _uid = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            _uid = Utils.RequestParam(Context, "uid");

            base.OnInit(e);

            CtrlPaging.Visible = true;
            CtrlPaging.UseListDisplay = true;
            try
            {
                #region "set templates based on entry id (eid) from url"

                _entryid = Utils.RequestQueryStringParam(Context, "eid");

                if (_entryid != "") _displayentrypage = true;

                #endregion

                #region "load templates"

                // Get Search
                var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "orderssearch.html", Utils.GetCurrentCulture(), DebugMode);
                _templSearch = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                rpSearch.ItemTemplate = _templSearch;

                var t1 = "ordersheader.html";
                var t2 = "ordersbody.html";
                var t3 = "ordersfooter.html";

                if (Utils.IsNumeric(_entryid))
                {
                    t1 = "ordersdetailheader.html";
                    t2 = "ordersdetail.html";
                    t3 = "ordersdetailfooter.html";
                }

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                if (Utils.IsNumeric(_entryid))
                {
                    var rpItemHTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitemheader.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItemH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    // Get Display Body
                    var rpItemTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitem.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItem.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    // Get Display Footer
                    var rpItemFTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitemfooter.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItemF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                }
                else
                {
                    rpItemH.Visible = false;
                    rpItem.Visible = false;
                    rpItemF.Visible = false;
                }
                #endregion


            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

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
                //remove the navigation data, it could be causing the error.
                var navigationData = new NavigationData(PortalId, "OrderAdmin", StoreSettings.Current.StorageTypeClient);
                navigationData.Delete();
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

                if (_displayentrypage)
                {
                    DisplayDataEntryRepeater(_entryid);
                }
                else
                {
                    var navigationData = new NavigationData(PortalId, "AdminOrders", StoreSettings.Current.StorageTypeClient);

                    //setup paging
                    var pagesize = StoreSettings.Current.GetInt("pagesize");
                    var pagenumber = 1;
                    var strpagenumber = Utils.RequestParam(Context, "page");
                    if (Utils.IsNumeric(strpagenumber)) pagenumber = Convert.ToInt32(strpagenumber);
                    var recordcount = 0;

                    // get search data
                    var sInfo = new NBrightInfo();
                    sInfo.XMLData = navigationData.XmlData;

                    // display search
                    base.DoDetail(rpSearch, sInfo);

                    var strFilter = navigationData.Criteria;

                    // if we have a uid, then we want to display only that clients orders.
                    if (Utils.IsNumeric(_uid)) strFilter += " and UserId = " + _uid + " ";

                    if (Utils.IsNumeric(navigationData.RecordCount))
                    {
                        recordcount = Convert.ToInt32(navigationData.RecordCount);
                    }
                    else
                    {
                        recordcount = ModCtrl.GetListCount(PortalId, -1, "ORDER", strFilter); 
                        navigationData.RecordCount = recordcount.ToString("");
                    }

                    //Default orderby if not set
                    const string strOrder = " Order by ModifiedDate DESC  ";
                    rpData.DataSource = ModCtrl.GetList(PortalId, -1, "ORDER", strFilter, strOrder, 0, pagenumber, pagesize, recordcount);
                    rpData.DataBind();
                    
                    if (pagesize > 0)
                    {
                        CtrlPaging.PageSize = pagesize;
                        CtrlPaging.CurrentPage = pagenumber;
                        CtrlPaging.TotalRecords = recordcount;
                        CtrlPaging.BindPageLinks();
                    }

                }
            }

            #endregion

            // display header (Do header after the data return so the productcount works)
            base.DoDetail(rpDataH);

            // display footer
            base.DoDetail(rpDataF);

            // display search
            base.DoDetail(rpSearch);
        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var tabId = TabId;
            var param = new string[3];
            if (_uid != "") param[0] = "uid=" + _uid;
            var navigationData = new NavigationData(PortalId, "AdminOrders", StoreSettings.Current.StorageTypeClient);

            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "reorder":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        orderData.CopyToCart(DebugMode);
                        tabId = StoreSettings.Current.GetInt("carttab");
                        if (tabId == 0) tabId = TabId;
                    }
                    Response.Redirect(Globals.NavigateURL(tabId, "", param), true);
                    break;
                case "editorder":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        orderData.ConvertToCart(DebugMode);
                        tabId = StoreSettings.Current.GetInt("carttab");
                        if (tabId == 0) tabId = TabId;
                    }
                    Response.Redirect(Globals.NavigateURL(tabId, "", param), true);
                    break;
                case "return":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "search":
                    var strXml = GenXmlFunctions.GetGenXml(rpSearch, "", "");
                    navigationData.Build(strXml, _templSearch);
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpSearch);
                    navigationData.XmlData = GenXmlFunctions.GetGenXml(rpSearch);
                    navigationData.Save();
                    if (DebugMode)
                    {
                        strXml = "<root><sql><![CDATA[" + navigationData.Criteria + "]]></sql>" + strXml + "</root>";
                        var xmlDoc = new System.Xml.XmlDataDocument();
                        xmlDoc.LoadXml(strXml);
                        xmlDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_search.xml");
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "orderby":
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpData);
                    navigationData.Save();
                    break;
                case "viewclient":
                    param[1] = "ctrl=clients";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


        private void DisplayDataEntryRepeater(String entryId)
        {
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var orderData = new OrderData(PortalId, Convert.ToInt32(entryId));

                //render the detail page
                base.DoDetail(rpData, orderData.GetInfo());

                base.DoDetail(rpItemH, orderData.GetInfo());
                rpItem.DataSource = orderData.GetCartItemList();
                rpItem.DataBind();
                base.DoDetail(rpItemF, orderData.GetInfo());

            }
        }


    }

}

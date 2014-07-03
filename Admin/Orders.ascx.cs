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
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Orders : NBrightBuyBase
    {

        private GenXmlTemplate _templSearch; 
        private String _entryid = "";
        private Boolean _displayentrypage = false;

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.CtrlTypeCode = "ORDER";
            base.DisableUserInfo = true;

            base.OnInit(e);

            CtrlPaging.Visible = false; // don't bother with paging on this module.
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
                var t3 = "ordersheader.html";

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
                var navigationData = new NavigationData(PortalId, "OrderAdmin", StoreSettings.Current.Get("DataStorageType"));
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
                    // check the display header to see if we have a sqlfilter defined.
                    var navigationData = new NavigationData(PortalId, "AdminOrders", StoreSettings.Current.Get("DataStorageType"));
                    var strFilter = navigationData.Criteria;
                    //Default orderby if not set
                    var strOrder = " Order by ModifiedDate DESC  ";
                    rpData.DataSource = ModCtrl.GetList(PortalId, -1, "ORDER", strFilter, strOrder, 200);
                    rpData.DataBind();

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
            var param = new string[3];
            var navigationData = new NavigationData(PortalId, "AdminOrders", StoreSettings.Current.Get("DataStorageType"));

            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "reorder":
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        orderData.CopyToCart(DebugMode);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
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

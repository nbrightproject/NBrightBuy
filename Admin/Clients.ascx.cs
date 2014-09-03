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
    public partial class Clients : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch; 
        private String _entryid = "";
        private Boolean _displayentrypage = false;

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            EnablePaging = true; 

            base.OnInit(e);

            CtrlPaging.Visible = true;
            CtrlPaging.UseListDisplay = true;
            try
            {
                #region "set templates based on entry id (eid) from url"

                _entryid = Utils.RequestQueryStringParam(Context, "uid");

                if (_entryid != "") _displayentrypage = true;

                #endregion

                #region "load templates"

                // Get Search
                var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "clientssearch.html", Utils.GetCurrentCulture(), DebugMode);
                _templSearch = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                rpSearch.ItemTemplate = _templSearch;

                var t1 = "clientsheader.html";
                var t2 = "clientsbody.html";
                var t3 = "clientsfooter.html";

                if (Utils.IsNumeric(_entryid))
                {
                    t1 = "clientsdetailheader.html";
                    t2 = "clientsdetail.html";
                    t3 = "clientsdetailfooter.html";
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
                var navigationData = new NavigationData(PortalId, "ClientAdmin");
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
                    var navigationData = new NavigationData(PortalId, "ClientsAdmin");
                    
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

                    if (Utils.IsNumeric(navigationData.RecordCount))
                    {
                        recordcount = Convert.ToInt32(navigationData.RecordCount);
                    }
                    else
                    {
                        recordcount = ModCtrl.GetDnnUsersCount(PortalId, "%" + sInfo.GetXmlProperty("genxml/textbox/txtsearch") + "%");
                        navigationData.RecordCount = recordcount.ToString("");
                    }

                    //display list, with search filter
                    var userlist = ModCtrl.GetDnnUsers(PortalId, "%" + sInfo.GetXmlProperty("genxml/textbox/txtsearch") + "%", 0,pagenumber,pagesize,recordcount);
                    rpData.DataSource = userlist;
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

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];
            var navigationData = new NavigationData(PortalId, "ClientsAdmin");

            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    param[0] = "uid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "return":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "search":
                    navigationData.XmlData = GenXmlFunctions.GetGenXml(rpSearch,"","");
                    navigationData.Save();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "unlockuser":
                    if (Utils.IsNumeric(cArg))
                    {
                        var clientData = new ClientData(PortalId, Convert.ToInt32(cArg));
                        clientData.UnlockUser();
                    }
                    param[0] = "uid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "validateuser":
                    if (Utils.IsNumeric(cArg))
                    {
                        var clientData = new ClientData(PortalId, Convert.ToInt32(cArg));
                        clientData.AuthoriseClient();
                        clientData.AddClientRole(ModSettings);
                        if (StoreSettings.Current.Get("resetpasswordonclientvalidate") == "True") clientData.ResetPassword();                            
                    }
                    param[0] = "uid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "resetpass":
                    if (Utils.IsNumeric(cArg))
                    {
                        var clientData = new ClientData(PortalId, Convert.ToInt32(cArg));
                        clientData.ResetPassword();
                    }
                    param[0] = "uid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "viewaddressbook":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        param[0] = "ctrl=addressbook";
                        param[1] = "uid=" + cArg;
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "vieworders":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        param[0] = "ctrl=orders";
                        param[1] = "uid=" + cArg;
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "createorder":
                    param[0] = "";
                    var tabId = TabId;
                    if (Utils.IsNumeric(cArg))
                    {
                        var cart = new CartData(PortalId);
                        cart.UserId = Convert.ToInt32(cArg);
                        cart.EditMode = "C";
                        cart.Save();
                        tabId = StoreSettings.Current.GetInt("productlisttab");
                        if (tabId==0) tabId = TabId;
                    }
                    Response.Redirect(Globals.NavigateURL(tabId, "", param), true);
                    break;
                case "updateemail":
                    if (Utils.IsNumeric(cArg))
                    {
                        var email = GenXmlFunctions.GetField(rpData,"email");
                        var clientData = new ClientData(PortalId, Convert.ToInt32(cArg));
                        clientData.UpdateEmail(email);
                    }
                    param[0] = "uid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }



        #endregion


        private void DisplayDataEntryRepeater(String entryId)
        {
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var clientData = new ClientData(PortalId, Convert.ToInt32(entryId));

                clientData.OutputDebugFile("debug_client.xml");
                
                //render the detail page
                base.DoDetail(rpData, clientData.GetInfo());

            }
        }


    }

}

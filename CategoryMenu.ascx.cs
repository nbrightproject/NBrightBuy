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

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class CategoryMenu : NBrightBuyBase
    {

        private String _catid = "";
        private String _catname = "";
        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private String _templH = "";
        private String _templD = "";
        private String _templDfoot = "";
        private String _templF = "";
        private GrpCatController _catGrpCtrl;
        private String _entryid = "";
        private String _tabid = "";
        private string _targetModuleKey;
        
        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            _catGrpCtrl = new GrpCatController(Utils.GetCurrentCulture());

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {
                _targetModuleKey = "";
                _targetModuleKey = ModSettings.Get("targetmodulekey");

                _entryid = Utils.RequestQueryStringParam(Context, "eid");
                _catid = Utils.RequestQueryStringParam(Context, "catid");
                var navigationdata = new NavigationData(PortalId, _targetModuleKey);
                if (Utils.IsNumeric(_catid)) navigationdata.Delete(); // if a category button has been clicked (in url) then clear search;
                if (Utils.IsNumeric(navigationdata.CategoryId) && navigationdata.FilterMode) _catid = navigationdata.CategoryId;
                if (_catid == "") _catid = ModSettings.Get("defaultcatid");

                _templH = ModSettings.Get("txtdisplayheader");
                _templD = ModSettings.Get("txtdisplaybody");
                _templDfoot = ModSettings.Get("txtdisplaybodyfoot");
                _templF = ModSettings.Get("txtdisplayfooter");

                _tabid = ModSettings.Get("ddllisttabid");
                if (!Utils.IsNumeric(_tabid)) _tabid = TabId.ToString("");

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, _templH, Utils.GetCurrentCulture(), DebugMode); 


                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                _templateHeader = (GenXmlTemplate)rpDataH.ItemTemplate;

                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, _templateHeader, ModSettings.Settings(), null, DebugMode);

                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, _templD, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, _templF, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory); 


            }
            catch (Exception exc)
            {
                rpDataF.ItemTemplate = new GenXmlTemplate(exc.Message, ModSettings.Settings());
                // catch any error and allow processing to continue, output error as footer template.
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
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {
                var catid = 0;
                if (Utils.IsNumeric(_catid)) catid = Convert.ToInt32(_catid);

                #region "Get default category into list for displaying header and footer templates on product (breadcrumb)"

                // if we have a product displaying, get the deault category for the category
                var obj = new GroupCategoryData();
                if (Utils.IsNumeric(_entryid))
                {
                    var catiddef = _catGrpCtrl.GetDefaultCatId(Convert.ToInt32(_entryid));
                    obj = _catGrpCtrl.GetCategory(catiddef);
                }
                var catl = new List<object> {obj};

                #endregion

                #region "Data Repeater"


                if (_templD.Trim() != "") // if we don;t have a template, don't do anything
                {
                    var menutype = ModSettings.Get("ddlmenutype").ToLower();

                    #region "Drill Down"

                    if (menutype == "drilldown")
                    {

                        var l = _catGrpCtrl.GetCategoriesWithUrl(catid, TabId);
                        if (l.Count == 0 && (ModSettings.Get("alwaysshow") == "True"))
                        {
                            // if we have no categories, it could be the end of branch or product view, so load the root menu
                            var catid2 = 0;
                            _catid = ModSettings.Get("defaultcatid");
                            if (Utils.IsNumeric(_catid)) catid2 = Convert.ToInt32(_catid);
                            l = _catGrpCtrl.GetCategoriesWithUrl(catid2, TabId);
                        }
                        rpData.DataSource = l;
                        rpData.DataBind();
                    }

                    #endregion

                    #region "treeview"

                    if (menutype == "treeview")
                    {
                        var catidtree = 0;
                        if (Utils.IsNumeric(ModSettings.Get("defaultcatid"))) catidtree = Convert.ToInt32(ModSettings.Get("defaultcatid"));

                        rpData.Visible = false;
                        var catBuiler = new CatMenuBuilder(_templD, ModSettings, catid, DebugMode);
                        var strOut = catBuiler.GetTreeCatList(50, catidtree, Convert.ToInt32(_tabid), ModSettings.Get("treeidentclass"), ModSettings.Get("treerootclass"));

                        // if debug , output the html used.
                        if (DebugMode) Utils.SaveFile(PortalSettings.HomeDirectoryMapPath + "debug_treemenu.html", strOut);

                        var l = new Literal {Text = strOut};
                        phData.Controls.Add(l);
                    }

                    #endregion

                    #region "Accordian"

                    if (menutype == "accordian")
                    {

                    }

                    #endregion

                    #region "megamenu"

                    if (menutype == "megamenu")
                    {

                    }

                    #endregion
                }

                // display header
                rpDataH.DataSource = catl;
                rpDataH.DataBind();

                // display footer
                rpDataF.DataSource = catl;
                rpDataF.DataBind();

                #endregion
        }

        #endregion



    }

}

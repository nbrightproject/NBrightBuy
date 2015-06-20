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
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

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
    public partial class CategoryMenu : NBrightBuyFrontOfficeBase
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
                _catname = Utils.RequestQueryStringParam(Context, "catref");
                if (_catid == "" && _catname != "") _catid = CategoryUtils.GetCatIdFromName(_catname);

                var navigationdata = new NavigationData(PortalId, _targetModuleKey);
                if (Utils.IsNumeric(_catid)) navigationdata.Delete(); // if a category button has been clicked (in url) then clear search;
                if (Utils.IsNumeric(navigationdata.CategoryId) && navigationdata.FilterMode) _catid = navigationdata.CategoryId;
                if (Utils.IsNumeric(_entryid)) 
                {
                    // Get catid from product
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(_entryid), Utils.GetCurrentCulture());
                    var catDef = prodData.GetDefaultCategory();
                    if (catDef != null) _catid = catDef.categoryid.ToString("");
                }
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
                if (catid == 0)
                {
                    catid = _catGrpCtrl.GetDefaultCatId(Convert.ToInt32(_entryid));
                }
            }
            else
            {
                if (catid != 0) obj = _catGrpCtrl.GetCategory(catid);
            }
            var catl = new List<object> {obj};

            if (Utils.IsNumeric(catid) && ModSettings.Get("injectseo") == "True")
            {
                var eid = Utils.RequestQueryStringParam(Context, "eid");
                var objSEOCat = ModCtrl.GetData(Convert.ToInt32(catid), "CATEGORYLANG", Utils.GetCurrentCulture());
                if (objSEOCat != null && eid == "")  // we may have a detail page and listonly module, in which can we need the product detail as page title
                {
                    //Page Title
                    var seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                    if (seoname == "") seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");

                    var newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
                    if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                    if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                    if (newBaseTitle != "") BasePage.Title = BasePage.Title + " > " + newBaseTitle;
                    //Page KeyWords
                    var newBaseKeyWords = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
                    if (newBaseKeyWords != "") BasePage.KeyWords = newBaseKeyWords;
                    //Page Description
                    var newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                    if (newBaseDescription == "") newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategorydesc");
                    if (newBaseDescription != "") BasePage.Description = newBaseDescription;

                }
            }


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

                        var cachekey = "CatMenu*" + ModuleId.ToString("") + "*" + catid + "*" + catidtree.ToString();
                        var strOut = (String) NBrightBuyUtils.GetModCache(cachekey);
                        if (strOut == null)
                        {

                            rpData.Visible = false;
                            var catBuiler = new CatMenuBuilder(_templD, ModSettings, catid, DebugMode);
                            strOut = catBuiler.GetTreeCatList(50, catidtree, Convert.ToInt32(_tabid), ModSettings.Get("treeidentclass"), ModSettings.Get("treerootclass"));

                            // if debug , output the html used.
                            if (StoreSettings.Current.DebugModeFileOut) Utils.SaveFile(PortalSettings.HomeDirectoryMapPath + "debug_treemenu.html", strOut);
                            NBrightBuyUtils.SetModCache(ModuleId,cachekey, strOut);
                        }
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

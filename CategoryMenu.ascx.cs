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
        private String _templF = "";
        private GrpCatController _catGrpCtrl;
        private String _entryid = "";
        
        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.EntityTypeCode = "CATEGORY";
            base.CtrlTypeCode = "CATEGORY";
            base.EntityTypeCodeLang = "CATEGORYLANG";

            base.OnInit(e);

            _catGrpCtrl = new GrpCatController(Utils.GetCurrentCulture());

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {
                _catid = Utils.RequestQueryStringParam(Context, "catid");
                _entryid = Utils.RequestQueryStringParam(Context, "eid");
                _catname = Utils.RequestQueryStringParam(Context, "category");

                _templH = ModSettings.Get("txtdisplayheader");
                _templD = ModSettings.Get("txtdisplaybody");
                _templF = ModSettings.Get("txtdisplayfooter");



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
            #region "Get category id"

            var catid = 0;
            // get category list data
            NBrightInfo objCat = null;
            if (_catname != "") // if catname passed in url, calculate what the catid is
            {
                objCat = ModCtrl.GetByGuidKey(PortalId, ModuleId, "CATEGORYLANG", _catname);
                if (objCat == null)
                {
                    // check it's not just a single language
                    objCat = ModCtrl.GetByGuidKey(PortalId, ModuleId, "CATEGORY", _catname);
                    if (objCat != null) _catid = objCat.ItemID.ToString("");
                }
                else
                {
                    _catid = objCat.ParentItemId.ToString("");
                }
            }

            if (_catid == "") _catid = ModSettings.Get("defaultcatid");
            if (_catid == "") _catid = "0";
            if (Utils.IsNumeric(_catid)) catid = Convert.ToInt32(_catid);
            
            #endregion

            #region "Get category into list"

            var obj = _catGrpCtrl.GetCategory(catid);
            if (obj == null)
            {
                // if we have a product displaying, get the deault category for the category
                if (Utils.IsNumeric(_entryid))
                {
                    catid = _catGrpCtrl.GetDefaultCatId(Convert.ToInt32(_entryid));
                    obj = _catGrpCtrl.GetCategory(catid);
                }
                if (obj == null) obj = new GroupCategoryData();
            }
            var catl = new List<object> { obj };

            #endregion

            // display header
            rpDataH.DataSource = catl;
            rpDataH.DataBind();

            #region "Data Repeater"


            if (_templD.Trim() != "") // if we don;t have a template, don't do anything
            {
                var menutype = ModSettings.Get("rblmenutype").ToLower();

                #region "Drill Down"

                if (menutype == "drilldown")
                {
                    var l = _catGrpCtrl.GetCategoriesWithUrl(catid, TabId);
                    // if we have no categories, it could be the end of branch or product view, so load the root menu
                    if (l.Count == 0)
                    {
                        catid = 0;
                        _catid = ModSettings.Get("defaultcatid");
                        if (Utils.IsNumeric(_catid)) catid = Convert.ToInt32(_catid);
                        l = _catGrpCtrl.GetCategoriesWithUrl(catid, TabId);
                    }
                    rpData.DataSource = l;
                    rpData.DataBind();
                }

                #endregion

                #region "treeview"

                if (menutype == "treeview")
                {

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

            #endregion

            // display footer
            rpDataF.DataSource = catl;
            rpDataF.DataBind();

        }

        #endregion


    }

}

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
    public partial class ProductView : NBrightBuyBase
    {

        private String _eid = "";
        private String _ename = "";
        private String _catid = "";
        private String _catname = "";
        private String _modkey = "";
        private String _pagemid = "";
        private String _pagenum = "1";
        private String _pagesize = "";
        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private String _strOrder = "";
        private String _templH = "";
        private String _templD = "";
        private String _templF = "";
        private Boolean _displayentrypage = false;
        private String _orderbyindex = "";
        private NavigationData _navigationdata;

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.EntityTypeCode = "PRD";
            base.CtrlTypeCode = "PRD";
            base.EntityTypeCodeLang = "PRDLANG";

            base.OnInit(e);

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            _navigationdata = new NavigationData(PortalId, TabId, ModuleKey, StoreSettings.Current.Get("storagetype"));

            // Pass in a template specifying the token to create a friendly url for paging. 
            // (NOTE: we need this in NBB becuase the edit product from list return url will copy the page number and hence paging will not work after editing if we don;t do this)
            CtrlPaging.HrefLinkTemplate = "[<tag type='valueof' databind='PreText' />][<tag type='nbb:hrefpagelink' moduleid='" + ModuleId.ToString("") + "' />][<tag type='valueof' databind='PostText' />]";
            CtrlPaging.UseListDisplay = true;
            try
            {
                _catid = Utils.RequestQueryStringParam(Context, "catid");
                _catname = Utils.RequestQueryStringParam(Context, "category");
                
                #region "set templates based on entry id (eid) from url"

                _eid = Utils.RequestQueryStringParam(Context, "eid");
                _ename = Utils.RequestQueryStringParam(Context, "entry");
                _modkey = Utils.RequestQueryStringParam(Context, "modkey");
                _pagemid = Utils.RequestQueryStringParam(Context, "pagemid");
                _pagenum = Utils.RequestQueryStringParam(Context, "page");
                _pagesize = Utils.RequestQueryStringParam(Context, "pagesize");
                _orderbyindex = Utils.RequestQueryStringParam(Context, "orderby");
                

                // see if we need to display the entry page.
                if ((_modkey == ModuleKey | _modkey == "") && (_eid != "" | _ename != "")) _displayentrypage = true;
                if (ModSettings.Get("listonly").ToLower() == "true") _displayentrypage = false;

                // get template codes
                if (_displayentrypage)
                {
                    _templH = ModSettings.Get("txtdisplayentryheader");
                    _templD = ModSettings.Get("txtdisplayentrybody");
                    _templF = ModSettings.Get("txtdisplayentryfooter");
                }
                else
                {
                    _templH = ModSettings.Get("txtdisplayheader");
                    _templD = ModSettings.Get("txtdisplaybody");
                    _templF = ModSettings.Get("txtdisplayfooter");
                }


                #endregion


                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, _templH, Utils.GetCurrentCulture(), DebugMode); 

                //-------------------------------------------------------------------------
                //Get default sort order and filter from the displayheader template.  Use template data, becuase repeater is not fully initialized yet.
                _strOrder = _navigationdata.OrderBy;
                if (String.IsNullOrEmpty(_strOrder)) _strOrder = GenXmlFunctions.GetSqlOrderBy(rpDataHTempl); // get default
                if (_orderbyindex != "") // if we have orderby set in url, find the meta tags
                {
                    _strOrder = GenXmlFunctions.GetSqlOrderBy(rpDataHTempl,_orderbyindex);
                    // save the selected orderby to the cookie, so we can page with it.
                    _navigationdata.OrderBy = _strOrder;
                }
                //-------------------------------------------------------------------------

                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                _templateHeader = (GenXmlTemplate)rpDataH.ItemTemplate;

                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, _templateHeader, ModSettings.Settings(), null, DebugMode);

                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, _templD, Utils.GetCurrentCulture(), DebugMode);
                //if body template doesn't contain a default moduleid add it.
                if (!rpDataTempl.ToLower().Contains("nbb:modeldefault")) rpDataTempl = "[<tag type='nbb:modeldefault' />]" + rpDataTempl;

                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, _templF, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory); 


            }
            catch (Exception exc)
            {
                // remove any cookie which might store SQL in error.
                _navigationdata.Delete();

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
                // remove any nav data which might store SQL in error.
                _navigationdata.Delete();
            }
        }

        private void PageLoad()
        {

            #region "Data Repeater"
            if (_templD.Trim() != "")  // if we don;t have a template, don't do anything
            {

                if (_displayentrypage)
                {
                    // get correct itemid, based on eid given
                    if (_ename != "")
                    {
                        var o = ModCtrl.GetByGuidKey(PortalId, ModuleId, EntityTypeCodeLang, _ename);
                        if (o == null)
                        {
                            o = ModCtrl.GetByGuidKey(PortalId, ModuleId, EntityTypeCode, _ename);
                            if (o != null)
                            {
                                _eid = o.ItemID.ToString("");
                            }
                        }
                        else
                        {
                            _eid = o.ParentItemId.ToString("");
                        }
                    }

                    DisplayDataEntryRepeater(_eid);

                }
                else
                {
                    #region "Get Paging setup"
                    //See if we have a pagesize, uses the "searchpagesize" tag token.
                    // : This can be overwritten by the cookie value if we need user selection of pagesize.
                    CtrlPaging.Visible = false;
                    var pageSize = 0;
                    if (Utils.IsNumeric(_navigationdata.PageSize)) pageSize = Convert.ToInt32(_navigationdata.PageSize);
                    //check for url param page size
                    if (Utils.IsNumeric(_pagesize) && (_pagemid == "" | _pagemid == ModuleId.ToString(CultureInfo.InvariantCulture)))
                    {
                        pageSize = Convert.ToInt32(_pagesize);
                    }

                    if (pageSize == 0)
                    {
                        var strPgSize = "";
                        if (_templateHeader != null) strPgSize = _templateHeader.GetHiddenFieldValue("searchpagesize");
                        if (_templateHeader != null && strPgSize == "") strPgSize = _templateHeader.GetHiddenFieldValue("pagesize");
                        if (Utils.IsNumeric(strPgSize)) pageSize = Convert.ToInt32(strPgSize);
                    }
                    if (pageSize > 0) CtrlPaging.Visible = true;
                    _navigationdata.PageSize = pageSize.ToString("");

                    var pageNumber = 1;

                    //check for url param paging
                    if (Utils.IsNumeric(_pagenum) && (_pagemid == "" | _pagemid == ModuleId.ToString(CultureInfo.InvariantCulture)))
                    {
                        pageNumber = Convert.ToInt32(_pagenum);
                    }

                    //Get returnlimt from module settings
                    var returnlimit = 0;
                    var strreturnlimit = ModSettings.Get("returnlimit");
                    if (Utils.IsNumeric(strreturnlimit)) returnlimit = Convert.ToInt32(strreturnlimit);

                    #endregion

                    #region "Get filter setup"

                    var strFilter = "";
                    if (_navigationdata.Exists && (_catid == "" && _catname == "")) // ignore navdata if we pass a catid in the url.
                    {
                        // if navdata is not deleted then get filter from navdata, created by productsearch module.
                        strFilter = _navigationdata.Criteria;
                        _strOrder = _navigationdata.OrderBy; 
                    }
                    else
                    {
                        // We have a category selected (in url), so overwrite categoryid navigationdata.
                        // This allows the return to the same category after a returning from a entry view.
                        _navigationdata.CategoryId = _catid;

                        // check the display header to see if we have a sqlfilter defined.
                        strFilter = GenXmlFunctions.GetSqlSearchFilters(rpDataH);
                    }

                    #endregion

                    #region "Get Category select setup"

                    //get default catid.
                    var defcatid = ModSettings.Get("defaultcatid");
                    if (Utils.IsNumeric(defcatid))
                    {
                        // if we have no filter use the default category
                        if (_catid == "" && strFilter.Trim() == "") _catid = defcatid;

                        // If we have a static list,then always display the default category
                        if (ModSettings.Get("staticlist") == "True")
                        {
                            _catid = defcatid;
                        }
                    }

                    //check if we are display categories 
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
                        // We have a category selected (in url), so overwrite categoryid navigationdata.
                        // This allows the return to the same category after a returning from a entry view.
                        _navigationdata.CategoryId = _catid;
                    }

                    if (Utils.IsNumeric(_catid))
                    {
                        var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                        var dbOwner = DataProvider.Instance().DatabaseOwner;
                        if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                        {
                            strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + _catid + ") ";
                        }
                        else
                            strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + _catid + ") ";

                        if (Utils.IsNumeric(_catid)) objCat = GetData(Convert.ToInt32(_catid), "CATEGORYLANG", Utils.GetCurrentCulture());
                        if (objCat != null)
                        {
                            // add SEO data from category
                            var basePage = (DotNetNuke.Framework.CDefault) base.Page;
                            //Page Title
                            var newBaseTitle = objCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseotitle");
                            if (newBaseTitle == "") newBaseTitle = objCat.GetXmlProperty("genxml/lang/genxml/textbox/txttitle");
                            if (newBaseTitle != "") basePage.Title = newBaseTitle;
                            //Page KeyWords
                            var newBaseKeyWords = objCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseokeywords");
                            if (newBaseKeyWords != "") basePage.KeyWords = newBaseKeyWords;
                            //Page Description
                            var newBaseDescription = objCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseodescription");
                            if (newBaseDescription == "") newBaseDescription = objCat.GetXmlProperty("genxml/lang/genxml/textbox/txtdescription");
                            if (newBaseDescription != "") basePage.Description = newBaseDescription;
                        }
                    }
                    else
                    {
                        _navigationdata.CategoryId = "";
                    }

                    #endregion


                    #region "itemlists (wishlist)"

                    // if we have a itemListName field then get the itemlist cookie.
                    var itemListName = "";
                    var itemListAction = "";
                    if (_templateHeader != null) itemListName = _templateHeader.GetHiddenFieldValue("itemlistname");
                    if (_templateHeader != null) itemListAction = _templateHeader.GetHiddenFieldValue("itemlistaction");
                    if (itemListAction == "wishlist" || itemListAction == "both")
                    {
                        var cw = new ItemListData(Request, Response, 0, itemListName);
                        var showList = !(itemListAction == "both" && !cw.Active);
                        if (showList)
                        {
                            if (cw.Exists && cw.ItemCount != "0")
                            {
                                strFilter = " and (";
                                foreach (var i in cw.GetItemList())
                                {
                                    strFilter += " NB1.itemid = '" + i + "' or";
                                }
                                strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") "; // remove the last "or"                    
                            }
                            else
                            {
                                //no data in list so select false itemid to stop anything displaying
                                strFilter += " and (NB1.itemid = '-1') ";
                            }
                        }
                    }

                    #endregion

                    //Default orderby if not set
                    if (String.IsNullOrEmpty(_strOrder)) _strOrder = " Order by ModifiedDate DESC  ";

                    // save navigation data
                    _navigationdata.PageModuleId = Utils.RequestParam(Context, "pagemid");
                    _navigationdata.PageNumber = Utils.RequestParam(Context, "page");
                    if (Utils.IsNumeric(_catid)) _navigationdata.PageName = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(_catid));

                    // save the last active modulekey to a cookie, so it can be used by the "NBrightBuyUtils.GetReturnUrl" function
                    NBrightCore.common.Cookie.SetCookieValue(PortalId, "NBrigthBuyLastActive", "ModuleKey", ModuleKey,1);

                    var recordCount = ModCtrl.GetDataListCount(PortalId, ModuleId, "PRD", strFilter, "PRDLANG", Utils.GetCurrentCulture(), DebugMode);

                    _navigationdata.RecordCount = recordCount.ToString("");
                    _navigationdata.Save();

                    if (returnlimit > 0 && returnlimit < recordCount) recordCount = returnlimit; 
                    rpData.DataSource = ModCtrl.GetDataList(PortalId, ModuleId, "PRD", "PRDLANG", Utils.GetCurrentCulture(), strFilter, _strOrder, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);
                    rpData.DataBind();

                    if (pageSize > 0)
                    {
                        CtrlPaging.PageSize = pageSize;
                        CtrlPaging.CurrentPage = pageNumber;
                        CtrlPaging.TotalRecords = recordCount;
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
            var param = new string[2];
            if (_eid != "") param[0] = "eid=" + _eid;
            if (_modkey != "") param[1] = "modkey=" + _modkey;

            switch (e.CommandName.ToLower())
            {
                case "wishlistadd":
                    if (Utils.IsNumeric(cArg))
                    {
                        var legacyItemId = NBrightBuyV2Utils.GetLegacyProductId(Convert.ToInt32(cArg));
                        WishList.AddProduct(PortalId, legacyItemId.ToString(""), UserInfo);                        
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "wishlistremove":
                    if (Utils.IsNumeric(cArg))
                    {
                        var legacyItemId = NBrightBuyV2Utils.GetLegacyProductId(Convert.ToInt32(cArg));
                        WishList.RemoveProduct(PortalId, legacyItemId.ToString(""));                        
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "addtobasket":
                    NBrightBuyV2Utils.AddToCart(rpData, StoreSettings.Current.DataInfo, Request, e.Item.ItemIndex, DebugMode);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        #region "Methods"

        private void DisplayDataEntryRepeater(String entryId)
        {
            base.ItemId = entryId;

            NBrightInfo objInfo = null;
            if (Utils.IsNumeric(ItemId) && ItemId != "0") objInfo = GetData(Convert.ToInt32(entryId), EntityTypeCodeLang, EntityLangauge);
            if (objInfo == null) objInfo = new NBrightInfo { ModuleId = ModuleId, PortalId = PortalId, XMLData = "<genxml></genxml>" };

            // if debug , output the xml used.
            if (DebugMode)
            {
                var xmlDoc = new System.Xml.XmlDataDocument();
                xmlDoc.LoadXml(objInfo.XMLData);
                xmlDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_entry.xml");
            }
            // insert page header text
            NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, (GenXmlTemplate)rpData.ItemTemplate, ModSettings.Settings(), objInfo, DebugMode);
            //render the detail page
            base.DoDetail(rpData, objInfo);
        }

        #endregion



    }

}

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
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ProductView : NBrightBuyFrontOfficeBase
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
        private const String EntityTypeCode = "PRD";
        private const String EntityTypeCodeLang = "PRDLANG";
        private String _itemListName = "";
        private String _print = "";
        private String _printtemplate = "";
        private String _guidkey = "";
        private Boolean _razortemplate = false;
        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            _eid = Utils.RequestQueryStringParam(Context, "eid");
            _print = Utils.RequestParam(Context, "print");
            _printtemplate = Utils.RequestParam(Context, "template");

            EnablePaging = true;
            
            base.OnInit(e);

            // if guidkey entered instead of eid, find it using the guid and assign to _eid
            _guidkey = Utils.RequestQueryStringParam(Context, "guidkey");
            if (_guidkey == "") _guidkey = Utils.RequestQueryStringParam(Context, "ref");
            if (_eid== "" && _guidkey != "")
            {
                var guidData = ModCtrl.GetByGuidKey(PortalId, -1, "PRD", _guidkey);
                if (guidData != null)
                    _eid = guidData.ItemID.ToString("D");
                else
                    _eid = "0";
            }

            // if we want to print we need to open the browser with a startup script, this points to a Printview.aspx. (Must go after the ModSettings has been init.)
            if (_print != "") Page.ClientScript.RegisterStartupScript(this.GetType(), "printproduct", "window.open('" + StoreSettings.NBrightBuyPath() + "/PrintView.aspx?itemid=" + _eid + "&printcode=" + _print + "&template=" + _printtemplate + "&theme=" + ModSettings.Get("themefolder") + "','_blank');", true);

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            _navigationdata = new NavigationData(PortalId, ModuleKey);

            // Pass in a template specifying the token to create a friendly url for paging. 
            // (NOTE: we need this in NBS becuase the edit product from list return url will copy the page number and hence paging will not work after editing if we don;t do this)
            CtrlPaging.HrefLinkTemplate = "[<tag type='valueof' databind='PreText' />][<tag type='if' databind='Text' testvalue='' display='{OFF}' />][<tag type='hrefpagelink' moduleid='" + ModuleId.ToString("") + "' />][<tag type='endif' />][<tag type='valueof' databind='PostText' />]";
            CtrlPaging.UseListDisplay = true;
            try
            {
                _catid = Utils.RequestQueryStringParam(Context, "catid");
                _catname = Utils.RequestQueryStringParam(Context, "catref");
                
                #region "set templates based on entry id (eid) from url"

                _ename = Utils.RequestQueryStringParam(Context, "entry");
                _modkey = Utils.RequestQueryStringParam(Context, "modkey");
                _pagemid = Utils.RequestQueryStringParam(Context, "pagemid");
                _pagenum = Utils.RequestQueryStringParam(Context, "page");
                _pagesize = Utils.RequestQueryStringParam(Context, "pagesize");
                _orderbyindex = Utils.RequestQueryStringParam(Context, "orderby");
                

                // see if we need to display the entry page.
                if ((_modkey == ModuleKey | _modkey == "") && (_eid != "" | _ename != "")) _displayentrypage = true;

                // if we have entry detail display, but no catd, get the default one.
                if (_displayentrypage && _catid == "" && Utils.IsNumeric(_eid))
                {
                    var prdData = ProductUtils.GetProductData(Convert.ToInt32(_eid),Utils.GetCurrentCulture());
                    var defcat = prdData.GetDefaultCategory();
                    if (defcat != null) _catid = defcat.categoryid.ToString("");
                }

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

                if (_templD.ToLower().EndsWith(".cshtml")) _razortemplate = true; 

                #endregion

                if (_razortemplate)
                {
                    
                }
                else
                {
                    

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

                var cachekey = "GenXmlTemplate*rpDataH" + _templH + "*" + ModuleId.ToString();
                _templateHeader = (GenXmlTemplate)Utils.GetCache(cachekey);
                if (_templateHeader == null || StoreSettings.Current.DebugMode)
                {
                    _templateHeader = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                }
                rpDataH.ItemTemplate = _templateHeader;

                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, _templateHeader, ModSettings.Settings(), null, DebugMode);

                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, _templD, Utils.GetCurrentCulture(), DebugMode);
                //if body template doesn't contain a default moduleid add it.
                if (!rpDataTempl.ToLower().Contains("nbs:modeldefault")) rpDataTempl = "[<tag type='nbs:modeldefault' />]" + rpDataTempl;
                // always add a productid hidden field to the data template (for add to cart)
                rpDataTempl = "[<tag type='hidden' id='productid' value='databind:itemid' />]" + rpDataTempl;

                cachekey = "GenXmlTemplate*rpData" + _templD + "*" + ModuleId.ToString();
                var gXml = (GenXmlTemplate)Utils.GetCache(cachekey);
                if (gXml == null || StoreSettings.Current.DebugMode)
                {
                    gXml = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);                    
                }
                rpData.ItemTemplate = gXml;

                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, _templF, Utils.GetCurrentCulture(), DebugMode);

                cachekey = "GenXmlTemplate*rpDataF" + _templF + "*" + ModuleId.ToString();
                gXml = (GenXmlTemplate)Utils.GetCache(cachekey);
                if (gXml == null || StoreSettings.Current.DebugMode)
                {
                    gXml = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                }
                rpDataF.ItemTemplate = gXml;

                }

            }
            catch (Exception exc)
            {
                // remove any cookie which might store SQL in error.
                _navigationdata.Delete();

                rpDataF.ItemTemplate = new GenXmlTemplate(exc.ToString(), ModSettings.Settings());
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
            NBrightInfo objCat = null;

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


                    #region "do standard nbright template - Depricated"

                    #region "Order BY"

                    // get orderby from header if it's there
                    var cachekey = "GetSqlOrderBy*rpDataH" + _templH + "*" + ModuleId.ToString();
                    _strOrder = (String) Utils.GetCache(cachekey);
                    if (_strOrder == null || StoreSettings.Current.DebugMode)
                    {
                        _strOrder = GenXmlFunctions.GetSqlOrderBy(rpDataH);
                    }

                    //Default orderby if not set
                    if (String.IsNullOrEmpty(_strOrder)) _strOrder = " Order by ModifiedDate DESC  ";
                    // NOTE: This setting may be overwritten by the navigatedata class in the filter setup

                    #endregion

                    #region "Get Paging setup"

                    //See if we have a pagesize, uses the "searchpagesize" tag token.
                    // : This can be overwritten by the cookie value if we need user selection of pagesize.
                    CtrlPaging.Visible = false;

                    #region "Get pagesize, from best place"

                    var pageSize = 0;
                    if (Utils.IsNumeric(_navigationdata.PageSize)) pageSize = Convert.ToInt32(_navigationdata.PageSize);
                    if (!Utils.IsNumeric(pageSize) && Utils.IsNumeric(ModSettings.Get("pagesize"))) pageSize = Convert.ToInt32(ModSettings.Get("pagesize"));
                    //check for url param page size
                    if (Utils.IsNumeric(_pagesize) && (_pagemid == "" | _pagemid == ModuleId.ToString(CultureInfo.InvariantCulture))) pageSize = Convert.ToInt32(_pagesize);
                    if (pageSize == 0)
                    {
                        var strPgSize = "";
                        if (_templateHeader != null) strPgSize = _templateHeader.GetHiddenFieldValue("searchpagesize");
                        if (_templateHeader != null && strPgSize == "") strPgSize = _templateHeader.GetHiddenFieldValue("pagesize");
                        if (Utils.IsNumeric(strPgSize)) pageSize = Convert.ToInt32(strPgSize);
                    }
                    if (pageSize > 0) CtrlPaging.Visible = true;
                    _navigationdata.PageSize = pageSize.ToString("");

                    #endregion

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

                    // check the display header to see if we have a sqlfilter defined.
                    var strFilter = "";
                    cachekey = "GetSqlSearchFilters*rpDataH" + _templH + "*" + ModuleId.ToString();
                    var strHeaderFilter = (String) Utils.GetCache(cachekey);
                    if (strHeaderFilter == null || StoreSettings.Current.DebugMode)
                    {
                        strHeaderFilter = GenXmlFunctions.GetSqlSearchFilters(rpDataH);
                    }

                    // filter mode and will persist past category selection.
                    if ((_catid == "" && _catname == ""))
                    {
                        if (!_navigationdata.FilterMode) _navigationdata.CategoryId = ""; // filter mode persist catid

                        // if navdata is not deleted then get filter from navdata, created by productsearch module.
                        strFilter = _navigationdata.Criteria;
                        if (!strFilter.Contains(strHeaderFilter)) strFilter += " " + strHeaderFilter;
                        if (!String.IsNullOrEmpty(_navigationdata.OrderBy)) _strOrder = _navigationdata.OrderBy;

                        if (_navigationdata.Mode.ToLower() == "s") _navigationdata.ResetSearch(); // single search so clear after
                    }
                    else
                    {
                        _navigationdata.ResetSearch();

                        // We have a category selected (in url), so overwrite categoryid navigationdata.
                        // This allows the return to the same category after a returning from a entry view.
                        _navigationdata.CategoryId = _catid;
                        strFilter = strHeaderFilter;
                    }

                    #endregion

                    #region "Get Category select setup"

                    //get default catid.
                    var catseo = _catid;
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
                    else
                    {
                        defcatid = ModSettings.Get("defaultpropertyid");
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
                    }

                    //check if we are display categories 
                    // get category list data
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
                            if (!String.IsNullOrEmpty(objCat.GUIDKey) && Utils.IsNumeric(_catid) && objCat.Lang != Utils.GetCurrentCulture())
                            {
                                // do a 301 redirect to correct url for the langauge (If the langauge is changed on the product list, we need to make sure we have the correct catref for the langauge)
                                var catGrpCtrl = new GrpCatController(Utils.GetCurrentCulture());
                                var activeCat = catGrpCtrl.GetCategory(Convert.ToInt32(_catid));
                                if (activeCat != null)
                                {
                                    var redirecturl = "";
                                    if (Utils.IsNumeric(_eid))
                                    {
                                        var prdData = ProductUtils.GetProductData(Convert.ToInt32(_eid), Utils.GetCurrentCulture());
                                        redirecturl = NBrightBuyUtils.GetEntryUrl(PortalId, _eid, _modkey, prdData.SEOName, TabId.ToString(), "", activeCat.categoryrefGUIDKey);
                                    }
                                    else
                                    {
                                        redirecturl = catGrpCtrl.GetCategoryUrl(activeCat, TabId);
                                    }

                                    try
                                    {
                                        if (redirecturl != "")
                                        {
                                            Response.Redirect(redirecturl, false);
                                            Response.StatusCode = (int) System.Net.HttpStatusCode.MovedPermanently;
                                            Response.End();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // catch err
                                    }
                                }
                            }
                        }
                        // We have a category selected (in url), so overwrite categoryid navigationdata.
                        // This allows the return to the same category after a returning from a entry view.
                        _navigationdata.CategoryId = _catid;
                        catseo = _catid;
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

                        if (Utils.IsNumeric(catseo))
                        {
                            var objSEOCat = ModCtrl.GetData(Convert.ToInt32(catseo), "CATEGORYLANG", Utils.GetCurrentCulture());
                            if (objSEOCat != null && _eid == "") // we may have a detail page and listonly module, in which can we need the product detail as page title
                            {
                                //Page Title
                                var seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                                if (seoname == "") seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");

                                var newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
                                if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                                if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                                if (newBaseTitle != "") BasePage.Title = newBaseTitle;
                                //Page KeyWords
                                var newBaseKeyWords = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
                                if (newBaseKeyWords != "") BasePage.KeyWords = newBaseKeyWords;
                                //Page Description
                                var newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                                if (newBaseDescription == "") newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategorydesc");
                                if (newBaseDescription != "") BasePage.Description = newBaseDescription;

                                if (PortalSettings.HomeTabId == TabId)
                                    PageIncludes.IncludeCanonicalLink(Page, Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias)); //home page always default of site.
                                else
                                {
                                    PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetListUrl(PortalId, TabId, objSEOCat.ItemID, seoname, Utils.GetCurrentCulture()));
                                }
                            }
                        }

                        if (_strOrder == "{bycategoryproduct}") _strOrder += _catid; // do special custom sort in each cateogry

                    }
                    else
                    {
                        if (!_navigationdata.FilterMode) _navigationdata.CategoryId = ""; // filter mode persist catid
                        if (_strOrder == "{bycategoryproduct}") _strOrder = " Order by ModifiedDate DESC  ";
                    }

                    #endregion

                    #region "Apply provider product filter"

                    // Special filtering can be done, by using the ProductFilter interface.
                    var productfilterkey = "";
                    if (_templateHeader != null) productfilterkey = _templateHeader.GetHiddenFieldValue("providerfilterkey");
                    if (productfilterkey != "")
                    {
                        var provfilter = FilterInterface.Instance(productfilterkey);
                        if (provfilter != null) strFilter = provfilter.GetFilter(strFilter, _navigationdata, ModSettings, Context);
                    }

                    #endregion

                    #region "itemlists (wishlist)"

                    // if we have a itemListName field then get the itemlist cookie.
                    if (_templateHeader != null) _itemListName = _templateHeader.GetHiddenFieldValue("itemlistname");
                    if (_itemListName != "")
                    {
                        var cw = new ItemListData(_itemListName);
                        if (cw.Exists && cw.ItemCount > 0)
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

                    #endregion


                    // save navigation data
                    _navigationdata.PageModuleId = Utils.RequestParam(Context, "pagemid");
                    _navigationdata.PageNumber = Utils.RequestParam(Context, "page");
                    if (Utils.IsNumeric(_catid)) _navigationdata.PageName = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(_catid));

                    // save the last active modulekey to a cookie, so it can be used by the "NBrightBuyUtils.GetReturnUrl" function
                    NBrightCore.common.Cookie.SetCookieValue(PortalId, "NBrigthBuyLastActive", "ModuleKey", ModuleKey, 1);

                    strFilter += " and (NB3.Visible = 1) "; // get only visible products

                    var recordCount = ModCtrl.GetDataListCount(PortalId, ModuleId, "PRD", strFilter, "PRDLANG", Utils.GetCurrentCulture(), DebugMode);

                    _navigationdata.RecordCount = recordCount.ToString("");
                    _navigationdata.Save();

                    if (returnlimit > 0 && returnlimit < recordCount) recordCount = returnlimit;

                    var l = ModCtrl.GetDataList(PortalId, ModuleId, "PRD", "PRDLANG", Utils.GetCurrentCulture(), strFilter, _strOrder, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);

                    if (_razortemplate)
                    {
                        #region "do razor template"

                        var strOut = NBrightBuyUtils.RazorTemplRender(_templD, ModuleId, "productviewrazor" + ModuleId.ToString(), l, "/DesktopModules/NBright/NBrightBuy", ModSettings.ThemeFolder, Utils.GetCurrentCulture());
                        var lit = new Literal();
                        lit.Text = strOut;
                        phData.Controls.Add(lit);

                        #endregion

                    }
                    else
                    {
                        rpData.DataSource = l;
                        rpData.DataBind();
                    }


                    if (_navigationdata.SingleSearchMode) _navigationdata.ResetSearch();

                    if (pageSize > 0)
                    {
                        CtrlPaging.PageSize = pageSize;
                        CtrlPaging.CurrentPage = pageNumber;
                        CtrlPaging.TotalRecords = recordCount;
                        CtrlPaging.BindPageLinks();
                    }

                    // display header (Do header after the data return so the productcount works)
                    if (objCat == null)
                        base.DoDetail(rpDataH, ModuleId);
                    else
                    {
                        if (StoreSettings.Current.DebugModeFileOut) objCat.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_categoryproductheader.xml");
                        DoDetail(rpDataH, objCat);
                    }

                    #endregion

                }
            }

            #endregion


            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var rtnTabId = TabId;
            var param = new string[4];


            if (_guidkey != "")                
                param[0] = "ref=" + _guidkey; //using ref is url
            else
                if (_eid != "") param[0] = "eid=" + _eid; // using id in url 

            if (_modkey != "") param[1] = "modkey=" + _modkey;

            if (_catname != "")                
                param[2] = "catref=" + _catname; //using ref is url
            else
                if (_catid != "") param[2] = "catid=" + _catid; //using id is url
            

            // redirect to cart after add to basket is arg is redirect
            if (cArg.ToLower() == "redirect" && Utils.IsNumeric(StoreSettings.Current.CartTabId)) rtnTabId = StoreSettings.Current.CartTabId;
            if (cArg.ToLower() == "redirectnoparam" && Utils.IsNumeric(StoreSettings.Current.CartTabId))
            {
                rtnTabId = StoreSettings.Current.CartTabId;
                param[0] = "";
                param[1] = "";
                param[2] = "";
            } 

            switch (e.CommandName.ToLower())
            {
                case "addtobasket":
                    var currentcart = new CartData(PortalId);
                    currentcart.AddItem(rpData, StoreSettings.Current.SettingsInfo, e.Item.ItemIndex, DebugMode);
                    currentcart.Save(StoreSettings.Current.DebugMode);
                    param[3] = "addcart=1";
                    Response.Redirect(Globals.NavigateURL(rtnTabId, "", param), true);
                    break;
                case "addalltobasket":
                    var currentcart2 = new CartData(PortalId);
                    foreach (RepeaterItem ri in rpData.Items)
                    {
                        currentcart2.AddItem(rpData, StoreSettings.Current.SettingsInfo, ri.ItemIndex, DebugMode);
                    }
                    currentcart2.Save(StoreSettings.Current.DebugMode);
                    param[3] = "addcart=1";
                    Response.Redirect(Globals.NavigateURL(rtnTabId, "", param), true);
                    break;
                case "addcookietobasket":
                    var currentcart3 = new CartData(PortalId);
                    currentcart3.AddCookieToCart();
                    currentcart3.Save(StoreSettings.Current.DebugMode);
                    param[3] = "addcart=1";
                    Response.Redirect(Globals.NavigateURL(rtnTabId, "", param), true);
                    break;
                case "docdownload":
                    var s = cArg.Split(':');
                    if (s.Length == 2)
                    {
                        var itemid = s[0];
                        var idx = s[1];
                        if (Utils.IsNumeric(idx) && Utils.IsNumeric(itemid))
                        {
                            var index = Convert.ToInt32(idx);
                            var prdData = ProductUtils.GetProductData(Convert.ToInt32(itemid), Utils.GetCurrentCulture());
                            if (prdData.Docs.Count >= index)
                            {
                                var docInfo = prdData.Docs[index -1];
                                var docFilePath = docInfo.GetXmlProperty("genxml/hidden/filepath");
                                var fileName = docInfo.GetXmlProperty("genxml/textbox/txtfilename"); ;
                                var fileExt = docInfo.GetXmlProperty("genxml/hidden/fileext");
                                var purchase = docInfo.GetXmlProperty("genxml/checkbox/chkpurchase");

                                if (fileName == "") fileName = "filename";
                                if (!fileName.EndsWith(fileExt)) fileName += fileExt;

                                if (purchase == "True")
                                {
                                    //[TODO: check if the document has been purchased]                                    
                                    Utils.ForceDocDownload(docFilePath, fileName, Response);
                                }
                                else
                                {
                                    Utils.ForceDocDownload(docFilePath, fileName, Response);                                                                    
                                }
                            }
                        }
                        
                    }
                    break;
                case "printproduct":
                    param[0] = "eid=" + _eid;
                    param[1] = "print=printproduct";
                    param[2] = "template=printproduct.html";
                    Response.Redirect(Globals.NavigateURL(TabId,"", param), true);
                    break;
                default :
                    //check for any command providers.
                    var providerList = NBrightCore.providers.GenXProviderManager.ProviderList;
                    if (providerList != null)
                    {
                        foreach (var prov in providerList)
                        {
                            prov.Value.CtrlItemCommand(source, e);
                        }
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        #region "Methods"

        private void DisplayDataEntryRepeater(String entryId)
        {
            var productData = ProductUtils.GetProductData(entryId, Utils.GetCurrentCulture());

            if (productData.Exists)
            {

                if (PortalSettings.HomeTabId == TabId)
                    PageIncludes.IncludeCanonicalLink(Page, Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias)); //home page always default of site.
                else
                    PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetEntryUrl(PortalId, _eid, "", productData.SEOName, TabId.ToString("")));

                // overwrite SEO data
                if (productData.SEOName != "")
                    BasePage.Title = productData.SEOTitle;
                else
                    BasePage.Title = productData.ProductName;

                if (productData.SEODescription != "") BasePage.Description = productData.SEODescription;
                if (productData.SEOTagwords != "") BasePage.KeyWords = productData.SEOTagwords;

                // if debug , output the xml used.
                if (DebugMode) productData.Info.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_entry.xml");
                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, (GenXmlTemplate)rpData.ItemTemplate, ModSettings.Settings(), productData.Info, DebugMode);

                //render the detail page
                base.DoDetail(rpData, productData.Info);

                DoDetail(rpDataH, productData.Info);  // do header here, so we pickup default cat for breadcrumb

                // do razor template
                //var razorTemplName = System.IO.Path.GetFileNameWithoutExtension(_templD) + ".cshtml";
                //var cachekey = "RazorTemplate" + razorTemplName + "*" + ModuleId.ToString() + "*" + productData.DataRecord.ItemID.ToString();
                //var razorTempl = (String)NBrightBuyUtils.GetModCache(cachekey);
                //var lit = new Literal();
                //if (razorTempl == null || StoreSettings.Current.DebugMode)
                //{
                //    razorTempl = ModCtrl.GetTemplateData(ModSettings, razorTemplName, Utils.GetCurrentCulture(),DebugMode);
                //    if (razorTempl != "")
                //    {
                //        razorTempl = GenXmlFunctions.RenderRepeater(productData.Info, razorTempl, "", "XMLData", "", ModSettings.Settings(), null);
                //        var razorTemplateKey = "RazorTemplateKey" + razorTemplName + "*" + ModuleId.ToString();
                //        razorTempl = NBrightBuyUtils.RenderRazor(productData, razorTempl, razorTemplateKey);
                //        productData.Info.GetXmlPropertyInt("");
                //        NBrightBuyUtils.SetModCache(ModuleId, cachekey, razorTempl);                        
                //    }
                //}
                //lit.Text = razorTempl;
                //phData.Controls.Add(lit);
            }

        }


        #endregion



    }

}

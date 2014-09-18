using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.SqlServer.Server;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class XmlConnector : IHttpHandler
    {
        private String _lang = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"
            
            var strOut = "";

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var itemId = Utils.RequestQueryStringParam(context, "itemid");
            var ctlType = Utils.RequestQueryStringParam(context, "ctltype");
            var idXref = Utils.RequestQueryStringParam(context, "idxref");
            var xpathpdf = Utils.RequestQueryStringParam(context, "pdf");
            var xpathref = Utils.RequestQueryStringParam(context, "pdfref");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");
            var moduleId = Utils.RequestQueryStringParam(context, "mid");
            var moduleKey = Utils.RequestQueryStringParam(context, "mkey");
            var parentid = Utils.RequestQueryStringParam(context, "parentid");
            var entryid = Utils.RequestQueryStringParam(context, "entryid");
            var entryxid = Utils.RequestQueryStringParam(context, "entryxid");
            var catid = Utils.RequestQueryStringParam(context, "catid");
            var catxid = Utils.RequestQueryStringParam(context, "catxid");
            var templatePrefix = Utils.RequestQueryStringParam(context, "tprefix");
            var value = Utils.RequestQueryStringParam(context, "value");
            var itemListName = Utils.RequestQueryStringParam(context, "listname");
            if (itemListName == "") itemListName = "ItemList";
            if (itemListName == "*") itemListName = "ItemList";

            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            //  so use the lang/lanaguge param to set it.
            if (lang == "") lang = language;
            if (!string.IsNullOrEmpty(lang)) _lang = lang;
            // default to current thread if we have no language.
            if (_lang == "") _lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            #endregion

            #endregion

            #region "Do processing of command"

            var intModuleId = 0;
            if (Utils.IsNumeric(moduleId)) intModuleId = Convert.ToInt32(moduleId);

            var objCtrl = new NBrightBuyController();

            var uInfo = new UserDataInfo(UserController.GetCurrentUserInfo().PortalID, intModuleId, objCtrl, ctlType);
            strOut = "ERROR!! - No Security rights for current user!";
            switch (paramCmd)
            {
                case "test":
                    strOut = "<root>" + UserController.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "setdata":
                    break;
                case "deldata":
                    break;
                //case "setcategoryadminform":
                //    if (CheckRights()) strOut = SetCategoryForm(context);
                //    break;
                case "getdata":
                    strOut = GetReturnData(context);
                    break;
                case "additemlist":
                    if (Utils.IsNumeric(itemId))
                    {
                        var cw = new ItemListData(-1, StoreSettings.Current.StorageTypeClient, itemListName);
                        cw.Add(itemId);
                        strOut = cw.ItemList;
                    }
                    break;
                case "removeitemlist":
                    if (Utils.IsNumeric(itemId))
                    {
                        var cw1 = new ItemListData(-1, StoreSettings.Current.StorageTypeClient, itemListName);
                        cw1.Remove(itemId);
                        strOut = cw1.ItemList;
                    }
                    break;
                case "deleteitemlist":
                        var cw2 = new ItemListData(-1, StoreSettings.Current.StorageTypeClient, itemListName);
                        cw2.Delete();
                        strOut = "deleted";
                    break;
                case "getproductselectlist":
                    strOut = GetProductList(context);
                    break;
                case "getproductlist":
                    strOut = GetProductList(context);
                    break;
                case "getcategoryproductlist":
                    strOut = GetCategoryProductList(context);
                    break;
                case "deletecatxref":
                    if (CheckRights()) strOut = DeleteCatXref(context);
                    break;
                case "selectcatxref":
                    if (CheckRights()) strOut = SelectCatXref(context);
                    break;
                case "deleteallcatxref":
                    if (CheckRights()) strOut = DeleteAllCatXref(context);
                    break;
                case "copyallcatxref":
                    if (CheckRights()) strOut = CopyAllCatXref(context);
                    break;
                case "moveallcatxref":
                    if (CheckRights()) strOut = CopyAllCatXref(context,true);
                    break;
                case "editproduct":
                    if (CheckRights()) strOut = GetProductGeneralData(context);
                    break;
                case "productdescription":
                    if (CheckRights()) strOut = GetProductDescription(context);
                    break;
                case "productmodels":
                    if (CheckRights()) strOut = GetProductModels(context);
                    break;
                case "productoptions":
                    if (CheckRights()) strOut = GetProductOptions(context);
                    break;
                case "productoptionvalues":
                    if (CheckRights()) strOut = GetProductOptionValues(context);
                    break;
                case "productimages":
                    if (CheckRights()) strOut = GetProductImages(context);
                    break;
                case "productdocs":
                    if (CheckRights()) strOut = GetProductDocs(context);
                    break;
                case "productrelatedproducts":
                    if (CheckRights()) strOut = GetProductModels(context);
                    break;
                case "productcategories":
                    if (CheckRights()) strOut = GetProductCategories(context);
                    break;
                case "productisincategory":
                    if (CheckRights()) strOut = ProductIsInCategory(context).ToString();
                    break;
                case "productgroupcategories":
                    if (CheckRights()) strOut = GetProductGroupCategories(context);
                    break;                    
                case "productrelated":
                    if (CheckRights()) strOut = GetProductRelated(context);
                    break;
                case "addproductmodels":
                    if (CheckRights()) strOut = AddProductModels(context);
                    break;
                case "addproductoptions":
                    if (CheckRights()) strOut = AddProductOptions(context);
                    break;
                case "addproductoptionvalues":
                    if (CheckRights()) strOut = AddProductOptionValues(context);
                    break;
                case "addproductcategory":
                    if (CheckRights()) strOut = AddProductCategory(context);
                    break;
                case "addproductgroupcategory":
                    if (CheckRights()) strOut = AddProductGroupCategory(context);
                    break;
                case "removeproductcategory":
                    if (CheckRights()) strOut = RemoveProductCategory(context);
                    break;
                case "removeproductgroupcategory":
                    if (CheckRights()) strOut = RemoveProductGroupCategory(context);
                    break;                    
                case "populatecategorylist":
                    if (CheckRights()) strOut = GetGroupCategoryListBox(context);
                    break;
                case "addrelatedproduct":
                    if (CheckRights()) strOut = AddRelatedProduct(context);
                    break;
                case "removerelatedproduct":
                    if (CheckRights()) strOut = RemoveRelatedProduct(context);
                    break;
            }

            #endregion

            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }



        #region "SQL Data return"

        private string GetReturnData(HttpContext context)
        {
            try
            {

                var strOut = "";

                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();

                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary();

                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

                if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                var objCtrl = new NBrightBuyController();

                // run SQL and template to return html
                if (settings.ContainsKey("sqltpl") && settings.ContainsKey("xsltpl"))
                {
                    var strSql = templCtrl.GetTemplateData(settings["sqltpl"], _lang);
                    var xslTemp = templCtrl.GetTemplateData(settings["xsltpl"], _lang);

                    // replace any settings tokens (This is used to place the form data into the SQL)
                    strSql = Utils.ReplaceSettingTokens(strSql, settings);
                    strSql = Utils.ReplaceUrlTokens(strSql);

                    strSql = GenXmlFunctions.StripSqlCommands(strSql); // don't allow anything to update through here.

                    strOut = objCtrl.GetSqlxml(strSql);
                    strOut = "<root>" + strOut + "</root>";
                    strOut = XslUtils.XslTransInMemory(strOut, xslTemp);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #endregion


        #region "Category Methods"

        private String GetCategoryProductList(HttpContext context)
        {
            try
            {
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DataProvider.Instance().DatabaseOwner;

                var settings = GetAjaxFields(context);
                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {Settings:itemid}) ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, settings);


                if (!settings.ContainsKey("filter")) settings.Add("filter", strFilter);
                return GetProductListData(settings);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        private string DeleteCatXref(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("parentitemid")) parentitemid = settings["parentitemid"];
                if (settings.ContainsKey("xrefitemid")) xrefitemid = settings["xrefitemid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    DeleteCatXref(xrefitemid, parentitemid);
                }
                else
                    return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        private void DeleteCatXref(String xrefitemid, String parentitemid)
        {
            if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
            {
                var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
            }
        }

        private string SelectCatXref(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("parentitemid")) parentitemid = settings["parentitemid"];
                if (settings.ContainsKey("xrefitemid")) xrefitemid = settings["xrefitemid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                }
                else
                    return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        private string DeleteAllCatXref(HttpContext context)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var settings = GetAjaxFields(context);

                if (settings.ContainsKey("itemid"))
                {
                    var strFilter = " and XrefItemId = {Settings:itemid} ";
                    strFilter = Utils.ReplaceSettingTokens(strFilter, settings);

                    var objCtrl = new NBrightBuyController();
                    var objList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", strFilter);

                    foreach (var obj in objList)
                    {
                        DeleteCatXref(settings["itemid"], obj.ParentItemId.ToString(""));
                    }
                    strOut = NBrightBuyUtils.GetResxMessage();
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return strOut;
        }

        private String CopyAllCatXref(HttpContext context,Boolean moverecords = false)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var settings = GetAjaxFields(context);
                var strFilter = " and XrefItemId = {Settings:itemid} ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, settings);

                var newcatid = "";
                if (settings.ContainsKey("selectedcatid")) newcatid = settings["selectedcatid"];

                if (Utils.IsNumeric(newcatid) && settings.ContainsKey("itemid"))
                {
                    var objCtrl = new NBrightBuyController();
                    var objList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", strFilter);

                    foreach (var obj in objList)
                    {
                        var strGuid = newcatid + "x" + obj.ParentItemId.ToString("");
                        var nbi = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATXREF", strGuid);
                        if (nbi == null)
                        {
                            if (!moverecords) obj.ItemID = -1;
                            obj.XrefItemId = Convert.ToInt32(newcatid);
                            obj.GUIDKey = strGuid;
                            obj.XMLData = null;
                            obj.TextData = null;
                            obj.Lang = null;
                            objCtrl.Update(obj);
                            //add all cascade xref 
                            var objGrpCtrl = new GrpCatController(_lang, true);
                            var parentcats = objGrpCtrl.GetCategory(Convert.ToInt32(newcatid));
                            foreach (var p in parentcats.Parents)
                            {
                                strGuid = p.ToString("") + "x" + obj.ParentItemId.ToString("");
                                nbi = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATCASCADE", strGuid);
                                if (nbi == null)
                                {
                                    obj.XrefItemId = p;
                                    obj.TypeCode = "CATCASCADE";
                                    obj.GUIDKey = strGuid;
                                    objCtrl.Update(obj);
                                }
                            }
                        }
                    }

                    if (moverecords) DeleteAllCatXref(context);

                    strOut = NBrightBuyUtils.GetResxMessage();
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return strOut;
        }

        private String GetGroupCategoryListBox(HttpContext context)
        {
            var settings = GetAjaxFields(context);
            var groupref = "";
            if (settings.ContainsKey("selectedgroupref")) groupref = settings["selectedgroupref"];
            var templ = "[<tag id='selectgroupcategory' cssclass='selectgroupcategory form-control' type='catlistbox' groupref='" + groupref + "' lang='" + _lang + "'/>]";
            return GenXmlFunctions.RenderRepeater(new NBrightInfo(),templ);    
        }

        #endregion

        #region "Product Methods"

        private String GetProductList(HttpContext context)
        {
            try
            {

                var settings = GetAjaxFields(context);

                return GetProductListData(settings);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        private String GetProductDescription(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);

                return  HttpUtility.HtmlDecode(prodData.Info.GetXmlProperty("genxml/lang/genxml/edt/description"));

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductGeneralData(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];
                
                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadmingeneral.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Info, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }
        
        private String GetProductModels(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadminmodels.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Models, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductOptions(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadminoptions.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Options, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductOptionValues(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadminoptionvalues.html", _lang);

                //get data
                var strOut = "";
                if (Utils.IsNumeric(productitemid))
                {
                    var prodData = ProductUtils.GetProductData(productitemid, _lang);
                    strOut = GenXmlFunctions.RenderRepeater(prodData.OptionValues, bodyTempl);  
                }

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductImages(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadminimages.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Imgs, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductDocs(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadmindocs.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Docs, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private Boolean ProductIsInCategory(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];
                if (!settings.ContainsKey("categoryref")) settings.Add("categoryref", "0");
                var categoryref = settings["categoryref"];

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);

                var l = prodData.GetCategories("",true);

                var cat = from i in l where i.categoryref == categoryref select i;

                if (cat.Any()) return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private String GetProductCategories(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadmincategories.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.GetCategories("cat"), bodyTempl);                
                
                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductGroupCategories(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadmingroupcategories.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.GetCategories("!cat"), bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String GetProductRelated(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];

                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadminrelated.html", _lang);

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.GetRelatedProducts(), bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String AddProductModels(HttpContext context)
        {

            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                var productitemid = settings["itemid"];
                var qty = settings["addqty"];
                if (!Utils.IsNumeric(qty)) qty = "1";

                var strOut = "No Product ID ('itemid' hidden fields needed on input form)";
                if (Utils.IsNumeric(productitemid))
                {
                    var itemId = Convert.ToInt32(productitemid);
                    var prodData = ProductUtils.GetProductData(itemId, _lang);
                    var lp = 1;
                    while (lp <= Convert.ToInt32(qty))
                    {
                        prodData.AddNewModel();
                        lp += 1;
                        if (lp > 50) break;  // we don;t want to create a stupid amount, it will slow the system!!!
                    }
                    prodData.Save();

                    var rtnList = new List<NBrightInfo>();
                    for (var i = 1; i < lp;i++ )
                    {
                        rtnList.Add(prodData.Models.Last());                        
                    }


                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("productadminmodels.html", _lang);

                    //get data
                    strOut = GenXmlFunctions.RenderRepeater(rtnList, bodyTempl);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private String AddProductOptions(HttpContext context)
        {

            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                var productitemid = settings["itemid"];
                var qty = settings["addqty"];
                if (!Utils.IsNumeric(qty)) qty = "1";

                var strOut = "No Product ID ('itemid' hidden fields needed on input form)";
                if (Utils.IsNumeric(productitemid))
                {
                    var itemId = Convert.ToInt32(productitemid);
                    var prodData = ProductUtils.GetProductData(itemId, _lang);
                    var lp = 1;
                    while (lp <= Convert.ToInt32(qty))
                    {
                        prodData.AddNewOption();
                        lp += 1;
                        if (lp > 50) break;  // we don;t want to create a stupid amount, it will slow the system!!!
                    }
                    prodData.Save();

                    var rtnList = new List<NBrightInfo>();
                    for (var i = 1; i < lp; i++)
                    {
                        rtnList.Add(prodData.Options.Last());
                    }


                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("productadminoptions.html", _lang);

                    //get data
                    strOut = GenXmlFunctions.RenderRepeater(rtnList, bodyTempl);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private String AddProductOptionValues(HttpContext context)
        {

            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                if (!settings.ContainsKey("selectedoptionid")) return "";

                var optionid = settings["selectedoptionid"];
                var productitemid = settings["itemid"];
                var qty = settings["addqty"];
                if (!Utils.IsNumeric(qty)) qty = "1";

                var strOut = "No Product ID ('itemid' hidden fields needed on input form)";
                if (Utils.IsNumeric(productitemid))
                {
                    var itemId = Convert.ToInt32(productitemid);
                    var prodData = ProductUtils.GetProductData(itemId, _lang);
                    var lp = 1;
                    while (lp <= Convert.ToInt32(qty))
                    {
                        prodData.AddNewOptionValue(optionid);
                        lp += 1;
                        if (lp > 50) break;  // we don;t want to create a stupid amount, it will slow the system!!!
                    }
                    prodData.Save();

                    var rtnList = new List<NBrightInfo>();
                    for (var i = 1; i < lp; i++)
                    {
                        rtnList.Add(prodData.OptionValues.Where(ov => ov.GetXmlProperty("genxml/hidden/optionid") == optionid).Select(o => o).Last());
                    }


                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("productadminoptionvalues.html", _lang);

                    //get data
                    strOut = GenXmlFunctions.RenderRepeater(rtnList, bodyTempl);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string AddProductCategory(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("itemid")) parentitemid = settings["itemid"];
                if (settings.ContainsKey("selectedcatid")) xrefitemid = settings["selectedcatid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string AddProductGroupCategory(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("itemid")) parentitemid = settings["itemid"];
                if (settings.ContainsKey("selectedcatid")) xrefitemid = settings["selectedcatid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    return GetProductGroupCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string RemoveProductCategory(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("itemid")) parentitemid = settings["itemid"];
                if (settings.ContainsKey("selectedcatid")) xrefitemid = settings["selectedcatid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string RemoveProductGroupCategory(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var parentitemid = "";
                var xrefitemid = "";
                if (settings.ContainsKey("itemid")) parentitemid = settings["itemid"];
                if (settings.ContainsKey("selectedcatid")) xrefitemid = settings["selectedcatid"];
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = new ProductData(Convert.ToInt32(parentitemid), _lang, false);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    return GetProductGroupCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string RemoveRelatedProduct(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var productid = "";
                var selectedrelatedid = "";
                if (settings.ContainsKey("itemid")) productid = settings["itemid"];
                if (settings.ContainsKey("selectedrelatedid")) selectedrelatedid = settings["selectedrelatedid"];
                if (Utils.IsNumeric(productid) && Utils.IsNumeric(selectedrelatedid))
                {
                    var prodData = new ProductData(Convert.ToInt32(productid), _lang, false);
                    prodData.RemoveRelatedProduct(Convert.ToInt32(selectedrelatedid));
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string AddRelatedProduct(HttpContext context)
        {
            try
            {
                var settings = GetAjaxFields(context);
                var productid = "";
                var selectedrelatedid = "";
                if (settings.ContainsKey("itemid")) productid = settings["itemid"];
                if (settings.ContainsKey("selectedrelatedid")) selectedrelatedid = settings["selectedrelatedid"];
                if (Utils.IsNumeric(selectedrelatedid) && Utils.IsNumeric(productid))
                {
                    var prodData = new ProductData(Convert.ToInt32(productid), _lang, false);
                    prodData.AddRelatedProduct(Convert.ToInt32(selectedrelatedid));
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        #endregion


        #region "functions"

        private String GetProductListData(Dictionary<String, String> settings,bool paging = true)
        {
            var strOut = "";

            if (!settings.ContainsKey("header")) settings.Add("header", "");
            if (!settings.ContainsKey("body")) settings.Add("body", "");
            if (!settings.ContainsKey("footer")) settings.Add("footer", "");
            if (!settings.ContainsKey("filter")) settings.Add("filter", "");
            if (!settings.ContainsKey("orderby")) settings.Add("orderby", "");
            if (!settings.ContainsKey("returnlimit")) settings.Add("returnlimit", "0");
            if (!settings.ContainsKey("pagenumber")) settings.Add("pagenumber", "0");
            if (!settings.ContainsKey("pagesize")) settings.Add("pagesize", "0");
            if (!settings.ContainsKey("searchtext")) settings.Add("searchtext", "");
            if (!settings.ContainsKey("searchcategory")) settings.Add("searchcategory", "");

            var header = settings["header"];
            var body = settings["body"];
            var footer = settings["footer"];
            var filter = settings["filter"];
            var orderby = settings["orderby"];
            var returnLimit = Convert.ToInt32(settings["returnlimit"]);    
            var pageNumber = Convert.ToInt32(settings["pagenumber"]);            
            var pageSize = Convert.ToInt32(settings["pagesize"]);

            var searchText = settings["searchtext"];
            var searchCategory = settings["searchcategory"];

            if (searchText != "") filter += " and (NB3.[ProductName] like '%" + searchText + "%' or NB3.[ProductRef] like '%" + searchText + "%' or NB3.[Summary] like '%" + searchText + "%' ) ";

            if (Utils.IsNumeric(searchCategory))
            {
                var objQual = DataProvider.Instance().ObjectQualifier;
                var dbOwner = DataProvider.Instance().DatabaseOwner;
                filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") ";
            }

            var recordCount = 0;

            var themeFolder = StoreSettings.Current.ThemeFolder;
            if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

            if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

            var objCtrl = new NBrightBuyController();

            var headerTempl = templCtrl.GetTemplateData(header, _lang);
            var bodyTempl = templCtrl.GetTemplateData(body, _lang);
            var footerTempl = templCtrl.GetTemplateData(footer, _lang);

            // replace any settings tokens (This is used to place the form data into the SQL)
            headerTempl = Utils.ReplaceSettingTokens(headerTempl, settings);
            headerTempl = Utils.ReplaceUrlTokens(headerTempl);
            bodyTempl = Utils.ReplaceSettingTokens(bodyTempl, settings);
            bodyTempl = Utils.ReplaceUrlTokens(bodyTempl);
            footerTempl = Utils.ReplaceSettingTokens(footerTempl, settings);
            footerTempl = Utils.ReplaceUrlTokens(footerTempl);

            var obj = new NBrightInfo(true);
            strOut = GenXmlFunctions.RenderRepeater(obj, headerTempl);

            if (paging) // get record count for paging
            {
                if (pageNumber == 0) pageNumber = 1;
                if (pageSize == 0) pageSize = StoreSettings.Current.GetInt("pagesize");
                recordCount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, "PRD", filter,"PRDLANG",_lang);
            }

            var objList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "PRD", "PRDLANG", _lang, filter, orderby, StoreSettings.Current.DebugMode,"",returnLimit,pageNumber,pageSize,recordCount);
            strOut += GenXmlFunctions.RenderRepeater(objList, bodyTempl);

            strOut += GenXmlFunctions.RenderRepeater(obj, footerTempl);

            // add paging if needed
            if (paging)
            {
                var pg = new NBrightCore.controls.PagingCtrl();
                strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
            }

            return strOut;
        }

        private Dictionary<String, String> GetAjaxFields(HttpContext context)
        {
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
            var objInfo = new NBrightInfo();

            objInfo.ItemID = -1;
            objInfo.TypeCode = "AJAXDATA";
            objInfo.XMLData = xmlData;
            var dic =  objInfo.ToDictionary();
            // set langauge if we have it passed.
            if (dic.ContainsKey("lang") && dic["lang"] != "") _lang = dic["lang"];
            return dic;
        }

        private Boolean CheckRights()
        {
            if (UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole))
            {
                return true;
            }
            return false;
        }


        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
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

            if (lang == "") lang = language; //if we don't have a lang param use the langauge one.

            if (!string.IsNullOrEmpty(lang) && System.Threading.Thread.CurrentThread.CurrentCulture.ToString() != lang)
            {
                // because we are using a webservice the system current thread culture might not be set correctly,
                //  so use the lang/lanaguge param to set it.
                // TODO: see if we can find a nicer way to do this.
                try
                {
                    var c = new System.Globalization.CultureInfo(lang);
                    System.Threading.Thread.CurrentThread.CurrentCulture = c;
                }
                catch
                {
                    // invalid lang culture, so ignore and use the system default.
                    lang = UserController.GetCurrentUserInfo().Profile.PreferredLocale;
                }
            }
            // default to current thread if we have no language.
            if (lang == "") lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            _lang = lang;

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
                case "setcategoryadminform":
                    if (CheckRights()) strOut = SetCategoryForm(context);
                    break;
                case "getdata":
                    strOut = GetReturnData(context);
                    break;
                case "additemlist":
                    if (Utils.IsNumeric(itemId))
                    {
                        var cw = new ItemListData(0, itemListName);
                        cw.Add(itemId);
                        strOut = cw.ItemList;
                    }
                    break;
                case "removeitemlist":
                    if (Utils.IsNumeric(itemId))
                    {
                        var cw1 = new ItemListData(0, itemListName);
                        cw1.Remove(itemId);
                        strOut = cw1.ItemList;
                    }
                    break;
                case "deleteitemlist":
                        var cw2 = new ItemListData(0, itemListName);
                        cw2.Delete();
                        strOut = "deleted";
                    break;
                case "getproductselectlist":
                    strOut = GetProductSelectList(context);
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
                    var strSql = templCtrl.GetTemplateData(settings["sqltpl"], Utils.GetCurrentCulture());
                    var xslTemp = templCtrl.GetTemplateData(settings["xsltpl"], Utils.GetCurrentCulture());

                    // replace any settings tokens (This is used to place the form data into the SQL)
                    strSql = Utils.ReplaceSettingTokens(strSql, settings);
                    strSql = Utils.ReplaceUrlTokens(strSql);

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

        private string SetCategoryForm(HttpContext context)
        {
            try
            {
                // get posted form data into a NBrigthInfo class so we can take the listbox value easily
                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();
                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary(); // put the fieds into a dictionary, so we can get them easy.

                var strOut = "No Category ID or Langauge ('itemid' and 'lang' hidden fields needed on input form)";
                if (settings.ContainsKey("itemid") && settings.ContainsKey("lang"))
                {

                    var strItemId = settings["itemid"];
                    if (Utils.IsNumeric(strItemId))
                    {
                        var itemId = Convert.ToInt32(strItemId);

                        var catData = new CategoryData(itemId, settings["lang"]);
                        catData.Update(objInfo);
                        catData.Save();
                        strOut = NBrightBuyUtils.GetResxMessage();
                    }
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion

        #region "Product Methods"

        private String GetProductSelectList(HttpContext context)
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

        private String GetCategoryProductList(HttpContext context)
        {
            try
            {
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DataProvider.Instance().DatabaseOwner;

                var settings = GetAjaxFields(context);
                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + settings["itemid"] + ") ";
                if (!settings.ContainsKey("filter")) settings.Add("filter",strFilter);
                return GetProductListData(settings);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }


        #endregion


        #region "functions"

        private String GetProductListData(Dictionary<String, String> settings)
        {
            var strOut = "";

            if (!settings.ContainsKey("header")) settings.Add("header", "");
            if (!settings.ContainsKey("body")) settings.Add("body", "");
            if (!settings.ContainsKey("footer")) settings.Add("footer", "");
            if (!settings.ContainsKey("filter")) settings.Add("filter", "");
            if (!settings.ContainsKey("orderby")) settings.Add("orderby", "");

            var header = settings["header"];
            var body = settings["body"];
            var footer = settings["footer"];
            var filter = settings["filter"];
            var orderby = settings["orderby"];


            

            var themeFolder = StoreSettings.Current.ThemeFolder;
            if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

            if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

            var objCtrl = new NBrightBuyController();

            var headerTempl = templCtrl.GetTemplateData(header, Utils.GetCurrentCulture());
            var bodyTempl = templCtrl.GetTemplateData(body, Utils.GetCurrentCulture());
            var footerTempl = templCtrl.GetTemplateData(footer, Utils.GetCurrentCulture());

            // replace any settings tokens (This is used to place the form data into the SQL)
            headerTempl = Utils.ReplaceSettingTokens(headerTempl, settings);
            headerTempl = Utils.ReplaceUrlTokens(headerTempl);
            bodyTempl = Utils.ReplaceSettingTokens(bodyTempl, settings);
            bodyTempl = Utils.ReplaceUrlTokens(bodyTempl);
            footerTempl = Utils.ReplaceSettingTokens(footerTempl, settings);
            footerTempl = Utils.ReplaceUrlTokens(footerTempl);

            var obj = new NBrightInfo(true);
            strOut = GenXmlFunctions.RenderRepeater(obj, headerTempl);

            var objList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "PRD", "PRDLANG", _lang, filter, orderby, StoreSettings.Current.DebugMode);
            strOut += GenXmlFunctions.RenderRepeater(objList, bodyTempl);

            strOut += GenXmlFunctions.RenderRepeater(obj, footerTempl);

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
            return objInfo.ToDictionary();
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
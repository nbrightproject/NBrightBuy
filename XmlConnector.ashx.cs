using System;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Microsoft.ApplicationBlocks.Data;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

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

            switch (paramCmd)
            {
                case "test":
                    strOut = "<root>" + UserController.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "setdata":
                    break;
                case "deldata":
                    break;
                case "getcategoryadminform":
                    strOut = GetCategoryForm(context);
                    break;
                case "setcategoryadminform":
                    strOut = SetCategoryForm(context);
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

        private string GetCategoryForm(HttpContext context)
        {
            try
            {
                // get posted form adta into a NBrigthInfo class so we can take the listbox value easily
                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();
                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary(); // put the fieds into a dictionary, so we can egt them easy.

                var strOut = "No Category ID";
                if (settings.ContainsKey("categorylistbox"))
                {

                    var strCatid = settings["categorylistbox"];
                    if (Utils.IsNumeric(strCatid))
                    {
                        var categoryId = Convert.ToInt32(strCatid);
                        var objTempl = NBrightBuyUtils.GetTemplateGetter("Cygnus");

                        // get template for non-localized fields
                        var strTempl = objTempl.GetTemplateData("categoryajaxdata.html", _lang);
                        var categoryData = new CategoryData(categoryId,_lang);
                        strOut = GenXmlFunctions.RenderRepeater(categoryData.Info, strTempl);

                        // get template for localized fields
                        strTempl = objTempl.GetTemplateData("categoryajaxdatalang.html", _lang);
                        categoryData = new CategoryData(categoryId, _lang);
                        strOut += GenXmlFunctions.RenderRepeater(categoryData.DataLangRecord, strTempl);
                    }
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string SetCategoryForm(HttpContext context)
        {
            try
            {
                // get posted form adta into a NBrigthInfo class so we can take the listbox value easily
                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();
                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary(); // put the fieds into a dictionary, so we can egt them easy.

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

    }
}
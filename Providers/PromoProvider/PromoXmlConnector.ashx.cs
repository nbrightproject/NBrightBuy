using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using DataProvider = DotNetNuke.Data.DataProvider;
using DotNetNuke.Entities.Tabs;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class PromoXmlConnector : IHttpHandler
    {
        private String _lang = "";
        private String _itemid = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOut = "";

            var moduleid = Utils.RequestQueryStringParam(context, "mid");
            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");
            _itemid = Utils.RequestQueryStringParam(context, "itemid");
            
            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            //  so use the lang/lanaguge param to set it.
            if (lang == "") lang = language;
            if (!string.IsNullOrEmpty(lang)) _lang = lang;

            // default to current thread if we have no language.
            if (_lang == "") _lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(_lang);

            #endregion

            #endregion

            #region "Do processing of command"

            strOut = "ERROR!! - No Security rights for current user!";
            if (CheckRights())
            {
                switch (paramCmd)
                {
                    case "test":
                        strOut = "<root>" + UserController.GetCurrentUserInfo().Username + "</root>";
                        break;
                    case "getdata":
                        strOut = GetData(context);
                        break;
                    case "getselectlangdata":
                        strOut = GetData(context);
                        break;
                    case "getlist":
                        strOut = GetData(context);
                        break;
                    case "addnew":
                        strOut = GetData(context, true);
                        break;
                    case "deleterecord":
                        strOut = DeleteData(context);
                        break;
                    case "savedata":
                        strOut = SaveData(context);
                        break;
                    case "selectlang":
                        strOut = SaveData(context);
                        break;
                }
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


        #region "Methods"

        private String GetData(HttpContext context,bool clearCache = false)
        {
            try
            {
                var objCtrl = new NBrightBuyController();
                var strOut = "";
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var newitem = ajaxInfo.GetXmlProperty("genxml/hidden/newitem");                
                var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
                if (editlang == "") editlang = _lang;

                if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // use moduleid -2 for razor

                if (clearCache) NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));


                if (newitem == "new")
                {
                    selecteditemid = "new"; // return list on new record
                    AddNew(moduleid);
                }

                var templateControlPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightBuy/Providers/PromoProvider");

                if (Utils.IsNumeric(selecteditemid))
                {
                    // do edit field data if a itemid has been selected
                    var obj = objCtrl.Get(Convert.ToInt32(selecteditemid), editlang);
                    strOut = NBrightBuyUtils.RazorTemplRender("editfields.cshtml", Convert.ToInt32(moduleid), _lang + itemid + editlang + selecteditemid, obj, templateControlPath, _lang);
                }
                else
                {
                    // Return list of items
                    var l = objCtrl.GetList(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), "PROMO", "", "", 0, 0, 0, 0, editlang);
                    strOut = NBrightBuyUtils.RazorTemplRender("editlist.cshtml", Convert.ToInt32(moduleid), _lang + editlang, l, templateControlPath, _lang);
                }

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String AddNew(String moduleid)
        {
            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

            var objCtrl = new NBrightBuyController();
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = "PROMO";
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.ItemID = -1;
            nbi.GUIDKey = "";
            var itemId = objCtrl.Update(nbi);
            nbi.ItemID = itemId;

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var nbi2 = new NBrightInfo(true);
                nbi2.PortalId = PortalSettings.Current.PortalId;
                nbi2.TypeCode = "PROMOLANG";
                nbi2.ModuleId = Convert.ToInt32(moduleid);
                nbi2.ItemID = -1;
                nbi2.Lang = lang;
                nbi2.ParentItemId = itemId;
                nbi2.GUIDKey = "";
                nbi2.ItemID = objCtrl.Update(nbi2);
            }

            NBrightBuyUtils.RemoveModCache(nbi.ModuleId);

            return nbi.ItemID.ToString("");
        }

        private String SaveData(HttpContext context)
        {
            try
            {
                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                if (lang == "") lang = _lang;
                if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

                if (Utils.IsNumeric(itemid))
                {
                    // get DB record
                    var nbi = objCtrl.Get(Convert.ToInt32(itemid));
                    if (nbi != null)
                    {
                        // get data passed back by ajax
                        var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                        // update record with ajax data
                        nbi.UpdateAjax(strIn);
                        objCtrl.Update(nbi);

                        // do langauge record
                        nbi = objCtrl.GetDataLang(Convert.ToInt32(itemid), lang);
                        nbi.UpdateAjax(strIn);
                        objCtrl.Update(nbi);

                        NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));
                        
                    }
                }
                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String DeleteData(HttpContext context)
        {
            try
            {
                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
                if (editlang == "") editlang = _lang;
                if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

                if (Utils.IsNumeric(itemid))
                {
                    // delete DB record
                    objCtrl.Delete(Convert.ToInt32(itemid));

                    NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));

                }
                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        #endregion

        private void SetContextLangauge(NBrightInfo ajaxInfo)
        {
            // set langauge if we have it passed.
            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            if (lang != "") _lang = lang;
            // set the context  culturecode, so any DNN functions use the correct culture 
            if (_lang != "" && _lang != System.Threading.Thread.CurrentThread.CurrentCulture.ToString()) System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(_lang);

        }

        private Boolean CheckRights()
        {
            if (UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }



    }
}
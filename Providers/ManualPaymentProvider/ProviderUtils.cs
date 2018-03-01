using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace ManualPaymentProvider
{
    public class ProviderUtils
    {


        public static String GetTemplateData(String templatename, NBrightInfo info)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightBuy/Providers/ManualPaymentProvider");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            return templ;
        }


        public static NBrightInfo GetData(string lang)
        {
            var objCtrl = new NBrightBuyController();

            var info = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "MANUALPAYMENT", "manualpayment");

            //--------------------------------------------------------------------------------
            // check for parent, if not there delete record, so it's recreated with LANG record. (backward compatiblity)
            //--------------------------------------------------------------------------------
            if (info != null)
            {
                var lg = objCtrl.GetDataLang(info.ItemID, lang);
                if (lg == null)
                {
                    objCtrl.Delete(info.ItemID);
                    info = null;
                }
            }
            //--------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------

            if (info == null)
            {
                info = new NBrightInfo(true);
                info.GUIDKey = "manualpayment";
                info.TypeCode = "MANUALPAYMENT";
                info.ModuleId = -1;
                info.PortalId = PortalSettings.Current.PortalId;
                var pid = objCtrl.Update(info);
                info = new NBrightInfo(true);
                info.GUIDKey = "";
                info.TypeCode = "MANUALPAYMENTLANG";
                info.ParentItemId = pid;
                info.Lang = lang;
                info.ItemID = objCtrl.Update(info);
            }

            // do edit field data if a itemid has been selected
            var nbi = objCtrl.Get(info.ItemID, "MANUALPAYMENTLANG", lang);
            return nbi;
        }

        public static string SaveData(HttpContext context)
        {
            try
            {

                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                if (Utils.IsNumeric(itemid))
                {
                    var nbi = objCtrl.Get(Convert.ToInt32(itemid));
                    // get data passed back by ajax
                    var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                    // update record with ajax data
                    nbi.UpdateAjax(strIn);
                    objCtrl.Update(nbi);

                    // do langauge record
                    var nbi2 = objCtrl.GetDataLang(Convert.ToInt32(itemid), lang);
                    nbi2.UpdateAjax(strIn);
                    objCtrl.Update(nbi2);

                    DataCache.ClearCache(); // clear ALL cache.
                }
                return "";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }




    }
}

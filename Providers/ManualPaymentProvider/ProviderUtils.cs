using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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
            var dic = new Dictionary<String, String>();
            foreach (var d in StoreSettings.Current.Settings())
            {
                dic.Add(d.Key,d.Value);
            }
            foreach (var d in info.ToDictionary())
            {
                if (dic.ContainsKey(d.Key))
                    dic[d.Key] = d.Value;
                else
                    dic.Add(d.Key, d.Value);
            }
            templ = Utils.ReplaceSettingTokens(templ, dic);
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

        public static NBrightInfo GetProviderSettings(String ctrlkey)
        {
            var info = (NBrightInfo)Utils.GetCache("ManualPaymentProvider" + PortalSettings.Current.PortalId.ToString(""));
            if (info == null)
            {
                var modCtrl = new NBrightBuyController();

                info = modCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "MANUALPAYMENT", ctrlkey);

                if (info == null)
                {
                    info = new NBrightInfo(true);
                    info.GUIDKey = ctrlkey;
                    info.TypeCode = "MANUALPAYMENT";
                    info.ModuleId = -1;
                    info.PortalId = PortalSettings.Current.PortalId;
                }

                Utils.SetCache("ManualPaymentProvider" + PortalSettings.Current.PortalId.ToString(""), info);                
            }

            return info;
        }


    }
}

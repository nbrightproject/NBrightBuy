using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Address
{
    public static class AddressAdminFunctions
    {
        #region "AddressAdmin Admin Methods"

        public static string ProcessCommand(string paramCmd,HttpContext context)
        {
            var strOut = "AddressAdmin - ERROR!! - No Security rights for current user!";
            if (NBrightBuyUtils.CheckManagerRights())
            {
                NBrightBuyUtils.SetContextLangauge(context);
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
                var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");

                switch (paramCmd)
                {
                    case "addressadmin_getlist":
                        strOut = GetAddressList(context);
                        break;
                    case "addressadmin_updateaddress":
                        break;
                    case "addressadmin_saveaddress":
                        break;
                    case "addressadmin_deleteaddress":
                        break;
                    case "addressadmin_editaddress":
                        strOut = GetAddress(context);
                        break;
                    case "addressadmin_newaddress":
                        break;
                    case "addressadmin_cancel":
                        break;

                }
            }
            return strOut;
        }




        #endregion

        private static String GetAddressList(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var addressData = new AddressData();
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                    var passSettings = ajaxInfo.ToDictionary();
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    var l = addressData.GetAddressList();
                    var strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", l, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        private static String GetAddress(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var addressData = new AddressData();
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                    var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                    var selectedindex = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedindex");
                    
                    var passSettings = ajaxInfo.ToDictionary();
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    var obj = addressData.GetAddress(selectedindex);


                    var strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", obj, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


    }
}

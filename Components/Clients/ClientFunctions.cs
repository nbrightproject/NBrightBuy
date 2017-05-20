using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;

namespace Nevoweb.DNN.NBrightBuy.Components.Clients
{
    public static class ClientFunctions
    {
        #region "Client Admin Methods"

        public static String ClientAdminList(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);

                    var paging = true;

                    if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return "";

                    var strOut = "";

                    if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                    if (!settings.ContainsKey("userid")) settings.Add("userid", "-1");
                    if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                    if (!settings.ContainsKey("returnlimit")) settings.Add("returnlimit", "0");
                    if (!settings.ContainsKey("pagenumber")) settings.Add("pagenumber", "0");
                    if (!settings.ContainsKey("pagesize")) settings.Add("pagesize", "0");
                    if (!settings.ContainsKey("searchtext")) settings.Add("searchtext", "");
                    if (!settings.ContainsKey("dtesearchdatefrom")) settings.Add("dtesearchdatefrom", "");
                    if (!settings.ContainsKey("dtesearchdateto")) settings.Add("dtesearchdateto", "");
                    if (!settings.ContainsKey("searchorderstatus")) settings.Add("searchorderstatus", "");
                    if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                    if (!Utils.IsNumeric(settings["userid"])) settings["pagenumber"] = "1";
                    if (!Utils.IsNumeric(settings["pagenumber"])) settings["pagenumber"] = "1";
                    if (!Utils.IsNumeric(settings["pagesize"])) settings["pagesize"] = "20";
                    if (!Utils.IsNumeric(settings["returnlimit"])) settings["returnlimit"] = "50";

                    var themeFolder = settings["themefolder"];
                    var razortemplate = settings["razortemplate"];
                    var returnLimit = Convert.ToInt32(settings["returnlimit"]);
                    var pageNumber = Convert.ToInt32(settings["pagenumber"]);
                    var pageSize = Convert.ToInt32(settings["pagesize"]);
                    var portalId = Convert.ToInt32(settings["portalid"]);
                    var userid = settings["userid"];

                    var searchText = settings["searchtext"];

                    var recordCount = 0;

                    if (themeFolder == "")
                    {
                        themeFolder = StoreSettings.Current.ThemeFolder;
                        if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    }

                    var objCtrl = new NBrightBuyController();

                    if (paging) // get record count for paging
                    {
                        if (pageNumber == 0) pageNumber = 1;
                        if (pageSize == 0) pageSize = 20;

                        // get only entity type required
                        recordCount = objCtrl.GetDnnUsersCount(portalId, "%" + searchText + "%");

                    }

                    var list = objCtrl.GetDnnUsers(portalId, "%" + searchText + "%", 0, pageNumber, pageSize, recordCount);

                    var passSettings = settings;
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", list, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);

                    // add paging if needed
                    if (paging && (recordCount > pageSize))
                    {
                        var pg = new NBrightCore.controls.PagingCtrl();
                        strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
                    }

                    return strOut;




                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String ClientAdminDetail(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    var strOut = "";

                    if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                    if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                    if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                    if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");

                    var themeFolder = settings["themefolder"];
                    var selecteditemid = settings["selecteditemid"];
                    var razortemplate = settings["razortemplate"];
                    var portalId = Convert.ToInt32(settings["portalid"]);

                    var passSettings = settings;
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    if (!Utils.IsNumeric(selecteditemid)) return "";

                    if (themeFolder == "")
                    {
                        themeFolder = StoreSettings.Current.ThemeFolder;
                        if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    }

                    var clientData = new ClientData(portalId, Convert.ToInt32(selecteditemid));
                    strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", clientData, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ClientAdminSave(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
                    if (userId > 0)
                    {
                        var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                        if (clientData.Exists)
                        {
                            clientData.Update(ajaxInfo);
                            clientData.Save();
                            return "";
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }


        #endregion


    }
}

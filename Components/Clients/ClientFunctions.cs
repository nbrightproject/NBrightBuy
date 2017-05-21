using System;
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

namespace Nevoweb.DNN.NBrightBuy.Components.Clients
{
    public static class ClientFunctions
    {
        #region "Client Admin Methods"

        public static string ProcessCommand(string paramCmd,HttpContext context)
        {
            var strOut = "CLIENT - ERROR!! - No Security rights for current user!";
            if (NBrightBuyUtils.CheckManagerRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");

                switch (paramCmd)
                {
                    case "client.admin_getlist":
                        strOut = ClientFunctions.ClientAdminList(context);
                        break;
                    case "client.admin_getdetail":
                        strOut = ClientFunctions.ClientAdminDetail(context);
                        break;
                    case "client.admin_save":
                        strOut = ClientFunctions.ClientAdminSave(context);
                        break;
                    case "client.discountcodes":
                        strOut = ClientFunctions.GetClientDiscountCodes(context);
                        break;
                    case "client.adddiscountcode":
                        strOut = ClientFunctions.AddClientDiscountCodes(context);
                        break;
                    case "client.vouchercodes":
                        strOut = ClientFunctions.GetClientVoucherCodes(context);
                        break;
                    case "client.addvouchercode":
                        strOut = ClientFunctions.AddClientVoucherCodes(context);
                        break;
                    case "unlockuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.UnlockUser();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "deleteuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.DeleteUser();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "restoreuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.RestoreUser();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "removeuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.RemoveUser();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "validateuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.AuthoriseClient();
                                clientData.AddClientEditorRole();
                                if (StoreSettings.Current.Get("resetpasswordonclientvalidate") == "True") clientData.ResetPassword();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "unauthoriseuser":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.UnAuthoriseClient();
                                clientData.RemoveClientEditorRole();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "resetpass":
                        if (userId > 0)
                        {
                            var clientData = new ClientData(PortalSettings.Current.PortalId, userId);
                            if (clientData.Exists)
                            {
                                clientData.ResetPassword();
                                return ClientFunctions.ClientAdminDetail(context);
                            }
                        }
                        break;
                    case "viewaddressbook":
                        //param[0] = "";

                        //if (Utils.IsNumeric(cArg))

                        //{

                        //    param[0] = "ctrl=addressbook";

                        //    param[1] = "uid=" + cArg;

                        //}

                        //Response.Redirect(Globals.NavigateURL(TabId, "", param), true);

                        break;

                    case "vieworders":

                        //param[0] = "";

                        //if (Utils.IsNumeric(cArg))

                        //{

                        //    param[0] = "ctrl=orders";

                        //    param[1] = "uid=" + cArg;

                        //}

                        //Response.Redirect(Globals.NavigateURL(TabId, "", param), true);

                        break;

                    case "createorder":

                        //param[0] = "";

                        //var tabId = TabId;

                        //if (Utils.IsNumeric(cArg))

                        //{

                        //    var cart = new CartData(PortalId);

                        //    cart.UserId = Convert.ToInt32(cArg);

                        //    cart.EditMode = "C";

                        //    cart.Save();

                        //    tabId = StoreSettings.Current.GetInt("productlisttab");

                        //    if (tabId == 0) tabId = TabId;

                        //}

                        //Response.Redirect(NBrightBuyUtils.AdminUrl(tabId, param), true);

                        break;
                }
            }
            return strOut;
        }


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


        public static String GetClientDiscountCodes(HttpContext context)
        {
            try
            {
                //get uploaded params
                var strOut = "";
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                if (!settings.ContainsKey("userid")) settings.Add("userid", "");
                var userid = settings["userid"];
                if (!settings.ContainsKey("portalid")) settings.Add("portalid", "");
                var portalid = settings["portalid"];
                if (Utils.IsNumeric(portalid) && Utils.IsNumeric(userid))
                {
                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("clientdiscountcodes.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
                    bodyTempl = Utils.ReplaceSettingTokens(bodyTempl, StoreSettings.Current.Settings());
                    //get data
                    var clientData = new ClientData(Convert.ToInt32(portalid), Convert.ToInt32(userid));
                    strOut = GenXmlFunctions.RenderRepeater(clientData.DiscountCodes, bodyTempl);
                }

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String AddClientDiscountCodes(HttpContext context)
        {

            try
            {
                var strOut = "Missing data ('userid', 'portalid' hidden fields needed on input form)";

                //get uploaded params
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                if (!settings.ContainsKey("userid")) settings.Add("userid", "");
                var userid = settings["userid"];
                if (!settings.ContainsKey("portalid")) settings.Add("portalid", "");
                var portalid = settings["portalid"];
                if (Utils.IsNumeric(portalid) && Utils.IsNumeric(userid))
                {
                    var clientData = new ClientData(Convert.ToInt32(portalid), Convert.ToInt32(userid));

                    var qty = settings["addqty"];
                    if (!Utils.IsNumeric(qty)) qty = "1";

                    var lp = 1;
                    var modelcount = clientData.DiscountCodes.Count;
                    while (lp <= Convert.ToInt32(qty))
                    {
                        clientData.AddNewDiscountCode();
                        lp += 1;
                        if (lp > 10) break; // we don;t want to create a stupid amount, it will slow the system!!!
                    }
                    clientData.Save();
                    var modelcount2 = clientData.DiscountCodes.Count;
                    var rtnList = new List<NBrightInfo>();
                    for (var i = modelcount; i < modelcount2; i++)
                    {
                        rtnList.Add(clientData.DiscountCodes[i]);
                    }

                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("clientdiscountcodes.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());

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

        public static String GetClientVoucherCodes(HttpContext context)
        {
            try
            {
                //get uploaded params
                var strOut = "";
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                if (!settings.ContainsKey("userid")) settings.Add("userid", "");
                var userid = settings["userid"];
                if (!settings.ContainsKey("portalid")) settings.Add("portalid", "");
                var portalid = settings["portalid"];
                if (Utils.IsNumeric(portalid) && Utils.IsNumeric(userid))
                {
                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("clientvouchercodes.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
                    bodyTempl = Utils.ReplaceSettingTokens(bodyTempl, StoreSettings.Current.Settings());
                    //get data
                    var clientData = new ClientData(Convert.ToInt32(portalid), Convert.ToInt32(userid));
                    strOut = GenXmlFunctions.RenderRepeater(clientData.VoucherCodes, bodyTempl);
                }

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String AddClientVoucherCodes(HttpContext context)
        {

            try
            {
                var strOut = "Missing data ('userid', 'portalid' hidden fields needed on input form)";

                //get uploaded params
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                if (!settings.ContainsKey("userid")) settings.Add("userid", "");
                var userid = settings["userid"];
                if (!settings.ContainsKey("portalid")) settings.Add("portalid", "");
                var portalid = settings["portalid"];
                if (Utils.IsNumeric(portalid) && Utils.IsNumeric(userid))
                {
                    var clientData = new ClientData(Convert.ToInt32(portalid), Convert.ToInt32(userid));

                    var qty = settings["addqty"];
                    if (!Utils.IsNumeric(qty)) qty = "1";

                    var lp = 1;
                    var modelcount = clientData.VoucherCodes.Count;
                    while (lp <= Convert.ToInt32(qty))
                    {
                        clientData.AddNewVoucherCode();
                        lp += 1;
                        if (lp > 10) break; // we don;t want to create a stupid amount, it will slow the system!!!
                    }
                    clientData.Save();
                    var modelcount2 = clientData.VoucherCodes.Count;
                    var rtnList = new List<NBrightInfo>();
                    for (var i = modelcount; i < modelcount2; i++)
                    {
                        rtnList.Add(clientData.VoucherCodes[i]);
                    }

                    // get template
                    var themeFolder = StoreSettings.Current.ThemeFolder;
                    if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                    var bodyTempl = templCtrl.GetTemplateData("clientvouchercodes.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());

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



        #endregion


    }
}

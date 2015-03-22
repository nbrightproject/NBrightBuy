using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Resources;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NBrightCore.TemplateEngine;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using MailPriority = DotNetNuke.Services.Mail.MailPriority;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public static class NBrightBuyUtils
    {

        /// <summary>
        /// Used to get the template system getter control.
        /// </summary>
        /// <returns></returns>
        public static TemplateGetter GetTemplateGetter(int portalId, string themeFolder)
        {
            var controlMapPath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath());
            themeFolder = "Themes\\" + themeFolder;
            var map = "";
            var storeThemeFolder = "";
            if (PortalSettings.Current == null) // might be ran from scheduler
            {
                var portalsettings = NBrightDNN.DnnUtils.GetPortalSettings(portalId);
                map = portalsettings.HomeDirectoryMapPath;
                var storeset = new StoreSettings(portalId);
                storeThemeFolder = storeset.ThemeFolder;
            }
            else
            {
                map = PortalSettings.Current.HomeDirectoryMapPath;
                storeThemeFolder = StoreSettings.Current.ThemeFolder;
            }
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(map, controlMapPath, "Themes\\config", themeFolder, "Themes\\" + storeThemeFolder);
            return templCtrl;
        }

        public static TemplateGetter GetTemplateGetter(string themeFolder)
        {
            return GetTemplateGetter(PortalSettings.Current.PortalId, themeFolder);
        }

        public static NBrightInfo GetSettings(int portalId, int moduleId, String ctrlTypeCode = "", bool useCache = true)
        {
            var obj = (NBrightInfo) GetModCache("NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""));
            if (obj == null | !useCache)
            {
                // single record for EntityTypeCode settings, so get record directly.
                var objCtrl = new NBrightBuyController();
                obj = objCtrl.GetByType(portalId, moduleId, "SETTINGS");
                if (obj == null)
                {
                    obj = new NBrightInfo {ItemID = -1};
                    obj.TypeCode = "SETTINGS";
                    obj.ModuleId = moduleId;
                    obj.PortalId = portalId;
                    obj.XMLData = "<genxml><hidden></hidden><textbox></textbox></genxml>";
                    obj.UserId = -1;
                    obj.GUIDKey = ctrlTypeCode;
                }
                SetModCache(moduleId, "NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""), obj);
            }
            return obj;
        }


        public static string FormatListtoXml(IEnumerable<NBrightInfo> objList)
        {
            var xmlOut = "";
            foreach (var obj in objList)
            {
                if (obj.XMLData.StartsWith("<root>"))
                {
                    // Data is Xref, so read actual data required.
                    var xrefId = GenXmlFunctions.GetGenXmlValue(obj.XMLData, "root/xref");
                    if (Utils.IsNumeric(xrefId))
                    {
                        var objCtrl = new Components.NBrightBuyController();
                        var obj2 = objCtrl.Get(Convert.ToInt32(xrefId));
                        if (obj2 != null)
                        {
                            var linedxrefId = GenXmlFunctions.GetGenXmlValue(obj.XMLData, "root/linkedxref");
                            xmlOut += "<item id=\"" + obj.ItemID.ToString("") + "\" idxref=\"" + linedxrefId + "\" modifieddate=\"" + obj.ModifiedDate.ToString("s") + "\">" + obj2.XMLData + "</item>";
                        }
                    }
                }
                else
                {
                    xmlOut += "<item id=\"" + obj.ItemID.ToString("") + "\" modifieddate=\"" + obj.ModifiedDate.ToString("s") + "\">" + obj.XMLData + "</item>";
                }
            }
            return xmlOut;
        }

        public static string DoXslTransOnTemplate(string strTemplateText, NBrightInfo objInfo)
        {
            if (strTemplateText.ToLower().Contains("<xsl:stylesheet"))
            {
                var xmlOut = "<root>";
                var l = new List<NBrightInfo> {objInfo};
                xmlOut += NBrightBuyUtils.FormatListtoXml(l);
                xmlOut += "</root>";
                return XslUtils.XslTransInMemory(xmlOut, strTemplateText);
            }
            return strTemplateText;
        }

        public static string GetUniqueKeyRef(int PortalId, int ModuleId, string KeyRef, int LoopCount)
        {
            var rtnKeyRef = KeyRef;
            if (LoopCount > 0) rtnKeyRef += LoopCount.ToString("");

            var strFilter = " and ModuleId != " + ModuleId + " and GUIDKey = '" + rtnKeyRef + "' ";

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(PortalId, -1, "SETTINGS", strFilter, "", 1, 1, 1, 1, "", ""));
            if (l.Count >= 1)
            {
                rtnKeyRef = GetUniqueKeyRef(PortalId, ModuleId, KeyRef, LoopCount + 1);
            }

            return rtnKeyRef;
        }


        #region "Build Urls"

        public static string GetEntryUrl(int portalId, string entryid, string modulekey, string seoname, string tabid, string catid = "", string catref = "")
        {

            var rdTabid = -1;
            var objTabInfo = new TabInfo();

            if (Utils.IsNumeric(tabid) && Convert.ToInt32(tabid) > 0) rdTabid = Convert.ToInt32(tabid);

            if (rdTabid == -1 && PortalSettings.Current != null)
            {
                objTabInfo = PortalSettings.Current.ActiveTab;
                rdTabid = objTabInfo.TabID;
            }

            var cachekey = "entryurl*" + entryid + "*" + catid + "*" + catref + "*" + modulekey + "*" + rdTabid;
            var urldata = "";
            var chacheData = Utils.GetCache(cachekey);
            if (chacheData != null) return (String)chacheData;


            if (objTabInfo.TabID != rdTabid)
            {
                var portalTabs = NBrightDNN.DnnUtils.GetPortalTabs(portalId);
                if (portalTabs != null)
                {
                    var rTab = portalTabs[Convert.ToInt32(rdTabid)];
                    if (rTab != null) objTabInfo = rTab;
                }
            }

            var rdModId = "";
            if (modulekey != "") rdModId = "&modkey=" + modulekey;

            var strurl = "~/Default.aspx?tabid=" + rdTabid + rdModId;

            if (StoreSettings.Current.GetBool(StoreSettingKeys.friendlyurlids))
            {
                if (Utils.IsNumeric(catid))
                {
                    var catData = new CategoryData(catid, Utils.GetCurrentCulture());
                    if (!strurl.EndsWith("?")) strurl += "&";
                    strurl += "catref=" + catData.DataLangRecord.GUIDKey;
                }
                if (catref != "")
                {
                    if (!strurl.EndsWith("?")) strurl += "&";
                    strurl += "catref=" + catref;
                }
                if (Utils.IsNumeric(entryid))
                {
                    var prdData = new ProductData(Convert.ToInt32(entryid), Utils.GetCurrentCulture());
                    if (!strurl.EndsWith("?")) strurl += "&";
                    strurl += "ref=" + prdData.DataRecord.GUIDKey;
                    seoname = prdData.SEOName;
                    seoname = Utils.UrlFriendly(seoname);
                }
            }
            else
            {
                if (Utils.IsNumeric(catid))
                {
                    if (!strurl.EndsWith("?")) strurl += "&";
                    strurl += "catid=" + catid;
                }
                if (Utils.IsNumeric(entryid))
                {
                    if (!strurl.EndsWith("?")) strurl += "&";
                    strurl += "eid=" + entryid;
                }
                seoname = Utils.UrlFriendly(seoname);
            }
            
            urldata = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(objTabInfo, strurl, seoname);
            
            Utils.SetCache(cachekey, urldata);

            return urldata;
        }


        public static string GetListUrl(int portalId, int tabId, int catId, string seoName, string lang)
        {
            seoName = Utils.CleanInput(seoName);
            if (seoName == "") seoName = "page";
            if (catId == -1) return GetSEOLink(portalId, tabId, "", seoName + ".aspx", "language=" + lang);
            return GetSEOLink(portalId, tabId, "", seoName + ".aspx", "CatID=" + catId.ToString(""), "language=" + lang);
        }

        public static string GetSEOLink(int portalId, int tabId, string controlKey, string title, params string[] additionalParameters)
        {

            DotNetNuke.Entities.Tabs.TabInfo tabInfo = (new DotNetNuke.Entities.Tabs.TabController()).GetTab(tabId, portalId, false);
            var langList = DnnUtils.GetCultureCodeList(portalId);


            if ((tabInfo != null))
            {
                string Path = "~/default.aspx?tabid=" + tabInfo.TabID;

                foreach (string p in additionalParameters)
                {
                    if (langList.Count > 1)
                    {
                        Path += "&" + p;
                    }
                    else
                    {
                        //only one langauge so don't add the langauge param.
                        if (!p.ToLower().StartsWith("language"))
                        {
                            Path += "&" + p;
                        }
                    }
                }
                if (string.IsNullOrEmpty(title)) title = "Default.aspx";
                return DotNetNuke.Common.Globals.FriendlyUrl(tabInfo, Path, title);
            }
            return "";
        }


        public static string GetReturnUrl(string tabid)
        {

            var redirectTab = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab;
            var rdTabid = redirectTab.TabID;

            if (Utils.IsNumeric(tabid))
            {
                if (Convert.ToInt32(tabid) > 0)
                {
                    rdTabid = Convert.ToInt32(tabid);
                }
            }

            if (Utils.IsNumeric(rdTabid))
            {
                if (Convert.ToInt32(rdTabid) != redirectTab.TabID)
                {
                    var portalTabs = NBrightDNN.DnnUtils.GetPortalTabs(PortalSettings.Current.PortalId);
                    if (portalTabs != null)
                    {
                        var rTab = portalTabs[Convert.ToInt32(rdTabid)];
                        if (rTab != null)
                        {
                            redirectTab = rTab;
                        }
                    }
                }
            }

            //get last active modulekey from cookie.
            var moduleKey = NBrightCore.common.Cookie.GetCookieValue(PortalSettings.Current.PortalId, "NBrigthBuyLastActive", "ModuleKey");

            var pagename = redirectTab.TabName + ".aspx";
            var page = "";
            var pagemid = "";
            var catid = "";
            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, moduleKey);
            if (navigationdata.PageNumber != "") page = "&page=" + navigationdata.PageNumber;
            if (navigationdata.PageModuleId != "") pagemid = "&pagemid=" + navigationdata.PageModuleId;
            if (navigationdata.CategoryId != "") catid = "&catid=" + navigationdata.CategoryId;
            if (navigationdata.PageName != "") pagename = navigationdata.PageName + ".aspx";
            var url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(redirectTab, "~/Default.aspx?tabid=" + redirectTab.TabID.ToString("") + page + pagemid + catid, pagename);

            return url;

        }

        public static string GetCurrentPageName(int catid)
        {
            var newBaseName = PortalSettings.Current.ActiveTab.TabName;
            var objCtrl = new NBrightBuyController();
            var catInfo = objCtrl.GetData(catid, "CATEGORYLANG");
            if (catInfo != null)
            {
                newBaseName = catInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (newBaseName == "") newBaseName = catInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                if (newBaseName == "") newBaseName = PortalSettings.Current.ActiveTab.TabName;
            }
            return newBaseName;
        }

        #endregion

        #region "Cacheing"

        /// <summary>
        /// Get Module level cache, is same as normal GetCache.  Created to stop confusion.
        /// </summary>
        /// <param name="CacheKey"></param>
        public static object GetModCache(string CacheKey)
        {
            return NBrightCore.common.Utils.GetCache(CacheKey);
        }

        /// <summary>
        /// Save into normal cache, but keep a list on the moduleid, so we can remove it at module level
        ///  </summary>
        /// <param name="moduleid">Moduleid use to store in the cache list, (not added to the cachekey)</param>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        public static void SetModCache(int moduleid, string CacheKey, object objObject)
        {
            var cList = (List<String>) NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
            if (cList == null) cList = new List<String>();
            if (!cList.Contains(CacheKey))
            {
                cList.Add(CacheKey);
                NBrightCore.common.Utils.SetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture), cList);
                NBrightCore.common.Utils.SetCache(CacheKey, objObject);
            }
        }

        public static void RemoveCache(String cacheKey)
        {
            NBrightCore.common.Utils.RemoveCache(cacheKey);
        }

        public static void RemoveModCache(int moduleid)
        {
            var cList = (List<String>) NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
            if (cList != null)
            {
                foreach (var s in cList)
                {
                    NBrightCore.common.Utils.RemoveCache(s);
                }
            }
            NBrightCore.common.Utils.RemoveCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
        }

        public static void RemoveModCachePortalWide(int portalid)
        {
            var mCtrl = new NBrightBuyController();
            var l = mCtrl.GetList(portalid, -1, "SETTINGS");
            foreach (var obj in l)
            {
                RemoveModCache(obj.ModuleId);
            }
            RemoveModCache(-1);
        }

        #endregion

        /// <summary>
        /// Include and template data into header, if specified in meta tag token (includeinheader).  
        /// </summary>
        /// <param name="modCtrl"></param>
        /// <param name="moduleId"></param>
        /// <param name="page"></param>
        /// <param name="template"></param>
        /// <param name="settings"></param>
        /// <param name="objInfo"></param>
        /// <param name="debugMode"></param>
        /// <returns></returns>
        public static string IncludePageHeaders(NBrightBuyController modCtrl, int moduleId, Page page, GenXmlTemplate template, Dictionary<String, String> settings, NBrightInfo objInfo = null, bool debugMode = false)
        {
            foreach (var mt in template.MetaTags)
            {
                var id = GenXmlFunctions.GetGenXmlValue(mt, "tag/@id");
                if (id == "includeinheader")
                {
                    var templ = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value");
                    if (templ != "")
                    {

                        var includetext = modCtrl.GetTemplateData(moduleId, templ, Utils.GetCurrentCulture(), settings, debugMode);
                        if (objInfo == null) objInfo = new NBrightInfo(); //create a object so we process the tag values (resourcekey)
                        includetext = GenXmlFunctions.RenderRepeater(objInfo, includetext);
                        if (includetext != "")
                        {
                            if (!page.Items.Contains("nbrightbuyinject")) page.Items.Add("nbrightbuyinject", "");
                            if (templ != "" && !page.Items["nbrightbuyinject"].ToString().Contains(templ + ","))
                            {
                                PageIncludes.IncludeTextInHeader(page, includetext);
                                page.Items["nbrightbuyinject"] = page.Items["nbrightbuyinject"] + templ + ",";
                            }
                        }
                    }
                }
            }
            return "";
        }

        public static string IncludePageHeaderDefault(NBrightBuyController modCtrl, Page page, String templatename, String themeFolder, bool debugMode = false)
        {

            if (!page.Items.Contains("nbrightbuyinject")) page.Items.Add("nbrightbuyinject", "");
            if (templatename != "" && !page.Items["nbrightbuyinject"].ToString().Contains(templatename + ","))
            {
                var includetext = modCtrl.GetTemplate(templatename,Utils.GetCurrentCulture(),themeFolder, debugMode);
                var objInfo = new NBrightInfo(); //create a object so we process the tag values (resourcekey)
                includetext = GenXmlFunctions.RenderRepeater(objInfo, includetext,"","XMLData","",StoreSettings.Current.Settings());
                if (includetext != "")
                {
                    PageIncludes.IncludeTextInHeader(page, includetext);
                    page.Items["nbrightbuyinject"] = page.Items["nbrightbuyinject"] + templatename + ",";
                }
            }
            return "";
        }

        public static GenXmlTemplate GetGenXmlTemplate(String templateData, Dictionary<String, String> settingsDic, String portalHomeDirectory)
        {
            return GetGenXmlTemplate(templateData, settingsDic, portalHomeDirectory,null);
        }

        /// <summary>
        /// Get the GenXmltemplate class and assign required resx files.
        /// </summary>
        /// <param name="templateData"></param>
        /// <param name="settingsDic"></param>
        /// <param name="portalHomeDirectory"></param>
        /// <param name="visibleStatusIn">List of the visible staus used for nested if</param>
        /// <returns></returns>
        public static GenXmlTemplate GetGenXmlTemplate(String templateData, Dictionary<String, String> settingsDic, String portalHomeDirectory, List<Boolean> visibleStatusIn)
        {
            if (templateData.Trim() != "") templateData = "[<tag type='tokennamespace' value='nbs' />]" + templateData; // add token namespoace for nbs (no need if empty)

            var itemTempl = new GenXmlTemplate(templateData, settingsDic,visibleStatusIn);
            // add default resx folder to template
            itemTempl.AddResxFolder(StoreSettings.NBrightBuyPath() + "/App_LocalResources/");
            if (settingsDic.ContainsKey("themefolder") && settingsDic["themefolder"] != "")
            {
                itemTempl.AddResxFolder(StoreSettings.NBrightBuyPath() + "/Themes/" + settingsDic["themefolder"] + "/resx/");
            }

            return itemTempl;
        }

        /// <summary>
        /// Save temnpoary form data into memory.  This save data we want to repopulate a formwith after pstback into memory, rather than the DB.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="msgcode"></param>
        /// <param name="result"></param>
        public static void SetFormTempData(int moduleId, String xmlData)
        {
            if (xmlData != "")
            {
                var sessionkey = "NBrightBuyForm*" + moduleId.ToString("");
                HttpContext.Current.Session[sessionkey] = xmlData;
            }
        }
        public static String GetFormTempData(int moduleId)
        {
            var xmlData = "";
            var sessionkey = "NBrightBuyForm*" + moduleId.ToString("");
            if (HttpContext.Current.Session[sessionkey] != null) xmlData = (String)HttpContext.Current.Session[sessionkey];
            if (xmlData != "")
            {
                HttpContext.Current.Session.Remove(sessionkey);
                return xmlData;
            }
            return "";
        }

        /// <summary>
        /// Set Notify Message
        /// </summary>
        /// <param name="moduleId">modelid</param>
        /// <param name="msgcode">message code to pick in resx</param>
        /// <param name="result">result code</param>
        /// <param name="resxpath">resx folder path of the message</param>
        public static void SetNotfiyMessage(int moduleId, String msgcode, NotifyCode result, String resxpath = "")
        {
            if (resxpath == "") resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
            if (msgcode != "")
            {
                var sessionkey = "NBrightBuyNotify*" + moduleId.ToString("");
                HttpContext.Current.Session[sessionkey] = msgcode + "_" + result;
                sessionkey = "NBrightBuyNotify*" + moduleId.ToString("") + "*resxpath";
                HttpContext.Current.Session[sessionkey] = resxpath;
            }
        }

        public static String GetNotfiyMessage(int moduleId)
        {
            var msgcode = "";
            var sessionkey = "NBrightBuyNotify*" + moduleId.ToString("");
            if (HttpContext.Current.Session[sessionkey] != null) msgcode = (String) HttpContext.Current.Session[sessionkey];
            if (msgcode != "")
            {
                var sessionkeyresx = "NBrightBuyNotify*" + moduleId.ToString("") + "*resxpath";
                var resxmsgpath = (String)HttpContext.Current.Session[sessionkeyresx];
                HttpContext.Current.Session.Remove(sessionkey);
                HttpContext.Current.Session.Remove(sessionkeyresx);

                var msgtempl = GetResxMessage(msgcode, resxmsgpath);
                return msgtempl;
            }
            return "";
        }

        public static String GetResxMessage(String msgcode = "general_ok",String resxmsgpath = "")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
            if (resxmsgpath == "") resxmsgpath = resxpath;
            var msg = DnnUtils.GetLocalizedString(msgcode, resxmsgpath, Utils.GetCurrentCulture());
            var level = "ok";
            if (msgcode.EndsWith("_" + NotifyCode.fail.ToString())) level = NotifyCode.fail.ToString();
            if (msgcode.EndsWith("_" + NotifyCode.warning.ToString())) level = NotifyCode.warning.ToString();
            if (msgcode.EndsWith("_" + NotifyCode.error.ToString())) level = NotifyCode.error.ToString();
            var msgtempl = DnnUtils.GetLocalizedString("notifytemplate_" + level, resxpath, Utils.GetCurrentCulture());
            if (msgtempl == null) msgtempl = msg;
            msgtempl = msgtempl.Replace("{message}", msg);
            return msgtempl;
        }

        public static void SendEmailToManager(String templateName, NBrightInfo dataObj, String emailsubjectresxkey = "", String fromEmail = "")
        {
            NBrightBuyUtils.SendEmail(StoreSettings.Current.ManagerEmail, templateName, dataObj, emailsubjectresxkey, fromEmail, StoreSettings.Current.Get("merchantculturecode"));
        }

        public static void SendEmailOrderToClient(String templateName, int orderId, String emailsubjectresxkey = "", String fromEmail = "", String emailmsg = "")
        {
            var ordData = new OrderData(orderId);
            var lang = ordData.Lang;
            if (ordData.GetInfo().UserId > 0)
            {
                // this order is linked to a DNN user, so get the order email from the DNN profile (so if it's updated since the order, we pickup the new one)
                var objUser = UserController.GetUserById(ordData.PortalId, ordData.GetInfo().UserId);
                if (objUser != null)
                {
                    if (ordData.EmailAddress != objUser.Email)
                    {
                        ordData.EmailAddress = objUser.Email;
                        ordData.SavePurchaseData();
                    }
                    if (objUser.Profile.PreferredLocale != "") lang = objUser.Profile.PreferredLocale;
                }
            }
            if (lang == "") lang = Utils.GetCurrentCulture();
            // we're going to send email to all email addreses linked to the order.
            var emailList = ordData.EmailAddress;
            if (!emailList.Contains(ordData.EmailShippingAddress) && Utils.IsEmail(ordData.EmailShippingAddress)) emailList += "," + ordData.EmailShippingAddress;
            if (!emailList.Contains(ordData.EmailBillingAddress) && Utils.IsEmail(ordData.EmailBillingAddress)) emailList += "," + ordData.EmailBillingAddress;
            ordData.PurchaseInfo.SetXmlProperty("genxml/emailmsg", emailmsg);
            SendEmail(emailList, templateName, ordData.GetInfo(), emailsubjectresxkey, fromEmail, lang);
        }

        public static void SendEmail(String toEmail, String templateName, NBrightInfo dataObj, String emailsubjectresxkey, String fromEmail,String lang)
        {
            dataObj = ProcessEventProvider(EventActions.BeforeSendEmail, dataObj, templateName);

            if (!dataObj.GetXmlPropertyBool("genxml/stopprocess"))
            {

                if (lang == "") lang = Utils.GetCurrentCulture();
                var emaillist = toEmail;
                if (emaillist != "")
                {
                    var emailsubject = "";
                    if (emailsubjectresxkey != "")
                    {
                        var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
                        emailsubject = DnnUtils.GetLocalizedString(emailsubjectresxkey, resxpath, lang);
                        if (emailsubject == null) emailsubject = emailsubjectresxkey;
                    }

                    // we can't use StoreSettings.Current.Settings(), becuase of external calls from providers to this function, so load in the settings directly  
                    var modCtrl = new NBrightBuyController();
                    var storeSettings = modCtrl.GetStoreSettings(dataObj.PortalId);

                    var strTempl = modCtrl.GetTemplateData(-1, templateName, lang, storeSettings.Settings(), storeSettings.DebugMode);

                    var emailbody = GenXmlFunctions.RenderRepeater(dataObj, strTempl, "", "XMLData", lang, storeSettings.Settings());
                    if (templateName.EndsWith(".xsl")) emailbody = XslUtils.XslTransInMemory(dataObj.XMLData, emailbody);
                    if (fromEmail == "") fromEmail = storeSettings.AdminEmail;
                    var emailarray = emaillist.Split(',');
                    emailsubject = storeSettings.Get("storename") + " : " + emailsubject;
                    foreach (var email in emailarray)
                    {
                        if (!string.IsNullOrEmpty(email) && Utils.IsEmail(fromEmail) && Utils.IsEmail(email))
                        {
                            // multiple attachments as csv with "|" seperator
                            DotNetNuke.Services.Mail.Mail.SendMail(fromEmail, email, "", emailsubject, emailbody, dataObj.GetXmlProperty("genxml/emailattachment"), "HTML", "", "", "", "");
                        }
                    }
                }
            }

            ProcessEventProvider(EventActions.AfterSendEmail, dataObj, templateName);


        }

        public static List<NBrightInfo> GetCategoryGroups(String lang, Boolean debugMode = false)
        {
            var objCtrl = new NBrightBuyController();
            var levelList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "GROUP", "GROUPLANG", lang, "", " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", debugMode);
            return levelList;
        }

        public static List<NBrightInfo> GetGenXmlListByAjax(string xmlAjaxData, string originalXml, string lang = "en-US", string xmlRootName = "genxml")
        {
            var rtnList = new List<NBrightInfo>();
            if (!String.IsNullOrEmpty(xmlAjaxData))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlAjaxData);
                var nodList = xmlDoc.SelectNodes("root/root");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        var xmlData = GenXmlFunctions.GetGenXmlByAjax(nod.OuterXml, "");
                        var objInfo = new NBrightInfo();
                        objInfo.ItemID = -1;
                        objInfo.TypeCode = "AJAXDATA";
                        objInfo.XMLData = xmlData;
                        rtnList.Add(objInfo);
                    }
            }
            return rtnList;
        }




        public static String FormatToStoreCurrency(double value)
        {


            var currencycode = StoreSettings.Current.Get("currencyculturecode");
            if (currencycode.StartsWith("\"")) return value.ToString(currencycode.Replace("\"", ""));
            if (currencycode == "") currencycode = StoreSettings.Current.Get("merchantculturecode");
            if (currencycode == "") currencycode = PortalSettings.Current.CultureCode;
            try
            {
                var currFormat = value.ToString("c", new CultureInfo(currencycode, false));
                if (currFormat.Length > 0 && Utils.IsNumeric(currFormat.Substring(0, 1))) return value.ToString("0.00", new CultureInfo(Utils.GetCurrentCulture(), false)) + StoreSettings.Current.Get("currencysymbol");
                return StoreSettings.Current.Get("currencysymbol") + value.ToString("0.00", new CultureInfo(Utils.GetCurrentCulture(), false));
                // NOTE: ATM we only have 1 currency.
            }
            catch (Exception)
            {
                return value.ToString("0.00", new CultureInfo(PortalSettings.Current.CultureCode, false));
            }
        }

        public static String GetCurrencyIsoCode()
        {
            var currencycode = StoreSettings.Current.Get("currencyculturecode");
            if (currencycode == "") currencycode = StoreSettings.Current.Get("merchantculturecode");
            if (currencycode == "") currencycode = PortalSettings.Current.CultureCode;
            try
            {
                return new RegionInfo(currencycode).ISOCurrencySymbol;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Dictionary<String, String> GetCountryList(String dnnlistname = "Country")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/CountryNames.ascx.resx";

            var objCtrl = new DotNetNuke.Common.Lists.ListController();
            var tList = objCtrl.GetListEntryInfoDictionary(dnnlistname);
            var rtnDic = new Dictionary<String, String>();

            var xmlNodeList = StoreSettings.Current.SettingsInfo.XMLDoc.SelectNodes("genxml/checkboxlist/countrycodelist/chk[@value='True']");
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNoda in xmlNodeList)
                {
                    if (xmlNoda.Attributes != null)
                    {
                        if (xmlNoda.Attributes.GetNamedItem("data") != null)
                        {
                            var datavalue = xmlNoda.Attributes["data"].Value;
                            //use the data attribute if there
                            if (tList.ContainsKey(datavalue))
                            {
                                var countryname = DnnUtils.GetLocalizedString(tList[datavalue].Text, resxpath, Utils.GetCurrentCulture());
                                if (String.IsNullOrEmpty(countryname)) countryname = tList[datavalue].Text;
                                rtnDic.Add(datavalue.Replace(dnnlistname + ":", ""),countryname);
                            }
                        }
                    }
                }
            }
            var sortlist = StoreSettings.Current.Get("countrysortorder");
            if (sortlist == "") return rtnDic;
            
            var rtnSort = new Dictionary<String, String>();
            var s = sortlist.Split(';');
            foreach (var c in s)
            {
                if (c != "")
                {
                    if (rtnDic.ContainsKey(c))
                    {
                        var d = rtnDic[c];
                        rtnSort.Add(c,d);
                        rtnDic.Remove(c);
                    }
                }
            }
            foreach (var c in rtnDic)
            {
                rtnSort.Add(c.Key,c.Value);    
            }

            return rtnSort;
        }

        public static Dictionary<String, String> GetRegionList(String countrycode, String dnnlistname = "Region")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/RegionNames.ascx.resx";
            var parentkey = "Country." + countrycode;
            var objCtrl = new DotNetNuke.Common.Lists.ListController();
            var tList = objCtrl.GetListEntryInfoDictionary(dnnlistname, parentkey);
            var rtnDic = new Dictionary<String, String>();

            foreach (var r in tList)
            {
                var datavalue = r.Value;
                var regionname = DnnUtils.GetLocalizedString(datavalue.Text, resxpath, Utils.GetCurrentCulture());
                if (String.IsNullOrEmpty(regionname)) regionname = datavalue.Text;
                rtnDic.Add(datavalue.Key, regionname);                
            }

            return rtnDic;
        }

        public static Boolean UpdateResxFields(Dictionary<String, String> resxFields, List<String> resxFolders, String lang, Boolean createFile)
        {
            var fileList = new Dictionary<String, String>(); // use NBrightInfo to edit res file to make it easier.

            if (lang != "") lang = "." + lang;

            foreach (var d in resxFields)
            {
                var resxKeyArray = d.Key.Split('.');
                if (resxKeyArray.Length == 2)
                {
                    var fileName = resxKeyArray[0];
                    var resxKey = resxKeyArray[1];
                    if (!fileList.ContainsKey(fileName)) fileList.Add(fileName, "");
                }
            }

            var updDone = false;
            foreach (var r in resxFolders)
            {
                foreach (var f in fileList)
                {
                    var fpath = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx.resx");
                    //var fpathlang = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx" + lang + ".resx");
                    var fpathlangportal = "";
                    if (lang.EndsWith(Localization.SystemLocale))
                        fpathlangportal = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx.Portal-" + PortalSettings.Current.PortalId.ToString("") + ".resx");
                    else
                        fpathlangportal = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx" + lang + ".Portal-" + PortalSettings.Current.PortalId.ToString("") + ".resx");
                    if (File.Exists(fpath))
                    {
                        if (!File.Exists(fpathlangportal)) CreateResourceFile(fpathlangportal);
                        foreach (var d in resxFields)
                        {
                            var resxKeyArray = d.Key.Split('.');
                            if (resxKeyArray.Length == 2)
                            {
                                var fileName = resxKeyArray[0];
                                var resxKey = resxKeyArray[1];
                                if (fileName == f.Key)
                                {
                                    if (UpdateResourceFile(resxKey + ".Text", d.Value, fpathlangportal)) updDone = true;
                                }
                            }
                        }
                    }
                }
            }

            return updDone;
        }

        public static void CreateResourceFile(String path)
        {
            var pathString = Path.GetDirectoryName(path);
            if (!Directory.Exists(pathString)) System.IO.Directory.CreateDirectory(pathString);

            ResXResourceWriter resourceWriter = new ResXResourceWriter(path);
            resourceWriter.Generate();
            resourceWriter.Close();
        }

        public static Boolean UpdateResourceFile(String key, String value, String path)
        {
            Hashtable resourceEntries = new Hashtable();

            //Get existing resources
            if (File.Exists(path))
            {
                ResXResourceReader reader = new ResXResourceReader(path);
                if (reader != null)
                {
                    IDictionaryEnumerator id = reader.GetEnumerator();
                    foreach (DictionaryEntry d in reader)
                    {
                        if (d.Value == null)
                            resourceEntries.Add(d.Key.ToString(), "");
                        else
                            resourceEntries.Add(d.Key.ToString(), d.Value.ToString());
                    }
                    reader.Close();
                }
            }
            //Modify resources here...
            var updDone = false;
                if (resourceEntries.ContainsKey(key))
                {
                    if ((string) resourceEntries[key] != value)
                    {
                        resourceEntries[key] = value;
                        updDone = true;
                    }
                }
                else
                {
                    resourceEntries.Add(key,value);
                    updDone = true;
                }

            //Write the combined resource file
            if (updDone)
            {
                ResXResourceWriter resourceWriter = new ResXResourceWriter(path);

                foreach (String key2 in resourceEntries.Keys)
                {
                    resourceWriter.AddResource(key2, resourceEntries[key2]);
                }
                resourceWriter.Generate();
                resourceWriter.Close();                
            }
            return updDone;
        }

        public static String AdminUrl(int tabId, string[] param )
        {
            var newparam = new string[(param.Length + 1)];
            var foundCtrl = false;
            for (int i = 0; i < param.Length; i++)
            {
                if (param[i] != null && param[i].StartsWith("ctrl=")) foundCtrl = true;
                newparam[i] = param[i];
            }
            if (!foundCtrl) newparam[param.Length] = "ctrl=" + HttpContext.Current.Session["nbrightbackofficectrl"];
            return Globals.NavigateURL(tabId, "", newparam);
        }

        public static List<String> GetAllFieldxPaths(NBrightInfo nbi, Boolean ignoreHiddenFields = false)
        {
            var ignorefields = nbi.GetXmlProperty("genxml/hidden/ignorefields");
            ignorefields = ignorefields + ",ignorefields";
            var ignoreAry = ignorefields.Split(',');
            
            var rtnList = new List<String>();
            var nods = nbi.XMLDoc.SelectNodes("genxml/hidden/*");
            if (!ignoreHiddenFields)
            {
                if (nods != null)
                {
                    foreach (XmlNode xmlNod in nods)
                    {
                        if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/hidden/" + xmlNod.Name);
                    }
                }                
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/textbox/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/textbox/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/checkbox/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/checkbox/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/dropdownlist/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/dropdownlist/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/radiobuttonlist/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/radiobuttonlist/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/edt/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/edt/" + xmlNod.Name);
                }
            }

            return rtnList;
        }

        public static int ValidateStore()
        {
            var objCtrl = new NBrightBuyController();
            var errcount = 0;

            // Validate Products
            var prodList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "PRD");
            foreach (var p in prodList)
            {
                var prodData = new ProductData(p.ItemID, StoreSettings.Current.EditLanguage);
                errcount += prodData.Validate();
            }

            // Validate Categories
            var catList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "CATEGORY");
            foreach (var c in catList)
            {
                var catData = new CategoryData(c.ItemID, StoreSettings.Current.EditLanguage);
                errcount += catData.Validate();
            }

            // Validate Groups
            var grpList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "GROUP");
            foreach (var c in grpList)
            {
                var grpData = new GroupData(c.ItemID, StoreSettings.Current.EditLanguage);
                errcount += grpData.Validate();
            }

            // reset catid from catref
            var l = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "SETTINGS", "", Utils.GetCurrentCulture(), " and [XMLdata].value('(genxml/catref)[1]','nvarchar(max)') != ''","");
            foreach (var s in l)
            {
                var info = ModuleSettingsResetCatIdFromRef(s);
                objCtrl.Update(info);
            }


            return errcount;
        }

        public static NBrightInfo ProcessEventProvider(EventActions eventaction, NBrightInfo nbrightInfo)
        {
            return ProcessEventProvider(eventaction, nbrightInfo, "");
        }

        public static NBrightInfo ProcessEventProvider(EventActions eventaction, NBrightInfo nbrightInfo, String eventinfo)
        {
            var rtnInfo = nbrightInfo;
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetEventsProviders();

            foreach (var d in provList)
            {
                var prov = d.Value;
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                if (handle != null)
                {
                    var eventprov = (EventInterface) handle.Unwrap();
                    if (eventprov != null)
                    {
                        if (eventaction == EventActions.ValidateCartBefore)
                        {
                            rtnInfo = eventprov.ValidateCartBefore(nbrightInfo);
                        }
                        else if (eventaction == EventActions.ValidateCartAfter)
                        {
                            rtnInfo = eventprov.ValidateCartAfter(nbrightInfo);
                        }
                        else if (eventaction == EventActions.ValidateCartItemBefore)
                        {
                            rtnInfo = eventprov.ValidateCartItemBefore(nbrightInfo);
                        }
                        else if (eventaction == EventActions.ValidateCartItemAfter)
                        {
                            rtnInfo = eventprov.ValidateCartItemAfter(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterCartSave)
                        {
                            rtnInfo = eventprov.AfterCartSave(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterCategorySave)
                        {
                            rtnInfo = eventprov.AfterCategorySave(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterProductSave)
                        {
                            rtnInfo = eventprov.AfterProductSave(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterSavePurchaseData)
                        {
                            rtnInfo = eventprov.AfterSavePurchaseData(nbrightInfo);
                        }
                        else if (eventaction == EventActions.BeforeOrderStatusChange)
                        {
                            rtnInfo = eventprov.BeforeOrderStatusChange(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterOrderStatusChange)
                        {
                            rtnInfo = eventprov.AfterOrderStatusChange(nbrightInfo);
                        }
                        else if (eventaction == EventActions.BeforePaymentOK)
                        {
                            rtnInfo = eventprov.BeforePaymentOK(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterPaymentOK)
                        {
                            rtnInfo = eventprov.AfterPaymentOK(nbrightInfo);
                        }
                        else if (eventaction == EventActions.BeforePaymentFail)
                        {
                            rtnInfo = eventprov.BeforePaymentFail(nbrightInfo);
                        }
                        else if (eventaction == EventActions.AfterPaymentFail)
                        {
                            rtnInfo = eventprov.AfterPaymentFail(nbrightInfo);
                        }
                        else if (eventaction == EventActions.BeforeSendEmail)
                        {
                            rtnInfo = eventprov.BeforeSendEmail(nbrightInfo,eventinfo);
                        }
                        else if (eventaction == EventActions.AfterSendEmail)
                        {
                            rtnInfo = eventprov.AfterSendEmail(nbrightInfo,eventinfo);
                        }

                    }
                }
            }
            return rtnInfo;
        }

        /// <summary>
        /// Helper function to help plugins get a theme template from their local theme folder.
        /// This function will get the template, replace settings tokens, replace url tokens
        /// </summary>
        /// <param name="templatename">name of template</param>
        /// <param name="templateControlPath">Relative Control path of plugin e.g."/DesktopModules/NBright/NBrightBuyPluginTempl"</param>
        /// <param name="themeFolder">Theme folder to use e.g. "config"</param>
        /// <param name="settings">Settings to use, default to storesettings</param>
        /// <returns></returns>
        public static String GetTemplateData(String templatename, String templateControlPath, String themeFolder = "config", Dictionary<String, String> settings = null)
        {
            themeFolder = "Themes\\" + themeFolder;
            if (settings == null) settings = StoreSettings.Current.Settings();
            var controlMapPath = HttpContext.Current.Server.MapPath(templateControlPath);
            var templCtrl = new TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, themeFolder, StoreSettings.Current.ThemeFolder);
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            templ = Utils.ReplaceSettingTokens(templ, settings);
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

        public static List<NBrightInfo> GetCatList(int parentid = 0, string groupref = "", String lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();

            var strFilter = "";
            if (groupref == "" || groupref == "0") // Because we've introduced Properties (for non-category groups) we will only display these if cat is not selected.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') != 'cat' ";
            else
            {
                if (groupref == "cat") strFilter = " and NB1.ParentItemId = " + parentid + " "; // only category have multipel levels.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') = '" + groupref + "' ";
            }

            var objCtrl = new NBrightBuyController();
            var levelList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", lang, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);

            var grpCtrl = new GrpCatController(lang);

            foreach (var c in levelList)
            {
                var g = grpCtrl.GetCategory(c.ItemID);
                if (g != null) c.SetXmlProperty("genxml/entrycount", g.entrycount.ToString(""));
            }

            return levelList;
        }

        public static NBrightInfo ModuleSettingsResetCatIdFromRef(NBrightInfo objInfo)
        {
            var ModCtrl = new NBrightBuyController();
            var catid = objInfo.GetXmlPropertyInt("genxml/dropdownlist/defaultcatid");
            var nbi = ModCtrl.Get(catid);
            if (nbi == null)
            {
                // categoryid no longer exists, see if we can get it back with the catref (might be lost due to cleardown and import)
                var catref = objInfo.GetXmlProperty("genxml/catref");
                nbi = ModCtrl.GetByGuidKey(objInfo.PortalId, -1, "CATEGORY", catref);
                if (nbi != null)
                {
                    objInfo.SetXmlProperty("genxml/dropdownlist/defaultcatid", nbi.ItemID.ToString(""));
                }
            }
            return objInfo;
        }

        public static NBrightInfo ModuleSettingsSaveCatRefFromId(NBrightInfo objInfo)
        {
            var catid = objInfo.GetXmlPropertyInt("genxml/dropdownlist/defaultcatid");
            var catData = new CategoryData(catid, Utils.GetCurrentCulture());
            objInfo.SetXmlProperty("genxml/catref", catData.CategoryRef);
            return objInfo;
        }

        public static int ModuleSettingsGetCatIdFromRef(NBrightInfo settingsInfo)
        {
            var ModCtrl = new NBrightBuyController();
            // categoryid no longer exists, see if we can get it back with the catref (might be lost due to cleardown and import)
            var catref = settingsInfo.GetXmlProperty("genxml/catref");
            var nbi = ModCtrl.GetByGuidKey(settingsInfo.PortalId, -1, "CATEGORY", catref);
            if (nbi != null) return nbi.ItemID;
            return -1;
        }



    }
}


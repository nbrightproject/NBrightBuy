using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using NBrightCore.TemplateEngine;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public static class NBrightBuyUtils
    {

        /// <summary>
        /// Use the setting to get the template system getter control.
        /// </summary>
        /// <returns></returns>
        public static TemplateGetter GetTemplateGetter(string themeFolder)
        {
            var objSettings = GetGlobalSettings(PortalSettings.Current.PortalId);
            //var controlMapPath = objSettings.GetXmlProperty("genxml/hidden/controlmappath");
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightBuy");
            themeFolder = "Themes/" + themeFolder;
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "NBrightTemplates", themeFolder);
            return templCtrl;
        }

        public static NBrightInfo GetSettings(int portalId, int moduleId, bool useCache = true)
        {
            var obj = (NBrightInfo)GetModCache("NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""));
            if (obj == null | !useCache)
            {
                // single record for EntityTypeCode settings, so get record directly.
                var objCtrl = new NBrightBuyController();
                obj = objCtrl.GetByType(portalId, moduleId, "SETTINGS");
                if (obj == null)
                {
                    obj = new NBrightInfo { ItemID = -1 };
                    obj.TypeCode = "SETTINGS";
                    obj.ModuleId = moduleId;
                    obj.PortalId = portalId;
                    obj.XMLData = "<genxml><hidden></hidden><textbox></textbox></genxml>";
                }
                SetModCache(-1, "NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""), obj);
            }
            return obj;
        }

        public static NBrightInfo GetGlobalSettings(int portalId, bool useCache = true)
        {
            var obj = (NBrightInfo)GetModCache("NBrightBuy_Global" + portalId.ToString(""));


            //[TODO: NB_Store v2.4 code for setting, needs to be changed/removed for v3]
            if (obj == null || !useCache)
            {
                var objNbsv2 = new NBrightInfo();
                objNbsv2.XMLData = "<genxml><hidden></hidden></genxml>";
                // load up NB_Store v2 settings
                var NBS2Ctrl = new SettingsController();
                var l = NBS2Ctrl.GetSettingList(portalId, Utils.GetCurrentCulture(), true, "");
                foreach (NB_Store_SettingsInfo i in l)
                {
                    objNbsv2.AddSingleNode(i.SettingName, i.SettingValue, "genxml/hidden");
                }

                //save these as the global settings record [TODO: change this for v3]
                var objCtrl = new NBrightBuyController();
                obj = objCtrl.GetByType(portalId, -1, "GLOBAL");
                if (obj == null) obj = new NBrightInfo { ItemID = -1 };
                obj.TypeCode = "GLOBAL";
                obj.PortalId = portalId;
                obj.XMLData = objNbsv2.XMLData;
                objCtrl.Update(obj);
                SetModCache(-1, "NBrightBuy_Global" + portalId.ToString(""), obj);
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
                var l = new List<NBrightInfo> { objInfo };
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

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(PortalId, -1, "SETTINGS", strFilter, "", 1, 1, 1, 1, "",""));
            if (l.Count >= 1)
            {
                rtnKeyRef = GetUniqueKeyRef(PortalId, ModuleId, KeyRef, LoopCount + 1);
            }

            return rtnKeyRef;
        }


        #region "Build Urls"

        public static string GetEntryUrl(int portalId, string entryid, string modulekey, string guidkey, string tabid)
        {
            var rdTabid = -1;
            var objTabInfo = new TabInfo();

            if (PortalSettings.Current != null)
            {
                objTabInfo = PortalSettings.Current.ActiveTab;
                rdTabid = objTabInfo.TabID;
            }

            if (Utils.IsNumeric(tabid) && Convert.ToInt32(tabid) > 0) rdTabid = Convert.ToInt32(tabid);

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

            guidkey = Utils.CleanInput(guidkey);
            var strurl = "~/Default.aspx?TabId=" + rdTabid.ToString("") + "&eid=" + entryid + rdModId;
            return DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(objTabInfo, strurl, guidkey.Replace(entryid + "-", "") + ".aspx");

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
            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, Convert.ToInt32(redirectTab.TabID), moduleKey,StoreSettings.Current.Get("storagetype"));
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
            var cList = (List<String>)NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
            if (cList == null) cList = new List<String>();
            if (!cList.Contains(CacheKey))
            {
                cList.Add(CacheKey);
                NBrightCore.common.Utils.SetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture), cList);
                NBrightCore.common.Utils.SetCache(CacheKey, objObject);
            }
        }

        public static void RemoveModCache(int moduleid)
        {
            var cList = (List<String>)NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
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
                        if (objInfo != null) includetext = GenXmlFunctions.RenderRepeater(objInfo, includetext);
                        if (includetext != "") PageIncludes.IncludeTextInHeader(page, includetext);                        
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Get the GenXmltemplate class and assign required resx files.
        /// </summary>
        /// <param name="templateData"></param>
        /// <param name="settingsDic"></param>
        /// <param name="portalHomeDirectory"></param>
        /// <returns></returns>
        public static GenXmlTemplate GetGenXmlTemplate(String templateData, Dictionary<String, String> settingsDic,String portalHomeDirectory)
        {
            var itemTempl = new GenXmlTemplate(templateData, settingsDic);
            // add default resx folder to template
            itemTempl.AddResxFolder("/DesktopModules/NBright/NBrightBuy/App_LocalResources/");
            if (settingsDic.ContainsKey("themefolder") && settingsDic["themefolder"] != "")
            {
                itemTempl.AddResxFolder("/DesktopModules/NBright/NBrightBuy/Themes/" + settingsDic["themefolder"] +
                                        "/resx/");
                itemTempl.AddResxFolder(portalHomeDirectory + "Themes/" + settingsDic["themefolder"] + "/resx/");
            }
            return itemTempl;
        }



    }

}


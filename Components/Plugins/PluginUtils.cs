using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.TemplateEngine;
using NBrightCore.render;
using NBrightDNN;
using NBrightCore.common;
using NBrightCore.providers;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class PluginUtils
    {

        public static List<NBrightInfo> GetPluginList()
        {
            var objCtrl = new NBrightBuyController();
            var rtnList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "PLUGIN","", "order by nb1.xmldata.value('(genxml/hidden/index)[1]','int')");
            if (rtnList.Count == 0)
            {
                rtnList = CreatePlugins();
            }
            return rtnList;
        }

        private static List<NBrightInfo> CreatePlugins()
        {
            var pluginList = new List<NBrightInfo>();

            var info = new NBrightInfo();
            info.PortalId = PortalSettings.Current.PortalId;
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(PortalSettings.Current.PortalId, "config");
            var menuplugin = templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
            if (menuplugin != "")
            {
                info.XMLData = menuplugin;
                pluginList = CalcPluginList(info);
            }
            else
            {
                // no menuplugin.xml exists, so must be new install, get new config
                var pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath() + "/Plugins");
                if (pluginfoldermappath != null && Directory.Exists(pluginfoldermappath))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(pluginfoldermappath + "\\menu.config");
                    pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath() + "/Themes/config/default");
                    xmlDoc.Save(pluginfoldermappath + "\\menuplugin.xml");
                    //load new config
                    menuplugin = templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
                    if (menuplugin != "")
                    {
                        info.XMLData = menuplugin;
                        pluginList = CalcPluginList(info);
                        File.Delete(pluginfoldermappath + "\\menuplugin.xml");
                    }
                }
            }

            var objCtrl = new NBrightBuyController();
            foreach (var p in pluginList)
            {
                p.ItemID = -1;
                p.GUIDKey = p.GetXmlProperty("genxml/textbox/ctrl");
                p.PortalId = PortalSettings.Current.PortalId;
                p.Lang = "";
                p.ParentItemId = 0;
                p.ModuleId = -1;
                p.XrefItemId = 0;
                p.UserId = 0;
                p.TypeCode = "PLUGIN";

                var interfaces = p.XMLDoc.SelectNodes("genxml/interfaces/*");
                if (interfaces.Count == 0)
                {
                    // possible legacy format, change.
                    p.SetXmlProperty("genxml/checkbox/disable", p.GetXmlProperty("genxml/checkbox/active"));
                    p.SetXmlProperty("genxml/interfaces", "");
                    p.SetXmlProperty("genxml/interfaces/genxml", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/dropdownlist", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/checkbox", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/dropdownlist/providertype", p.GetXmlProperty("genxml/dropdownlist/providertype"));
                    p.SetXmlProperty("genxml/interfaces/genxml/checkbox/active", p.GetXmlProperty("genxml/checkbox/active"));
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox/namespaceclass", p.GetXmlProperty("genxml/textbox/namespaceclass"));
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox/assembly", p.GetXmlProperty("genxml/textbox/assembly"));
                }

                objCtrl.Update(p);
            }

            return pluginList;
        }


        /// <summary>
        /// Build list of plugins from XML config or legacy files.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static List<NBrightInfo> CalcPluginList(NBrightInfo info)
        {
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(PortalSettings.Current.PortalId, "config");

            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = info.XMLDoc.SelectNodes("genxml/plugin/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    newInfo.ItemID = rtnList.Count;
                    newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""));
                    newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                    rtnList.Add(newInfo);
                }
            }

            // get the systemlevel, incase this is an update and we have new system level provider that needs to be added
            // Some systems create their own portal specific menu we assume they don't require new updates from NBS core, so take that if we have one.
            var menupluginsys = templCtrl.GetTemplateData("menuplugin" + PortalSettings.Current.PortalId + ".xml", Utils.GetCurrentCulture(), true, true, false, StoreSettings.Current.Settings());
            // if no portal specific menus exist, take the default
            if (menupluginsys == "") menupluginsys = templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, false, StoreSettings.Current.Settings());
            var infosys = new NBrightInfo();
            infosys.XMLData = menupluginsys;
            if (infosys.XMLDoc != null)
            {
                var xmlNodeList2 = infosys.XMLDoc.SelectNodes("genxml/plugin/*");
                if (xmlNodeList2 != null)
                {
                    foreach (XmlNode carNod in xmlNodeList2)
                    {
                        var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                        newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                        var resultsys = rtnList.Where(p => p.GUIDKey == newInfo.GUIDKey);
                        if (!resultsys.Any())
                        {
                            // add the missing plugin to the active list
                            newInfo.ItemID = rtnList.Count;
                            newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""));
                            newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                            rtnList.Add(newInfo);
                        }
                    }
                }
            }

            return rtnList;
        }


        /// <summary>
        /// Search filesystem for any new plugins that have been added. Removed any deleted ones.
        /// </summary>
        public void UpdateSystemPlugins()
        {
                // Add new plugins
                var updated = false;
                var pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath() + "/Plugins");
            if (pluginfoldermappath != null && Directory.Exists(pluginfoldermappath))
            {
                var objCtrl = new NBrightBuyController();
                var ctrlList = new Dictionary<String, int>();
                var flist = Directory.GetFiles(pluginfoldermappath);
                foreach (var f in flist)
                {
                    if (f.EndsWith(".xml"))
                    {
                        var datain = File.ReadAllText(f);
                        try
                        {
                            var nbi = new NBrightInfo();
                            nbi.XMLData = datain;
                            // check if we are injecting multiple
                            var nodlist = nbi.XMLDoc.SelectNodes("root/genxml");
                            if (nodlist != null && nodlist.Count == 0)
                            {
                                nbi.ItemID = -1;
                                nbi.GUIDKey = nbi.GetXmlProperty("genxml/textbox/ctrl");
                                nbi.PortalId = PortalSettings.Current.PortalId;
                                nbi.Lang = "";
                                nbi.ParentItemId = 0;
                                nbi.ModuleId = -1;
                                nbi.XrefItemId = 0;
                                nbi.UserId = 0;
                                nbi.TypeCode = "PLUGIN";
                                objCtrl.Update(nbi);
                            }
                            else
                            {
                                foreach (XmlNode nod in nodlist)
                                {
                                    var nbi2 = new NBrightInfo();
                                    nbi2.XMLData = nod.OuterXml;
                                    nbi2.ItemID = -1;
                                    nbi2.GUIDKey = nbi.GetXmlProperty("genxml/textbox/ctrl");
                                    nbi2.PortalId = PortalSettings.Current.PortalId;
                                    nbi2.Lang = "";
                                    nbi2.ParentItemId = 0;
                                    nbi2.ModuleId = -1;
                                    nbi2.XrefItemId = 0;
                                    nbi2.UserId = 0;
                                    nbi2.TypeCode = "PLUGIN";
                                    objCtrl.Update(nbi2);
                                }
                            }


                            ctrlList.Add(nbi.GetXmlProperty("genxml/textbox/ctrl"), nbi.GetXmlPropertyInt("genxml/hidden/index"));

                            updated = true;
                            File.Delete(f);
                        }
                        catch (Exception)
                        {
                            // data might not be XML complient (ignore)
                        }
                    }
                }

            }

            if (updated)
            {
                ClearPluginCache(PortalSettings.Current.PortalId);
            }

        }

        public void RemoveDeletedSystemPlugins()
        {
            // remove delete plugins.
            var pluginList = GetPluginList();
            foreach (var p in pluginList)
            {
                var remove = false;
                var ctrlpath = p.GetXmlProperty("path");
                if (ctrlpath != "")
                {
                    var ctrlmappath = System.Web.Hosting.HostingEnvironment.MapPath(ctrlpath);
                    var assembly = p.GetXmlProperty("assembly");
                    if (ctrlpath == "" || !File.Exists(ctrlmappath))
                    {
                        if (assembly != "")
                        {
                            if (!assembly.EndsWith(".dll")) assembly = assembly + ".dll";
                            var binmappath = System.Web.Hosting.HostingEnvironment.MapPath("/bin/" + assembly);
                            if (!File.Exists(binmappath)) remove = true;
                        }
                        else
                            remove = true;
                    }

                    if (remove)
                    {
                        var objCtrl = new NBrightBuyController();
                        objCtrl.Delete(p.ItemID);
                    }
                }
            }
            ClearPluginCache(PortalSettings.Current.PortalId);
        }

        public void ClearPluginCache(int portalId)
        {
            var cachekey = "pluginlist" + portalId;
            NBrightBuyUtils.RemoveCache(cachekey);
        }



    }
}
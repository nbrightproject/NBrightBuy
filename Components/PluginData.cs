using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.WebControls;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class PluginData
    {
        private List<NBrightInfo> _pluginList;
        public NBrightInfo Info;
        private NBrightCore.TemplateEngine.TemplateGetter _templCtrl;
        private Boolean portallevel;

        public PluginData(int portalId, Boolean systemlevel = false)
        {
            _templCtrl = NBrightBuyUtils.GetTemplateGetter("config");

            portallevel = !systemlevel;

            var menuplugin = _templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, portallevel);
            if (menuplugin != "")
            {
                Info = new NBrightInfo();
                Info.XMLData = menuplugin;
                _pluginList = new List<NBrightInfo>();
                _pluginList = GetPluginList();
            }
        }


        /// <summary>
        /// Search filesystem for any new plugins that have been added. Removed any deleted ones.
        /// </summary>
        public void UpdateSystemPlugins()
        {
            if (!portallevel) // only want to edit system level file
            {
                // remove delete plugins.
                var updated = false;
                foreach (var p in _pluginList)
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
                            updated = true;
                            _pluginList.Remove(p);
                        }
                    }
                }

                if (updated) Save();

                // Add new plugins
                updated = false;
                var pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath("/DesktopModules/NBright/NBrightBuy/Plugins");
                if (pluginfoldermappath != null && Directory.Exists(pluginfoldermappath))
                {
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
                                AddPlugin(nbi);
                                updated = true;
                                File.Delete(f);
                            }
                            catch (Exception)
                            {
                                // data might not be XML complient (ignore)
                            }
                        }
                    }
                    if (updated) Save();
                }

            }

        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                //save cart
                var strXml = "<plugin>";
                var lp = 0;
                foreach (var info in _pluginList)
                {
                    info.SetXmlProperty("genxml/hidden/index", lp.ToString(""), TypeCode.String, false);
                    info.SetXmlProperty("genxml/textbox/ctrl", info.GetXmlProperty("genxml/textbox/ctrl").Trim().ToLower());
                    strXml += info.XMLData;
                    lp += 1;
                }
                strXml += "</plugin>";
                Info.RemoveXmlNode("genxml/plugin");
                Info.AddXmlNode(strXml, "plugin", "genxml");
                _templCtrl.SaveTemplate("menuplugin.xml", Info.XMLData, portallevel);
            }
        }

        public void RemovePortalLevel()
        {
             _templCtrl.RemovePortalLevelTemplate("menuplugin.xml");
        }

        #region "base methods"

        /// <summary>
        /// Add Adddress
        /// </summary>
        /// <param name="rpData"></param>
        public String AddPlugin(Repeater rpData)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData);
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            AddPlugin(objInfoIn);
            return ""; // if everything is OK, don't send a message back.
        }

        public void AddNewPlugin()
        {
            var objInfoIn = new NBrightInfo(true);
            AddPlugin(objInfoIn,0);
        }

        public String AddPlugin(NBrightInfo pluginInfo, int index = -1)
        {
            // look to see if we already have the plugin
            var ctrl = pluginInfo.GetXmlProperty("genxml/textbox/ctrl");
            if (ctrl != "")
            {
                var ctrllist = from i in _pluginList where i.GetXmlProperty("genxml/textbox/ctrl") == ctrl select i;
                var nBrightInfos = ctrllist as IList<NBrightInfo> ?? ctrllist.ToList();
                if (nBrightInfos.Any())
                {
                    index = nBrightInfos.First().GetXmlPropertyInt("genxml/hidden/index");
                }
            }

            if (index == -1)
                _pluginList.Add(pluginInfo);
            else
                UpdatePlugin(pluginInfo.XMLData, index);

            return ""; // if everything is OK, don't send a message back.
        }

        public void RemovePlugin(int index)
        {
            _pluginList.RemoveAt(index);
        }

        public void UpdatePlugin(String xmlData, int index)
        {
            if (_pluginList.Count > index)
            {
                _pluginList[index].XMLData = xmlData;
            }
        }

        public void UpdatePlugin(Repeater rpData, int index)
        {
            if (_pluginList.Count > index)
            {
                var strXml = GenXmlFunctions.GetGenXml(rpData);
                UpdatePlugin(strXml, index);
            }
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetPluginList()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = Info.XMLDoc.SelectNodes("genxml/plugin/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                    newInfo.ItemID = rtnList.Count;
                    newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""));
                    rtnList.Add(newInfo);
                }
            }
            return rtnList;
        }

        public NBrightInfo GetShippingProviderDefault()
        {
            var l = GetShippingProviders();
            if (l.Count > 0) return l.First().Value;
            return null;
        }

        public Dictionary<String,NBrightInfo> GetShippingProviders(Boolean activeOnly = true)
        {
            return GetProviders("02", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetTaxProviders(Boolean activeOnly = true)
        {
            return GetProviders("03", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetPromoProviders(Boolean activeOnly = true)
        {
            return GetProviders("04", activeOnly);
        }
        public Dictionary<String, NBrightInfo> GetSchedulerProviders(Boolean activeOnly = true)
        {
            return GetProviders("05", activeOnly);
        }
        public Dictionary<String, NBrightInfo> GetEventsProviders(Boolean activeOnly = true)
        {
            return GetProviders("06", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetPaymentProviders(Boolean activeOnly = true)
        {
            return GetProviders("07", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetDiscountCodeProviders(Boolean activeOnly = true)
        {
            return GetProviders("08", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetFilterProviders(Boolean activeOnly = true)
        {
            return GetProviders("09", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetTemplateExtProviders(Boolean activeOnly = true)
        {
            return GetProviders("10", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetOtherProviders(Boolean activeOnly = true)
        {
            return GetProviders("99", activeOnly);
        }

        public NBrightInfo GetPaymentProviderDefault()
        {
            var l = GetPaymentProviders();
            if (l.Count > 0) return l.First().Value;
            return null;
        }


        private Dictionary<String, NBrightInfo> GetProviders(String providerType, Boolean activeOnly = true)
        {
            var l = GetPluginList();
            var pList = new Dictionary<String, NBrightInfo>();

            foreach (var p in l)
            {
                //UI Only;Shipping;Tax;Promotions;Scheduler;Events;Payments;Other
                //01;02;03;04;05;06;07;08
                if (p.GetXmlProperty("genxml/dropdownlist/providertype") == providerType && (p.GetXmlProperty("genxml/checkbox/active") == "True" || !activeOnly))
                {
                    var ctrlkey = p.GetXmlProperty("genxml/textbox/ctrl");
                    var lp = 1;
                    while (pList.ContainsKey(ctrlkey))
                    {
                        ctrlkey = p.GetXmlProperty("genxml/textbox/assembly") + lp.ToString("");
                        lp += 1;
                    }
                    pList.Add(ctrlkey, p);
                }
            }
            return pList;
        }


        public List<NBrightInfo> GetSubList(String groupname)
        {
            var rtnList = new List<NBrightInfo>();
            if (groupname != "")
            {
                var xmlNodeList = Info.XMLDoc.SelectNodes("genxml/plugin/*[./textbox/group='" + groupname + "']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode carNod in xmlNodeList)
                    {
                        var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                        newInfo.ItemID = rtnList.Count;
                        rtnList.Add(newInfo);
                    }
                }                
            }
            return rtnList;
        }

        public NBrightInfo GetPluginByCtrl(String ctrlname)
        {
            var p = new NBrightInfo(true);
            p.SetXmlProperty("genxml/checkbox/hidden", "True");
            var nod = Info.XMLDoc.SelectSingleNode("genxml/plugin/*[./textbox/ctrl='"+ ctrlname + "']");
            if (nod != null) p.XMLData = nod.OuterXml;
            return p;
        }

        public NBrightInfo GetPlugin(int index)
        {
            if (index < 0 || index >= _pluginList.Count) return null;
            return _pluginList[index];
        }


        #endregion



    }
}

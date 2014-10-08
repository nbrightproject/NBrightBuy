using System;
using System.Collections.Generic;
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
        public PluginData(int portalId)
        {
            _templCtrl = NBrightBuyUtils.GetTemplateGetter("config");

            // plugineditlevel 0=portallevel or 1=system.  This setting is set by the Plugin.ascx.cs
            var portallevel = !Convert.ToBoolean(StoreSettings.Current.GetInt("plugineditlevel"));

            var menuplugin = _templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(),true,true,portallevel);
            if (menuplugin != "")
            {
                Info = new NBrightInfo();
                Info.XMLData = menuplugin;
                _pluginList = new List<NBrightInfo>();
                _pluginList = GetPluginList();
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
                    info.SetXmlProperty("genxml/hidden/index", lp.ToString("D"), TypeCode.String, false);
                    info.SetXmlProperty("genxml/textbox/ctrl", info.GetXmlProperty("genxml/textbox/ctrl").Trim().ToLower());
                    strXml += info.XMLData;
                    lp += 1;
                }
                strXml += "</plugin>";
                Info.RemoveXmlNode("genxml/plugin");
                Info.AddXmlNode(strXml, "plugin", "genxml");
                // plugineditlevel 0=portallevel or 1=system.  This setting is set by the Plugin.ascx.cs
                var portallevel  = !Convert.ToBoolean(StoreSettings.Current.GetInt("plugineditlevel"));
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
            if (Utils.IsNumeric(pluginInfo.GetXmlProperty("genxml/hidden/index")))
            {
                if (pluginInfo.GetXmlProperty("genxml/hidden/index") == "") // no index, add the address
                {
                    if (index == -1)
                        _pluginList.Add(pluginInfo);
                    else
                        _pluginList.Insert(index,pluginInfo);
                }
                else
                {
                    var idx = Convert.ToInt32(pluginInfo.GetXmlProperty("genxml/hidden/index"));
                    UpdatePlugin(pluginInfo.XMLData, idx);
                }
            }
            else
            {
                if (index == -1)
                    _pluginList.Add(pluginInfo);
                else
                    _pluginList.Insert(index, pluginInfo);
            }
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

        public NBrightInfo GetPaymentProviderDefault()
        {
            var l = GetPaymentProviders();
            if (l.Count > 0) return l.First().Value;
            return null;
        }

        public Dictionary<String, NBrightInfo> GetPaymentProviders(Boolean activeOnly = true)
        {
            return GetProviders("07", activeOnly);
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
                        ctrlkey = p.GetXmlProperty("genxml/textbox/assembly") + lp.ToString("D");
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

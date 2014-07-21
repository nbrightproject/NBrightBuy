using System;
using System.Collections.Generic;
using System.Linq;
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
using NEvoWeb.Modules.NB_Store;

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

            var menuplugin = _templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture());
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
        private void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                //save cart
                var strXml = "<plugin>";
                foreach (var info in _pluginList)
                {
                    strXml += info.XMLData;
                }
                strXml += "</plugin>";
                Info.RemoveXmlNode("genxml/plugin");
                Info.AddXmlNode(strXml, "plugin", "genxml");
                _templCtrl.SaveTemplate("menuplugin.xml",Info.XMLData);
            }
        }

        #region "base methods"

        /// <summary>
        /// Add Adddress
        /// </summary>
        /// <param name="rpData"></param>
        public String AddPlugin(Repeater rpData)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            AddPlugin(objInfoIn);
            return ""; // if everything is OK, don't send a message back.
        }

        public String AddPlugin(NBrightInfo pluginInfo, int index = -1)
        {
            if (Utils.IsNumeric(pluginInfo.GetXmlProperty("genxml/hidden/index")))
            {
                if (pluginInfo.GetXmlProperty("genxml/hidden/index") == "-1") // index of -1, add the address
                {
                    if (index == -1)
                        _pluginList.Add(pluginInfo);
                    else
                        _pluginList.Insert(index,pluginInfo);
                    Save();
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
                Save();
            }
            return ""; // if everything is OK, don't send a message back.
        }

        public void RemovePlugin(int index)
        {
            _pluginList.RemoveAt(index);
            Save();
        }

        public void UpdatePlugin(String xmlData, int index)
        {
            if (_pluginList.Count > index)
            {
                _pluginList[index].XMLData = xmlData;
                Save();
            }
        }

        public void UpdatePlugin(Repeater rpData, int index)
        {
            if (_pluginList.Count > index)
            {
                var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);
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
            var p = new NBrightInfo();
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

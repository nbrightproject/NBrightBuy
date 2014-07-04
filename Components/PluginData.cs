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

        public PluginData(int portalId)
        {
            var modCtrl = new NBrightBuyController();
            Info = modCtrl.GetByType(portalId, -1, "PLUGINDATA");
            if (Info == null)
            {
                _pluginList = new List<NBrightInfo>();

                Info = new NBrightInfo();
                Info.ItemID = -1;
                Info.UserId = -1;
                Info.PortalId = portalId;
                Info.ModuleId = -1;
                Info.TypeCode = "PLUGINDATA";
                Info.XMLData = "<genxml></genxml>";

                // add default mennu items [TODO: make this param driven and localized]
                var strXml = "<genxml><files /><hidden><index></index></hidden><textbox><ctrl>{ctrl}</ctrl><name>{name}</name><path>{pluginpath}</path><help></help></textbox><checkbox /><dropdownlist /><checkboxlist /><radiobuttonlist /><edt /></genxml>";
                var newPlugin = strXml.Replace("{name}", "DashBoard").Replace("{pluginpath}", "").Replace("{ctrl}", "dashboard");
                var info = new NBrightInfo();
                info.XMLData = newPlugin; 
                AddPlugin(info);
                
                newPlugin = strXml.Replace("{name}", "Orders").Replace("{pluginpath}", "/DesktopModules/NBright/NBrightBuy/Admin/Orders.ascx").Replace("{ctrl}","orders");
                info = new NBrightInfo();
                info.XMLData = newPlugin;
                AddPlugin(info);

                newPlugin = strXml.Replace("{name}", "Clients").Replace("{pluginpath}", "/DesktopModules/Admin/Security/users.ascx").Replace("{ctrl}", "clients");
                info = new NBrightInfo();
                info.XMLData = newPlugin;
                AddPlugin(info);

                newPlugin = strXml.Replace("{name}", "Exit").Replace("{pluginpath}", "").Replace("{ctrl}", "");
                info = new NBrightInfo();
                info.XMLData = newPlugin;
                AddPlugin(info);

                
                Save(true);
            }

            _pluginList = GetPluginList();
        }


        /// <summary>
        /// Save cart
        /// </summary>
        private void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                //save cart
                var strXML = "<plugin>";
                foreach (var info in _pluginList)
                {
                    strXML += info.XMLData;
                }
                strXML += "</plugin>";
                Info.RemoveXmlNode("genxml/plugin");
                Info.AddXmlNode(strXML, "plugin", "genxml");

                var modCtrl = new NBrightBuyController();
                Info.ItemID = modCtrl.Update(Info);
                if (debugMode) Info.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_plugindata.xml");

            }
        }

        #region "base methods"

        /// <summary>
        /// Add Adddress
        /// </summary>
        /// <param name="rpData"></param>
        /// <param name="debugMode"></param>
        public String AddPlugin(Repeater rpData, Boolean debugMode = false)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            AddPlugin(objInfoIn);
            return ""; // if everything is OK, don't send a message back.
        }

        public String AddPlugin(NBrightInfo pluginInfo, Boolean debugMode = false)
        {
            // load into NBrigthInfo class, so it's easier to get at xml values.
            if (debugMode) pluginInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_pluginadd.xml");

            if (Utils.IsNumeric(pluginInfo.GetXmlProperty("genxml/hidden/index")))
            {
                if (pluginInfo.GetXmlProperty("genxml/hidden/index") == "-1") // index of -1, add the address
                {
                    _pluginList.Add(pluginInfo);
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
                _pluginList.Add(pluginInfo);
                Save(debugMode);
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
                    newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""));
                    rtnList.Add(newInfo);
                }
            }
            return rtnList;
        }

        public NBrightInfo GetPlugin(int index)
        {
            if (index < 0 || index >= _pluginList.Count) return null;
            return _pluginList[index];
        }


        #endregion



    }
}

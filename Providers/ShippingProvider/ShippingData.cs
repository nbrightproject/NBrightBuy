using System;
using System.Collections.Generic;
using System.Dynamic;
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
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ShippingData
    {
        private List<NBrightInfo> _shippingList;
        public NBrightInfo  Info;

        public ShippingData()
        {
            PopulateData();
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
                //save cart
                var strXML = "<list>";
                var lp = 0;
                foreach (var info in _shippingList)
                {
                    info.SetXmlProperty("genxml/hidden/index",lp.ToString("D"));
                    strXML += info.XMLData;
                    lp += 1;
                }
                strXML += "</list>";
                Info.RemoveXmlNode("genxml/list");
                Info.AddXmlNode(strXML, "list", "genxml");
                if (Info != null)
                {
                    var modCtrl = new NBrightBuyController();
                    Info.ItemID = modCtrl.Update(Info);
                }
        }

        #region "properties"

        public String CalculationUnit
        {
            get
            {
                return Info.GetXmlProperty("genxml/radiobuttonlist/rblunit");
            }
        }

        #endregion

        #region "base methods"

        public void Update(Repeater rpData)
        {
            Info.XMLData = GenXmlFunctions.GetGenXml(rpData);
        }

        public String AddNewRule()
        {
            var ruleInfo = new NBrightInfo(true);
            ruleInfo.ItemID = -1;
            ruleInfo.SetXmlProperty("genxml/hidden/index","-1");
            return UpdateRule(ruleInfo);
        }

        public String UpdateRule(RepeaterItem rpItem, Boolean debugMode = false)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpItem);
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            UpdateRule(objInfoIn, debugMode);
            return ""; // if everything is OK, don't send a message back.
        }

        public String UpdateRule(NBrightInfo ruleInfo, Boolean debugMode = false)
        {
            var addIndex = ruleInfo.GetXmlProperty("genxml/hidden/index");
            if (!Utils.IsNumeric(addIndex)) addIndex = "-1"; // assume new .
            var ruleIndex = Convert.ToInt32(addIndex);
            if (debugMode) ruleInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_ruleadd.xml");
            if (ruleIndex >= 0)
                {
                    UpdateRule(ruleInfo.XMLData, ruleIndex);
                }
                else
                {
                    _shippingList.Add(ruleInfo);
                }
            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveRule(int index)
        {
            _shippingList.RemoveAt(index);
        }

        private void UpdateRule(String xmlData, int index)
        {
            if (_shippingList.Count > index)
            {
                _shippingList[index].XMLData = xmlData;
            }
        }

        public void UpdateRule(Repeater rpData)
        {
            foreach (RepeaterItem i in rpData.Items)
            {
                var strXml = GenXmlFunctions.GetGenXml(i);
                var nbi = new NBrightInfo(true);
                nbi.XMLData = strXml;
                if (nbi.GetXmlPropertyBool("genxml/hidden/isdirty"))
                {
                    UpdateRule(i);                    
                }
            }
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetRuleList()
        {
            var rtnList = new List<NBrightInfo>();
                var xmlNodeList = Info.XMLDoc.SelectNodes("genxml/list/*");
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

        public NBrightInfo GetRule(int index)
        {
            if (index < 0 || index >= _shippingList.Count) return null;
            return _shippingList[index];
        }

        public Double CalculateShipping(String countryRef, String regionRef, Double rangeValue, Double total)
        {
            // calc if we have free shipping limit
            var freeShipAmt = Info.GetXmlPropertyDouble("genxml/textbox/freeshiplimit");
            var freeShipRefs = Info.GetXmlProperty("genxml/textbox/freeshipcountrycodes");
            if (total >= freeShipAmt && ("," + freeShipRefs + ",").Contains("," + countryRef + ",")) return 0;

            // calc range date

            return 1;
        }

        #endregion

        #region "private functions"

        private void PopulateData()
        {
            var modCtrl = new NBrightBuyController();
            Info = modCtrl.GetByGuidKey(PortalSettings.Current.PortalId,-1,"SHIPPING","NBSdefault");
            if (Info == null)
            {
                Info = new NBrightInfo(true);
                Info.GUIDKey = "NBSdefault";
                Info.TypeCode = "SHIPPING";
                Info.ModuleId = -1;
                Info.PortalId = PortalSettings.Current.PortalId;
            }
            _shippingList = GetRuleList();
        }

        #endregion


    }
}

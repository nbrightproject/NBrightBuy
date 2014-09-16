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
        private void Save(Boolean debugMode = false)
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
                    Exists = true;
                }
        }

        #region "base methods"

        /// <summary>
        /// Add Adddress
        /// </summary>
        /// <param name="rpData"></param>
        /// <param name="debugMode"></param>
        public String AddAddress(Repeater rpData, Boolean debugMode = false)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", "");
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            var addIndex = objInfoIn.GetXmlProperty("genxml/hidden/index"); // addresses updated from maager should have a index hidden field.
            if (addIndex == "") addIndex = objInfoIn.GetXmlProperty("genxml/dropdownlist/selectaddress"); // updated from cart should have a selected address
            if (!Utils.IsNumeric(addIndex)) addIndex = "-1"; // assume new address.
            var addressIndex = Convert.ToInt32(addIndex);
            AddAddress(objInfoIn,addressIndex);
            return ""; // if everything is OK, don't send a message back.
        }

        public String AddAddress(NBrightInfo addressInfo, int addressIndex, Boolean debugMode = false)
        {
            if (debugMode) addressInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addressadd.xml");
                if (addressIndex >= 0)
                {
                    UpdateAddress(addressInfo.XMLData, addressIndex);
                }
                else
                {
                    _shippingList.Add(addressInfo);
                    Save(debugMode);
                }
            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveAddress(int index)
        {
            _shippingList.RemoveAt(index);
            Save();
        }

        public void UpdateAddress(String xmlData, int index)
        {
            if (_shippingList.Count > index)
            {
                _shippingList[index].XMLData = xmlData;
                Save();
            }
        }

        public void UpdateAddress(Repeater rpData, int index)
        {
            if (_shippingList.Count > index)
            {
                var strXml = GenXmlFunctions.GetGenXml(rpData);
                UpdateAddress(strXml, index);
            }
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetAddressList()
        {
            var rtnList = new List<NBrightInfo>();
                var xmlNodeList = Info.XMLDoc.SelectNodes("genxml/address/*");
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

        public NBrightInfo GetAddress(int index)
        {
            if (index < 0 || index >= _shippingList.Count) return null;
            return _shippingList[index];
        }

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }

        #endregion

        #region "private functions"

        private void PopulateData()
        {
            Exists = false;
            _shippingList = GetAddressList();
            Save();
        }

        #endregion


    }
}

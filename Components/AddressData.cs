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
    public class AddressData
    {
        private List<NBrightInfo> _addressList;
        private UserData _uData;

        public AddressData()
        {
            Exists = false;
            _uData = new UserData();
            _addressList = GetAddressList();
        }


        /// <summary>
        /// Save cart
        /// </summary>
        private void Save(Boolean debugMode = false)
        {
            if (_uData.Exists)
            {
                //save cart
                var strXML = "<address>";
                foreach (var info in _addressList)
                {
                    strXML += info.XMLData;
                }
                strXML += "</address>";
                _uData.Info.RemoveXmlNode("genxml/address");
                _uData.Info.AddXmlNode(strXML, "address", "genxml");
                _uData.Save(debugMode);

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

            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);

            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            if (debugMode) objInfoIn.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addressadd.xml");

            _addressList.Add(objInfoIn);
            Save(debugMode);

            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveAddress(int index)
        {
            _addressList.RemoveAt(index);
            Save();
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetAddressList()
        {
            var rtnList = new List<NBrightInfo>();
            if (_uData.Exists)
            {
                var xmlNodeList = _uData.Info.XMLDoc.SelectNodes("genxml/address/*");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode carNod in xmlNodeList)
                    {
                        var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                        rtnList.Add(newInfo);
                    }
                }                
            }
            return rtnList;
        }

        public NBrightInfo GetAddress(int index)
        {
            return _addressList[index];
        }

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }


        #endregion


    }
}

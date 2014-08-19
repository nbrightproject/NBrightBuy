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

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class AddressData
    {
        private List<NBrightInfo> _addressList;
        public UserData UserData;

        public AddressData()
        {
            PopulateData("");
        }

        public AddressData(String userId)
        {
            PopulateData(userId);
        }


        /// <summary>
        /// Save cart
        /// </summary>
        private void Save(Boolean debugMode = false)
        {
            if (UserData.Exists)
            {
                //save cart
                var strXML = "<address>";
                foreach (var info in _addressList)
                {
                    strXML += info.XMLData;
                }
                strXML += "</address>";
                UserData.Info.RemoveXmlNode("genxml/address");
                UserData.Info.AddXmlNode(strXML, "address", "genxml");
                UserData.Save(debugMode);

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
            AddAddress(objInfoIn);
            return ""; // if everything is OK, don't send a message back.
        }

        public String AddAddress(NBrightInfo addressInfo, Boolean debugMode = false)
        {
            // load into NBrigthInfo class, so it's easier to get at xml values.
            if (debugMode) addressInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addressadd.xml");
            var addrExists = AddressExists(addressInfo);

            if (addressInfo.GetXmlPropertyBool("genxml/hidden/default"))
            {
                // if we have a default DNN address, we don;t want to ONLY update it from here.
                //NOTE: Email address is not part of the Profile
                var strIdx = addressInfo.GetXmlProperty("genxml/hidden/index");
                if (Utils.IsNumeric(strIdx))
                {
                    var idx = Convert.ToInt32(strIdx);
                    UpdateAddress(addressInfo.XMLData, idx);
                }
                else
                {
                    _addressList.Add(addressInfo);
                    Save(debugMode);                    
                }
            }
            else
            {
                if (Utils.IsNumeric(addressInfo.GetXmlProperty("genxml/hidden/index")))
                {
                    if (addressInfo.GetXmlProperty("genxml/hidden/index") == "-1") // index of -1, add the address
                    {
                        _addressList.Add(addressInfo);
                        Save();
                    }
                    else
                    {
                        var idx = Convert.ToInt32(addressInfo.GetXmlProperty("genxml/hidden/index"));
                        UpdateAddress(addressInfo.XMLData, idx);
                    }
                }
                else
                {
                    _addressList.Add(addressInfo);
                    Save(debugMode);
                }
            }
            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveAddress(int index)
        {
            _addressList.RemoveAt(index);
            Save();
        }

        public void UpdateAddress(String xmlData, int index)
        {
            if (_addressList.Count > index)
            {
                _addressList[index].XMLData = xmlData;
                Save();
                UpdateDnnProfile(_addressList[index]);
            }
        }

        public void UpdateAddress(Repeater rpData, int index)
        {
            if (_addressList.Count > index)
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
            if (UserData.Exists)
            {
                var xmlNodeList = UserData.Info.XMLDoc.SelectNodes("genxml/address/*");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode carNod in xmlNodeList)
                    {
                        var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                        newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""));
                        rtnList.Add(newInfo);
                    }
                }
            }
            return rtnList;
        }

        public NBrightInfo GetAddress(int index)
        {
            if (index < 0 || index >= _addressList.Count) return null;
            return _addressList[index];
        }

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }


        public NBrightInfo GetDefaultAddress()
        {
            NBrightInfo aInfo = null; 
            if (UserData.Exists)
            {
                var xmlNodeList = UserData.Info.XMLDoc.SelectNodes("genxml/address/*[./hidden/default='True']");
                if (xmlNodeList != null && xmlNodeList.Count > 0)
                {
                    aInfo = new NBrightInfo { XMLData = xmlNodeList[0].OuterXml };
                }
            }
            return aInfo;
        }


        #endregion

        #region "private functions"

        private Boolean AddressExists(NBrightInfo nInfo)
        {
            var newAddr = GetCompareAddress(nInfo);
            //we don;t know the index, so search for it, if not found then create a new one. If found ignore
            var found = false;
            foreach (var a in _addressList)
            {
                var addr = GetCompareAddress(a);
                if (addr == newAddr)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private String GetCompareAddress(NBrightInfo nInfo)
        {
            var newAddr = "";
            var nodlist = nInfo.XMLDoc.SelectNodes("genxml/textbox/*");
            if (nodlist != null)
                foreach (XmlNode n in nodlist)
                {
                    newAddr += n.InnerText.Replace(" ", "").ToLower();
                }
            return newAddr;
        }

        private void UpdateDnnProfile(NBrightInfo defaultAddr)
        {
            if (defaultAddr.GetXmlProperty("genxml/hidden/default") == "True")
            {
                var flag = false;
                var prop1 = DnnUtils.GetUserProfileProperties(UserData.Info.UserId.ToString(""));
                var prop2 = DnnUtils.GetUserProfileProperties(UserData.Info.UserId.ToString(""));
                foreach (var p in prop1)
                {
                    var n = defaultAddr.XMLDoc.SelectSingleNode("genxml/textbox/" + p.Key.ToLower());
                    if (n != null)
                    {
                        prop2[p.Key] = n.InnerText;
                        flag = true;
                    }
                }
                if (flag) DnnUtils.SetUserProfileProperties(UserData.Info.UserId.ToString(""), prop2);
            }
        }

        private void PopulateData(String userId)
        {
            Exists = false;
            UserData = new UserData(userId);
            _addressList = GetAddressList();
            //if we have no address create a default one from DNN profile
            if (GetDefaultAddress() == null && UserData.Exists)
            {
                var newDefault = new NBrightInfo(true);
                newDefault.AddSingleNode("default", "True", "genxml/hidden");
                newDefault.SetXmlProperty("genxml/hidden/index", _addressList.Count.ToString(""));
                var prop = DnnUtils.GetUserProfileProperties(UserData.Info.UserId.ToString(""));
                foreach (var p in prop)
                {
                    newDefault.SetXmlProperty("genxml/textbox/" + p.Key.ToLower(), p.Value);
                }
                _addressList.Add(newDefault);
                Save();
            }
            else
            {
                UpdateDefaultProfileAddress(); //alway update default address to profile, to keep it in-line.
            }
        }

        private void UpdateDefaultProfileAddress()
        {
            var da = GetDefaultAddress();
            if (da != null)
            {
                var prop = DnnUtils.GetUserProfileProperties(UserData.Info.UserId.ToString(""));
                foreach (var p in prop)
                {
                    da.SetXmlProperty("genxml/textbox/" + p.Key.ToLower(), p.Value);
                }
                AddAddress(da);
            }
        }

        #endregion


    }
}

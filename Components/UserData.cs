using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class UserData
    {
        public NBrightInfo Info;
        private UserInfo _userInfo;
        private Dictionary<string,NBrightInfo> _docList;

        public UserData()
        {
            PopulateData("");
        }

        public UserData(String userId)
        {
            PopulateData(userId);
        }

        public void AddNewPurchasedDoc(string key, string downloadfilename, string filerelpath, string filename)
        {
            var strXml = "<genxml><downloadfilename>" + downloadfilename + "</downloadfilename><filerelpath>" + filerelpath + "</filerelpath><filename>" + filename + "</filename></genxml>";
            var nbi = new NBrightInfo();
            nbi.GUIDKey = key;
            nbi.XMLData = strXml;
            _docList.Add(key,nbi);
        }

        public void RemovePurchasedDoc(string key)
        {
            if (_docList != null && _docList.ContainsKey(key)) _docList.Remove(key);
        }
        public bool HasPurchasedDoc(string key)
        {
            if (_docList != null && _docList.ContainsKey(key)) return true;
            return false;
        }
        public string GetPurchasedFileName(string key)
        {
            if (_docList != null && _docList.ContainsKey(key))
            {
                return _docList[key].GetXmlProperty("genxml/filename");
            }
            return "";
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                Info.SetXmlProperty("genxml/purchaseddocs","");
                var strDocs = "<docs>";
                foreach (var d in _docList)
                {
                    var nbi = d.Value;
                    if (nbi.XMLData != "") strDocs += nbi.XMLData;
                }
                strDocs += "</docs>";
                Info.SetXmlProperty("genxml/purchaseddocs", strDocs);

                var modCtrl = new NBrightBuyController();
                Info.ItemID = modCtrl.Update(Info);
                if (StoreSettings.Current.DebugModeFileOut) Info.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_userdata.xml");
                Exists = true;
            }
        }

        public void DeleteUserData()
        {
            //remove DB record
            var modCtrl = new NBrightBuyController();
            modCtrl.Delete(Info.ItemID);
            Exists = false;
        }

        /// <summary>
        /// Get NBright UserData
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetUserData()
        {
            return Info;
        }

        /// <summary>
        /// Set to true if usedata exists
        /// </summary>
        public bool Exists { get; private set; }

        public void UpdateEmail(String email)
        {
            if (_userInfo != null && Utils.IsEmail(email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public String GetEmail()
        {
            if (_userInfo != null)
            {
                return _userInfo.Email;
            }
            return "";
        }


        private void PopulateData(String userId)
        {
            _docList = new Dictionary<string, NBrightInfo>();
            Exists = false;
            if (Utils.IsNumeric(userId))
                _userInfo = UserController.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(userId));
            else
                _userInfo = UserController.Instance.GetCurrentUserInfo();

            if (_userInfo != null && _userInfo.UserID != -1) // only create userdata if we have a user logged in.
            {
                var modCtrl = new NBrightBuyController();
                Info = modCtrl.GetByType(_userInfo.PortalID, -1, "USERDATA", _userInfo.UserID.ToString(""));
                if (Info == null && _userInfo.UserID != -1)
                {
                    Info = new NBrightInfo();
                    Info.ItemID = -1;
                    Info.UserId = _userInfo.UserID;
                    Info.PortalId = _userInfo.PortalID;
                    Info.ModuleId = -1;
                    Info.TypeCode = "USERDATA";
                    Info.XMLData = "<genxml></genxml>";
                    Save();
                }
                else
                    Exists = true;

                var nodlist = Info.XMLDoc.SelectNodes("genxml/purchaseddocs/docs");
                if (nodlist != null)
                {
                    foreach (XmlNode nod in nodlist)
                    {
                        var nbi = new NBrightInfo();
                        nbi.XMLData = nod.OuterXml;
                        _docList.Add(nbi.GUIDKey, nbi);
                    }
                }

            }
        }

    }
}

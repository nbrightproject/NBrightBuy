using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ClientData
    {
        private NBrightInfo _clientInfo;
        public NBrightInfo DataRecord;
        public int PortalId;
        private UserInfo _userInfo;

        public List<NBrightInfo> DiscountCodes;

        public ClientData(int portalId, int userid)
        {
            PortalId = portalId;
            PopulateClientData(userid);
        }


        #region "base methods"


        /// <summary>
        /// Get Client Cart
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetInfo()
        {
            return _clientInfo;
        }

        public void AddClientRole(ModSettings modSettings)
        {
            if (_userInfo != null)
            {
                if (!_userInfo.IsInRole("Client"))
                {
                    var rc = new DotNetNuke.Security.Roles.RoleController();
                    var ri = rc.GetRoleByName(PortalId, "Client");
                    if (ri != null) rc.AddUserRole(PortalId, _userInfo.UserID, ri.RoleID, Null.NullDate);
                    if (StoreSettings.Current.Get("sendclientroleemail") == "True") NBrightBuyUtils.SendEmail(_userInfo.Email, "addclientrole.html", _clientInfo, "", "", _userInfo.Profile.PreferredLocale);
                }
            }
        }

        public void ResetPassword()
        {
            if (_userInfo != null)
            {
                UserController.ResetPassword(_userInfo, "");
                _userInfo.Membership.UpdatePassword = true;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
                DotNetNuke.Services.Mail.Mail.SendMail(_userInfo, DotNetNuke.Services.Mail.MessageType.PasswordReminder, (PortalSettings)HttpContext.Current.Items["PortalSettings"]);
            }
        }

        public void UpdateEmail(String email)
        {
            // update email
            if (_userInfo != null && Utils.IsEmail(email) && (_userInfo.Email != email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public void Update(Repeater rpData)
        {
            // update email
            var email = GenXmlFunctions.GetField(rpData, "email");
            if (_userInfo != null && Utils.IsEmail(email) && (_userInfo.Email != email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public void UnlockUser()
        {
            if (_userInfo != null) UserController.UnLockUser(_userInfo);
        }

        public void AuthoriseClient()
        {
            if (_userInfo != null)
            {
                _userInfo.Membership.Approved = true;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public void OutputDebugFile(String fileName)
        {
            _clientInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + fileName);
        }

        public void UpdateDiscountCodes(String xmlAjaxData)
        {
            var modelList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");

            // build xml for data records
            var strXml = "<genxml><discountcodes>";
            foreach (var modelInfo in modelList)
            {
                var objInfo = new NBrightInfo(true);

                var fields = modelInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    objInfo.SetXmlProperty(f, modelInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</discountcodes></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/models", "genxml");
        }

        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            objCtrl.Update(DataRecord);
        }

        public void AddNewDiscountCode()
        {
            var strXml = "<genxml><discountcodes><genxml><textbox><coderef>" + Utils.GetUniqueKey() + "</coderef></textbox></genxml></discountcodes></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/discountcodes") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/discountcodes", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/discountcodes/genxml", "genxml/discountcodes");
            }
        }


    #endregion

        #region "private methods/functions"

        private void PopulateClientData(int userId)
        {
            _clientInfo = new NBrightInfo(true);
            _clientInfo.ItemID = userId;
            _clientInfo.PortalId = PortalId;

            _userInfo = UserController.GetUserById(PortalId, userId);

            _clientInfo.ModifiedDate = _userInfo.Membership.CreatedDate;

            foreach (var propertyInfo in _userInfo.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var pv = propertyInfo.GetValue(_userInfo, null);
                    _clientInfo.SetXmlProperty("genxml/textbox/" + propertyInfo.Name.ToLower(), pv.ToString());
                }
            }

            foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition p in _userInfo.Profile.ProfileProperties)
            {
                _clientInfo.SetXmlProperty("genxml/textbox/" + p.PropertyName.ToLower(), p.PropertyValue);
            }

            _clientInfo.AddSingleNode("membership","","genxml");
            foreach (var propertyInfo in _userInfo.Membership.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var pv = propertyInfo.GetValue(_userInfo.Membership, null);
                    if (pv != null) _clientInfo.SetXmlProperty("genxml/membership/" + propertyInfo.Name.ToLower(), pv.ToString());
                }
            }


            var objCtrl = new NBrightBuyController();
            DataRecord = objCtrl.GetByType(PortalId, -1, "CLIENT", _userInfo.UserID.ToString(""));
            if (DataRecord == null)
            {
                DataRecord = new NBrightInfo(true);
                DataRecord.ItemID = -1;
                DataRecord.UserId = _userInfo.UserID;
                DataRecord.PortalId = PortalId;
                DataRecord.ModuleId = -1;                
                DataRecord.TypeCode = "CLIENT";
            }
            DiscountCodes = GetEntityList("discountcodes");


        }

        private List<NBrightInfo> GetEntityList(String entityName)
        {
            var l = new List<NBrightInfo>();
            if (DataRecord != null)
            {
                var xmlNodList = DataRecord.XMLDoc.SelectNodes("genxml/" + entityName + "/*");
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    var lp = 1;
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        obj.ItemID = lp;
                        obj.Lang = DataRecord.Lang;
                        obj.ParentItemId = DataRecord.ItemID;
                        l.Add(obj);
                        lp += 1;
                    }
                }
            }
            return l;
        }


        #endregion


    }
}

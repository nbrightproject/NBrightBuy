using System;
using System.Collections.Generic;
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

        public UserData()
        {
            PopulateData("");
        }

        public UserData(String userId)
        {
            PopulateData(userId);
        }

        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                var modCtrl = new NBrightBuyController();
                Info.ItemID = modCtrl.Update(Info);
                if (debugMode) Info.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_userdata.xml");
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
            Exists = false;
            if (Utils.IsNumeric(userId))
                _userInfo = UserController.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(userId));
            else
                _userInfo = UserController.GetCurrentUserInfo();

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
            }
        }

    }
}

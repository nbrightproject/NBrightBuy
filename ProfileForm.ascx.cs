// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ProfileForm : NBrightBuyBase
    {

        private String _templinp = "";
        private ProfileData _profileData;
        private const string NotifyRef = "profileupdated";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _profileData = new ProfileData();

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpInp.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {
                _templinp = ModSettings.Get("txtinputform");

                // Get Display
                var rpInpTempl = ModCtrl.GetTemplateData(ModSettings, _templinp, Utils.GetCurrentCulture(), DebugMode);
                rpInp.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpInpTempl, ModSettings.Settings(), PortalSettings.HomeDirectory); 


            }
            catch (Exception exc)
            {
                rpInp.ItemTemplate = new GenXmlTemplate(exc.Message, ModSettings.Settings());
                // catch any error and allow processing to continue, output error as footer template.
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            var objprof = _profileData.GetProfile();
            if (objprof == null) objprof = new NBrightInfo(true); //assume new address
            base.DoDetail(rpInp, objprof);

        }

        #endregion


        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];
            var redirecttabid = "";
            var emailtemplate = "";

            switch (e.CommandName.ToLower())
            {
                case "saveprofile":
                    _profileData.UpdateProfile(rpInp, DebugMode);

                    emailtemplate = ModSettings.Get("emailtemplate");
                    NBrightBuyUtils.SendEmailToManager(emailtemplate, _profileData.GetProfile(), "profileupdated_emailsubject.Text");

                    param[0] = "msg=" + NotifyRef + "_" + NotifyCode.ok;
                    if (!UserInfo.IsInRole("Client") && ModSettings.Get("clientrole") == "True")
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef, NotifyCode.ok);
                    else
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + "clientrole", NotifyCode.ok);

                    redirecttabid = ModSettings.Get("ddlredirecttabid");
                    if (!Utils.IsNumeric(redirecttabid)) redirecttabid = TabId.ToString("");
                    Response.Redirect(Globals.NavigateURL(Convert.ToInt32(redirecttabid), "", param), true);
                    break;
                case "register":

                    var notifyCode = CreateUser(); //create a new user and login
                    if (notifyCode == NotifyCode.ok)
                    {
                        var objUserInfo = UserController.GetCurrentUserInfo();
                        _profileData = new ProfileData(objUserInfo.UserID, rpInp, DebugMode); //craete and update a profile for this new logged in user.
                        emailtemplate = ModSettings.Get("emailtemplate");
                        NBrightBuyUtils.SendEmailToManager(emailtemplate, _profileData.GetProfile(), "profileupdated_emailsubject.Text");
                    }


                    param[0] = "msg=" + NotifyRef + "_" + notifyCode;
                    if (!UserInfo.IsInRole("Client") && ModSettings.Get("clientrole") == "True")
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef, notifyCode);
                    else
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + "clientrole", notifyCode);

                    if (notifyCode == NotifyCode.ok) redirecttabid = ModSettings.Get("ddlredirecttabid");
                    if (!Utils.IsNumeric(redirecttabid)) redirecttabid = TabId.ToString("");
                    Response.Redirect(Globals.NavigateURL(Convert.ToInt32(redirecttabid), "", param), true);
                    break;
            }

        }

        #endregion

        private NotifyCode CreateUser()
        {

            if (!this.UserInfo.IsInRole("Registered Users"))
            {
                // Create and hydrate User
                var objUser = new UserInfo();
                objUser.Profile.InitialiseProfile(this.PortalId, true);
                objUser.PortalID = PortalId;
                objUser.DisplayName = GenXmlFunctions.GetField(rpInp, "DisplayName"); 
                objUser.Email = GenXmlFunctions.GetField(rpInp,"Email");
                objUser.FirstName = GenXmlFunctions.GetField(rpInp, "FirstName"); 
                objUser.LastName = GenXmlFunctions.GetField(rpInp, "LastName");
                objUser.Username = GenXmlFunctions.GetField(rpInp, "Username");
                if (objUser.Username == "") objUser.Username = GenXmlFunctions.GetField(rpInp, "Email");                
                objUser.Membership.CreatedDate = System.DateTime.Now;
                objUser.Membership.Password = DotNetNuke.Entities.Users.UserController.GeneratePassword(9);
                objUser.Membership.UpdatePassword = true;
                objUser.Membership.Approved = PortalSettings.UserRegistration == (int) Globals.PortalRegistrationType.PublicRegistration;

                // Create the user
                var createStatus = UserController.CreateUser(ref objUser);

                DataCache.ClearPortalCache(PortalId, true);
                bool boNotify = false;
                switch (createStatus)
                {
                    case UserCreateStatus.Success:
                        //boNotify = true;
                        if (objUser.Membership.Approved) UserController.UserLogin(this.PortalId, objUser,PortalSettings.PortalName, AuthenticationLoginBase.GetIPAddress(),false);
                        return NotifyCode.ok;
                    case UserCreateStatus.DuplicateEmail:
                    case UserCreateStatus.DuplicateUserName:
                    case UserCreateStatus.UsernameAlreadyExists:
                    case UserCreateStatus.UserAlreadyRegistered:
                    default:
                        // registration error
                        boNotify = false;
                        break;
                }

            }

            return NotifyCode.fail;
                 
        }


    }

}

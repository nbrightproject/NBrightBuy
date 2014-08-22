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
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;


namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class EditLanguage : NBrightBuyBase
    {
        private String _entryid = "";

        public event AfterChangeEventHandler AfterChange;
        public delegate void AfterChangeEventHandler(object source, RepeaterCommandEventArgs e, string previousEditLang);
        public event BeforeChangeEventHandler BeforeChange;
        public delegate void BeforeChangeEventHandler(object source, RepeaterCommandEventArgs e, string newEditLang);
        public event ItemCommandEventHandler ItemCommand;
        public delegate void ItemCommandEventHandler(object source, RepeaterCommandEventArgs e);


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _entryid = Utils.RequestParam(Context, "eid");

            // Get Display Body
            var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, "selecteditlanguage.html", Utils.GetCurrentCulture(), DebugMode);
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {

                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    var enabledlanguages = LocaleController.Instance.GetLocales(PortalId);
                    var dispList = new List<Locale>();
                    foreach (var l in enabledlanguages)
                    {
                        dispList.Add(l.Value);
                    }
                    rpData.DataSource = dispList;
                    rpData.DataBind();
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


        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "selectlang":
                    if (_entryid != "") param[0] = "eid=" + _entryid;
                    if (BeforeChange != null) BeforeChange(source, e, StoreSettings.Current.EditLanguage);
                    StoreSettings.Current.EditLanguage = cArg;
                    if (AfterChange != null) AfterChange(source, e, StoreSettings.Current.EditLanguage);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                default:
                    if (ItemCommand != null)
                    {
                        ItemCommand(source, e);
                    }
                    break;

            }

        }

        #endregion


    }

}

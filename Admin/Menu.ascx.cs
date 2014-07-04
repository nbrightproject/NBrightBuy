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
using System.Web.UI.WebControls;
using DotNetNuke.Common;
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
    public partial class Menu : NBrightBuyBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {

 
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

        } 


        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    var rpDataTemplH = ModCtrl.GetTemplateData(ModSettings, "menuheader.html", Utils.GetCurrentCulture(), DebugMode);
                    var l = new Literal();
                    l.Text = rpDataTemplH;
                    phMenuH.Controls.Add(l);

                    l = new Literal();
                    l.Text = GetMenu();
                    phMenuF.Controls.Add(l);

                    var rpDataTemplF = ModCtrl.GetTemplateData(ModSettings, "menufooter.html", Utils.GetCurrentCulture(), DebugMode);
                    l = new Literal();
                    l.Text = rpDataTemplF;
                    phMenuF.Controls.Add(l);

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

        private String GetMenu()
        {
            var strOut = "<ul>";
            var pluginData = new PluginData(PortalId);
            foreach (var p in pluginData.GetPluginList())
            {
                strOut += "<li>";
                var ctrl = p.GetXmlProperty("genxml/textbox/ctrl");
                if (ctrl == "")
                {
                    strOut += "<a href='/'>" + p.GetXmlProperty("genxml/textbox/name") + "</a>";
                }
                else
                {
                    var param = new string[3];
                    param[0] = "ctrl=" + ctrl;
                    strOut += "<a href='" + Globals.NavigateURL(TabId, "", param) + "'>" + p.GetXmlProperty("genxml/textbox/name") + "</a>";
                }
                strOut += "</li>";
            }

            strOut += "<ul>";
            return strOut;
        }


        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "link":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


    }

}

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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;


namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Container : DotNetNuke.Entities.Modules.PortalModuleBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                if (UserId > 0) //do nothing if user not logged on
                {

                    var ctrl = Utils.RequestQueryStringParam(Context, "ctrl");

                    // anyone only in the client role is only allowed in the products control
                    if (UserInfo.IsInRole(StoreSettings.ClientRole) && (!UserInfo.IsInRole(StoreSettings.EditorRole) && !UserInfo.IsInRole(StoreSettings.ManagerRole) && !UserInfo.IsInRole("Administrators")))
                    {
                        ctrl = "products";
                    }

                    if (ctrl == "")
                        ctrl = (String) HttpContext.Current.Session["nbrightbackofficectrl"];
                    else
                        HttpContext.Current.Session["nbrightbackofficectrl"] = ctrl;

                    if (String.IsNullOrEmpty(ctrl))
                    {
                        var plugins = new PluginData(PortalId);
                        var p = plugins.GetPlugin(0);
                        if (p != null) ctrl = p.GUIDKey;
                        if (ctrl=="") ctrl = "orders";
                        if (StoreSettings.Current.Settings().Count == 0) ctrl = "settings";
                        HttpContext.Current.Session["nbrightbackofficectrl"] = ctrl;
                    }

                    var ctlpath = GetControlPath(ctrl);
                    if (ctlpath == "")  // ctrl may not exist in system, so default to products
                    {
                        ctrl = "products";
                        ctlpath = GetControlPath(ctrl);                        
                    }

                    // check for group data, this MUST be there otherwise this is the first time into the BO, so redirect to Admin.
                    var l = NBrightBuyUtils.GetCategoryGroups(Utils.GetCurrentCulture());
                    if (!l.Any())
                    {
                        ctrl = "settings";
                        ctlpath = GetControlPath(ctrl);
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, "settingssetup", NotifyCode.fail);
                    }

                    if (ctlpath != "")
                    {
                        // make compatible with running DNN in virtual directory
                        if (HttpContext.Current.Request.ApplicationPath != null && !ctlpath.StartsWith(HttpContext.Current.Request.ApplicationPath)) ctlpath = HttpContext.Current.Request.ApplicationPath + ctlpath;
                        var c1 = LoadControl(ctlpath);
                        phData.Controls.Add(c1);
                    }
                    else
                    {
                        var c1 = new Literal();
                        c1.Text = "INVALID CONTROL: " + ctrl;
                        phData.Controls.Add(c1);
                    }
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

        private String GetControlPath(String ctrl)
        {
            var pluginData = new PluginData(PortalId);
            var p = pluginData.GetPluginByCtrl(ctrl);
            return p.GetXmlProperty("genxml/textbox/path");
        }

    }

}

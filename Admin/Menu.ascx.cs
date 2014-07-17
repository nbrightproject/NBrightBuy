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
    public partial class Menu : NBrightBuyAdminBase
    {

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (UserId > 0) //do nothing if user not logged on
                {
                    base.OnLoad(e);
                    if (Page.IsPostBack == false)
                    {
                        var rpDataTemplH = ModCtrl.GetTemplateData(ModSettings, "menuheader.html",
                            Utils.GetCurrentCulture(), DebugMode);
                        var l = new Literal();
                        l.Text = rpDataTemplH;
                        phMenuH.Controls.Add(l);

                        l = new Literal();
                        l.Text = GetMenu();
                        phMenuF.Controls.Add(l);

                        var rpDataTemplF = ModCtrl.GetTemplateData(ModSettings, "menufooter.html",
                            Utils.GetCurrentCulture(), DebugMode);
                        l = new Literal();
                        l.Text = rpDataTemplF;
                        phMenuF.Controls.Add(l);
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

        private String GetMenu()
        {
            var strCacheKey = "bomenuhtml*" + Utils.GetCurrentCulture() + "*" + PortalId.ToString("");

            var strOut = "";
            if (HttpContext.Current.Session[strCacheKey] != null) strOut = (String)HttpContext.Current.Session[strCacheKey];

            if (StoreSettings.Current.DebugMode || strOut == "")
            {
                var pluginData = new PluginData(PortalId);
                const string resxpath = "/DesktopModules/NBright/NBrightBuy/App_LocalResources/Plugins.ascx.resx";

                var bomenuattributes = DnnUtils.GetLocalizedString("bomenuattributes", resxpath, Utils.GetCurrentCulture());
                var bosubmenuattributes = DnnUtils.GetLocalizedString("bosubmenuattributes", resxpath, Utils.GetCurrentCulture());
                var nameprefix = DnnUtils.GetLocalizedString("nameprefix", resxpath, Utils.GetCurrentCulture());
                var groupprefix = DnnUtils.GetLocalizedString("groupprefix", resxpath, Utils.GetCurrentCulture());
                var nameappendix = DnnUtils.GetLocalizedString("groupappendix", resxpath, Utils.GetCurrentCulture());
                var groupappendix = DnnUtils.GetLocalizedString("groupappendix", resxpath, Utils.GetCurrentCulture());

                
                //get group list (these are the sections/first level of the menu)
                var groupList = new Dictionary<String, String>();
                foreach (var p in pluginData.GetPluginList())
                {
                    var grpname = p.GetXmlProperty("genxml/textbox/group");
                    if (grpname != "" && p.GetXmlPropertyBool("genxml/checkbox/hidden") == false)
                    {
                        if (!groupList.ContainsKey(grpname))
                        {
                            var resxname = DnnUtils.GetLocalizedString(grpname, resxpath, Utils.GetCurrentCulture());
                            if (resxname == "") resxname = grpname;
                            resxname = groupprefix.Replace("{ctrl}", "group" + grpname.ToLower()) + resxname + groupappendix.Replace("{ctrl}", "group" + grpname.ToLower());  
                            groupList.Add(grpname, resxname);
                        }
                    }
                }


                strOut = "<ul " + bomenuattributes + ">";
                foreach (var grpname in groupList)
                {
                    // Build the subgroup, if it doesn't exists then we don't need the parent group li section.
                    var strOutSub = "<ul " + bosubmenuattributes + ">";
                    var subexists = false;
                    foreach (var p in pluginData.GetPluginList())
                    {
                        var grp = p.GetXmlProperty("genxml/textbox/group");
                        if (grpname.Key == grp && p.GetXmlPropertyBool("genxml/checkbox/hidden") == false && IsInRoles(p.GetXmlProperty("genxml/textbox/roles")))
                        {
                            var path = p.GetXmlProperty("genxml/textbox/path");
                            if (File.Exists(MapPath(path)))
                            {
                                strOutSub += "<li>";
                                var ctrl = p.GetXmlProperty("genxml/textbox/ctrl");
                                var param = new string[3];
                                param[0] = "ctrl=" + ctrl;
                                var dispname = DnnUtils.GetLocalizedString(ctrl, resxpath, Utils.GetCurrentCulture());
                                if (string.IsNullOrEmpty(dispname)) dispname = p.GetXmlProperty("genxml/textbox/name");
                                dispname = p.GetXmlProperty("genxml/textbox/prefix") + dispname + p.GetXmlProperty("genxml/textbox/appendix");
                                dispname = nameprefix.Replace("{ctrl}", ctrl) + dispname + nameappendix.Replace("{ctrl}", ctrl);
                                strOutSub += "<a href='" + Globals.NavigateURL(TabId, "", param) + "'>" + dispname + "</a>";
                                strOutSub += "</li>";
                                subexists = true;
                            }
                        }
                    }
                    strOutSub += "</ul>";

                    if (subexists)
                    {
                        strOut += "<li>";
                        strOut += "<a href='#'>" + grpname.Value + "</a>";
                        strOut += strOutSub;
                        strOut += "</li>";                        
                    }

                }
                strOut += "<li>";
                strOut += "<a href='/'>" + DnnUtils.GetLocalizedString("Exit", resxpath, Utils.GetCurrentCulture()) + "</a>";
                strOut += "</li>";

                strOut += "</ul>";

                HttpContext.Current.Session[strCacheKey] = strOut;
            }

            return strOut;
        }

        private Boolean IsInRoles(String roleCSV)
        {
            if (roleCSV == "") return true;
            var s = roleCSV.Split(',');
            foreach (var r in s)
            {
                if (UserInfo.IsInRole(r)) return true;
            }
            return false;
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

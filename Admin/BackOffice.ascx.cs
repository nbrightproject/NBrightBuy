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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
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
    public partial class BackOffice : NBrightBuyAdminBase
    {


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {


                #region "load templates"

                var t1 = "backoffice.html";

                // Get Display Body
                var dataTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                // insert page header text
                var headerTempl = NBrightBuyUtils.GetGenXmlTemplate(dataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, headerTempl, ModSettings.Settings(), null, DebugMode);

                var aryTempl = Utils.ParseTemplateText(dataTempl);

                foreach (var s in aryTempl)
                {
                    var htmlDecode = System.Web.HttpUtility.HtmlDecode(s);
                    if (htmlDecode != null && htmlDecode.ToLower() == "<tag:menu>")
                    {
                        var c1 = LoadControl("/DesktopModules/NBright/NBrightBuy/Admin/Menu.ascx");
                        phData.Controls.Add(c1);
                    } if (htmlDecode != null && htmlDecode.ToLower() == "<tag:container>")
                    {
                        var c1 = LoadControl("/DesktopModules/NBright/NBrightBuy/Admin/Container.ascx");
                        phData.Controls.Add(c1);
                    }
                    else
                    {
                        var lc = new Literal { Text = s };
                        phData.Controls.Add(lc);
                    }
                }


                #endregion


            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

        }

    }

}

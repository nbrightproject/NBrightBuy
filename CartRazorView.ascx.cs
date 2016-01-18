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
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Deals with display of all cart modules that use Razor templates
    /// </summary>
    public partial class CartRazorView : NBrightBuyFrontOfficeBase
    {

        private String _eid = "";
        private String _ename = "";
        private String _catid = "";
        private String _catname = "";
        private String _modkey = "";
        private String _pagemid = "";
        private String _pagenum = "1";
        private String _pagesize = "";
        private String _strOrder = "";
        private String _templD = "";
        private Boolean _displayentrypage = false;
        private String _orderbyindex = "";
        private NavigationData _navigationdata;
        private const String EntityTypeCode = "PRD";
        private const String EntityTypeCodeLang = "PRDLANG";
        private String _itemListName = "";
        private String _print = "";
        private String _printtemplate = "";
        private String _guidkey = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {           
            base.OnInit(e);

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                var lit = new Literal();
                lit.Text = "NO MODULE SETTINGS";
                phData.Controls.Add(lit);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    // do razor code
                    RazorPageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
                // remove any nav data which might store SQL in error.
                _navigationdata.Delete();
            }
        }

        private void RazorPageLoad()
        {

            var strOut = "";
            var template = ModuleConfiguration.DesktopModule.ModuleName + ".cshtml";
            var theme = ModSettings.Get("themefolder");

            // insert page header text
            NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, "pageheader" + _templD, ModSettings.ThemeFolder, ModSettings.Settings());

            strOut = NBrightBuyUtils.RenderCart(theme, template);

            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);

        }

        #endregion

    }

}

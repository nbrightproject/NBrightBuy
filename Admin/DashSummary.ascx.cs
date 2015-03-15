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
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class DashSummary : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {


            }
            catch (Exception exc)
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
            #region "get Dashboard"

            var statsInfo = GetStats();
            var statsData = new NBrightInfo(true);

            var nodList = statsInfo.XMLDoc.SelectNodes("root/row");
            foreach (XmlNode nod in nodList)
            {
                statsData.SetXmlProperty("genxml/" + nod.FirstChild.Name, nod.FirstChild.InnerText);
            }


            // Get Dash
            var tdash = "dashboard.html";
            var templDash = ModCtrl.GetTemplateData(ModSettings, tdash, Utils.GetCurrentCulture(), DebugMode);
            var strDash = GenXmlFunctions.RenderRepeater(statsData, templDash);

            var lit = new Literal();
            lit.Text = strDash;
            phData.Controls.Add(lit);

            #endregion

            #region "get order stats"

            strDash = "<script>$( document ).ready(function() { var data = {";
            strDash += "'xScale': 'ordinal',";
            strDash += "'yScale': 'linear',";
            strDash += "'main': [{";
            strDash += "'className': '.ordertotals',";
            strDash += "'data': [";
            nodList = statsInfo.XMLDoc.SelectNodes("root/orderstats/*");
            foreach (XmlNode nod in nodList)
            {
                var nbi = new NBrightInfo();
                nbi.XMLData = nod.OuterXml;

                strDash += "{'x': '" + nbi.GetXmlPropertyInt("item/createdyear") + "-" + nbi.GetXmlPropertyInt("item/createdmonth") + "',";
                strDash += "'y': " + nbi.GetXmlPropertyDouble("item/appliedtotal").ToString() + "},";

            }
            strDash = strDash.TrimEnd(',');
            strDash += "]}";
            strDash += "]}; var myChart = new xChart('bar', data, '#orderstats'); });</script>";

            var litstats = new Literal();
            litstats.Text = strDash;
            phData.Controls.Add(litstats);

            #endregion


            #region "get order list"

            var orderList = new List<NBrightInfo>();
            nodList = statsInfo.XMLDoc.SelectNodes("root/orders/*");
            foreach (XmlNode nod in nodList)
            {
                var nbi = new NBrightInfo();
                nbi.FromXmlItem(nod.OuterXml);
                var xmlData = nod.SelectSingleNode("genxml/genxml");
                if (xmlData != null) nbi.XMLData = xmlData.OuterXml;
                orderList.Add(nbi);
            }

            templDash = ModCtrl.GetTemplateData(ModSettings, "dashordersheader.html", Utils.GetCurrentCulture(),
                DebugMode);
            strDash = GenXmlFunctions.RenderRepeater(statsData, templDash);
            var litoh = new Literal();
            litoh.Text = strDash;
            phData.Controls.Add(litoh);

            templDash = ModCtrl.GetTemplateData(ModSettings, "dashordersbody.html", Utils.GetCurrentCulture(), DebugMode);
            strDash = GenXmlFunctions.RenderRepeater(orderList, templDash);
            var litob = new Literal();
            litob.Text = strDash;
            phData.Controls.Add(litob);

            templDash = ModCtrl.GetTemplateData(ModSettings, "dashordersfooter.html", Utils.GetCurrentCulture(),
                DebugMode);
            strDash = GenXmlFunctions.RenderRepeater(statsData, templDash);
            var litof = new Literal();
            litof.Text = strDash;
            phData.Controls.Add(litof);

            #endregion



        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "refresh":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "editorder":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


        private NBrightInfo GetStats()
        {
            var statsInfo = new NBrightInfo(true);

            var statsXml = ModCtrl.GetSqlxml("exec NBrightBuy_DashboardStats " + PortalId);
            statsInfo.XMLData = statsXml;

            return statsInfo;
        }


    }

}

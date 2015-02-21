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
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ProductSearch : NBrightBuyFrontOfficeBase
    {

        public String RedirectTabId { get; set; }
        public String TargetModulekey { get; set; }
        public String Themefolder { get; set; }
        public String Searchtemplate { get; set; }


        private GenXmlTemplate _templD;
        private int _redirecttabid;
        private string _targetModuleKey;
        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // get setting via control params
            if (!String.IsNullOrEmpty(RedirectTabId) && !ModSettings.Settings().ContainsKey("redirecttabid")) ModSettings.Settings().Add("redirecttabid", RedirectTabId);
            if (!String.IsNullOrEmpty(TargetModulekey) && !ModSettings.Settings().ContainsKey("targetmodulekey")) ModSettings.Settings().Add("targetmodulekey", TargetModulekey);
            if (!String.IsNullOrEmpty(Themefolder) && !ModSettings.Settings().ContainsKey("themefolder")) ModSettings.Settings().Add("themefolder", Themefolder);
            if (!String.IsNullOrEmpty(Searchtemplate) && !ModSettings.Settings().ContainsKey("txtsearchtemplate")) ModSettings.Settings().Add("txtsearchtemplate", Searchtemplate);

            if (ModSettings.Get("txtsearchtemplate") == "")  // if we don't have module setting jump out
            {
                rpData.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            // Get Display template
            var templDname = ModSettings.Get("txtsearchtemplate");
            var templD = ModCtrl.GetTemplateData(ModSettings, templDname, Utils.GetCurrentCulture(), DebugMode);


            // Get Display Body
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(templD, ModSettings.Settings(), PortalSettings.HomeDirectory);
            _templD = (GenXmlTemplate)rpData.ItemTemplate;


        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // must assign a redirect tab, so postback cookie works.
            _redirecttabid = TabId;
            if (Utils.IsNumeric(RedirectTabId)) 
                _redirecttabid = Convert.ToInt32(RedirectTabId); // use passed in value over module setting (This stops clashbetween skin object and module)
            else
                if (Utils.IsNumeric(ModSettings.Get("redirecttabid"))) _redirecttabid = Convert.ToInt32(ModSettings.Get("redirecttabid"));                
            
            _targetModuleKey = "";
            _targetModuleKey = ModSettings.Get("targetmodulekey");

            var obj = new NBrightInfo();
                
            var searchcookie = new NavigationData(PortalId, _targetModuleKey);
            if (searchcookie.XmlData != "") obj.XMLData = searchcookie.XmlData;
            DoDetail(rpData, obj);
        }


        #endregion



        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var param = new string[2];
            var navigationData = new NavigationData(PortalId, _targetModuleKey);
            switch (e.CommandName.ToLower())
            {
                case "search":
                    //strXml = GenXmlFunctions.GetField(rpDataH, "sqladvsearch");
                    var strXml = GenXmlFunctions.GetGenXml(rpData, "", "");
                    navigationData.Build(strXml,_templD);
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpData);
                    navigationData.XmlData = GenXmlFunctions.GetGenXml(rpData);
                    navigationData.Mode = GenXmlFunctions.GetField(rpData, "navigationmode").ToLower();
                    navigationData.Save();
                    if (StoreSettings.Current.DebugModeFileOut)
                    {
                        strXml = "<root><sql><![CDATA[" + navigationData.Criteria + "]]></sql>" + strXml + "</root>";
                        var xmlDoc = new System.Xml.XmlDataDocument();
                        xmlDoc.LoadXml(strXml);
                        xmlDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_search.xml");
                    }                    
                    Response.Redirect(Globals.NavigateURL(_redirecttabid, "", param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "orderby":
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpData);
                    navigationData.Save();
                    break;
            }

        }

        #endregion



    }

}

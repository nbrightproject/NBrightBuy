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
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
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
    public partial class Import : NBrightBuyAdminBase
    {

        private Dictionary<String, String> _recordXref;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {


                #region "load templates"

                var t1 = "import.html";

                // Get Display Body
                var dataTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                // insert page header text
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(dataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

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

            #region "Data Repeater"
            if (UserId > 0) // only logged in users can see data on this module.
            {
                base.DoDetail(rpData);
            }

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
                case "import":
                    param[0] = "";
                    var importXML = GenXmlFunctions.GetGenXml(rpData, "", StoreSettings.Current.FolderUploadsMapPath);
                    var nbi = new NBrightInfo(false);
                    nbi.XMLData = importXML;
                    _recordXref = new Dictionary<string, string>();
                    DoImport(nbi);
                    DoImportImages(nbi);
                    DoImportDocs(nbi);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "cancel":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        private void DoImport(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hidimagefile");
            if (System.IO.File.Exists(fname))
            {

                var xmlFile = new XmlDataDocument();
                xmlFile.Load(fname);

                if (GenXmlFunctions.GetField(rpData, "importproducts") == "True")
                {
                    ImportRecord(xmlFile,"PRD");
                    ImportRecord(xmlFile, "PRDLANG");
                    ImportRecord(xmlFile, "PRDXREF");
                }

                if (GenXmlFunctions.GetField(rpData, "importcategories") == "True")
                {
                    ImportRecord(xmlFile, "CATEGORY");
                    ImportRecord(xmlFile, "CATEGORYLANG");
                }

                if (GenXmlFunctions.GetField(rpData, "importcategories") == "True" && GenXmlFunctions.GetField(rpData, "importproducts") == "True")
                {
                    ImportRecord(xmlFile, "CATCASCADE");
                    ImportRecord(xmlFile, "CATXREF");
                }

                if (GenXmlFunctions.GetField(rpData, "importproperties") == "True")
                {
                    ImportRecord(xmlFile, "GROUP");
                    ImportRecord(xmlFile, "GROUPLANG");
                }

                if (GenXmlFunctions.GetField(rpData, "importsettings") == "True")
                {
                    ImportRecord(xmlFile, "SETTINGS");
                }

                if (GenXmlFunctions.GetField(rpData, "importorders") == "True")
                {
                    ImportRecord(xmlFile, "ORDER");
                }
            }
        }

        private void DoImportImages(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hidimagefile");
            if (System.IO.File.Exists(fname)) DnnUtils.UnZip(fname, StoreSettings.Current.FolderImages);
        }

        private void DoImportDocs(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hiddocsfile");
            if (System.IO.File.Exists(fname)) DnnUtils.UnZip(fname, StoreSettings.Current.FolderImages);
        }

        private void ImportRecord(XmlDataDocument xmlFile, String typeCode)
        {
            var nodList = xmlFile.SelectNodes("root/item[./typecode='" + typeCode + "']");
            if (nodList != null)
                foreach(XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.FromXmlItem(nod.OuterXml);
                    var olditemid = nbi.ItemID.ToString("");

                    _recordXref.Add(olditemid,"");
                
                }
        }
    }

}

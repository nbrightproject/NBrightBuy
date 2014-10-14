using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
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
    /// The EditNBrightIndex class is used to manage content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Products : NBrightBuyAdminBase
    {

        private int _eid = 0;

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            if (Utils.IsNumeric(Utils.RequestParam(Context, "eid"))) _eid = Convert.ToInt32(Utils.RequestParam(Context, "eid"));

            base.OnInit(e);

            var t2 = "productadminlist.html";
            if (_eid > 0) t2 = "productadmin.html";

            // Get Display Body
            var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
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
            if (UserId > 0) // only logged in users can see data on this module.
            {
                var prodData = ProductUtils.GetProductData(_eid, StoreSettings.Current.EditLanguage);
                base.DoDetail(rpData,prodData.Info);
            }
        }

        #endregion

        #region "Event handlers"

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "delete":
                    Delete(cArg);
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "save":
                    Update(_eid);
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "copy":
                    if (_eid > 0 && Utils.IsNumeric(cArg))
                    {
                        var newid = Copy(cArg);
                        Update(Convert.ToInt32(newid));
                        param[0] = "eid=" + newid;
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "saveexit":
                    Update(_eid);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "edit":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "return":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "addnew":
                    param[0] = "eid=" + AddNew();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;

            }

        }


        #endregion

        private String AddNew()
        {
            var prodData = new ProductData(-1, StoreSettings.Current.EditLanguage);
            if (!prodData.Exists) return prodData.CreateNew().ToString("");
            return prodData.Info.ItemID.ToString("");
        }

        private void Update(int productid)
        {
            if (productid > 0)
            {
                var prodData = ProductUtils.GetProductData(productid, StoreSettings.Current.EditLanguage);
                var strXml = GenXmlFunctions.GetGenXml(rpData);
                var updInfo = new NBrightInfo(true);
                updInfo.XMLData = strXml;

                GenXmlFunctions.UploadImgFile(rpData, "image", StoreSettings.Current.FolderImagesMapPath);
                var fName = ((HtmlGenericControl)rpData.Items[0].FindControl("hidimage")).Attributes["value"];
                updInfo.SetXmlProperty("genxml/hidden/hidimage",fName);

                GenXmlFunctions.UploadFile(rpData, "document", StoreSettings.Current.FolderDocumentsMapPath);
                fName = ((HtmlGenericControl)rpData.Items[0].FindControl("hiddocument")).Attributes["value"];
                updInfo.SetXmlProperty("genxml/hidden/hiddocument", fName);
                
                var imgCtrl = (FileUpload)rpData.Items[0].FindControl("image");
                updInfo.SetXmlProperty("genxml/hidden/postedimagename", imgCtrl.PostedFile.FileName);
                var docCtrl = (FileUpload)rpData.Items[0].FindControl("document");
                updInfo.SetXmlProperty("genxml/hidden/posteddocumentname", docCtrl.PostedFile.FileName);

                prodData.Update(updInfo.XMLData);
                prodData.Save();

                if (StoreSettings.Current.DebugMode) prodData.OutputDebugFile("debug_productupdate.xml");

                
            }
        }

        private void Delete( String productId)
        {
            if (Utils.IsNumeric(productId) && Convert.ToInt32(productId) > 0)
            {
                var prodData = ProductUtils.GetProductData(Convert.ToInt32(productId), StoreSettings.Current.EditLanguage);
                prodData.Delete();
            }
        }

        private String Copy(String productId)
        {
            if (Utils.IsNumeric(productId) && Convert.ToInt32(productId) > 0)
            {
                var prodData = ProductUtils.GetProductData(Convert.ToInt32(productId), StoreSettings.Current.EditLanguage);
                var newid = prodData.Copy();
                return newid.ToString("");
            }
            return "";
        }


    }

}
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
        private String _page = "";

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            _page = Utils.RequestParam(Context, "page");
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
            var rtntab = Utils.RequestQueryStringParam(Request, "rtntab");
            var rtneid = Utils.RequestQueryStringParam(Request, "rtneid");

            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "delete":
                    Delete(cArg);
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "save":
                    Update(_eid);
                    param[0] = "eid=" + cArg;
                    if (rtntab != "") param[1] = "rtntab=" + rtntab;
                    if (rtneid != "") param[2] = "rtneid=" + rtneid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "copy":
                    if (_eid > 0 && Utils.IsNumeric(cArg))
                    {
                        var newid = Copy(cArg);
                        param[0] = "eid=" + newid;
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "saveexit":
                    Update(_eid);
                    if (rtntab != "") param[0] = "tabid=" + rtntab;
                    if (rtneid != "") param[1] = "eid=" + rtneid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "edit":
                    param[0] = "eid=" + cArg;
                    if (_page != "") param[1] = "page=" + _page;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "return":
                    if (rtntab != "") param[0] = "tabid=" + rtntab;
                    if (rtneid != "") param[1] = "eid=" + rtneid;
                    if (_page != "") param[2] = "page=" + _page;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "addnew":
                    param[0] = "eid=" + AddNew();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;

            }

        }


        #endregion

        private String AddNew()
        {
            var prodData = ProductUtils.GetProductData(-1, StoreSettings.Current.EditLanguage);
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

                GenXmlFunctions.UploadFile(rpData, "document", StoreSettings.Current.FolderDocumentsMapPath);
                var ctrl = ((HtmlGenericControl) rpData.Items[0].FindControl("hiddocument"));
                if (ctrl != null)
                {
                    var fName = ctrl.Attributes["value"];
                    updInfo.SetXmlProperty("genxml/hidden/hiddocument", fName);

                    var docCtrl = (FileUpload) rpData.Items[0].FindControl("document");
                    updInfo.SetXmlProperty("genxml/hidden/posteddocumentname", docCtrl.PostedFile.FileName);
                }

                prodData.Update(updInfo.XMLData);
                prodData.Save();
                prodData.FillEmptyLanguageFields();

                if (StoreSettings.Current.DebugModeFileOut) prodData.OutputDebugFile(PortalSettings.HomeDirectoryMapPath + "debug_productupdate.xml");

                NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                ProductUtils.RemoveProductDataCache(prodData);
                
            }
        }

        private void Delete( String productId)
        {
            if (Utils.IsNumeric(productId) && Convert.ToInt32(productId) > 0)
            {
                var prodData = ProductUtils.GetProductData(Convert.ToInt32(productId), StoreSettings.Current.EditLanguage);
                prodData.Delete();
                NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                ProductUtils.RemoveProductDataCache(prodData);
            }
        }

        private String Copy(String productId)
        {
            if (Utils.IsNumeric(productId) && Convert.ToInt32(productId) > 0)
            {
                var prodData = ProductUtils.GetProductData(Convert.ToInt32(productId), StoreSettings.Current.EditLanguage);
                var newid = prodData.Copy();
                Update(newid);
                prodData = ProductUtils.GetProductData(Convert.ToInt32(newid), StoreSettings.Current.EditLanguage);
                // update modelid, so it's unique
                var nodList = prodData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
                if (nodList != null)
                {
                    var lp = 1;
                    foreach (var nod in nodList)
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/models/genxml[" + lp.ToString("") + "]/hidden/modelid", Utils.GetUniqueKey());
                        lp += 1;
                    }
                }
                prodData.Save();
                ProductUtils.RemoveProductDataCache(prodData);
                return newid.ToString("");
            }
            return "";
        }


    }

}
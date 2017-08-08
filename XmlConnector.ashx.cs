using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.SqlServer.Server;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using NBrightBuy.render;
using NBrightCore.images;
using Nevoweb.DNN.NBrightBuy.Components.Clients;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Components.Orders;
using Nevoweb.DNN.NBrightBuy.Components.Payments;
using RazorEngine.Compilation.ImpromptuInterface;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class XmlConnector : IHttpHandler
    {
        private readonly JavaScriptSerializer _js = new JavaScriptSerializer();
        private String _lang = "";
        private String _editlang = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOut = "";

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var itemId = Utils.RequestQueryStringParam(context, "itemid");
            var ctlType = Utils.RequestQueryStringParam(context, "ctltype");
            var idXref = Utils.RequestQueryStringParam(context, "idxref");
            var xpathpdf = Utils.RequestQueryStringParam(context, "pdf");
            var xpathref = Utils.RequestQueryStringParam(context, "pdfref");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");
            var moduleId = Utils.RequestQueryStringParam(context, "mid");
            var moduleKey = Utils.RequestQueryStringParam(context, "mkey");
            var parentid = Utils.RequestQueryStringParam(context, "parentid");
            var entryid = Utils.RequestQueryStringParam(context, "entryid");
            var entryxid = Utils.RequestQueryStringParam(context, "entryxid");
            var catid = Utils.RequestQueryStringParam(context, "catid");
            var catxid = Utils.RequestQueryStringParam(context, "catxid");
            var templatePrefix = Utils.RequestQueryStringParam(context, "tprefix");
            var value = Utils.RequestQueryStringParam(context, "value");
            var itemListName = Utils.RequestQueryStringParam(context, "listname");
            if (itemListName == "") itemListName = "ItemList";
            if (itemListName == "*") itemListName = "ItemList";

            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            _editlang = NBrightBuyUtils.SetContextLangauge(context);

            #endregion

            #endregion

            try
            {

                #region "Do processing of command"

                if (paramCmd.StartsWith("client."))
                {
                    strOut = ClientFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("orderadmin_"))
                {
                    strOut = OrderFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("payment_"))
                {
                    strOut = PaymentFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("product_"))
                {
                    strOut = ProductFunctions.ProcessCommand(paramCmd, context, _editlang);
                }
                else
                {
                    strOut = "ERROR!! - No Security rights for current user!";
                    switch (paramCmd)
                    {
                        case "test":
                            strOut = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                            break;
                        case "setdata":
                            break;
                        case "deldata":
                            break;
                        case "getdata":
                            strOut = GetReturnData(context);
                            break;
                        case "additemlist":
                            if (Utils.IsNumeric(itemId))
                            {
                                var cw = new ItemListData(itemListName);
                                cw.Add(itemId);
                                strOut = cw.ItemList;
                            }
                            break;
                        case "removeitemlist":
                            if (Utils.IsNumeric(itemId))
                            {
                                var cw1 = new ItemListData(itemListName);
                                cw1.Remove(itemId);
                                strOut = cw1.ItemList;
                            }
                            break;
                        case "deleteitemlist":
                            var cw2 = new ItemListData(itemListName);
                            cw2.Delete();
                            strOut = "deleted";
                            break;
                        case "getcategoryproductlist":
                            strOut = GetCategoryProductList(context);
                            break;
                        case "deletecatxref":
                            if (NBrightBuyUtils.CheckRights()) strOut = DeleteCatXref(context);
                            break;
                        case "selectcatxref":
                            if (NBrightBuyUtils.CheckRights()) strOut = SelectCatXref(context);
                            break;
                        case "deleteallcatxref":
                            if (NBrightBuyUtils.CheckRights()) strOut = DeleteAllCatXref(context);
                            break;
                        case "copyallcatxref":
                            if (NBrightBuyUtils.CheckRights()) strOut = CopyAllCatXref(context);
                            break;
                        case "moveallcatxref":
                            if (NBrightBuyUtils.CheckRights()) strOut = CopyAllCatXref(context, true);
                            break;
                        case "cattaxupdate":
                            if (NBrightBuyUtils.CheckRights()) strOut = CatTaxUpdate(context);
                            break;
                        case "fileupload":
                            if (NBrightBuyUtils.CheckRights())
                            {
                                strOut = FileUpload(context);
                            }
                            break;
                        case "fileclientupload":
                            if (StoreSettings.Current.GetBool("allowupload"))
                            {
                                strOut = FileUpload(context, itemId);
                            }
                            break;
                        case "addtobasket":
                            strOut = AddToBasket(context);
                            break;
                        case "addalltobasket":
                            strOut = AddAllToBasket(context);
                            break;
                        case "addcookietobasket":
                            break;
                        case "docdownload":

                            var fname = Utils.RequestQueryStringParam(context, "filename");
                            var filekey = Utils.RequestQueryStringParam(context, "key");
                            if (filekey != "")
                            {
                                var uData = new UserData();
                                if (uData.HasPurchasedDocByKey(filekey)) fname = uData.GetPurchasedFileName(filekey);
                                fname = StoreSettings.Current.FolderDocuments + "/" + fname;
                            }
                            if (fname != "")
                            {
                                strOut = fname; // return this is error.
                                var downloadname = Utils.RequestQueryStringParam(context, "downloadname");
                                var fpath = HttpContext.Current.Server.MapPath(fname);
                                if (downloadname == "") downloadname = Path.GetFileName(fname);
                                try
                                {
                                    Utils.ForceDocDownload(fpath, downloadname, context.Response);
                                }
                                catch (Exception ex)
                                {
                                    // ignore, robots can cause error on thread abort.
                                    //Exceptions.LogException(ex);
                                }
                            }
                            break;
                        case "printproduct":
                            break;
                        case "removefromcart":
                            RemoveFromCart(context);
                            strOut = "removefromcart";
                            break;
                        case "recalculatecart":
                            RecalculateCart(context);
                            strOut = "recalculatecart";
                            break;
                        case "recalculatesummary":
                            RecalculateSummary(context);
                            strOut = "recalculatecart";
                            break;
                        case "redirecttopayment":
                            strOut = RedirectToPayment(context);
                            break;
                        case "updatebilladdress":
                            strOut = UpdateCartAddress(context, "bill");
                            break;
                        case "updateshipaddress":
                            strOut = UpdateCartAddress(context, "ship");
                            break;
                        case "updateshipoption":
                            strOut = UpdateCartAddress(context, "shipoption");
                            break;
                        case "rendercart":
                            strOut = RenderCart(context);
                            break;
                        case "renderpostdata":
                            strOut = RenderPostData(context);
                            break;
                        case "clearcart":
                            var currentcart = new CartData(PortalSettings.Current.PortalId);
                            currentcart.DeleteCart();
                            strOut = "clearcart";
                            break;
                        case "shippingprovidertemplate":
                            strOut = GetShippingProviderTemplates(context);
                            break;
                        case "getsettings":
                            strOut = GetSettings(context);
                            break;
                        case "savesettings":
                            if (NBrightBuyUtils.CheckRights()) strOut = SaveSettings(context);
                            break;
                        case "updateprofile":
                            strOut = UpdateProfile(context);
                            break;
                        case "dosearch":
                            strOut = DoSearch(context);
                            break;
                        case "resetsearch":
                            strOut = ResetSearch(context);
                            break;
                        case "orderby":
                            strOut = DoOrderBy(context);
                            break;

                    }
                }

                if (strOut == "")
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetAjaxProviders();
                    foreach (var d in provList)
                    {
                        if (paramCmd.StartsWith(d.Key + "_"))
                        {
                            var ajaxprov = AjaxInterface.Instance(d.Key);
                            if (ajaxprov != null)
                            {
                                strOut = ajaxprov.ProcessCommand(paramCmd, context, _editlang);
                            }

                        }
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
                Exceptions.LogException(ex);
            }


            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region "fileupload"

        private void UpdateProductImages(HttpContext context)
        {
            //get uploaded params
            var settings = NBrightBuyUtils.GetAjaxDictionary(context);
            if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
            var productitemid = settings["itemid"];
            var imguploadlist = settings["imguploadlist"];

            if (Utils.IsNumeric(productitemid))
            {
                var imgs = imguploadlist.Split(',');
                foreach (var img in imgs)
                {
                    if (ImgUtils.IsImageFile(Path.GetExtension(img)) && img != "")
                    {
                        string fullName = StoreSettings.Current.FolderTempMapPath + "\\" + img;
                        if (File.Exists(fullName))
                        {
                            var imgResize = StoreSettings.Current.GetInt(StoreSettingKeys.productimageresize);
                            if (imgResize == 0) imgResize = 800;
                            var imagepath = ResizeImage(fullName, imgResize);
                            var imageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + Path.GetFileName(imagepath);
                            AddNewImage(Convert.ToInt32(productitemid), imageurl, imagepath);                                                    
                        }
                    }
                }
                // clear any cache for the product.
                ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, Convert.ToInt32(productitemid));

                var cachekey = "AjaxProductImgs*" + productitemid;
                Utils.RemoveCache(cachekey);

            }
        }

        private String ResizeImage(String fullName, int imgSize = 640)
        {
            if (ImgUtils.IsImageFile(Path.GetExtension(fullName)))
            {
                var extension = Path.GetExtension(fullName);
                var newImageFileName = StoreSettings.Current.FolderImagesMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + Utils.GetUniqueKey() + extension;
                if (extension != null && extension.ToLower() == ".png")
                {
                    newImageFileName = ImgUtils.ResizeImageToPng(fullName, newImageFileName, imgSize);
                }
                else
                {
                    newImageFileName = ImgUtils.ResizeImageToJpg(fullName, newImageFileName, imgSize);
                }
                Utils.DeleteSysFile(fullName);

                return newImageFileName;

            }
            return "";
        }


        private void AddNewImage(int itemId,String imageurl, String imagepath)
        {
            var objCtrl = new NBrightBuyController();
            var dataRecord = objCtrl.Get(itemId);
            if (dataRecord != null)
            {
                var strXml = "<genxml><imgs><genxml><hidden><imagepath>" + imagepath + "</imagepath><imageurl>" + imageurl + "</imageurl></hidden></genxml></imgs></genxml>";
                if (dataRecord.XMLDoc.SelectSingleNode("genxml/imgs") == null)
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs", "genxml");
                }
                else
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs/genxml", "genxml/imgs");
                }
                objCtrl.Update(dataRecord);
            }
        }


        private string FileUpload(HttpContext context, string itemid = "")
        {
            try
            {

                var strOut = "";
                switch (context.Request.HttpMethod)
                {
                    case "HEAD":
                    case "GET":
                        break;
                    case "POST":
                    case "PUT":
                        strOut = UploadFile(context, itemid);
                        break;
                    case "DELETE":
                        break;
                    case "OPTIONS":
                        break;

                    default:
                        context.Response.ClearHeaders();
                        context.Response.StatusCode = 405;
                        break;
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }

        // Upload file to the server
        private String UploadFile(HttpContext context, string itemid = "")
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                return UploadWholeFile(context, statuses, itemid);
            }
            else
            {
                return UploadPartialFile(headers["X-File-Name"], context, statuses, itemid);
            }
        }

        // Upload partial file
        private String UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
            if (fexpr.Match(fileName.ToLower()).Success)
            {

                if (itemid != "") itemid += "_";
                if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
                var inputStream = context.Request.Files[0].InputStream;
                var fullName = StoreSettings.Current.FolderTempMapPath + "\\" + itemid + fileName;

                using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
                {
                    var buffer = new byte[1024];

                    var l = inputStream.Read(buffer, 0, 1024);
                    while (l > 0)
                    {
                        fs.Write(buffer, 0, l);
                        l = inputStream.Read(buffer, 0, 1024);
                    }
                    fs.Flush();
                    fs.Close();
                }
                statuses.Add(new FilesStatus(new FileInfo(fullName)));
            }
            return "";
        }

        // Upload entire file
        private String UploadWholeFile(HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            if (itemid != "") itemid += "_";
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
                if (fexpr.Match(file.FileName.ToLower()).Success)
                {                    
                    file.SaveAs(StoreSettings.Current.FolderTempMapPath + "\\" + itemid + file.FileName);
                    statuses.Add(new FilesStatus(Path.GetFileName(itemid + file.FileName), file.ContentLength));
                }
            }
            return "";
        }

        private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                    context.Response.ContentType = "application/json";
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = _js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }



        #endregion

        #region "SQL Data return"

        private string GetReturnData(HttpContext context)
        {
            try
            {

                var strOut = "";

                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();

                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary();

                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

                if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                var objCtrl = new NBrightBuyController();

                // run SQL and template to return html
                if (settings.ContainsKey("sqltpl") && settings.ContainsKey("xsltpl"))
                {
                    var strSql = templCtrl.GetTemplateData(settings["sqltpl"], _lang, true, true, true, StoreSettings.Current.Settings());
                    var xslTemp = templCtrl.GetTemplateData(settings["xsltpl"], _lang, true, true, true, StoreSettings.Current.Settings());

                    // replace any settings tokens (This is used to place the form data into the SQL)
                    strSql = Utils.ReplaceSettingTokens(strSql, settings);
                    strSql = Utils.ReplaceUrlTokens(strSql);

                    strSql = GenXmlFunctions.StripSqlCommands(strSql); // don't allow anything to update through here.

                    strOut = objCtrl.GetSqlxml(strSql);
                    if (!strOut.StartsWith("<root>")) strOut = "<root>" + strOut + "</root>"; // always wrap with root node.
                    strOut = XslUtils.XslTransInMemory(strOut, xslTemp);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #endregion

        #region "Category Methods"

        private String GetCategoryProductList(HttpContext context)
        {
            try
            {
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DataProvider.Instance().DatabaseOwner;

                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {Settings:itemid}) ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, settings);


                if (!settings.ContainsKey("filter")) settings.Add("filter", strFilter);
                if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                settings["razortemplate"] = "Admin_CategoryProducts.cshtml";
                settings["themefolder"] = "config";
                
                return ProductFunctions.ProductAdminList(settings,true,_editlang);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        private string DeleteCatXref(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                var selectproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                if (selectproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectproductid, lang, false);
                    prodData.RemoveCategory(catid);
                }
                else
                    return "Invalid parentitemid or xrefitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }


        private string SelectCatXref(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                var selectedproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                if (selectedproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectedproductid, _lang, false);
                    prodData.AddCategory(catid);
                }
                else
                    return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        private string CatTaxUpdate(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                var taxrate = ajaxInfo.GetXmlProperty("genxml/hidden/selecttaxrate");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid,_lang);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var strXml = "<genxml><models>";
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, lang);
                        foreach (var mod in prdData.Models)
                        {
                            mod.SetXmlProperty("genxml/dropdownlist/taxrate", taxrate);
                            strXml += mod.XMLData;
                        }
                        strXml += "</models></genxml>";
                        prdData.DataRecord.ReplaceXmlNode(strXml, "genxml/models", "genxml");
                        prdData.Save();
                    }
                }
                else
                    return "Invalid catid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        private string DeleteAllCatXref(HttpContext context)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, _lang);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, lang);
                        prdData.RemoveCategory(catid);
                    }
                }
                strOut = NBrightBuyUtils.GetResxMessage();
                DataCache.ClearCache();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return strOut;
        }


        private String CopyAllCatXref(HttpContext context,Boolean moverecords = false)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                var newcatid = "";
                if (settings.ContainsKey("selectedcatid")) newcatid = settings["selectedcatid"];

                if (Utils.IsNumeric(newcatid) && settings.ContainsKey("itemid"))
                {

                    NBrightBuyUtils.CopyAllCatXref(Convert.ToInt32(settings["itemid"]), Convert.ToInt32(newcatid), moverecords);

                    strOut = NBrightBuyUtils.GetResxMessage();
                    DataCache.ClearCache();
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return strOut;
        }

        #endregion

        #region "Front Office Actions"

        private String RenderCart(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/carttemplate");
            if (carttemplate == "") carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/minicarttemplate");
            var theme = ajaxInfo.GetXmlProperty("genxml/hidden/carttheme");
            if (theme == "") theme = ajaxInfo.GetXmlProperty("genxml/hidden/minicarttheme");
            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
            if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
            var razorTempl = "";
            if (carttemplate != "")
            {
                if (lang == "") lang = Utils.GetCurrentCulture();
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                if (UserController.Instance.GetCurrentUserInfo().UserID != -1)  // If we have a user, do save to update userid, so addrees checkout can get addresses.
                {
                    if (currentcart.UserId != UserController.Instance.GetCurrentUserInfo().UserID && currentcart.EditMode == "")
                    {
                        currentcart.Save();
                    }
                }

                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0,"", currentcart, controlpath, theme, lang, StoreSettings.Current.Settings());
            }
            return razorTempl;
        }

        /// <summary>
        /// This token used the ajax posted context data to render the razor template specified in "carttemplate"
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private String RenderPostData(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/carttemplate");
            var theme = ajaxInfo.GetXmlProperty("genxml/hidden/carttheme");
            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
            if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
            var razorTempl = "";
            if (carttemplate != "")
            {
                if (lang == "") lang = Utils.GetCurrentCulture();
                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0, "", ajaxInfo, controlpath, theme, lang, StoreSettings.Current.Settings());
            }
            return razorTempl;
        }

        private string AddToBasket(HttpContext context)
        {
            try
            {
                var strOut = "";
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var settings = ajaxInfo.ToDictionary();

                if (settings.ContainsKey("productid"))
                {
                    if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                    var currentcart = new CartData(Convert.ToInt16(settings["portalid"]));
                    currentcart.AddAjaxItem(ajaxInfo, StoreSettings.Current.SettingsInfo,StoreSettings.Current.DebugMode);
                    currentcart.Save(StoreSettings.Current.DebugMode);
                    strOut = "OK";
                }


                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string AddAllToBasket(HttpContext context)
        {
            try
            {
                var strOut = "";
                var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);

                foreach (var ajaxInfo in ajaxInfoList)
                {
                    var settings = ajaxInfo.ToDictionary();

                    if (settings.ContainsKey("productid"))
                    {
                        if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                        var currentcart = new CartData(Convert.ToInt16(settings["portalid"]));
                        currentcart.AddAjaxItem(ajaxInfo, StoreSettings.Current.SettingsInfo, StoreSettings.Current.DebugMode);
                        currentcart.Save(StoreSettings.Current.DebugMode);
                        strOut = "OK";
                    }

                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string RecalculateCart(HttpContext context)
        {
                var strOut = "";
                var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                foreach (var ajaxInfo in ajaxInfoList)
                {
                        currentcart.MergeCartInputData(currentcart.GetItemIndex(ajaxInfo.GetXmlProperty("genxml/hidden/itemcode")), ajaxInfo);
                }
                currentcart.Save(StoreSettings.Current.DebugMode,true);
                strOut = "OK";
                return strOut;
        }

        private string UpdateProfile(HttpContext context)
        {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var profileData = new ProfileData();
                profileData.UpdateProfileAjax(ajaxInfo.XMLData, StoreSettings.Current.DebugMode);

                return "OK";
        }

        private string ResetSearch(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.Delete();

                return "RESET";
        }

        private string DoSearch(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var tagList = new List<string>();
                var nodList = ajaxInfo.XMLDoc.SelectNodes("genxml/hidden/*");
                foreach (XmlNode nod in nodList)
                {
                    tagList.Add(nod.InnerText);
                }
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.Build(ajaxInfo.XMLData,tagList);
                navData.Mode = ajaxInfo.GetXmlProperty("genxml/hidden/navigationmode").ToLower();
                navData.CategoryId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagenumber") != "") navData.PageNumber = ajaxInfo.GetXmlProperty("genxml/hidden/pagenumber");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagesize") != "") navData.PageSize = ajaxInfo.GetXmlProperty("genxml/hidden/pagesize");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagename") != "") navData.PageName = ajaxInfo.GetXmlProperty("genxml/hidden/pagename");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagemoduleid") != "") navData.PageModuleId  = ajaxInfo.GetXmlProperty("genxml/hidden/pagemoduleid");
                navData.SearchFormData = ajaxInfo.XMLData;
                navData.Save();

                return "OK";
        }

        private string DoOrderBy(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.OrderByIdx = ajaxInfo.GetXmlProperty("genxml/hidden/orderbyidx");
                navData.OrderBy = " order by " + ajaxInfo.GetXmlProperty("genxml/hidden/orderby" + navData.OrderByIdx);
                navData.Save();

                return "OK";
        }

        private string UpdateCartAddress(HttpContext context,String addresstype = "")
        {
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context,true);

                currentcart.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartsummary"); // (Legacy) we need to set this so the cart calcs shipping

                if (addresstype == "bill")
                {
                    currentcart.AddBillingAddress(ajaxInfo);
                    currentcart.Save();
                }

                if (addresstype == "ship")
                {
                    if (currentcart.GetShippingOption() == "2") // need to test this, becuase in legacy code the shipping option is set to "2" when we save the shipping address.
                    {
                        currentcart.AddShippingAddress(ajaxInfo);
                        currentcart.Save();
                    }
                }

                if (addresstype == "shipoption")
                {
                    var shipoption = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/rblshippingoptions");
                    currentcart.SetShippingOption(shipoption);
                    currentcart.Save();
                }

                return addresstype;
        }

        private string RedirectToPayment(HttpContext context)
        {
            try
            {
                RecalculateSummary(context);

                var currentcart = new CartData(PortalSettings.Current.PortalId);

                if (currentcart.GetCartItemList().Count > 0)
                {
                    currentcart.SetValidated(true);
                    if (currentcart.EditMode == "E") currentcart.ConvertToOrder();
                }
                else
                {
                    currentcart.SetValidated(true);
                }
                currentcart.Save();

                var rtnurl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId);
                if (currentcart.EditMode == "E")
                {
                    // is order being edited, so return to order status after edit.
                    // ONLY if the cartsummry is being displayed to the manager.
                    currentcart.ConvertToOrder();
                    // redirect to back office
                    var param = new string[2];
                    param[0] = "ctrl=orders";
                    param[1] = "eid=" + currentcart.PurchaseInfo.ItemID.ToString("");
                    var strbackofficeTabId = StoreSettings.Current.Get("backofficetabid");
                    var backofficeTabId = PortalSettings.Current.ActiveTab.TabID;
                    if (Utils.IsNumeric(strbackofficeTabId)) backofficeTabId = Convert.ToInt32(strbackofficeTabId);
                    rtnurl = Globals.NavigateURL(backofficeTabId, "", param);
                }
                return rtnurl;

            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return "ERROR";
            }
        }

        private string RecalculateSummary(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            var currentcart = new CartData(PortalSettings.Current.PortalId);
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var shipoption = currentcart.GetShippingOption(); // ship option already set in address update.

                currentcart.AddExtraInfo(ajaxInfo);
                currentcart.SetShippingOption(shipoption);
                currentcart.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartsummary"); // (Legacy) we need to set this so the cart calcs shipping
                currentcart.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider"));

            var shipref = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider");
            var displayanme = "";
            var shipInfo = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SHIPPING", shipref);
            if (shipInfo != null)
            {
                displayanme = shipInfo.GetXmlProperty("genxml/textbox/shippingdisplayname");
            }
            if (displayanme == "") displayanme = shipref;
            currentcart.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/hidden/shippingdisplayanme", displayanme);

            var shippingproductcode = ajaxInfo.GetXmlProperty("genxml/hidden/shippingproductcode");
                currentcart.PurchaseInfo.SetXmlProperty("genxml/shippingproductcode", shippingproductcode);
                var pickuppointref = ajaxInfo.GetXmlProperty("genxml/hidden/pickuppointref");
                currentcart.PurchaseInfo.SetXmlProperty("genxml/pickuppointref", pickuppointref);
                var pickuppointaddr = ajaxInfo.GetXmlProperty("genxml/hidden/pickuppointaddr");
                currentcart.PurchaseInfo.SetXmlProperty("genxml/pickuppointaddr", pickuppointaddr);

                currentcart.Lang = ajaxInfo.Lang;  // set lang so we can send emails in same language the order was made in.

                currentcart.Save(StoreSettings.Current.DebugMode,true);

                return "OK";
        }

        private string RemoveFromCart(HttpContext context)
        {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                currentcart.RemoveItem(ajaxInfo.GetXmlProperty("genxml/hidden/itemcode"));
                currentcart.Save(StoreSettings.Current.DebugMode);
                return "OK";
        }


        private String GetShippingProviderTemplates(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var activeprovider = ajaxInfo.GetXmlProperty("genxml/hidden/shippingprovider");
            var currentcart = new CartData(PortalSettings.Current.PortalId);

            var shipoption = currentcart.GetShippingOption(); // we don't want to overwrite the selected shipping option.
            currentcart.AddExtraInfo(ajaxInfo);
            currentcart.SetShippingOption(shipoption);
            currentcart.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartsummary"); // (Legacy) we need to set this so the cart calcs shipping
            currentcart.Save();

            if (activeprovider == "") activeprovider = currentcart.PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");


            var strRtn = "";
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetShippingProviders();
            if (provList != null && provList.Count > 0)
            {
                if (activeprovider == "") activeprovider = provList.First().Key;
                foreach (var d in provList)
                {
                    var p = d.Value;
                    var shippingkey = p.GetXmlProperty("genxml/textbox/ctrl");
                    var shipprov = ShippingInterface.Instance(shippingkey);
                    if (shipprov != null)
                    {
                        if (activeprovider == d.Key)
                        {
                            var razorTempl = shipprov.GetTemplate(currentcart.PurchaseInfo);
                            var objList = new List<NBrightInfo>();
                            objList.Add(currentcart.PurchaseInfo);
                            if (razorTempl.StartsWith("@"))
                            {
                                strRtn += NBrightBuyUtils.RazorRender(objList, razorTempl, shippingkey + "shippingtemplate", StoreSettings.Current.DebugMode);
                            }
                            else
                            {
                                strRtn += GenXmlFunctions.RenderRepeater(objList[0], razorTempl, "", "XMLData", "", StoreSettings.Current.Settings(), null);
                            }
                        }
                    }
                }
            }
            return strRtn;
        }


        #endregion

        #region "Settings"

        private String GetSettings(HttpContext context, bool clearCache = false)
        {
            try
            {
                var strOut = "";
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                var themefolder = ajaxInfo.GetXmlProperty("genxml/dropdownlist/themefolder");
                var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
                if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
                if (razortemplate == "") return ""; // assume no settings requirted
                if (moduleid == "") moduleid = "-1";

                // do edit field data if a itemid has been selected
                var obj = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid));
                obj.ModuleId = Convert.ToInt32(moduleid); // assign for new records
                strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, obj.ModuleId, "settings", obj, controlpath, themefolder, _lang,null);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private String SaveSettings(HttpContext context)
        {
            try
            {
                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                if (Utils.IsNumeric(moduleid))
                {
                    // get DB record
                    var nbi = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid)); 
                    if (nbi.ModuleId == 0) // new setting record
                    {
                        nbi = CreateSettingsInfo(moduleid, nbi);
                    }
                    // get data passed back by ajax
                    var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                    // update record with ajax data
                    nbi.UpdateAjax(strIn);


                    if (nbi.GetXmlProperty("genxml/hidden/modref") == "") nbi.SetXmlProperty("genxml/hidden/modref", Utils.GetUniqueKey(10));
                    if (nbi.TextData == "") nbi.TextData = "NBrightBuy";
                    objCtrl.Update(nbi);
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId); // make sure all new settings are used.
                }
                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private NBrightInfo CreateSettingsInfo(String moduleid, NBrightInfo nbi)
        {

            var objCtrl = new NBrightBuyController();
            nbi = objCtrl.GetByType(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), "SETTINGS");
            if (nbi == null)
            {
                nbi = new NBrightInfo(true); // populate empty XML so we can update nodes.
                nbi.GUIDKey = "";
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.ModuleId = Convert.ToInt32(moduleid);
                nbi.TypeCode = "SETTINGS";
                nbi.Lang = "";
            }
            //rebuild xml
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.GUIDKey = Utils.GetUniqueKey(10);
            nbi.SetXmlProperty("genxml/hidden/modref", nbi.GUIDKey);
            return nbi;
        }


        #endregion

    }
}
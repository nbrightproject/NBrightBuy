using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Razor;
using System.Web.Script.Serialization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;

namespace Nevoweb.DNN.NBrightBuy.Components.Clients
{
    public static class ProductFunctions
    {
        #region "product Admin Methods"
        private static string _editlang = "";

        public static string ProcessCommand(string paramCmd,HttpContext context,string editlang = "")
        {
            _editlang = editlang;
            if (_editlang == "") _editlang = Utils.GetCurrentCulture();

            var strOut = "PRODUCT - ERROR!! - No Security rights or function command.";
            if (NBrightBuyUtils.CheckManagerRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");

                switch (paramCmd)
                {
                    case "product_admin_getlist":
                        strOut = ProductFunctions.ProductAdminList(context);
                        break;
                    case "product_admin_getdetail":
                        strOut = ProductFunctions.ProductAdminDetail(context);
                        break;                        
                    case "product_adminaddnew":
                        strOut = ProductFunctions.ProductAdminAddNew(context);
                        break;
                    case "product_admin_save":
                        strOut = ProductFunctions.ProductAdminSave(context);
                        break;
                    case "product_admin_saveexit":
                        strOut = ProductFunctions.ProductAdminSaveExit(context);
                        break;
                    case "product_admin_saveas":
                        strOut = ProductFunctions.ProductAdminSaveAs(context);
                        break;
                    case "product_admin_selectlist":
                        strOut = ProductFunctions.ProductAdminList(context);
                        break;
                    case "product_moveproductadmin":
                        strOut = ProductFunctions.MoveProductAdmin(context);
                        break;
                    case "product_addproductmodels":
                        strOut = ProductFunctions.AddModel(context);
                        break;
                    case "product_addproductoptions":
                        strOut = ProductFunctions.AddOption(context);
                        break;
                    case "product_addproductoptionvalues":
                        strOut = ProductFunctions.AddOptionValues(context);
                        break;
                    case "product_admin_delete":
                        strOut = ProductFunctions.DeleteProduct(context);
                        break;
                    case "product_updateproductimages":
                        strOut = ProductFunctions.UpdateProductImages(context);
                        break;
                    case "product_updateproductdocs":
                        strOut = ProductFunctions.UpdateProductDocs(context);
                        break;
                    case "product_addproductcategory":
                        strOut = ProductFunctions.AddProductCategory(context);
                        break;
                    case "product_removeproductcategory":
                        strOut = ProductFunctions.RemoveProductCategory(context);
                        break;
                    case "product_setdefaultcategory":
                        strOut = ProductFunctions.SetDefaultCategory(context);
                        break;
                    case "product_populatecategorylist":
                        strOut = ProductFunctions.GetPropertyListBox(context);
                        break;
                    case "product_addproperty":
                        strOut = ProductFunctions.AddProperty(context);
                        break;
                    case "product_removeproperty":
                        strOut = ProductFunctions.RemoveProperty(context);
                        break;
                    case "product_removerelated":
                        strOut = ProductFunctions.RemoveRelatedProduct(context);
                        break;
                    case "product_addrelatedproduct":
                        strOut = ProductFunctions.AddRelatedProduct(context);
                        break;
                    case "product_getproductselectlist":
                        strOut = ProductFunctions.GetProductSelectList(context);
                        break;
                    case "product_getclientselectlist":
                        strOut = ProductFunctions.GetClientSelectList(context);
                        break;
                    case "product_addproductclient":
                        strOut = ProductFunctions.AddProductClient(context);
                        break;
                    case "product_productclients":
                        strOut = ProductFunctions.GetProductClients(context);
                        break;
                    case "product_removeproductclient":
                        strOut = ProductFunctions.RemoveProductClient(context);
                        break;
                    case "product_selectchangedisable":
                        strOut = ProductFunctions.ProductDisable(context);
                        break;
                    case "product_selectchangehidden":
                        strOut = ProductFunctions.ProductHidden(context);
                        break;
                }
            }
            return strOut;
        }

        public static String ProductAdminDetail(HttpContext context, int productid = 0)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    var strOut = "";
                    var selecteditemid = settings["selecteditemid"];
                    if (productid > 0) selecteditemid = productid.ToString();
                    if (Utils.IsNumeric(selecteditemid))
                    {

                        if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                        if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                        if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                        if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");

                        var themeFolder = settings["themefolder"];

                        var razortemplate = settings["razortemplate"];
                        var portalId = Convert.ToInt32(settings["portalid"]);

                        var passSettings = settings;
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";

                        if (themeFolder == "")
                        {
                            themeFolder = StoreSettings.Current.ThemeFolder;
                            if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                        }

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid),"PRDLANG", _editlang);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, "/DesktopModules/NBright/NBrightBuy", themeFolder, _editlang, passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ProductAdminSaveExit(HttpContext context)
        {
            try
            {
                ProductSave(context);
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static String ProductAdminSaveAs(HttpContext context)
        {
            try
            {
                ProductSave(context, true);
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ProductAdminAddNew(HttpContext context)
        {
            try
            {
                var productid = ProductSave(context, true);
                if (productid > 0)
                {
                    return ProductAdminDetail(context, productid);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ProductAdminSave(HttpContext context)
        {
            try
            {
                try
                {
                    ProductSave(context);
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static int ProductSave(HttpContext context, bool newrecord = false)
        {
            if (NBrightBuyUtils.CheckManagerRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var itemid = -1;
                if (!newrecord)
                {
                    itemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                }
                if (itemid != 0)
                {
                    var prdData = new ProductData(itemid, _editlang);
                    var modelXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdatemodeldata"));
                    var optionXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptiondata"));
                    var optionvalueXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptionvaluesdata"));

                    prdData.UpdateModels(modelXml, _editlang);
                    prdData.UpdateOptions(optionXml, _editlang);
                    prdData.UpdateOptionValues(optionvalueXml, _editlang);
                    prdData.UpdateImages(ajaxInfo);
                    prdData.UpdateDocs(ajaxInfo);

                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateproductimages");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptionvaluesdata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptiondata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdatemodeldata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptionvaluesdata");
                    var productXml = ajaxInfo.XMLData;

                    prdData.Update(productXml);
                    prdData.Save();

                    // remove save GetData cache
                    var strCacheKey = prdData.Info.ItemID.ToString("") + "*PRDLANG*" + "*" + _editlang;
                    Utils.RemoveCache(strCacheKey);
                    DataCache.ClearCache();

                    return prdData.Info.ItemID;

                }

            }
            return -1;
        }


        public static string ProductDisable(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                if (parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), _editlang, false);
                    if (prodData.Disabled)
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkdisable", "False");
                    }
                    else
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkdisable", "True");
                    }
                    prodData.Save();
                    // remove save GetData cache
                    var strCacheKey = prodData.Info.ItemID.ToString("") + "*PRDLANG*" + "*" + _editlang;
                    Utils.RemoveCache(strCacheKey);

                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string ProductHidden(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                if (parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), _editlang, false);
                    if (prodData.DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden"))
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "False");
                    }
                    else
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "True");
                    }
                    prodData.Save();
                    // remove save GetData cache
                    var strCacheKey = prodData.Info.ItemID.ToString("") + "*PRDLANG*" + "*" + _editlang;
                    Utils.RemoveCache(strCacheKey);

                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        public static String ProductAdminList(HttpContext context, bool paging = true)
        {
            var settings = NBrightBuyUtils.GetAjaxDictionary(context);
            return ProductAdminList(settings, paging);
        }

        public static String ProductAdminList(Dictionary<string,string> settings, bool paging = true, string editlang = "")
        {

            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return "";

                    if (_editlang == "") _editlang = editlang;

                    var strOut = "";

                    if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                    if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                    if (!settings.ContainsKey("header")) settings.Add("header", "");
                    if (!settings.ContainsKey("body")) settings.Add("body", "");
                    if (!settings.ContainsKey("footer")) settings.Add("footer", "");
                    if (!settings.ContainsKey("filter")) settings.Add("filter", "");
                    if (!settings.ContainsKey("orderby")) settings.Add("orderby", "");
                    if (!settings.ContainsKey("returnlimit")) settings.Add("returnlimit", "0");
                    if (!settings.ContainsKey("pagenumber")) settings.Add("pagenumber", "0");
                    if (!settings.ContainsKey("pagesize")) settings.Add("pagesize", "0");
                    if (!settings.ContainsKey("searchtext")) settings.Add("searchtext", "");
                    if (!settings.ContainsKey("searchcategory")) settings.Add("searchcategory", "");
                    if (!settings.ContainsKey("cascade")) settings.Add("cascade", "False");

                    if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                    // select a specific entity data type for the product (used by plugins)
                    if (!settings.ContainsKey("entitytypecode")) settings.Add("entitytypecode", "PRD");
                    if (!settings.ContainsKey("entitytypecodelang")) settings.Add("entitytypecodelang", "PRDLANG");
                    var entitytypecodelang = settings["entitytypecodelang"];
                    var entitytypecode = settings["entitytypecode"];

                    var themeFolder = settings["themefolder"];
                    var header = settings["header"];
                    var body = settings["body"];
                    var footer = settings["footer"];
                    var filter = settings["filter"];
                    var orderby = settings["orderby"];
                    var returnLimit = Convert.ToInt32(settings["returnlimit"]);
                    var pageNumber = Convert.ToInt32(settings["pagenumber"]);
                    var pageSize = Convert.ToInt32(settings["pagesize"]);
                    var cascade = Convert.ToBoolean(settings["cascade"]);
                    var razortemplate = settings["razortemplate"];
                    var portalId = Convert.ToInt32(settings["portalid"]);

                    var searchText = settings["searchtext"];
                    var searchCategory = settings["searchcategory"];

                    if (searchText != "") filter += " and (NB3.[ProductName] like '%" + searchText + "%' or NB3.[ProductRef] like '%" + searchText + "%' or NB3.[Summary] like '%" + searchText + "%' ) ";

                    if (Utils.IsNumeric(searchCategory))
                    {
                        if (orderby == "{bycategoryproduct}") orderby += searchCategory;
                        var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                        var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                        if (!cascade)
                            filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") ";
                        else
                            filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") or (typecode = 'CATCASCADE' and XrefItemId = " + searchCategory + ")) ";
                    }
                    else
                    {
                        if (orderby == "{bycategoryproduct}") orderby = " order by NB3.productname ";
                    }

                    // logic for client list of products
                    if (NBrightBuyUtils.IsClientOnly())
                    {
                        filter += " and NB1.ItemId in (select ParentItemId from dbo.[NBrightBuy] as NBclient where NBclient.TypeCode = 'USERPRDXREF' and NBclient.UserId = " + UserController.Instance.GetCurrentUserInfo().UserID.ToString("") + ") ";
                    }

                    var recordCount = 0;

                    if (themeFolder == "")
                    {
                        themeFolder = StoreSettings.Current.ThemeFolder;
                        if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                    }


                    var objCtrl = new NBrightBuyController();

                    if (paging) // get record count for paging
                    {
                        if (pageNumber == 0) pageNumber = 1;
                        if (pageSize == 0) pageSize = 20;

                        // get only entity type required
                        recordCount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, entitytypecode, filter, entitytypecodelang, _editlang);

                    }

                    // get selected entitytypecode.
                    var list = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, entitytypecode, entitytypecodelang, _editlang, filter, orderby, StoreSettings.Current.DebugMode, "", returnLimit, pageNumber, pageSize, recordCount);

                    var passSettings = settings;
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", list, "/DesktopModules/NBright/NBrightBuy", themeFolder, _editlang, passSettings);

                    // add paging if needed
                    if (paging && (recordCount > pageSize))
                    {
                        var pg = new NBrightCore.controls.PagingCtrl();
                        strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
                    }

                    return strOut;

                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String MoveProductAdmin(HttpContext context)
        {
            try
            {

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var moveproductid = ajaxInfo.GetXmlPropertyInt("moveproductid");
                var movetoproductid = ajaxInfo.GetXmlPropertyInt("movetoproductid");
                var searchcategory = ajaxInfo.GetXmlPropertyInt("searchcategory");
                if (searchcategory > 0 && movetoproductid > 0 && moveproductid > 0)
                {
                    var objCtrl = new NBrightBuyController();
                    objCtrl.GetListCustom(PortalSettings.Current.PortalId, -1, "NBrightBuy_MoveProductinCateogry", 0, "", searchcategory + ";" + moveproductid + ";" + movetoproductid);
                }

                DataCache.ClearCache();

                return ProductFunctions.ProductAdminList(context);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String AddModel(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                        var addqty = ajaxInfo.GetXmlPropertyInt("genxml/hidden/addqty");

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";


                        var itemId = Convert.ToInt32(selecteditemid);
                        var prodData = ProductUtils.GetProductData(itemId, _editlang);
                        var lp = 1;
                        var rtnKeys = new List<String>();
                        while (lp <= addqty)
                        {
                            rtnKeys.Add(prodData.AddNewModel());
                            lp += 1;
                            if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                        }
                        prodData.Save();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);
                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);

                        if (themeFolder == "")
                        {
                            themeFolder = StoreSettings.Current.ThemeFolder;
                            if (ajaxInfo.GetXmlProperty("genxml/hidden/themefolder") != "") themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        }

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.Get(Convert.ToInt32(selecteditemid), "PRDLANG", _editlang);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, "/DesktopModules/NBright/NBrightBuy", themeFolder, _editlang, passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String AddOption(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (selecteditemid > 0)
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                        var addqty = ajaxInfo.GetXmlPropertyInt("genxml/hidden/addqty");

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";


                        var itemId = Convert.ToInt32(selecteditemid);
                        var prodData = ProductUtils.GetProductData(itemId, _editlang);
                        var lp = 1;
                        var rtnKeys = new List<String>();
                        while (lp <= addqty)
                        {
                            rtnKeys.Add(prodData.AddNewOption());
                            lp += 1;
                            if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                        }
                        prodData.Save();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);
                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(selecteditemid, "PRDLANG", _editlang);

                        strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductOptions.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, passSettings);

                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String AddOptionValues(HttpContext context)
        {

            try
            {
                //get uploaded params
                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                if (!settings.ContainsKey("selectedoptionid")) return "";

                var optionid = settings["selectedoptionid"];
                var productitemid = settings["selecteditemid"];
                var qty = settings["addqty"];
                if (!Utils.IsNumeric(qty)) qty = "1";

                var strOut = "No Product ID ('itemid' hidden fields needed on input form)";
                if (Utils.IsNumeric(productitemid) )
                {
                    var itemId = Convert.ToInt32(productitemid);
                    if (itemId > 0)
                    {

                        var prodData = ProductUtils.GetProductData(itemId, _editlang);


                        var passSettings = settings;
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        var lp = 1;
                        while (lp <= Convert.ToInt32(qty))
                        {
                            prodData.AddNewOptionValue(optionid);
                            lp += 1;
                            if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                        }
                        prodData.Save();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);


                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                        strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductOptionValues.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, passSettings);

                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    }
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String DeleteProduct(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var itemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (itemid > 0)
                    {
                        var prdData = new ProductData(itemid, _editlang);
                        prdData.Delete();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemid);
                        NBrightBuyUtils.RemoveModCachePortalWide(ajaxInfo.PortalId);
                        return "OK";
                   }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #region "fileupload"

        public static string UpdateProductImages(HttpContext context)
        {
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            var imguploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/imguploadlist");
            var strOut = "";

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


                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductImages.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

            }
            return strOut;
        }

        public static String ResizeImage(String fullName, int imgSize = 640)
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


        public static void AddNewImage(int itemId, String imageurl, String imagepath)
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


        public static string FileUpload(HttpContext context, string itemid = "")
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
        public static String UploadFile(HttpContext context, string itemid = "")
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
        public static String UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses, string itemid = "")
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
        public static String UploadWholeFile(HttpContext context, List<FilesStatus> statuses, string itemid = "")
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



        #endregion

        #region "Docs"


        public static string UpdateProductDocs(HttpContext context)
        {
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var settings = ajaxInfo.ToDictionary();

            var strOut = "";

            if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
            var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            var docuploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/docuploadlist");

            if (Utils.IsNumeric(productitemid))
            {
                var docs = docuploadlist.Split(',');
                foreach (var doc in docs)
                {
                    if (doc != "")
                    {
                        string fullName = StoreSettings.Current.FolderTempMapPath + "\\" + doc;
                        var extension = Path.GetExtension(fullName);
                        if ((extension.ToLower() == ".pdf" || extension.ToLower() == ".zip"))
                        {
                            if (File.Exists(fullName))
                            {
                                var newDocFileName = StoreSettings.Current.FolderDocumentsMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + Guid.NewGuid() + extension;
                                File.Copy(fullName, newDocFileName, true);
                                var docurl = StoreSettings.Current.FolderDocuments.TrimEnd('/') + "/" + Path.GetFileName(newDocFileName);
                                AddNewDoc(Convert.ToInt32(productitemid), newDocFileName, doc);
                            }
                        }
                    }
                }
                // clear any cache for the product.
                ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, Convert.ToInt32(productitemid));

                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductDocs.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

            }
            return strOut;
        }

        public static void AddNewDoc(int itemId, String filepath, String orginalfilename)
        {
            var objCtrl = new NBrightBuyController();
            var dataRecord = objCtrl.Get(itemId);
            if (dataRecord != null)
            {
                var fileext = Path.GetExtension(orginalfilename);
                var strXml = "<genxml><docs><genxml><hidden><filepath>" + filepath + "</filepath><fileext>" + fileext + "</fileext></hidden><textbox><txtfilename>" + orginalfilename + "</txtfilename></textbox></genxml></docs></genxml>";
                if (dataRecord.XMLDoc.SelectSingleNode("genxml/docs") == null)
                {
                    dataRecord.AddXmlNode(strXml, "genxml/docs", "genxml");
                }
                else
                {
                    dataRecord.AddXmlNode(strXml, "genxml/docs/genxml", "genxml/docs");
                }
                objCtrl.Update(dataRecord);
            }
        }



        #endregion


        #region "Categories"

        public static string AddProductCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlProperty("genxml/hidden/selectedcatid");
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), _editlang, false);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string SetDefaultCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), _editlang, false);
                    prodData.SetDefaultCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        public static string RemoveProductCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), _editlang, false);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }



        public static String GetProductCategories(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductCategories.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        #endregion


        #region "Properties"

        public static String GetPropertyListBox(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            ajaxInfo.Lang = Utils.GetCurrentCulture();
            var strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductPropertySelect.cshtml", 0, "", ajaxInfo, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

            return strOut;
        }

        public static string AddProperty(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(parentitemid, _editlang, false);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProperties(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string RemoveProperty(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(parentitemid, _editlang, false);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProperties(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static String GetProperties(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductProperties.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        #endregion

        #region "related products"

        public static string RemoveRelatedProduct(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selectedrelatedid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedrelatedid");
                if (productid > 0 && selectedrelatedid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), _editlang, false);
                    prodData.RemoveRelatedProduct(Convert.ToInt32(selectedrelatedid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string AddRelatedProduct(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selectedrelatedid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedrelatedid");
                if (selectedrelatedid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), _editlang, false);
                    if (prodData.Exists) prodData.AddRelatedProduct(Convert.ToInt32(selectedrelatedid));

                    // do bi-direction
                    var prodData2 = ProductUtils.GetProductData(Convert.ToInt32(selectedrelatedid), _editlang, false);
                    if (prodData2.Exists) prodData2.AddRelatedProduct(Convert.ToInt32(productid));

                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static String GetProductRelated(HttpContext context)
        {
            try
            {

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductRelated.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String GetProductSelectList(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                ajaxInfo.SetXmlProperty("genxml/hidden/razortemplate", "Admin_ProductSelectList.cshtml");
                return ProductAdminList(ajaxInfo.ToDictionary(), true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion


        #region "Clients"

        public static string RemoveProductClient(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selecteduserid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteduserid");
                if (selecteduserid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), _editlang, false);
                    if (!(NBrightBuyUtils.IsClientOnly() && (Convert.ToInt32(selecteduserid) == UserController.Instance.GetCurrentUserInfo().UserID)))
                    {
                        // ClientEditor role cannot remove themselves.
                        prodData.RemoveClient(Convert.ToInt32(selecteduserid));
                    }
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductClients(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string AddProductClient(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selecteduserid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteduserid");
                if (selecteduserid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), _editlang, false);
                    if (prodData.Exists) prodData.AddClient(Convert.ToInt32(selecteduserid));

                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductClients(context);
                }
                return "Invalid itemid or selecteduserid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string GetClientSelectList(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var searchtext = ajaxInfo.GetXmlProperty("genxml/hidden/searchtext");

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _editlang);
                var objCtrl = new NBrightBuyController();
                var userlist = objCtrl.GetDnnUsers(prodData.Info.PortalId, "%" + searchtext + "%", 0, 1, 20, 20);
                var strOut = "";
                if (userlist.Count > 0)
                {
                    strOut = NBrightBuyUtils.RazorTemplRenderList("Admin_ProductClientSelect.cshtml", 0, "", userlist, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());
                }
                return  strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static string GetProductClients(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), "PRDLANG", _editlang);

                strOut = NBrightBuyUtils.RazorTemplRender("Admin_ProductClients.cshtml", 0, "", info, "/DesktopModules/NBright/NBrightBuy", "config", _editlang, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        #endregion


        #endregion


    }
}

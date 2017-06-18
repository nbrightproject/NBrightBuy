using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
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
                    case "product_admin_save":
                        strOut = ProductFunctions.ProductAdminSave(context);
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
                }
            }
            return strOut;
        }

        public static String ProductAdminDetail(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    var strOut = "";
                    var selecteditemid = settings["selecteditemid"];
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

        public static String ProductAdminSave(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var itemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                    if (itemid > 0)
                    {
                        var prdData = new ProductData(itemid, _editlang);
                        var modelXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdatemodeldata"));
                        ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdatemodeldata");
                        var optionXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptiondata"));
                        ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptiondata");
                        var optionvalueXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptionvaluesdata"));
                        ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptionvaluesdata");

                        var productXml = ajaxInfo.XMLData;

                        prdData.Update(productXml);
                        prdData.UpdateModels(modelXml,_editlang);
                        prdData.UpdateOptions(optionXml, _editlang);
                        prdData.UpdateOptionValues(optionvalueXml, _editlang);
                        prdData.Save();

                        // remove save GetData cache
                        var strCacheKey = prdData.Info.ItemID.ToString("") + "*PRDLANG*" + "*" + _editlang;
                        Utils.RemoveCache(strCacheKey);
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ProductAdminList(HttpContext context, bool paging = true)
        {
            var settings = NBrightBuyUtils.GetAjaxDictionary(context);
            return ProductAdminList(settings, paging);
        }

        public static String ProductAdminList(Dictionary<string,string> settings, bool paging = true)
        {

            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return "";

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
                            rtnKeys.Add(prodData.AddNewOption());
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


        #endregion


    }
}

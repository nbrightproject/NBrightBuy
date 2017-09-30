using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Razor;
using System.Web.Script.Serialization;
using System.Windows.Forms.VisualStyles;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components.Clients
{
    public static class CategoryFunctions
    {
        public static string EditLangCurrent = "";
        public static string EntityTypeCode = "";
        public static string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        private static  NBrightBuyController _objCtrl = null;
        private static bool DebugMode => StoreSettings.Current.DebugMode;

        public static void ResetTemplateRelPath()
        {
            TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        }

        public static string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            _objCtrl = new NBrightBuyController();

            EditLangCurrent = editlang;
            if (EditLangCurrent == "") EditLangCurrent = Utils.GetCurrentCulture();

            var strOut = "CATEGORY - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            if (EntityTypeCode == "") EntityTypeCode = "CAT"; // default to category

            switch (paramCmd)
            {
                case "category_admin_getlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryAdminList(context);
                    break;
                case "category_admin_getdetail":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryAdminDetail(context);
                    break;
                case "category_admin_addnew":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryAdminAddNew(context);
                    break;                    
                case "category_admin_savelist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryAdminSaveList(context);
                    break;
                case "product_admin_save":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminSave(context);
                    break;
                case "product_admin_saveexit":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminSaveExit(context);
                    break;
                case "product_admin_saveas":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminSaveAs(context);
                    break;
                case "product_admin_selectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminList(context);
                    break;
                case "category_admin_movecategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = MoveCategoryAdmin(context);
                    break;
                case "product_addproductmodels":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddModel(context);
                    break;
                case "product_addproductoptions":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddOption(context);
                    break;
                case "product_addproductoptionvalues":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddOptionValues(context);
                    break;
                case "category_admin_delete":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = DeleteCategory(context);
                    break;
                case "category_updateimages":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = UpdateCategoryImages(context);
                    break;
                case "product_updateproductdocs":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.UpdateProductDocs(context);
                    break;
                case "product_addproductcategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddProductCategory(context);
                    break;
                case "product_removeproductcategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.RemoveProductCategory(context);
                    break;
                case "product_setdefaultcategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.SetDefaultCategory(context);
                    break;
                case "product_populatecategorylist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.GetPropertyListBox(context);
                    break;
                case "product_addproperty":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddProperty(context);
                    break;
                case "product_removeproperty":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.RemoveProperty(context);
                    break;
                case "product_removerelated":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.RemoveRelatedProduct(context);
                    break;
                case "product_addrelatedproduct":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddRelatedProduct(context);
                    break;
                case "category_getproductselectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = ProductFunctions.GetProductSelectList(context);
                    break;
                case "product_getclientselectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.GetClientSelectList(context);
                    break;
                case "product_addproductclient":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.AddProductClient(context);
                    break;
                case "product_productclients":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.GetProductClients(context);
                    break;
                case "product_removeproductclient":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.RemoveProductClient(context);
                    break;
                case "product_selectchangedisable":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductDisable(context);
                    break;
                case "category_selectchangehidden":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryHidden(context);
                    break;
                case "product_ajaxview_getlist":
                    //strOut = ProductFunctions.ProductAjaxViewList(context);
                    break;
                case "category_categoryproductlist":
                    strOut = GetCategoryProductList(context);
                    break;
                case "category_removeimage":
                    strOut = RemoveCategoryImage(context);
                    break;
                case "category_displayproductselect":
                    strOut = CategoryProductSelect(context);
                    break;
                    

            }
            return strOut;
        }


        public static String CategoryAdminList(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var currentlang = ajaxInfo.GetXmlProperty("genxml/hidden/currentlang");
            var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
            if (razortemplate == "") razortemplate = "Admin_CategoryList.cshtml";
            var themefolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            if (themefolder == "") themefolder = "config";

            var grpCats = NBrightBuyUtils.GetCatList(ajaxInfo.GetXmlPropertyInt("genxml/hidden/catid"), "cat", currentlang);
            var strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", grpCats, TemplateRelPath,
                themefolder, EditLangCurrent, StoreSettings.Current.Settings());

            return strOut;
        }

        public static String CategoryAdminAddNew(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var currentlang = ajaxInfo.GetXmlProperty("genxml/hidden/currentlang");
            var categoryData = CategoryUtils.GetCategoryData(-1, currentlang);
            categoryData.GroupType = "cat";
            var grpCtrl = new GrpCatController(Utils.GetCurrentCulture());
            var grp = grpCtrl.GetGrpCategoryByRef(categoryData.GroupType);
            if (grp != null) categoryData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", grp.categoryid.ToString(""));
            categoryData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "true");
            categoryData.DataRecord.ParentItemId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/catid");
            categoryData.Save();
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context);
        }

        public static String DeleteCategory(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var selectedcatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            var currentlang = ajaxInfo.GetXmlProperty("genxml/hidden/currentlang");
            var categoryData = CategoryUtils.GetCategoryData(selectedcatid, currentlang);
            categoryData.Delete();
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context);
        }


        public static String CategoryAdminSaveList(HttpContext context)
        {
            var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);

            foreach (var nbi in ajaxInfoList)
            {
                if (nbi.GetXmlPropertyBool("genxml/hidden/isdirty"))
                {
                    var categoryData = CategoryUtils.GetCategoryData(nbi.GetXmlPropertyInt("genxml/hidden/itemid"), nbi.GetXmlProperty("genxml/hidden/categorylang"));
                    if (categoryData.Exists)
                    {
                        categoryData.Name = nbi.GetXmlProperty("genxml/textbox/txtcategoryname");
                        categoryData.Save();
                    }
                }
            }
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return "";
        }

        public static String MoveCategoryAdmin(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var movecatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movecatid");
            var movetocatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movetocatid");

            if (movecatid > 0 && movetocatid > 0)
            {
                MoveRecord(movetocatid, movecatid);
            }

            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context);
        }

        private static void MoveRecord(int movetocatid, int movecatid)
        {
            if (movecatid > 0)
            {
                var movData = CategoryUtils.GetCategoryData(movetocatid, StoreSettings.Current.EditLanguage);
                var selData = CategoryUtils.GetCategoryData(movecatid, StoreSettings.Current.EditLanguage);
                if (movData.Exists && selData.Exists)
                {
                    var fromParentItemid = selData.DataRecord.ParentItemId;
                    var toParentItemid = movData.DataRecord.ParentItemId;
                    var reindex = toParentItemid != fromParentItemid;
                    var objGrpCtrl = new GrpCatController(StoreSettings.Current.EditLanguage);
                    var movGrp = objGrpCtrl.GetGrpCategory(movData.Info.ItemID);
                    if (!movGrp.Parents.Contains(selData.Info.ItemID)) // cannot move a category into itself (i.e. move parent into sub-category)
                    {
                        selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", toParentItemid.ToString(""));
                        selData.DataRecord.ParentItemId = toParentItemid;
                        selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", movData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));
                        var strneworder = movData.DataRecord.GetXmlPropertyDouble("genxml/hidden/recordsortorder");
                        var selorder = selData.DataRecord.GetXmlPropertyDouble("genxml/hidden/recordsortorder");
                        var neworder = Convert.ToDouble(strneworder, CultureInfo.GetCultureInfo("en-US"));
                        if (strneworder < selorder)
                            neworder = neworder - 0.5;
                        else
                            neworder = neworder + 0.5;
                        selData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", neworder.ToString(""), TypeCode.Double);
                        var objCtrl = new NBrightBuyController();
                        objCtrl.Update(selData.DataRecord);

                        FixRecordSortOrder(toParentItemid.ToString("")); //reindex all siblings (this is so we get a int on the recordsortorder)
                        FixRecordGroupType(selData.Info.ItemID.ToString(""), selData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));

                        if (reindex)
                        {
                            objGrpCtrl.ReIndexCascade(fromParentItemid); // reindex from parent and parents.
                            objGrpCtrl.ReIndexCascade(selData.Info.ItemID); // reindex select and parents
                        }
                        NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
                    }
                }
            }
        }

        private static void FixRecordGroupType(String parentid, String groupType)
        {
            if (Utils.IsNumeric(parentid) && Convert.ToInt32(parentid) > 0)
            {
                // fix any incorrect sort orders
                var strFilter = " and NB1.ParentItemId = " + parentid + " ";
                var levelList = _objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLangCurrent, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
                foreach (NBrightInfo catinfo in levelList)
                {
                    var grouptype = catinfo.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, StoreSettings.Current.EditLanguage);
                    if (grouptype != groupType)
                    {
                        catData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", groupType);
                        _objCtrl.Update(catData.DataRecord);
                    }
                    FixRecordGroupType(catData.Info.ItemID.ToString(""), groupType);
                }
            }
        }

        private static void FixRecordSortOrder(String parentid)
        {
            if (!Utils.IsNumeric(parentid)) parentid = "0";
            // fix any incorrect sort orders
            Double lp = 1;
            var strFilter = " and NB1.ParentItemId = " + parentid + " ";
            var levelList = _objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLangCurrent, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var recordsortorder = catinfo.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(recordsortorder) || Convert.ToDouble(recordsortorder, CultureInfo.GetCultureInfo("en-US")) != lp)
                {
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, StoreSettings.Current.EditLanguage);
                    catData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", lp.ToString(""));
                    _objCtrl.Update(catData.DataRecord);
                }
                lp += 1;
            }
        }


        public static string CategoryHidden(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (parentitemid > 0)
                {
                    var catData = CategoryUtils.GetCategoryData(parentitemid, StoreSettings.Current.EditLanguage);

                    if (catData.DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden"))
                    {
                        catData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "False");
                    }
                    else
                    {
                        catData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "True");
                    }
                    catData.Save();
                    // remove save GetData cache
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        public static String CategoryAdminDetail(HttpContext context, int catid = 0)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                    if (catid > 0) selecteditemid = catid;
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = PortalSettings.Current.PortalId;

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (selecteditemid <= 0) return "";

                        if (themeFolder == "") themeFolder = StoreSettings.Current.ThemeFolder;

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid), EntityTypeCode + "LANG", EditLangCurrent,true);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, TemplateRelPath, themeFolder, EditLangCurrent, passSettings);
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

        private static String GetCategoryProductList(HttpContext context)
        {
            try
            {
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {Settings:itemid}) ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, settings);


                if (!settings.ContainsKey("filter")) settings.Add("filter", strFilter);
                if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                settings["razortemplate"] = "Admin_CategoryProducts.cshtml";
                settings["themefolder"] = "config";

                var list = ProductFunctions.ProductAdminList(settings, true, EditLangCurrent);
                if (!settings.ContainsKey("entitytypecode")) settings.Add("entitytypecode", "PRD");
                var entitytypecode = settings["entitytypecode"];

                var pluginData = new PluginData(PortalSettings.Current.PortalId);
                var provList = pluginData.GetAjaxProviders();
                foreach (var d in provList)
                {
                    var ajaxprov = AjaxInterface.Instance(d.Key);
                    if (ajaxprov != null)
                    {
                        if (entitytypecode != ajaxprov.Ajaxkey)
                        {
                            var provlist = ProductFunctions.ProductAdminList(settings, true, "", ajaxprov.Ajaxkey);
                            list = provlist + list;
                        }
                    }
                }

                return list;


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        private static String CategoryProductSelect(HttpContext context)
        {
            try
            {
                var strOut = "";
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = "config";
                        var razortemplate = "Admin_CategoryProductSelect.cshtml";

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (selecteditemid <= 0) return "";

                        if (themeFolder == "") themeFolder = StoreSettings.Current.ThemeFolder;

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid), EntityTypeCode + "LANG", EditLangCurrent, true);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, TemplateRelPath, themeFolder, EditLangCurrent, passSettings);
                    }
                }
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        #region "fileupload"

        public static string UpdateCategoryImages(HttpContext context)
        {
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var catitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            var imguploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/imguploadlist");
            var strOut = "";

            if (catitemid > 0)
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
                            var imagepath = ProductFunctions.ResizeImage(fullName, imgResize);
                            var imageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + Path.GetFileName(imagepath);
                            AddNewImage(catitemid, imageurl, imagepath);
                        }
                    }
                }
            }
            return CategoryAdminDetail(context);
        }

        private static void AddNewImage(int itemId, String imageurl, String imagepath)
        {
            var catData = new CategoryData(itemId,EditLangCurrent);
            if (catData.Exists)
            {
                catData.DataRecord.SetXmlProperty("genxml/hidden/imageurl", imageurl);
                catData.DataRecord.SetXmlProperty("genxml/hidden/imagepath", imagepath);
                catData.Save();
            }
        }

        public static string RemoveCategoryImage(HttpContext context)
        {
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var catitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");

            var catData = new CategoryData(catitemid, EditLangCurrent);
            if (catData.Exists)
            {
                catData.DataRecord.SetXmlProperty("genxml/hidden/imageurl", "");
                catData.DataRecord.SetXmlProperty("genxml/hidden/imagepath", "");
                catData.Save();
            }
            return CategoryAdminDetail(context);
        }

        #endregion

    }
}

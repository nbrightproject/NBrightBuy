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
using System.Windows.Forms;
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
                case "category_admin_movecategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = MoveCategoryAdmin(context);
                    break;
                case "category_admin_delete":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = DeleteCategory(context);
                    break;
                case "category_updateimages":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = UpdateCategoryImages(context);
                    break;
                case "category_getproductselectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = ProductFunctions.ProductAdminList(context, true, EditLangCurrent,"",true);
                    break;
                case "category_selectchangehidden":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryHidden(context);
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
                case "category_selectcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = SelectCatXref(context);
                    break;
                case "category_deletecatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = DeleteCatXref(context);
                    break;
                case "category_deleteallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = DeleteAllCatXref(context);
                    break;
                case "category_copyallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CopyAllCatXref(context);
                    break;
                case "category_moveallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CopyAllCatXref(context, true);
                    break;
                case "category_cattaxupdate":
                    if (NBrightBuyUtils.CheckRights()) strOut = CatTaxUpdate(context);
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
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {Settings:itemid}) ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, ajaxInfo.ToDictionary());

                ajaxInfo.SetXmlProperty("genxml/hidden/filter", strFilter);
                ajaxInfo.SetXmlProperty("genxml/hidden/razortemplate", "Admin_CategoryProducts.cshtml");
                ajaxInfo.SetXmlProperty("genxml/hidden/themefolder", "config");

                return ProductFunctions.ProductAdminList(context, true, EditLangCurrent,"",true);

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

        private static string SelectCatXref(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var selectedproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                if (selectedproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectedproductid, EditLangCurrent, false);
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
        private static string DeleteAllCatXref(HttpContext context)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, EditLangCurrent);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, EditLangCurrent);
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

        private static string DeleteCatXref(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var selectproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                if (selectproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectproductid, EditLangCurrent, false);
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


        private static String CopyAllCatXref(HttpContext context, Boolean moverecords = false)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var newcatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/newcatid");

                if (newcatid > 0 && catid > 0 && catid != newcatid)
                {

                    NBrightBuyUtils.CopyAllCatXref(catid, Convert.ToInt32(newcatid), moverecords);

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

        private static string CatTaxUpdate(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var taxrate = ajaxInfo.GetXmlProperty("genxml/hidden/selecttaxrate");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, EditLangCurrent);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var strXml = "<genxml><models>";
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, EditLangCurrent);
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



    }
}

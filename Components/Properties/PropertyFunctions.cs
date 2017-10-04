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
    public static class PropertyFunctions
    {
        public static string UiLang = "";
        public static string EditLangCurrent = "";
        public static string EntityTypeCode = "";
        public static string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        private static  NBrightBuyController _objCtrl = new NBrightBuyController();
        private static bool DebugMode => StoreSettings.Current.DebugMode;

        public static void ResetTemplateRelPath()
        {
            TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        }

        public static string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {

            EditLangCurrent = editlang;
            if (EditLangCurrent == "") EditLangCurrent = Utils.GetCurrentCulture();

            var strOut = "PROPERTY - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            UiLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang");
            if (UiLang == "") UiLang = EditLangCurrent;
            var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            if (EntityTypeCode == "") EntityTypeCode = "CAT"; // default to category

            switch (paramCmd)
            {
                case "property_admin_getlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategoryAdminList(context, "property");
                    break;
                case "property_admin_getdetail":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategoryAdminDetail(context);
                    break;
                case "property_admin_addnew":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategoryAdminAddNew(context,"property");
                    break;                    
                case "property_admin_savelist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategoryAdminSaveList(context);
                    break;
                case "property_admin_save":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategorySave(context);
                    break;
                case "property_admin_saveexit":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategorySave(context);
                    break;
                case "property_admin_movecategory":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.MoveCategoryAdmin(context, "property");
                    break;
                case "property_admin_delete":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.DeleteCategory(context, "property");
                    break;
                case "property_updateimages":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.UpdateCategoryImages(context);
                    break;
                case "property_getproductselectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = ProductFunctions.ProductAdminList(context, true, EditLangCurrent,"",true);
                    break;
                case "property_selectchangehidden":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    strOut = CategoryFunctions.CategoryHidden(context);
                    break;
                case "property_categoryproductlist":
                    strOut = CategoryFunctions.GetCategoryProductList(context);
                    break;
                case "property_removeimage":
                    strOut = CategoryFunctions.RemoveCategoryImage(context);
                    break;
                case "property_displayproductselect":
                    strOut = CategoryFunctions.CategoryProductSelect(context);
                    break;
                case "property_selectcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.SelectCatXref(context);
                    break;
                case "property_deletecatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.DeleteCatXref(context);
                    break;
                case "property_deleteallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.DeleteAllCatXref(context);
                    break;
                case "property_copyallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.CopyAllCatXref(context);
                    break;
                case "property_moveallcatxref":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.CopyAllCatXref(context, true);
                    break;
                case "property_cattaxupdate":
                    if (NBrightBuyUtils.CheckRights()) strOut = CategoryFunctions.CatTaxUpdate(context);
                    break;
            }
            return strOut;
        }



    }
}

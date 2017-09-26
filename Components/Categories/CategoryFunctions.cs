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
        private static bool DebugMode => StoreSettings.Current.DebugMode;

        public static void ResetTemplateRelPath()
        {
            TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        }

        public static string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
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
                    strOut = CategoryFunctions.CategoryAdminList(context);
                    break;
                case "product_admin_getdetail":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminDetail(context);
                    break;
                case "product_adminaddnew":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductAdminAddNew(context);
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
                case "product_moveproductadmin":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.MoveProductAdmin(context);
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
                    strOut = ProductFunctions.AddOptionValues(context);
                    break;
                case "product_admin_delete":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.DeleteProduct(context);
                    break;
                case "product_updateproductimages":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.UpdateProductImages(context);
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
                case "product_getproductselectlist":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.GetProductSelectList(context);
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
                case "product_selectchangehidden":
                    if (!NBrightBuyUtils.CheckManagerRights()) break;
                    //strOut = ProductFunctions.ProductHidden(context);
                    break;
                case "product_ajaxview_getlist":
                    //strOut = ProductFunctions.ProductAjaxViewList(context);
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
            var strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", grpCats, TemplateRelPath, themefolder, EditLangCurrent, StoreSettings.Current.Settings());

            return strOut;
        }



    }
}

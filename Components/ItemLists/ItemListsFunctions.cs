using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.ItemLists
{
    public static class ItemListsFunctions
    {

        private static string _entityTypeCode;
        private static string _entityTypeCodeLang;
        private static string _themeFolder;
        private static string _templatename;

        public static string ProcessCommand(string paramCmd, HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            var itemId = ajaxInfo.GetXmlProperty("genxml/hidden/productid");
            var itemlistname = ajaxInfo.GetXmlProperty("genxml/hidden/itemlistname");
            _entityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            _entityTypeCodeLang = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecodelang");
            if (_entityTypeCode == "") _entityTypeCode = "PRD";
            if (_entityTypeCodeLang == "") _entityTypeCodeLang = "PRDLANG";
            _themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            _templatename = ajaxInfo.GetXmlProperty("genxml/hidden/templatename");
            if (_templatename == "") _templatename = "NBS_favoriteslist";
            if (_themeFolder == "") _themeFolder = "ClassicRazor";

            var strOut = "ORDER - ERROR!! - No Security rights for current user!";

            var cw = new ItemListData();

            switch (paramCmd)
            {
                case "itemlist_add":
                    if (Utils.IsNumeric(itemId))
                    {
                        cw.Add(itemlistname, itemId);
                        strOut = cw.products;
                    }
                    break;
                case "itemlist_remove":
                    if (Utils.IsNumeric(itemId))
                    {
                        cw.Remove(itemlistname, itemId);
                        strOut = cw.products;
                    }
                    break;
                case "itemlist_deletelist":
                    cw.DeleteList(itemlistname);
                    strOut = "deleted";
                    break;
                case "itemlist_productlist":
                    strOut = GetProductItemListHtml(cw, itemlistname);
                    break;
            }

            return strOut;
        }


        public static List<NBrightInfo> GetProductItemList(ItemListData itemListData,string listkey = "")
        {
            var strOut = "";
            var strFilter = "";
            var rtnList = new List<NBrightInfo>();
            if (itemListData.Exists && itemListData.ItemCount > 0)
            {
                var ModCtrl = new NBrightBuyController();
                if (listkey == "")
                {
                    foreach (var i in itemListData.listnames)
                    {
                        strFilter = " and (";
                        foreach (var i2 in itemListData.productsInList[i.Key])
                        {
                            strFilter += " NB1.itemid = '" + i2 + "' or";
                        }
                        strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                        strFilter += " and (NB3.Visible = 1) ";
                        var l = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, _entityTypeCode, _entityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, "");
                        foreach (var n in l)
                        {
                            n.SetXmlProperty("genxml/listkey",i.Key);
                            rtnList.Add(n);
                        }
                    }
                }
                else
                {
                    strFilter = " and (";
                    foreach (var i in itemListData.productsInList[listkey])
                    {
                        strFilter += " NB1.itemid = '" + i + "' or";
                    }
                    strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                    strFilter += " and (NB3.Visible = 1) ";
                    var l = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, _entityTypeCode, _entityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, "");
                    foreach (var n in l)
                    {
                        n.SetXmlProperty("genxml/listkey", listkey);
                        rtnList.Add(n);
                    }
                }
            }

            return rtnList;
        }

        public static string GetProductItemListHtml(ItemListData itemListData, string listkey = "")
        {
            var strOut = "";
            var rtnList = GetProductItemList(itemListData,listkey);
            var modelsetings = StoreSettings.Current.Settings();
            modelsetings.Add("listkeys", itemListData.listkeys);
            strOut = NBrightBuyUtils.RazorTemplRenderList(_templatename, -1, "", rtnList, "/DesktopModules/NBright/NBrightBuy", _themeFolder, Utils.GetCurrentCulture(), modelsetings);

            return strOut;
        }


    }

}

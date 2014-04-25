// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public static class NBrightBuyV2Utils
    {
        // this is a tempaory Link shared static class to interface with v2 of NB_Store during mirgation versions of NBrightBuy.

        public static int GetLegacyProductId(int productId)
        {
            var objCtrl = new NBrightBuyController();
            var obj = objCtrl.Get(productId,"",Utils.GetCurrentCulture());
            return GetLegacyProductId(obj);
        }

        public static int GetLegacyProductId(NBrightInfo objProduct)
        {
            // get legacyid (ProductId from v2 tables) using the modelid (nasty !!!)
            // we don't expose the legacyid in the NBright class becasue it will not be required once we move everything to the new DB structure. (Maybe a mistake!)
            if (objProduct != null)
            {
                var modid = objProduct.GetXmlProperty("genxml/models/genxml[1]/hidden/modelid");
                if (Utils.IsNumeric(modid)) // 1 model should always exist
                {
                    var objPCtrl = new ProductController();
                    var objM = objPCtrl.GetModel(Convert.ToInt32(modid), Utils.GetCurrentCulture());
                    if (objM != null) return objM.ProductID;
                }                
            }
            return -1;
        }

        public static Boolean HasRelatedProducts(NBrightInfo objProduct)
        {
            var productid = GetLegacyProductId(objProduct);
            var objPCtrl = new ProductController();
            var arylist = objPCtrl.GetProductRelatedList(PortalSettings.Current.PortalId, productid, Utils.GetCurrentCulture(), -1, false);
            if (arylist.Count > 0) return true;
            return false;
        }

        public static List<NBrightInfo> GetRelatedProducts(NBrightInfo objProduct,Boolean getAll = false)
        {
            var objCtrl = new NBrightBuyController();
            var productid = GetLegacyProductId(objProduct);
            var objPCtrl = new ProductController();
            var arylist = objPCtrl.GetProductRelatedList(PortalSettings.Current.PortalId, productid, Utils.GetCurrentCulture(), -1, getAll);
            var strSelectedIds = "";
            foreach (NB_Store_ProductRelatedListInfo obj in arylist)
            {
                strSelectedIds += obj.RelatedProductID.ToString("") + ",";
            }
            var strFilter = " and [LegacyItemId] in (" + strSelectedIds.TrimEnd(',') + ") ";
            var relList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRD", strFilter, "", 0, 0, 0, 0, "PRDLANG", Utils.GetCurrentCulture());
            return relList;
        }

        public static Boolean HasDocuments(NBrightInfo objProduct)
        {
            var productid = GetLegacyProductId(objProduct);
            var objPCtrl = new ProductController();
            var arylist = objPCtrl.GetProductDocList(productid, Utils.GetCurrentCulture());
            if (arylist.Count > 0) return true;
            return false;
        }

        public static Boolean HasPurchaseDocuments(NBrightInfo objProduct)
        {
            var productid = GetLegacyProductId(objProduct);
            var objPCtrl = new ProductController();
            var arylist = objPCtrl.GetProductDocList(productid, Utils.GetCurrentCulture());
            return arylist.Cast<NB_Store_ProductDocInfo>().Any(l => l.Purchase);
        }


        public static bool DocIsPurchaseOnlyByDocId(int docId)
        {
            var objCtrl = new ProductController();
            NB_Store_ProductDocInfo objDoc = default(NB_Store_ProductDocInfo);
            objDoc = objCtrl.GetProductDoc(docId, Utils.GetCurrentCulture());
            return objDoc.Purchase;
        }

        public static bool DocHasBeenPurchasedByDocId(int userId, int docId)
        {
            var objCtrl = new ProductController();
            NB_Store_ProductDocInfo objDoc = default(NB_Store_ProductDocInfo);
            objDoc = objCtrl.GetProductDoc(docId, Utils.GetCurrentCulture());
            return DocHasBeenPurchased(userId, objDoc.ProductID);
        }

        public static bool DocHasBeenPurchased(int userId, int productId)
        {
            if (userId >= 0)
            {
                var objCtrl = new ProductController();
                if (objCtrl.CheckIfProductPurchased(productId, userId) > 0) return true;
            }
            return false;
        }

        public static void AddToCart(Repeater rpData, NBrightInfo nbSettings, HttpRequest request, int rowIndex, Boolean debugMode = false)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER, rowIndex);

            if (debugMode)
            {
                var xmlDoc = new System.Xml.XmlDataDocument();
                xmlDoc.LoadXml(strXml);
                xmlDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addtobasket.xml");
            }

            // load into NbrigthInfo class, so it's easier to get at xml values.
            var objInfo = new NBrightInfo();
            objInfo.XMLData = strXml;

            var modelId = -1;
            var intQty = 1;
            var strXmlOut = "";

            // Get ModelID
            var strmodelId = objInfo.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
            if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfo.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
            if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfo.GetXmlProperty("genxml/hidden/modeldefault");
            if (Utils.IsNumeric(strmodelId)) modelId = Convert.ToInt32(strmodelId);

            // Get Qty
            var strqtyId = objInfo.GetXmlProperty("genxml/textbox/selectedaddqty");
            if (Utils.IsNumeric(strqtyId)) intQty = Convert.ToInt32(strqtyId);

            //build optionCode for cart
            var optCode = "";
            var nodList = objInfo.XMLDoc.SelectNodes("genxml/dropdownlist/*[starts-with(name(), 'optionddl')]");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    optCode += nod.InnerText + "-";
                }
            nodList = objInfo.XMLDoc.SelectNodes("genxml/checkbox/*[starts-with(name(), 'optionchk')]");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    var ctlName = nod.Name;
                    var chk = (CheckBox)rpData.Items[0].FindControl(ctlName);
                    if (chk.Checked) optCode += chk.Attributes["optionvalueid"] + "-";
                }

            // Get Text Input
            var txtOption = "";
            var optSep = nbSettings.GetXmlProperty("genxml/hidden/optionseperator.text");
            if (optSep == "") optSep = ",";
            var showOptionNames = nbSettings.GetXmlProperty("genxml/hidden/showOptionNames.flag") == "1";
            nodList = objInfo.XMLDoc.SelectNodes("genxml/textbox/*[starts-with(name(), 'optiontxt')]");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    var ctlName = nod.Name;
                    var txt = (TextBox)rpData.Items[0].FindControl(ctlName);
                    if (txt.Text != "")
                    {
                        var txtDesc = "";
                        if (showOptionNames) txtDesc = txt.Attributes["optiondesc"];
                        strXmlOut += "<" + nod.Name.ToLower() + ">" + txtDesc + txt.Text + "</" + nod.Name.ToLower() + ">";
                        if (txt.ToolTip == "")
                            txtOption += txtDesc + txt.Text + optSep;
                        else
                            txtOption += txtDesc + txt.ToolTip + "=" + txt.Text + optSep;                        
                    }
                }
            txtOption = txtOption.TrimEnd(Convert.ToChar(optSep));

            // save File upload name
            nodList = objInfo.XMLDoc.SelectNodes("genxml/files/ctrl");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    var fname = objInfo.GetXmlProperty("genxml/hidden/hid" + nod.InnerText);
                    if (fname != "")
                    {
                        var docPath = PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER + "\\" + fname;
                        var folderInfo = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, SharedFunctions.ORDERUPLOADFOLDER);
                        //[TODO: Not sure about this "folderInfo.StorageLocation", taken from NB_Store v2 code, but need to check if still OK in DNN6/7 now]
                        if (folderInfo.StorageLocation == Convert.ToInt32(FolderController.StorageLocationTypes.SecureFileSystem)) docPath = docPath + Globals.glbProtectedExtension;
                        //[TODO: allow for multiple file uploads by using a "key" param on the fileProvider.ashx, default to match v2 functionality here]
                        //strXmlOut += "<" + nod.InnerText.ToLower() + ">" + docPath + "</" + nod.InnerText.ToLower() + ">"; 
                        strXmlOut += "<fuupload>" + docPath + "</fuupload>"; 
                    }
                }

            if (strXmlOut != "") strXmlOut = "<root>" + strXmlOut + "</root>";

            //validate optcode and get ItemDesc
            optCode = modelId.ToString("") + "-" + optCode;
            var uInfo = UserController.GetCurrentUserInfo();
            var optCinfo = SharedFunctions.GetOptCodeInfo(PortalSettings.Current.PortalId, optCode, uInfo, txtOption, strXmlOut);
            optCode = optCinfo.OptCode;

            // Create Cart item
            var cartId = CurrentCart.getCartID(PortalSettings.Current.PortalId);

            // Add to Cart (v2)
            var objCartCtrl = new CartController();
            var objCartInfo = objCartCtrl.GetCartItemByOptCode(cartId, optCode);

            if (objCartInfo == null || txtOption != "") //create new cart item if textbox data has been entered
            {
                objCartInfo = new NB_Store_CartItemsInfo();
                objCartInfo.OptCode = optCode;
                objCartInfo.ItemDesc = optCinfo.ItemDesc;
                objCartInfo.DateCreated = DateTime.Now;
                objCartInfo.Discount = optCinfo.Discount;
                objCartInfo.ItemID = -1;
                objCartInfo.ModelID = modelId;
                objCartInfo.Quantity = intQty;
                objCartInfo.Tax = 0;
                objCartInfo.UnitCost = optCinfo.UnitCost;
                objCartInfo.XMLInfo = strXmlOut;
            }
            else
            {
                objCartInfo.DateCreated = DateTime.Now;
                objCartInfo.Discount = objCartInfo.Discount;
                //[TODO: "increment.cart" setting needs creating in NB_Store v2 and store level.]
                var incrementCart = nbSettings.GetXmlProperty("genxml/hidden/increment.cart") != "0";
                if (incrementCart)
                    objCartInfo.Quantity = objCartInfo.Quantity + intQty;
                else
                    objCartInfo.Quantity = intQty;
                objCartInfo.Tax = 0;
                objCartInfo.ItemDesc = optCinfo.ItemDesc;
                objCartInfo.UnitCost = optCinfo.UnitCost;
            }

            if (modelId >= 0) CurrentCart.AddItemToCart(PortalSettings.Current.PortalId,objCartInfo,request);

            //validate cart so cart level discount are calculated
            CurrentCart.ValidateCart(PortalSettings.Current.PortalId, uInfo);

        }


        public static Boolean IsInCart(NBrightInfo objProduct)
        {
            var productid = GetLegacyProductId(objProduct);
            var objPCtrl = new ProductController();
            var cartL = CurrentCart.GetCurrentCartItems(PortalSettings.Current.PortalId);
            foreach (NB_Store_CartItemsInfo c in cartL)
            {
                var m = objPCtrl.GetModel(c.ModelID, Utils.GetCurrentCulture());
                if (m.ProductID == productid) return true;
            }
            return false;
        }


        public static String FormatToStoreCurrency(Double value)
        {
            return SharedFunctions.FormatToStoreCurrency(PortalSettings.Current.PortalId, value);
        }

        public static String GetCurrencyIsoCode()
        {
            return SharedFunctions.getCurrencyISOCode();            
        }

    }

}


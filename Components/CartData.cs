using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class CartData
    {
        private int _cartId;
        private int _portalId;
        private string _cookieName;
        private DataStorageType _storageType;
        private HttpCookie _cookie;
        private NBrightInfo _cartInfo;
        private List<NBrightInfo> _itemList; 

        public CartData(int portalId, String storageType = "Cookie", string nameAppendix = "")
        {
            _cookieName = "NBrightBuyCart" + "*" + portalId.ToString("") + "*" + nameAppendix;
            Exists = false;
            _portalId = portalId;
            _cartId = GetCartId();
        }


        /// <summary>
        /// Save cart
        /// </summary>
        private void Save(Boolean debugMode = false)
        {
            //save cart
            var strXML = "<items>";
            foreach(var info in _itemList)
            {
                strXML += info.XMLData;
            }
            strXML += "</items>";
            _cartInfo.RemoveXmlNode("genxml/items");
            _cartInfo.AddXmlNode(strXML, "items", "genxml");

            var modCtrl = new NBrightBuyController();
            _cartInfo.ItemID = _cartId;
            _cartInfo.PortalId = _portalId;
            _cartInfo.ModuleId = -1;
            _cartInfo.TypeCode = "CART";
            var uInfo = UserController.GetCurrentUserInfo();
            _cartInfo.UserId = uInfo.UserID;
            _cartId = modCtrl.Update(_cartInfo);

            if (debugMode) _cartInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_currentcart.xml");

            //save cartid for client
            if (_storageType == DataStorageType.SessionMemory)
            {
                // save data to cache
                HttpContext.Current.Session[_cookieName] = _cartId;
            }
            else
            {
                _cookie["cartId"] = _cartId.ToString("");
                _cookie.Expires = DateTime.Now.AddDays(1d);
                HttpContext.Current.Response.Cookies.Add(_cookie);
            }
            Exists = true;

        }

        #region "base methods"

        /// <summary>
        /// Add product to cart
        /// </summary>
        /// <param name="rpData"></param>
        /// <param name="nbSettings"></param>
        /// <param name="rowIndex"></param>
        /// <param name="debugMode"></param>
        public String AddItem(Repeater rpData, NBrightInfo nbSettings, int rowIndex, Boolean debugMode = false)
        {

            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER, rowIndex);

            if (debugMode)
            {
                var xmlDoc = new System.Xml.XmlDataDocument();
                xmlDoc.LoadXml(strXml);
                xmlDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addtobasket.xml");
            }

            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            var objInfo = new NBrightInfo();
            objInfo.XMLData = "<item></item>";

            // get productid
            var strproductid = objInfoIn.GetXmlProperty("genxml/hidden/productid");
            if (Utils.IsNumeric(strproductid))
            {
                var itemcode = ""; // The itemcode var is used to decide if a cart item is new or already existing in the cart.
                var productData = new ProductData(Convert.ToInt32(strproductid));

                objInfo.AddSingleNode("productid", strproductid, "item");
                itemcode += strproductid + "-";

                // Get ModelID
                var strmodelId = objInfoIn.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/hidden/modeldefault");
                objInfo.AddSingleNode("modelid", strmodelId, "item");
                itemcode += strmodelId + "-";

                // Get Qty
                var strqtyId = objInfoIn.GetXmlProperty("genxml/textbox/selectedaddqty");
                objInfo.AddSingleNode("qty", strqtyId, "item");

                #region "Get model and product data"

                objInfo.AddSingleNode("productname", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"), "item");
                objInfo.AddSingleNode("summary", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary"), "item");

                objInfo.AddSingleNode("modelref", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtmodelref"), "item");
                objInfo.AddSingleNode("modeldesc", productData.Info.GetXmlProperty("genxml/lang/genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtmodelname"), "item");
                objInfo.AddSingleNode("modelextra", productData.Info.GetXmlProperty("genxml/lang/genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtextra"), "item");
                objInfo.AddSingleNode("unitcost", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtunitcost"), "item");
                objInfo.AddSingleNode("dealercost", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtdealercost"), "item");

                #endregion


                #region "Get option Data"

                //build option data for cart
                var strXmlIn = "<options>";
                var nodList = objInfoIn.XMLDoc.SelectNodes("genxml/textbox/*[starts-with(name(), 'optiontxt')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        strXmlIn += "<option>";
                        var idx = nod.Name.Replace("optiontxt", "");
                        var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                        var optvaltext = nod.InnerText;
                        strXmlIn += "<optid>" + optionid + "</optid>";
                        strXmlIn += "<optvaltext>" + optvaltext + "</optvaltext>";
                        var itemcodeText = "";
                        if (optvaltext.Length > 0) itemcodeText = optvaltext.Replace(" ", "").Substring(0, optvaltext.Replace(" ", "").Length - 1);
                        itemcode += optionid + itemcodeText + "-";
                        strXmlIn += "<optname>" + productData.Info.GetXmlProperty("genxml/lang/genxml/options/genxml[./hidden/optionid='" + optionid + "']/textbox/txtoptiondesc") + "</optname>";
                        strXmlIn += "</option>";
                    }
                nodList = objInfoIn.XMLDoc.SelectNodes("genxml/dropdownlist/*[starts-with(name(), 'optionddl')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        strXmlIn += "<option>";
                        var idx = nod.Name.Replace("optionddl", "");
                        var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                        strXmlIn += "<optid>" + optionid + "</optid>";
                        strXmlIn += "<optval>" + nod.InnerText + "</optval>";
                        itemcode += optionid + ":" + nod.InnerText + "-";
                        strXmlIn += "<optname>" + productData.Info.GetXmlProperty("genxml/lang/genxml/options/genxml[./hidden/optionid='" + optionid + "']/textbox/txtoptiondesc") + "</optname>";
                        strXmlIn += "<optvalcost>" + productData.Info.GetXmlProperty("genxml/optionvalues/genxml[./hidden/optionvalueid='" + nod.InnerText + "']/textbox/txtaddedcost") + "</optvalcost>";
                        strXmlIn += "<optvaltext>" + productData.Info.GetXmlProperty("genxml/lang/genxml/optionvalues/genxml[./hidden/optionvalueid='" + nod.InnerText + "']/textbox/txtoptionvaluedesc") + "</optvaltext>";
                        strXmlIn += "</option>";
                    }
                nodList = objInfoIn.XMLDoc.SelectNodes("genxml/checkbox/*[starts-with(name(), 'optionchk')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        if (nod.InnerText.ToLower() == "true")
                        {
                            strXmlIn += "<option>";
                            var idx = nod.Name.Replace("optionchk", "");
                            var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                            strXmlIn += "<optid>" + optionid + "</optid>";
                            itemcode += optionid + "-";
                            strXmlIn += "<optname>" + productData.Info.GetXmlProperty("genxml/lang/genxml/options/genxml[./hidden/optionid='" + optionid + "']/textbox/txtoptiondesc") + "</optname>";
                            strXmlIn += "<optvalueid>" + productData.Info.GetXmlProperty("genxml/optionvalues[@optionid='" + optionid + "']/genxml/hidden/optionvalueid") + "</optvalueid>";
                            strXmlIn += "<optvalcost>" + productData.Info.GetXmlProperty("genxml/optionvalues[@optionid='" + optionid + "']/genxml/textbox/txtaddedcost") + "</optvalcost>";
                            strXmlIn += "<optvaltext>" + productData.Info.GetXmlProperty("genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                            strXmlIn += "</option>";
                        }
                    }

                strXmlIn += "</options>";
                objInfo.AddXmlNode(strXmlIn, "options", "item");
                
                #endregion

                objInfo.AddSingleNode("itemcode", itemcode.TrimEnd('-'),"item");

                //Validate Cart
                objInfo = ValidateCartItem(objInfo);
                if (objInfo.XMLData == "") return objInfo.TextData;  // if we find a validation error (xmlData removed) return message status code created in textdata.

                //replace the item if it's already in the list.
                var nodItem = _cartInfo.XMLDoc.SelectSingleNode("genxml/items/item[itemcode='" + itemcode.TrimEnd('-') + "']");
                if (nodItem == null)
                    _itemList.Add(objInfo); //add as new item
                else
                {
                    //replace item
                    var qty = nodItem.SelectSingleNode("qty");
                    if (qty != null && Utils.IsNumeric(qty.InnerText) && Utils.IsNumeric(strqtyId))
                    {
                        //add new qty and replace item
                        _cartInfo.RemoveXmlNode("genxml/items/item[itemcode='" + itemcode.TrimEnd('-') + "']");
                        _itemList = GetCartItemList();
                        var newQty = Convert.ToString(Convert.ToInt32(qty.InnerText) + Convert.ToInt32(strqtyId));
                        objInfo.SetXmlProperty("item/qty", newQty,TypeCode.String,false);
                        _itemList.Add(objInfo);
                    }
                }

                // Update xml to cart on DB.
                Save(debugMode);

                // return the message status code in textData, non-critical (usually empty)
                return objInfo.TextData;
            }

            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveItem(int index)
        {
            _itemList.RemoveAt(index);
            Save();
        }

        public void UpdateItemQty(int index,int qty)
        {
            var itemqty = _itemList[index].GetXmlPropertyInt("item/qty");
            itemqty += qty;
            if (itemqty <=0 )
                RemoveItem(index);
            else
                _itemList[index].SetXmlProperty("item/qty",itemqty.ToString(""),TypeCode.String,false);
            Save();
        }


        public void DeleteCart()
        {

        }

        /// <summary>
        /// Get Current Cart
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetCart()
        {
            return _cartInfo;
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetCartItemList()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = _cartInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    rtnList.Add(newInfo);
                }
            }
            return rtnList;
        }      

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }


        public String ValidateCart()
        {
            if (Interfaces.CartInterface.Instance() != null)
            {
                _cartInfo = Interfaces.CartInterface.Instance().ValidateCart(_cartInfo);
                if (_cartInfo.XMLData == "") return _cartInfo.TextData;  // if we find a validation error (xmlData removed) return message status code created in textdata.
            }
            return "";
        }

        public NBrightInfo ValidateCartItem(NBrightInfo cartItem)
        {
            if (Interfaces.CartInterface.Instance() != null)
            {
                cartItem = Interfaces.CartInterface.Instance().ValidateCartItem(cartItem);
            }
            return cartItem;
        }

        #endregion

        #region "private methods/functions"

        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        private int GetCartId()
        {
            var cartId = "";
            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "cartId"] != null) cartId = (String)HttpContext.Current.Session[_cookieName + "cartId"];
            }
            else
            {
                _cookie = HttpContext.Current.Request.Cookies[_cookieName];
                if (_cookie == null)
                {
                    _cookie = new HttpCookie(_cookieName);
                }
                else
                {
                    if (_cookie["cartId"] != null) cartId = _cookie["cartId"];
                }
            }
            if (!Utils.IsNumeric(cartId)) cartId = "-1";

            //populate cart data
            var modCtrl = new NBrightBuyController();
            _cartInfo = modCtrl.Get(Convert.ToInt32(cartId));
            if (_cartInfo == null) _cartInfo = new NBrightInfo { XMLData = "<genxml><items></items></genxml>" };

            //build item list
            _itemList = GetCartItemList();

            return Convert.ToInt32(cartId);
        }


        #endregion


    }
}

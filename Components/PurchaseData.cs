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
    public class PurchaseData
    {
        private int _entryId;
        private NBrightInfo _purchaseInfo;
        private List<NBrightInfo> _itemList;
        private int _userId = -1;

        public String PurchaseTypeCode;
        public int PortalId;

        /// <summary>
        /// EditMode is a flag to indicate the update process of the cart/order R=Re-order, C=Create New Order for CLient, E=Edit order for client, {Empty}=Normal front office cart
        /// </summary>
        public String EditMode;

        /// <summary>
        /// Save Purchase record
        /// </summary>
        /// <param name="copyrecord">Copy this data as a new record in the DB with a new id</param>
        /// <returns></returns>
        public int Save(Boolean copyrecord = false )
        {
            if (copyrecord) _entryId = -1;
            var strXML = "<items>";
            foreach (var info in _itemList)
            {
                strXML += info.XMLData;
            }
            strXML += "</items>";
            _purchaseInfo.RemoveXmlNode("genxml/items");
            _purchaseInfo.AddXmlNode(strXML, "items", "genxml");

            var modCtrl = new NBrightBuyController();
            _purchaseInfo.ItemID = _entryId;
            _purchaseInfo.PortalId = PortalId;
            _purchaseInfo.ModuleId = -1;
            _purchaseInfo.TypeCode = PurchaseTypeCode;
            _purchaseInfo.SetXmlProperty("genxml/carteditmode",EditMode);
            if (UserId == -1) UserId = UserController.GetCurrentUserInfo().UserID;
            _purchaseInfo.UserId = UserId;
            _entryId = modCtrl.Update(_purchaseInfo);
            return _entryId;
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
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
            objInfo.XMLData = "<genxml></genxml>";

            // get productid
            var strproductid = objInfoIn.GetXmlProperty("genxml/hidden/productid");
            if (Utils.IsNumeric(strproductid))
            {
                var itemcode = ""; // The itemcode var is used to decide if a cart item is new or already existing in the cart.
                var productData = new ProductData(Convert.ToInt32(strproductid));

                objInfo.AddSingleNode("productid", strproductid, "genxml");
                itemcode += strproductid + "-";

                // Get ModelID
                var strmodelId = objInfoIn.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/hidden/modeldefault");
                objInfo.AddSingleNode("modelid", strmodelId, "genxml");
                itemcode += strmodelId + "-";

                // Get Qty
                var strqtyId = objInfoIn.GetXmlProperty("genxml/textbox/selectedaddqty");
                objInfo.AddSingleNode("qty", strqtyId, "genxml");

                #region "Get model and product data"

                objInfo.AddSingleNode("productname", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"), "genxml");
                objInfo.AddSingleNode("summary", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary"), "genxml");

                objInfo.AddSingleNode("modelref", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtmodelref"), "genxml");
                objInfo.AddSingleNode("modeldesc", productData.Info.GetXmlProperty("genxml/lang/genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtmodelname"), "genxml");
                objInfo.AddSingleNode("modelextra", productData.Info.GetXmlProperty("genxml/lang/genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtextra"), "genxml");
                objInfo.AddSingleNode("unitcost", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtunitcost"), "genxml");
                objInfo.AddSingleNode("dealercost", productData.Info.GetXmlProperty("genxml/models/genxml[./hidden/modelid='" + strmodelId + "']/textbox/txtdealercost"), "genxml");

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
                objInfo.AddXmlNode(strXmlIn, "options", "genxml");

                #endregion

                objInfo.AddSingleNode("itemcode", itemcode.TrimEnd('-'), "genxml");

                //Validate Cart
                objInfo = ValidateCartItem(objInfo);
                if (objInfo.XMLData == "") return objInfo.TextData; // if we find a validation error (xmlData removed) return message status code created in textdata.

                //replace the item if it's already in the list.
                var nodItem = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
                if (nodItem == null)
                    _itemList.Add(objInfo); //add as new item
                else
                {
                    //replace item
                    var qty = nodItem.SelectSingleNode("qty");
                    if (qty != null && Utils.IsNumeric(qty.InnerText) && Utils.IsNumeric(strqtyId))
                    {
                        //add new qty and replace item
                        _purchaseInfo.RemoveXmlNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
                        _itemList = GetCartItemList();
                        var newQty = Convert.ToString(Convert.ToInt32(qty.InnerText) + Convert.ToInt32(strqtyId));
                        objInfo.SetXmlProperty("genxml/qty", newQty, TypeCode.String, false);
                        _itemList.Add(objInfo);
                    }
                }

                //add nodes for any fields that might exist in cart template
                objInfo.AddSingleNode("textbox", "", "genxml");
                objInfo.AddSingleNode("dropdownlist", "", "genxml");
                objInfo.AddSingleNode("radiobuttonlist", "", "genxml");
                objInfo.AddSingleNode("checkbox", "", "genxml");

                // return the message status code in textData, non-critical (usually empty)
                return objInfo.TextData;
            }

            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveItem(int index)
        {
            _itemList.RemoveAt(index);
        }

        public void UpdateItemQty(int index, int qty)
        {
            var itemqty = _itemList[index].GetXmlPropertyInt("genxml/qty");
            itemqty += qty;
            if (itemqty <= 0)
                RemoveItem(index);
            else
                _itemList[index].SetXmlProperty("genxml/qty", itemqty.ToString(""), TypeCode.String, false);
        }


        public void DeleteCart()
        {
            //remove DB record
            var modCtrl = new NBrightBuyController();
            modCtrl.Delete(_entryId);
        }

        /// <summary>
        /// Get Current Cart
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetInfo()
        {
            return _purchaseInfo;
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetCartItemList()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = _purchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                    rtnList.Add(newInfo);
                }
            }
            return rtnList;
        }


        public String ValidateCart()
        {
            if (Interfaces.CartInterface.Instance() != null)
            {
                _purchaseInfo = Interfaces.CartInterface.Instance().ValidateCart(_purchaseInfo);
                if (_purchaseInfo.XMLData == "") return _purchaseInfo.TextData; // if we find a validation error (xmlData removed) return message status code created in textdata.
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

        /// <summary>
        /// Merges data entered in the cartview into the cart item
        /// </summary>
        /// <param name="index">index of cart item</param>
        /// <param name="inputInfo">genxml data of cartview item</param>
        /// <param name="debugMode">Debug mode</param>
        public void MergeCartInputData(int index, NBrightInfo inputInfo)
        {
            //get cart item
            _itemList = GetCartItemList();
            if (_itemList[index] != null)
            {
                var nods = inputInfo.XMLDoc.SelectNodes("genxml/textbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name, nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                            _itemList[index].SetXmlProperty("genxml/textbox/" + nod.Name, nod.InnerText);
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/dropdownlist/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name, nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/dropdownlist/" + nod.Name, nod.InnerText);
                            if (nod.Attributes != null && nod.Attributes["selectedtext"] != null) _itemList[index].SetXmlProperty("genxml/dropdownlist/" + nod.Name + "text", nod.Attributes["selectedtext"].Value);
                        }
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/radiobuttonlist/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name, nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name, nod.InnerText);
                            if (nod.Attributes != null && nod.Attributes["selectedtext"] != null) _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name + "text", nod.Attributes["selectedtext"].Value);
                        }
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        _itemList[index].SetXmlProperty("genxml/checkbox/" + nod.Name, nod.InnerText);
                    }
                }

            }
        }

        /// <summary>
        /// Add/Upate billing Address
        /// </summary>
        public void AddBillingAddress(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddBillingAddress(addInfo);
        }

        public void AddBillingAddress(NBrightInfo info)
        {
            var strXml = "<billaddress>";
            strXml += info.XMLData;
            strXml += "</billaddress>";
            _purchaseInfo.RemoveXmlNode("genxml/billaddress");
            _purchaseInfo.AddXmlNode(strXml, "billaddress", "genxml");
        }

        public NBrightInfo GetBillingAddress()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/billaddress");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Add/Upate Shipping Address
        /// </summary>
        public void AddShippingAddress(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddShippingAddress(addInfo);
        }

        public void AddShippingAddress(NBrightInfo info)
        {
            var strXml = "<shipaddress>";
            strXml += info.XMLData;
            strXml += "</shipaddress>";
            _purchaseInfo.RemoveXmlNode("genxml/shipaddress");
            _purchaseInfo.AddXmlNode(strXml, "shipaddress", "genxml");
            SetShippingOption("2");
        }

        public NBrightInfo GetShippingAddress()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/shipaddress");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        public void DeleteShippingAddress()
        {
            _purchaseInfo.RemoveXmlNode("genxml/shipaddress");
        }


        /// <summary>
        /// Add/Upate promotion code
        /// </summary>
        public void AddPromoCode(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddPromoCode(addInfo);
        }

        public void AddPromoCode(NBrightInfo info)
        {
            var strXml = "<promocode>";
            strXml += info.XMLData;
            strXml += "</promocode>";
            _purchaseInfo.RemoveXmlNode("genxml/promocode");
            _purchaseInfo.AddXmlNode(strXml, "promocode", "genxml");
        }

        public NBrightInfo GetPromoCode()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/promocode");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Add/Upate tax data
        /// </summary>
        public void AddTaxData(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddTaxData(addInfo);
        }

        public void AddTaxData(NBrightInfo info)
        {
            var strXml = "<taxdata>";
            strXml += info.XMLData;
            strXml += "</taxdata>";
            _purchaseInfo.RemoveXmlNode("genxml/taxdata");
            _purchaseInfo.AddXmlNode(strXml, "taxdata", "genxml");
        }

        public NBrightInfo GetTaxData()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/taxdata");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Add/Upate Extra Info
        /// </summary>
        public void AddExtraInfo(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddExtraInfo(addInfo);
        }

        public void AddExtraInfo(NBrightInfo info)
        {
            var strXml = "<extrainfo>";
            strXml += info.XMLData;
            strXml += "</extrainfo>";
            _purchaseInfo.RemoveXmlNode("genxml/extrainfo");
            _purchaseInfo.AddXmlNode(strXml, "extrainfo", "genxml");
        }

        public NBrightInfo GetExtraInfo()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/extrainfo");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Add/Upate Extra Shipping data
        /// </summary>
        public void AddShipData(Repeater rpData)
        {
            var strXML = GenXmlFunctions.GetGenXml(rpData);
            var addInfo = new NBrightInfo();
            addInfo.XMLData = strXML;
            AddShipData(addInfo);
        }

        public void AddShipData(NBrightInfo info)
        {
            var strXml = "<shipdata>";
            strXml += info.XMLData;
            strXml += "</shipdata>";
            _purchaseInfo.RemoveXmlNode("genxml/shipdata");
            _purchaseInfo.AddXmlNode(strXml, "shipdata", "genxml");
        }

        public NBrightInfo GetShipData()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = _purchaseInfo.XMLDoc.SelectSingleNode("genxml/shipdata");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Get the shipping option: 1 = use billing, 2=shipping, 3 = collection
        /// </summary>
        public String GetShippingOption()
        {
            return _purchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
        }

        /// <summary>
        /// Set the shipping option:
        /// </summary>
        /// <param name="value"> 1 = use billing, 2=shipping, 3 = collection</param>
        public void SetShippingOption(String value)
        {
            _purchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions", value, TypeCode.String, false);
        }

        /// <summary>
        /// Get the IsValidated (A cart is validated by the cart process and can only be converted to an ORDER when it have been validated)
        /// </summary>
        public Boolean IsValidated()
        {
            if (_purchaseInfo.GetXmlProperty("genxml/isvalidated") == "True") return true;
            return false;
        }

        /// <summary>
        /// Get the IsClientOrderMode (If the cart is being edited/created by a manager then this flag is set to true.)
        /// </summary>
        public Boolean IsClientOrderMode()
        {
            if (_purchaseInfo.GetXmlProperty("genxml/clientmode") == "True") return true;
            return false;
        }

        /// <summary>
        /// Set IsValidated:
        /// </summary>
        /// <param name="value"> </param>
        public void SetValidated(Boolean value)
        {
            if (value)
                _purchaseInfo.SetXmlProperty("genxml/isvalidated", "True", TypeCode.String, false);
            else
                _purchaseInfo.SetXmlProperty("genxml/isvalidated", "False", TypeCode.String, false);
        }


        public void OutputDebugFile(String fileName)
        {
            _purchaseInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + fileName);
        }

    #endregion

        #region "private methods/functions"

        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        protected int PopulatePurchaseData(int entryId)
        {
            _entryId = entryId;
            //populate cart data
            var modCtrl = new NBrightBuyController();
            _purchaseInfo = modCtrl.Get(Convert.ToInt32(_entryId));
            if (_purchaseInfo == null)
            {
                _purchaseInfo = new NBrightInfo { XMLData = "<genxml><items></items></genxml>" };
                _purchaseInfo.TypeCode = PurchaseTypeCode;
                if (entryId == -1)
                {
                    _purchaseInfo.UserId = UserController.GetCurrentUserInfo().UserID; // new cart created from front office, so give current userid.
                    EditMode = "";
                }
            }
            PurchaseTypeCode = _purchaseInfo.TypeCode;
            EditMode = _purchaseInfo.GetXmlProperty("genxml/carteditmode");
            UserId = _purchaseInfo.UserId; //retain theuserid, if created by a manager for a client.
            var currentuserInfo = UserController.GetCurrentUserInfo();
            if (UserId > 0 && currentuserInfo != null && UserId != currentuserInfo.UserID) // 0 is default userid for new cart
            {
                _purchaseInfo.SetXmlProperty("genxml/clientmode", "True", TypeCode.String, false);
                _purchaseInfo.SetXmlProperty("genxml/clientdisplayname", currentuserInfo.DisplayName);
            }
            else
            {
                _purchaseInfo.SetXmlProperty("genxml/clientmode", "False", TypeCode.String, false);
                _purchaseInfo.SetXmlProperty("genxml/clientdisplayname", "");
            }


            //build item list
            _itemList = GetCartItemList();
            return Convert.ToInt32(_entryId);
        }


        #endregion


    }
}

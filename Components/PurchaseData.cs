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
        public NBrightInfo PurchaseInfo;
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
        public int SavePurchaseData(Boolean copyrecord = false )
        {
            if (copyrecord) _entryId = -1;
            var strXml = "<items>";
            foreach (var info in _itemList)
            {
                strXml += info.XMLData;
            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");

            var modCtrl = new NBrightBuyController();
            PurchaseInfo.ItemID = _entryId;
            PurchaseInfo.PortalId = PortalId;
            PurchaseInfo.ModuleId = -1;
            PurchaseInfo.TypeCode = PurchaseTypeCode;
            PurchaseInfo.SetXmlProperty("genxml/carteditmode",EditMode);
            if (UserId == -1) UserId = UserController.GetCurrentUserInfo().UserID;
            PurchaseInfo.UserId = UserId;
            _entryId = modCtrl.Update(PurchaseInfo);
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
                var productData = ProductUtils.GetProductData(Convert.ToInt32(strproductid), Utils.GetCurrentCulture());

                objInfo.AddSingleNode("productid", strproductid, "genxml");
                itemcode += strproductid + "-";

                // Get ModelID
                var strmodelId = objInfoIn.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (strmodelId == "") strmodelId = objInfoIn.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (strmodelId == "") strmodelId = objInfoIn.GetXmlProperty("genxml/hidden/modeldefault");
                objInfo.AddSingleNode("modelid", strmodelId, "genxml");
                itemcode += strmodelId + "-";

                // Get Qty
                var strqtyId = objInfoIn.GetXmlProperty("genxml/textbox/selectedaddqty");
                objInfo.AddSingleNode("qty", strqtyId, "genxml");

                #region "Get model and product data"

                objInfo.AddSingleNode("productname", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"), "genxml");
                objInfo.AddSingleNode("summary", productData.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary"), "genxml");

                var modelInfo = productData.GetModel(strmodelId);

                objInfo.AddSingleNode("modelref", modelInfo.GetXmlProperty("genxml/textbox/txtmodelref"), "genxml");
                objInfo.AddSingleNode("modeldesc", modelInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtmodelname"), "genxml");
                objInfo.AddSingleNode("modelextra", modelInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtextra"), "genxml");
                objInfo.AddSingleNode("unitcost", modelInfo.GetXmlProperty("genxml/textbox/txtunitcost"), "genxml");
                objInfo.AddSingleNode("dealercost", modelInfo.GetXmlProperty("genxml/textbox/txtdealercost"), "genxml");

                #endregion


                #region "Get option Data"

                //build option data for cart
                Double additionalCosts = 0;
                var strXmlIn = "<options>";
                var nodList = objInfoIn.XMLDoc.SelectNodes("genxml/textbox/*[starts-with(name(), 'optiontxt')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        strXmlIn += "<option>";
                        var idx = nod.Name.Replace("optiontxt", "");
                        var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                        var optionInfo = productData.GetOption(optionid);
                        var optvaltext = nod.InnerText;
                        strXmlIn += "<optid>" + optionid + "</optid>";
                        strXmlIn += "<optvaltext>" + optvaltext + "</optvaltext>";
                        var itemcodeText = "";
                        if (optvaltext.Length > 0) itemcodeText = optvaltext.Replace(" ", "").Substring(0, optvaltext.Replace(" ", "").Length - 1);
                        itemcode += optionid + itemcodeText + "-";
                        strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                        strXmlIn += "</option>";
                    }
                nodList = objInfoIn.XMLDoc.SelectNodes("genxml/dropdownlist/*[starts-with(name(), 'optionddl')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        strXmlIn += "<option>";
                        var idx = nod.Name.Replace("optionddl", "");
                        var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                        var optionvalueid = nod.InnerText;
                        var optionValueInfo = productData.GetOptionValue(optionid, optionvalueid);
                        var optionInfo = productData.GetOption(optionid);
                        strXmlIn += "<optid>" + optionid + "</optid>";
                        strXmlIn += "<optvalueid>" + optionvalueid + "</optvalueid>";
                        itemcode += optionid + ":" + optionvalueid + "-";
                        strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                        strXmlIn += "<optvalcost>" + optionValueInfo.GetXmlProperty("genxml/textbox/txtaddedcost") + "</optvalcost>";
                        strXmlIn += "<optvaltext>" + optionValueInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                        strXmlIn += "</option>";
                        additionalCosts += optionValueInfo.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
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
                            var optionvalueid = nod.InnerText;
                            var optionValueInfo = productData.GetOptionValue(optionid, optionvalueid);
                            var optionInfo = productData.GetOption(optionid);
                            strXmlIn += "<optid>" + optionid + "</optid>";
                            strXmlIn += "<optvalueid>" + optionvalueid + "</optvalueid>";
                            itemcode += optionid + ":" + optionvalueid + "-";
                            strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                            strXmlIn += "<optvalcost>" + optionValueInfo.GetXmlProperty("genxml/textbox/txtaddedcost") + "</optvalcost>";
                            strXmlIn += "<optvaltext>" + optionValueInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                            strXmlIn += "</option>";
                            additionalCosts += optionValueInfo.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                        }
                    }

                strXmlIn += "</options>";
                objInfo.AddXmlNode(strXmlIn, "options", "genxml");

                #endregion

                //add additional costs from optionvalues (Add to both dealer and unit cost)
                if (additionalCosts > 0)
                {
                    var uc = objInfo.GetXmlPropertyDouble("genxml/unitcost");
                    var dc = objInfo.GetXmlPropertyDouble("genxml/dealercost");
                    uc += additionalCosts;
                    dc += additionalCosts;
                    objInfo.SetXmlPropertyDouble("genxml/unitcost", uc);
                    objInfo.SetXmlPropertyDouble("genxml/dealercost", dc);
                }


                objInfo.AddSingleNode("itemcode", itemcode.TrimEnd('-'), "genxml");

                //replace the item if it's already in the list.
                var nodItem = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
                if (nodItem == null)
                    _itemList.Add(objInfo); //add as new item
                else
                {
                    //replace item
                    var qty = nodItem.SelectSingleNode("qty");
                    if (qty != null && Utils.IsNumeric(qty.InnerText) && Utils.IsNumeric(strqtyId))
                    {
                        //add new qty and replace item
                        PurchaseInfo.RemoveXmlNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
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
            SavePurchaseData();
        }

        public void UpdateItemQty(int index, int qty)
        {
            var itemqty = _itemList[index].GetXmlPropertyInt("genxml/qty");
            itemqty += qty;
            if (itemqty <= 0)
                RemoveItem(index);
            else
                _itemList[index].SetXmlProperty("genxml/qty", itemqty.ToString(""), TypeCode.String, false);
            SavePurchaseData();
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
            return PurchaseInfo;
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetCartItemList()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
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
            PurchaseInfo.RemoveXmlNode("genxml/billaddress");
            PurchaseInfo.AddXmlNode(strXml, "billaddress", "genxml");
        }

        public NBrightInfo GetBillingAddress()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/billaddress");
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
            PurchaseInfo.RemoveXmlNode("genxml/shipaddress");
            PurchaseInfo.AddXmlNode(strXml, "shipaddress", "genxml");
            SetShippingOption("2");
        }

        public NBrightInfo GetShippingAddress()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/shipaddress");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        public void DeleteShippingAddress()
        {
            PurchaseInfo.RemoveXmlNode("genxml/shipaddress");
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
            PurchaseInfo.RemoveXmlNode("genxml/promocode");
            PurchaseInfo.AddXmlNode(strXml, "promocode", "genxml");
        }

        public NBrightInfo GetPromoCode()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/promocode");
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
            PurchaseInfo.RemoveXmlNode("genxml/taxdata");
            PurchaseInfo.AddXmlNode(strXml, "taxdata", "genxml");
        }

        public NBrightInfo GetTaxData()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/taxdata");
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
            PurchaseInfo.RemoveXmlNode("genxml/extrainfo");
            PurchaseInfo.AddXmlNode(strXml, "extrainfo", "genxml");
        }

        public NBrightInfo GetExtraInfo()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/extrainfo");
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
            PurchaseInfo.RemoveXmlNode("genxml/shipdata");
            PurchaseInfo.AddXmlNode(strXml, "shipdata", "genxml");
        }

        public NBrightInfo GetShipData()
        {
            var rtnInfo = new NBrightInfo();
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/shipdata");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Get the shipping option: 1 = use billing, 2=shipping, 3 = collection
        /// </summary>
        public String GetShippingOption()
        {
            return PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
        }

        /// <summary>
        /// Set the shipping option:
        /// </summary>
        /// <param name="value"> 1 = use billing, 2=shipping, 3 = collection</param>
        public void SetShippingOption(String value)
        {
            PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions", value, TypeCode.String, false);
        }

        /// <summary>
        /// Get the IsValidated (A cart is validated by the cart process and can only be converted to an ORDER when it has been validated)
        /// </summary>
        public Boolean IsValidated()
        {
            if (PurchaseInfo.GetXmlProperty("genxml/isvalidated") == "True") return true;
            return false;
        }

        /// <summary>
        /// Get the IsClientOrderMode (If the cart is being edited/created by a manager then this flag is set to true.)
        /// </summary>
        public Boolean IsClientOrderMode()
        {
            if (PurchaseInfo.GetXmlProperty("genxml/clientmode") == "True") return true;
            return false;
        }

        /// <summary>
        /// Set IsValidated:
        /// </summary>
        /// <param name="value"> </param>
        public void SetValidated(Boolean value)
        {
            if (value)
                PurchaseInfo.SetXmlProperty("genxml/isvalidated", "True", TypeCode.String, false);
            else
                PurchaseInfo.SetXmlProperty("genxml/isvalidated", "False", TypeCode.String, false);
        }


        public String EmailAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/emailaddress");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/emailaddress", value);
            }
        }


        public void OutputDebugFile(String fileName)
        {
            PurchaseInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + fileName);
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
            PurchaseInfo = modCtrl.Get(Convert.ToInt32(_entryId));
            if (PurchaseInfo == null)
            {
                PurchaseInfo = new NBrightInfo(true);
                PurchaseInfo.TypeCode = PurchaseTypeCode;
                //add items node so we can add items
                PurchaseInfo.AddSingleNode("items","","genxml");
                
                if (entryId == -1)
                {
                    PurchaseInfo.UserId = UserController.GetCurrentUserInfo().UserID; // new cart created from front office, so give current userid.
                    EditMode = "";
                }
            }
            PurchaseTypeCode = PurchaseInfo.TypeCode;
            EditMode = PurchaseInfo.GetXmlProperty("genxml/carteditmode");
            UserId = PurchaseInfo.UserId; //retain theuserid, if created by a manager for a client.
            var currentuserInfo = UserController.GetCurrentUserInfo();
            if (UserId > 0 && currentuserInfo != null && UserId != currentuserInfo.UserID) // 0 is default userid for new cart
            {
                PurchaseInfo.SetXmlProperty("genxml/clientmode", "True", TypeCode.String, false);
                PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", currentuserInfo.DisplayName);
            }
            else
            {
                PurchaseInfo.SetXmlProperty("genxml/clientmode", "False", TypeCode.String, false);
                PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", "");
            }


            //build item list
            PopulateItemList();

            return Convert.ToInt32(_entryId);
        }


        public void PopulateItemList()
        {
            _itemList = GetCartItemList();
        }

        #endregion


    }
}

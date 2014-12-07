using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;


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
            if (UserId != UserController.GetCurrentUserInfo().UserID && EditMode == "") UserId = UserController.GetCurrentUserInfo().UserID;
            PurchaseInfo.UserId = UserId;
            _entryId = modCtrl.Update(PurchaseInfo);
            return _entryId;
        }

        public String Lang
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/lang");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/lang", value);
            }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public String ShippingCountry
        {
            get { return PurchaseInfo.GetXmlProperty("genxml/shippingcountry"); }
            set { PurchaseInfo.SetXmlProperty("genxml/shippingcountry",value); }
        }

        public String ShippingRegion
        {
            get { return PurchaseInfo.GetXmlProperty("genxml/shippingregion"); }
            set { PurchaseInfo.SetXmlProperty("genxml/shippingregion", value); }
        }


        #region "Stock Control"
        /// <summary>
        /// Save transent model qty to cache.
        /// </summary>
        public void SaveModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");
                var prd = new ProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model != null && model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.UpdateModelTransQty(modelid, _entryId, qty);
                }
            }
        }

        /// <summary>
        /// Release transient qty for this cart
        /// </summary>
        public void ReleaseModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");

                var prd = new ProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.ReleaseModelTransQty(modelid, _entryId, qty);
                }
            }
        }
        /// <summary>
        /// Apply Transient qty for this cart onto the model
        /// </summary>
        public void ApplyModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");

                var prd = new ProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.ApplyModelTransQty(modelid, _entryId, qty);
                }
            }
        }

        #endregion

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

            var strXml = GenXmlFunctions.GetGenXml(rpData, "", StoreSettings.Current.FolderUploadsMapPath, rowIndex);


            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;

            var strproductid = objInfoIn.GetXmlProperty("genxml/hidden/productid");
            // Get ModelID
            var modelidlist = new List<String>();
            var qtylist = new Dictionary<String, String>();
            var nodList = objInfoIn.XMLDoc.SelectNodes("genxml/repeaters/repeater[1]/*");
            if (nodList != null && nodList.Count > 0)
            {
                foreach (XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.XMLData = nod.OuterXml;
                    var strmodelId = nbi.GetXmlProperty("genxml/hidden/modelid");
                    var strqtyId = nbi.GetXmlProperty("genxml/textbox/selectedmodelqty");
                    if (Utils.IsNumeric(strqtyId))
                    {
                        modelidlist.Add(strmodelId);
                        qtylist.Add(strmodelId, strqtyId);                                                            
                    }
                }
            }
            if (qtylist.Count == 0)
            {
                var strmodelId = objInfoIn.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (strmodelId == "") strmodelId = objInfoIn.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (strmodelId == "") strmodelId = objInfoIn.GetXmlProperty("genxml/hidden/modeldefault");
                modelidlist.Add(strmodelId);
                var strqtyId = objInfoIn.GetXmlProperty("genxml/textbox/selectedaddqty");
                qtylist.Add(strmodelId,strqtyId);
            }

            var strRtn = "";
            foreach (var m in modelidlist)
            {
                strRtn += AddSingleItem(strproductid, m, qtylist[m], objInfoIn, debugMode);
            }
            return strRtn;
        }

        public String AddSingleItem(String strproductid, String strmodelId, String strqtyId, NBrightInfo objInfoIn, Boolean debugMode = false)
        {
            if (!Utils.IsNumeric(strqtyId) || Convert.ToInt32(strqtyId) <= 0) return "";

            if (debugMode) objInfoIn.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addtobasket.xml");

            var objInfo = new NBrightInfo();
            objInfo.XMLData = "<genxml></genxml>";

            // get productid
            if (Utils.IsNumeric(strproductid))
            {
                var itemcode = ""; // The itemcode var is used to decide if a cart item is new or already existing in the cart.
                var productData = ProductUtils.GetProductData(Convert.ToInt32(strproductid), Utils.GetCurrentCulture());

                objInfo.AddSingleNode("productid", strproductid, "genxml");
                itemcode += strproductid + "-";

                objInfo.AddSingleNode("modelid", strmodelId, "genxml");
                itemcode += strmodelId + "-";

                // Get Qty
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
                objInfo.AddSingleNode("taxratecode", modelInfo.GetXmlProperty("genxml/dropdownlist/taxrate"), "genxml");
                objInfo.AddSingleNode("saleprice", modelInfo.GetXmlProperty("genxml/textbox/txtsaleprice"), "genxml");

                // flag if dealer
                var userInfo = UserController.GetCurrentUserInfo();
                if (userInfo != null && userInfo.IsInRole(StoreSettings.DealerRole) && StoreSettings.Current.Get("enabledealer") == "True")
                    objInfo.SetXmlProperty("genxml/isdealer", "True");
                else
                    objInfo.SetXmlProperty("genxml/isdealer", "False");


                //move all product and model data into cart item, so we can display bespoke fields.
                objInfo.AddSingleNode("productxml", productData.Info.XMLData, "genxml");

                #endregion


                #region "Get option Data"

                //build option data for cart
                Double additionalCosts = 0;
                var strXmlIn = "<options>";
                if (objInfoIn.XMLDoc != null)
                {

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
                            itemcode += optionid + "-" + Utils.GetUniqueKey() + "-";
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
                                strXmlIn += "<option>";
                                var idx = nod.Name.Replace("optionchk", "");
                                var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                                var optionvalueid = nod.InnerText;
                                var optionValueInfo = productData.GetOptionValue(optionid, ""); // checkbox does not have optionvalueid
                                var optionInfo = productData.GetOption(optionid);
                                strXmlIn += "<optid>" + optionid + "</optid>";
                                strXmlIn += "<optvalueid>" + optionvalueid + "</optvalueid>";
                                itemcode += optionid + ":" + optionvalueid + "-";
                                strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                                strXmlIn += "<optvalcost>" + optionValueInfo.GetXmlProperty("genxml/textbox/txtaddedcost") + "</optvalcost>";
                                strXmlIn += "<optvaltext>" + optionValueInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                                strXmlIn += "</option>";
                                if (nod.InnerText.ToLower() == "true") additionalCosts += optionValueInfo.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                        }
                }
                strXmlIn += "</options>";
                objInfo.AddXmlNode(strXmlIn, "options", "genxml");

                #endregion

                //add additional costs from optionvalues (Add to both dealer and unit cost)
                if (additionalCosts > 0)
                {
                    objInfo.SetXmlPropertyDouble("genxml/additionalcosts", additionalCosts);
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

                SavePurchaseData(); // need to save after each add, so it exists in data when we check it already exists for updating.

                // return the message status code in textData, non-critical (usually empty)
                return objInfo.TextData;
            }
            return "";
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
        public List<NBrightInfo> GetCartItemList(Boolean groupByProduct = false)
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                    newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/itemcode");
                    rtnList.Add(newInfo);
                }
            }

            if (groupByProduct)
            {

                var grouped = from p in rtnList group p by p.GetXmlProperty("genxml/productid") into g select new {g.Key,Value = g};
                rtnList = new List<NBrightInfo>();
                foreach (var group in grouped)
                {
                    // inject header record for the product
                    var itemheader = (NBrightInfo)group.Value.First().Clone();
                    itemheader.SetXmlProperty("genxml/groupheader","True");
                    itemheader.SetXmlProperty("genxml/seeditemcode", itemheader.GUIDKey);
                    itemheader.GUIDKey = "";
                    rtnList.Add(itemheader);

                    foreach (var item in group.Value)
                    {
                        rtnList.Add(item);
                    }
                }
            }

            return rtnList;
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public void RemoveItem(String itemCode)
        {
            var removeindex = GetItemIndex(itemCode);
            if (removeindex >= 0) RemoveItem(removeindex);
        }

        public void MergeCartInputData(String itemCode, NBrightInfo inputInfo)
        {
            var index = GetItemIndex(itemCode);
            if (index >= 0) MergeCartInputData(index,inputInfo);
        }

        /// <summary>
        /// Merges data entered in the cartview into the cart item
        /// </summary>
        /// <param name="index">index of cart item</param>
        /// <param name="inputInfo">genxml data of cartview item</param>
        public void MergeCartInputData(int index, NBrightInfo inputInfo)
        {
            //get cart item
            //_itemList = GetCartItemList();  // Don;t get get here, it resets previous altered itemlist records.
            if (_itemList[index] != null)
            {
                #region "merge option data"
                var nodList = inputInfo.XMLDoc.SelectNodes("genxml/hidden/*[starts-with(name(), 'optionid')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        var idx = nod.Name.Replace("optionid", "");
                        var optid = nod.InnerText;
                        if (inputInfo.GetXmlProperty("genxml/textbox/optiontxt" + idx) != "")
                        {
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvaltext", inputInfo.GetXmlProperty("genxml/textbox/optiontxt" + idx));   
                        }
                        if (inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx) != "")
                        {
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvalueid", inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx));
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvaltext", inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx + "/@selectedtext"));                            
                        }
                        if (inputInfo.GetXmlProperty("genxml/checkbox/optionchk" + idx) != "")
                        {
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvalueid", inputInfo.GetXmlProperty("genxml/checkbox/optionchk" + idx));
                        }
                    }

                #endregion

                var nods = inputInfo.XMLDoc.SelectNodes("genxml/textbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                            _itemList[index].SetXmlProperty("genxml/textbox/" + nod.Name.ToLower(), nod.InnerText);
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/dropdownlist/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/dropdownlist/" + nod.Name.ToLower(), nod.InnerText);
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
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name.ToLower(), nod.InnerText);
                            if (nod.Attributes != null && nod.Attributes["selectedtext"] != null) _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name + "text", nod.Attributes["selectedtext"].Value);
                        }
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        _itemList[index].SetXmlProperty("genxml/checkbox/" + nod.Name.ToLower(), nod.InnerText);
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
            var rtnInfo = new NBrightInfo(true);
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/extrainfo");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;

            var nodList = PurchaseInfo.XMLDoc.SelectNodes("genxml/*");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    if (nod.FirstChild != null && nod.FirstChild.Name != "genxml") rtnInfo.SetXmlProperty("genxml/" + nod.Name, nod.InnerText);                
                }

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
            if (PurchaseInfo.GetXmlProperty("genxml/clientmode") == "True")
            {
                if (!UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) && !UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) && !UserController.GetCurrentUserInfo().IsInRole("Administrators")) // user not editor, so stop edit mode.
                {                    
                    PurchaseInfo.SetXmlProperty("genxml/clientmode", "False");
                    SavePurchaseData();
                    return false;
                }
                return true;
            }
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
                return PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress", value);
            }
        }
        public String EmailBillingAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/email");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/email", value);
            }
        }
        public String EmailShippingAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/shipaddress/genxml/textbox/deliveryemail");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/deliveryemail", value);
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
            if (UserId > 0 && EditMode != "") // 0 is default userid for new cart
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

        public int GetItemIndex(String itemCode)
        {
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                var lp = 0;
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    if (newInfo.GetXmlProperty("genxml/itemcode") == itemCode)
                    {
                        return lp;
                        break;
                    }
                    lp += 1;
                }
            }
            return -1;
        }

        #endregion


    }
}

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
        private void Save(List<NBrightInfo> saveList)
        {
            //save cart
            var strXML = "<items>";
            foreach(var info in saveList)
            {
                strXML += info.XMLData;
            }
            strXML += "</items>";
            var modCtrl = new NBrightBuyController();
            var cartInfo = new NBrightInfo();
            cartInfo.ItemID = _cartId;
            cartInfo.XMLData = strXML;
            cartInfo.PortalId = _portalId;
            cartInfo.ModuleId = -1;
            cartInfo.TypeCode = "CART";
            var uInfo = UserController.GetCurrentUserInfo();
            cartInfo.UserId = uInfo.UserID;
            _cartId = modCtrl.Update(cartInfo);
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

        /// <summary>
        /// Delete cart 
        /// </summary>
        public void Delete()
        {
        }


        #region "base methods"

        /// <summary>
        /// Add product to cart
        /// </summary>
        /// <param name="rpData"></param>
        /// <param name="nbSettings"></param>
        /// <param name="rowIndex"></param>
        /// <param name="debugMode"></param>
        public String AddToCart(Repeater rpData, NBrightInfo nbSettings, int rowIndex, Boolean debugMode = false)
        {

            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER, rowIndex);

            if (debugMode)
            {
                var xmlDoc = new System.Xml.XmlDataDocument();
                xmlDoc.LoadXml(strXml);
                xmlDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addtobasket.xml");
            }

            // load into NbrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            var objInfo = new NBrightInfo();
            objInfo.XMLData = "<item></item>";

            // get productid
            var strproductid = objInfoIn.GetXmlProperty("genxml/hidden/productid");
            if (Utils.IsNumeric(strproductid))
            {
                var productData = new ProductData(Convert.ToInt32(strproductid));

                objInfo.AddSingleNode("productid", strproductid, "item");

                // Get ModelID
                var strmodelId = objInfoIn.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (!Utils.IsNumeric(strmodelId)) strmodelId = objInfoIn.GetXmlProperty("genxml/hidden/modeldefault");
                objInfo.AddSingleNode("modelid", strmodelId, "item");

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
                        strXmlIn += "<optid>" + optionid + "</optid>";
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

                //Validate Cart
                if (Interfaces.CartInterface.Instance() != null)
                {
                    objInfo = Interfaces.CartInterface.Instance().ValidateCart(objInfo);                    
                }

                // Update xml to cart on DB.
                var cartInfo = GetCurrentCart();
                cartInfo.Add(objInfo);
                Save(cartInfo);

            }

            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveItem(int modelId)
        {

        }

        public void EditProduct(int qty)
        {

        }

        public void DeleteCart()
        {

        }

        /// <summary>
        /// Get Current Cart
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetCurrentCart()
        {
            var rtnList = new List<NBrightInfo>();
            var modCtrl = new NBrightBuyController();
            var cartInfo = modCtrl.Get(Convert.ToInt32(_cartId));
            if (cartInfo == null) cartInfo = new NBrightInfo { XMLData = "<items></items>" };
            var xmlNodeList = cartInfo.XMLDoc.SelectNodes("items/*");
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

        public void MarkProduct(int productId)
        {

        }

        public void SaveAsList(string listName)
        {

        }

        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        public int GetCartId()
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
            return Convert.ToInt32(cartId);
        }


        #endregion

        #region "functions"


        #endregion


    }
}

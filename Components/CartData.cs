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
            var strXML = "<root>";
            foreach(var info in saveList)
            {
                strXML += info.XMLData;
            }
            strXML += "</root>";
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
        public void AddToCart(Repeater rpData, NBrightInfo nbSettings, int rowIndex, Boolean debugMode = false)
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


            //validate optcode and get ItemDesc
            optCode = modelId.ToString("") + "-" + optCode;
            objInfo.SetXmlProperty("genxml/@optcode",optCode);

            // Update xml to cart on DB.
            var cartInfo = GetCurrentCart();
            cartInfo.Add(objInfo);
            Save(cartInfo);

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
            if (cartInfo == null) cartInfo = new NBrightInfo {XMLData = "<root></root>"};
            var xmlNodeList = cartInfo.XMLDoc.SelectNodes("root/*");
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


        
    }
}

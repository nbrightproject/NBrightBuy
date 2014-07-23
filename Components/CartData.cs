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
    public class CartData: PurchaseData
    {
        private int _cartId;
        private string _cookieName;
        private HttpCookie _cookie;

        public CartData(int portalId, string nameAppendix = "",String cartid = "")
        {
            _cookieName = "NBrightBuyCart" + "*" + portalId.ToString("") + "*" + nameAppendix;
            Exists = false;
            PortalId = portalId;
            _cartId = GetCartId(cartid);                
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            ValidateCart();
            //save cart
            _cartId = base.SavePurchaseData();
            if (debugMode) OutputDebugFile("debug_currentcart.xml");
            SaveCartId();
            Exists = true;

        }

        public void ConvertToOrder(Boolean debugMode = false)
        {
            ValidateCart();
            if (IsValidated())
            {
                PurchaseTypeCode = "ORDER";
                base.SavePurchaseData();
                if (debugMode) OutputDebugFile("debug_convertedcart.xml");
                Exists = false;                
            }
        }


        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }

        #region "private methods/functions"

        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        private int GetCartId(String cartId = "")
        {
            if (!Utils.IsNumeric(cartId))
            {

                if (StoreSettings.Current.StorageTypeClient  == DataStorageType.SessionMemory)
                {
                    if (HttpContext.Current.Session[_cookieName + "cartId"] != null) cartId = (String) HttpContext.Current.Session[_cookieName + "cartId"];
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
            }
            else
            {
                _cartId = Convert.ToInt32(cartId);
                SaveCartId(); //created from order, so save cartid for client.
            }

            //populate cart data
            var rtnid = PopulatePurchaseData(Convert.ToInt32(cartId));
            if (PurchaseTypeCode == "CART") return rtnid;

            // this class has only rights to edit CART items, so reset cartid to new cart Item.           
            PurchaseTypeCode = "CART";
            return PopulatePurchaseData(-1);

        }

        private void SaveCartId()
        {
            //save cartid for client
            if (StoreSettings.Current.StorageTypeClient == DataStorageType.SessionMemory)
            {
                // save data to cache
                HttpContext.Current.Session[_cookieName] = _cartId;
            }
            else
            {
                _cookie = HttpContext.Current.Request.Cookies[_cookieName];
                if (_cookie == null) _cookie = new HttpCookie(_cookieName);
                _cookie["cartId"] = _cartId.ToString("");
                _cookie.Expires = DateTime.Now.AddDays(1d);
                HttpContext.Current.Response.Cookies.Add(_cookie);
            }

        }

        #endregion

        #region "cart validation and calculation"

        public void ValidateCart()
        {
            var itemList = GetCartItemList();
            Double totalcost = 0;
            Double totaldealercost = 0;
            var strXml = "<items>";
            foreach (var info in itemList)
            {
                strXml += ValidateCartItem(PortalId, UserId, info).XMLData;
                totalcost += info.GetXmlPropertyDouble("genxml/totalcost");
                totaldealercost += info.GetXmlPropertyDouble("genxml/totaldealercost");

            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");
            PopulateItemList();

            // calculate totals
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalcost", totalcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealercost", totaldealercost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/displaytotalcost", DisplayCost(PortalId, UserId, totalcost, totaldealercost));
            SavePurchaseData();
        }

        private NBrightInfo ValidateCartItem(int portalId, int userId, NBrightInfo cartItemInfo)
        {
            var unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/unitcost");
            var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
            var dealercost = cartItemInfo.GetXmlPropertyDouble("genxml/dealercost");
            var totalcost = qty * unitcost;
            var totaldealercost = qty * dealercost;

            var optNods = cartItemInfo.XMLDoc.SelectNodes("genxml/options/*");
            if (optNods != null)
            {
                var lp = 0;
                foreach (XmlNode nod in optNods)
                {
                    var optvalcostnod = nod.SelectSingleNode("option/optvalcost");
                    if (optvalcostnod != null)
                    {
                        var optvalcost = optvalcostnod.InnerText;
                        if (Utils.IsNumeric(optvalcost))
                        {
                            var optvaltotal = Convert.ToDouble(optvalcost) * qty;
                            cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", optvaltotal);
                            totalcost += optvaltotal;
                            totaldealercost += optvaltotal;
                        }
                    }
                    lp += 1;
                }
            }
            cartItemInfo.SetXmlPropertyDouble("genxml/totalcost", totalcost);
            cartItemInfo.SetXmlPropertyDouble("genxml/totaldealercost", totaldealercost);
            cartItemInfo.SetXmlPropertyDouble("genxml/displaytotalcost", DisplayCost(portalId, userId, totalcost, totaldealercost));
            cartItemInfo.SetXmlPropertyDouble("genxml/displaycost", DisplayCost(portalId, userId, unitcost, dealercost));

            return cartItemInfo;
        }

        private Double DisplayCost(int portalId, int userId, Double cost, Double dealercost)
        {
            var userInfo = UserController.GetUserById(portalId, userId);
            if (userInfo != null)
            {
                if (userInfo.IsInRole(StoreSettings.EditorRole)) return dealercost;
            }
            return cost;
        }


        #endregion

    }
}

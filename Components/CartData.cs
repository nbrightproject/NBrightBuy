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
            Save();
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            //save cart so any added items are included
            _cartId = base.SavePurchaseData();
            ValidateCart();
            //save cart after validation so calculated costs are saved.
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

        #region "Stock control"

        public int GetQtyOfModelInCart(String modelid)
        {
            var itemList = GetCartItemList();
            return itemList.Where(c => c.GetXmlProperty("genxml/modelid") == modelid).Sum(c => c.GetXmlPropertyInt("genxml/qty"));
        }

        #endregion

        #region "cart validation and calculation"

        public void ValidateCart()
        {
            var itemList = GetCartItemList();
            Double subtotalcost = 0;
            Double subtotaldealercost = 0;
            Double totaldealerbonus = 0;
            Double totaldiscount = 0;
            Double totaldealerdiscount = 0;

            var strXml = "<items>";
            foreach (var info in itemList)
            {
                strXml += ValidateCartItem(PortalId, UserId, info).XMLData;
                subtotalcost += info.GetXmlPropertyDouble("genxml/totalcost");
                subtotaldealercost += info.GetXmlPropertyDouble("genxml/totaldealercost");
                totaldealerbonus += info.GetXmlPropertyDouble("genxml/totaldealerbonus");
                totaldiscount += info.GetXmlPropertyDouble("genxml/totaldiscount");
                totaldealerdiscount += info.GetXmlPropertyDouble("genxml/totaldealerdiscount");
            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");
            PopulateItemList();

            // calculate totals
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotalcost", subtotalcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotaldealercost", subtotaldealercost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedsubtotal", AppliedCost(PortalId, UserId, subtotalcost, subtotaldealercost));

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealerdiscount", totaldealerdiscount);
            PurchaseInfo.SetXmlPropertyDouble("genxml/applieddiscount", AppliedCost(PortalId, UserId, totaldiscount, totaldealerdiscount));

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", totaldealerbonus);


            //add shipping
            Double shippingcost = 0;
            Double shippingdealercost = 0;
            PurchaseInfo.SetXmlPropertyDouble("genxml/shippingcost", shippingcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/shippingdealercost", shippingdealercost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedshipping", AppliedCost(PortalId, UserId, shippingcost, shippingdealercost));
            //add tax
            Double taxcost = 0;
            Double taxdealercost = 0;
            PurchaseInfo.SetXmlPropertyDouble("genxml/taxcost", taxcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/taxdealercost", taxdealercost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedtax", AppliedCost(PortalId, UserId, taxcost, taxdealercost));

            //cart full total
            var dealertotal = (subtotaldealercost + shippingdealercost + taxdealercost);
            var total = (subtotalcost + shippingcost + taxcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/dealertotal", dealertotal);
            PurchaseInfo.SetXmlPropertyDouble("genxml/total", total);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedtotal", AppliedCost(PortalId, UserId, total, dealertotal));

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
            cartItemInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", (totalcost - totaldealercost));

            //add promo
            var discount = 0;
            var dealerdiscount = 0;
            var totaldiscount = discount * qty;
            var totaldealerdiscount = dealerdiscount * qty;
            cartItemInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);
            cartItemInfo.SetXmlPropertyDouble("genxml/totaldealerdiscount", totaldealerdiscount);

            cartItemInfo.SetXmlPropertyDouble("genxml/appliedtotalcost", AppliedCost(portalId, userId, (totalcost - totaldiscount), totaldealercost));
            cartItemInfo.SetXmlPropertyDouble("genxml/appliedcost", AppliedCost(portalId, userId, (unitcost - discount), (dealercost - dealerdiscount)));

            return cartItemInfo;
        }

        private Double AppliedCost(int portalId, int userId, Double cost, Double dealercost)
        {
            //always return nortmal price for non-registered users
            if (UserController.GetCurrentUserInfo().UserID == -1) return cost;

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

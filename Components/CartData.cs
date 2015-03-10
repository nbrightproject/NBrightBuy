using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

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
            //save cart so any added items are included
            _cartId = base.SavePurchaseData();
            ValidateCart();
            //save cart after validation so calculated costs are saved.
            _cartId = base.SavePurchaseData();
            if (StoreSettings.Current.DebugModeFileOut) OutputDebugFile("debug_currentcart.xml");
            SaveCartId();
            Exists = true;

        }

        public Boolean ConvertToOrder(Boolean debugMode = false)
        {
            var itemList = GetCartItemList();
            if (IsValidated() && itemList.Count > 0)
            {
                PurchaseTypeCode = "ORDER";
                if (base.PurchaseInfo.GetXmlProperty("genxml/createddate") == "") base.PurchaseInfo.SetXmlProperty("genxml/createddate", DateTime.Now.ToString(CultureInfo.GetCultureInfo(Utils.GetCurrentCulture())), TypeCode.DateTime);
                if (base.PurchaseInfo.GetXmlProperty("genxml/ordernumber") == "") base.PurchaseInfo.SetXmlProperty("genxml/ordernumber", StoreSettings.Current.Get("orderprefix") + DateTime.Today.Year.ToString("").Substring(2, 2) + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + _cartId);

                Save();
                var ordData = new OrderData(PortalId, base.PurchaseInfo.ItemID);
                ordData.OrderStatus = "010";
                if (ordData.EditMode == "") // don't update if we are in edit mode, we dont; want manager email to be altered.
                {
                    // if the client has updated the email address, link this back to DNN profile. (We assume they alway place there current email address on th order.)
                    var objUser = UserController.GetUserById(PortalSettings.Current.PortalId, ordData.UserId);
                    if (objUser != null && objUser.Email != ordData.EmailAddress)
                    {
                        var clientData = new ClientData(PortalId, ordData.UserId);
                        clientData.UpdateEmail(ordData.EmailAddress);
                    }                    
                }
                ordData.Save();

                if (StoreSettings.Current.DebugModeFileOut) OutputDebugFile("debug_convertedcart.xml");
                Exists = false;
                return true;
            }
            return false;
        }






        /// <summary>
        /// This function take a cookie created on the client and adds all items to the cart
        /// NOTE: The cookie value is expected in a standard format, whcih is defined by the use of "nbbqtycookie.js"
        /// This function will only work, if the product/model being added does not have any options.
        /// </summary>
        public void AddCookieToCart()
        {
            var foundCookie = HttpContext.Current.Request.Cookies["nbrightbuy_qtyselected"];
            var data = new Dictionary<String, String>();

            // extract cookie data
            if (foundCookie != null && foundCookie.Value != "")
            {
                var list = foundCookie.Value.Split('*');
                for (var c = 0; c < list.Count(); c++)
                {
                    var list2 = list[c].Split(':');
                    if (list2.Count() == 2)
                    {
                        data.Add(list2[0], list2[1]);
                    }
                }

                // do add to cart
                foreach (var c in data)
                {
                    var s = c.Key.Split('-');
                    if (s.Count() == 2)
                    {
                        var productid = s[0];
                        var modelid = s[1];
                        var qty = c.Value;
                        if (Utils.IsNumeric(qty))
                        {
                            AddSingleItem(productid, modelid, qty, new NBrightInfo());
                        }
                    }
                }

                foundCookie.Expires = DateTime.Now.AddYears(-30);
                HttpContext.Current.Response.Cookies.Add(foundCookie);

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
            PurchaseInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartBefore, PurchaseInfo);

            var itemList = GetCartItemList();
            Double subtotalcost = 0;
            Double subtotaldealercost = 0;
            Double totaldealerbonus = 0;
            Double totaldiscount = 0;
            Double totaldealerdiscount = 0;
            Double totalqty = 0;
            Double totalweight = 0;
            Double totalunitcost = 0;

            var strXml = "<items>";
            foreach (var info in itemList)
            {
                var cartItem = ValidateCartItem(PortalId, UserId, info);
                if (cartItem != null)
                {
                    strXml += cartItem.XMLData;
                    totalunitcost += info.GetXmlPropertyDouble("genxml/unitcost");
                    subtotalcost += info.GetXmlPropertyDouble("genxml/totalcost");
                    subtotaldealercost += info.GetXmlPropertyDouble("genxml/totaldealercost");
                    totaldealerbonus += info.GetXmlPropertyDouble("genxml/totaldealerbonus");
                    totaldiscount += info.GetXmlPropertyDouble("genxml/totaldiscount");
                    totaldealerdiscount += info.GetXmlPropertyDouble("genxml/totaldealerdiscount");
                    totalqty += info.GetXmlPropertyDouble("genxml/qty");
                    totalweight += info.GetXmlPropertyDouble("genxml/totalweight"); 
                }
            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");
            PopulateItemList();

            // calculate totals
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalqty", totalqty);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalweight", totalweight);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalunitcost", totalunitcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotalcost", subtotalcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotaldealercost", subtotaldealercost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedsubtotal", AppliedCost(PortalId, UserId, subtotalcost, subtotaldealercost));

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealerdiscount", totaldealerdiscount);
            var applieddiscount = AppliedCost(PortalId, UserId, totaldiscount, totaldealerdiscount);
            PurchaseInfo.SetXmlPropertyDouble("genxml/applieddiscount", applieddiscount);

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", totaldealerbonus);


            //add shipping
            Double shippingcost = 0;
            Double shippingdealercost = 0;
            var shippingkey = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");
            var currentcartstage = PurchaseInfo.GetXmlProperty("genxml/currentcartstage");
            if (currentcartstage == "cartaddress" || currentcartstage == "cartsummary") // can only calc shipping on this stage.
            {
                ShippingInterface shipprov = null;
                if (shippingkey == "")
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetShippingProviders();
                    foreach (var d in provList)
                    {
                        var isValid = true;
                        var shipprov1 = ShippingInterface.Instance(d.Key);
                        if (shipprov1 != null) isValid = shipprov1.IsValid(PurchaseInfo);
                        if (isValid)
                        {
                            shipprov = shipprov1;
                            if (shipprov != null) PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", shipprov.Shippingkey);
                            break;
                        }
                    }
                }
                else
                    shipprov = ShippingInterface.Instance(shippingkey);

                if (shipprov != null && !shipprov.IsValid(PurchaseInfo)) // Check we have a valid one!! if not the get another!!
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetShippingProviders();
                    foreach (var d in provList)
                    {
                        var isValid = true;
                        var shipprov1 = ShippingInterface.Instance(d.Key);
                        if (shipprov1 != null) isValid = shipprov1.IsValid(PurchaseInfo);
                        if (isValid)
                        {
                            shipprov = shipprov1;
                            if (shipprov != null) PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", shipprov.Shippingkey);
                            break;
                        }
                    }
                }


                if (shipprov != null)
                {
                    PurchaseInfo = shipprov.CalculateShipping(PurchaseInfo);
                    shippingcost = PurchaseInfo.GetXmlPropertyDouble("genxml/shippingcost");
                    shippingdealercost = PurchaseInfo.GetXmlPropertyDouble("genxml/shippingdealercost");
                }
                PurchaseInfo.SetXmlPropertyDouble("genxml/appliedshipping", AppliedCost(PortalId, UserId, shippingcost, shippingdealercost));
            }
            else
            {
                // clear the provider if not cartshipping stage
                PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", "");
            }


            //add tax
            Double appliedtax = 0;
            var taxproviderkey = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/hidden/taxproviderkey");
            var taxprov = TaxInterface.Instance(taxproviderkey);
            if (taxprov != null)
            {
                PurchaseInfo = taxprov.Calculate(PurchaseInfo);
                appliedtax = PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtax");
            }

            //cart full total
            var dealertotal = (subtotaldealercost + shippingdealercost + appliedtax);
            var total = (subtotalcost + shippingcost + appliedtax);
            PurchaseInfo.SetXmlPropertyDouble("genxml/dealertotal", dealertotal);
            PurchaseInfo.SetXmlPropertyDouble("genxml/total", total);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedtotal", AppliedCost(PortalId, UserId, (total - totaldiscount), (dealertotal - totaldealerdiscount)));

            if (PurchaseInfo.GetXmlProperty("genxml/clientmode") == "True")
            {
                // user not editor, so stop edit mode.
                if (!UserController.GetCurrentUserInfo().IsInRole("Administrators") && !UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) && !UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole)) PurchaseInfo.SetXmlProperty("genxml/clientmode", "False");
            }

            PurchaseInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartAfter, PurchaseInfo);

            SavePurchaseData();
        }

        private NBrightInfo ValidateCartItem(int portalId, int userId, NBrightInfo cartItemInfo)
        {
            cartItemInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartItemBefore, cartItemInfo);

            var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
            var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");
            var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");

            var prd = new ProductData(prdid, Utils.GetCurrentCulture());
            if (!prd.Exists) return null; //Invalid product remove from cart
            var prdModel = prd.GetModel(modelid);
            if (prdModel == null) return null; // Invalid Model remove from cart
            // check if dealer (for tax calc)
            var userInfo = UserController.GetUserById(portalId, userId);
            if (userInfo != null && userInfo.IsInRole(StoreSettings.DealerRole) && StoreSettings.Current.Get("enabledealer") == "True")
                cartItemInfo.SetXmlProperty("genxml/isdealer", "True");
            else
                cartItemInfo.SetXmlProperty("genxml/isdealer", "False");

            // check for price change
            var unitcost = prdModel.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
            var dealercost = prdModel.GetXmlPropertyDouble("genxml/textbox/txtdealercost");
            var saleprice = prdModel.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");


            // calc sale price
            var sellcost = unitcost;
            if (saleprice > 0 && saleprice < sellcost) sellcost = saleprice;

            //stock control
            if (prdModel != null)
            {
                var stockon = prdModel.GetXmlPropertyBool("genxml/checkbox/chkstockon");
                var stocklevel = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtyremaining");
                var minStock = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtyminstock");
                if (minStock == 0) minStock = StoreSettings.Current.GetInt("minimumstocklevel");
                var maxStock = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtystockset");
                var weight = prdModel.GetXmlPropertyDouble("genxml/textbox/weight");
                if (stockon)
                {
                    stocklevel = stocklevel - minStock;
                    stocklevel = stocklevel - prd.GetModelTransQty(modelid, _cartId);
                    if (stocklevel < qty)
                    {
                        qty = stocklevel;
                        if (qty <= 0)
                        {
                            qty = 0;
                            cartItemInfo.SetXmlProperty("genxml/validatecode", "OUTOFSTOCK");
                        }
                        else
                        {
                            cartItemInfo.SetXmlProperty("genxml/validatecode", "STOCKADJ");
                        }
                        base.SetValidated(false);
                        cartItemInfo.SetXmlPropertyDouble("genxml/qty", qty.ToString(""));
                    }
                }

                Double additionalCosts = 0;
                var optNods = cartItemInfo.XMLDoc.SelectNodes("genxml/options/*");
                if (optNods != null)
                {
                    var lp = 1;
                    foreach (XmlNode nod in optNods)
                    {
                        var optid = nod.SelectSingleNode("optid");
                        if (optid != null)
                        {
                            var optvalueid = nod.SelectSingleNode("optvalueid");
                            if (optvalueid != null && optvalueid.InnerText != "False")
                            {
                                XmlNode  optvalcostnod;
                                if (optvalueid.InnerText == "True")
                                    optvalcostnod = cartItemInfo.XMLDoc.SelectSingleNode("genxml/productxml/genxml/optionvalues[@optionid='" + optid.InnerText + "']/genxml/textbox/txtaddedcost");
                                else
                                    optvalcostnod = cartItemInfo.XMLDoc.SelectSingleNode("genxml/productxml/genxml/optionvalues/genxml[./hidden/optionvalueid='" + optvalueid.InnerText + "']/textbox/txtaddedcost");

                                if (optvalcostnod != null)
                                {
                                    var optvalcost = optvalcostnod.InnerText;
                                    if (Utils.IsNumeric(optvalcost))
                                    {
                                        cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvalcost", optvalcost);
                                        var optvaltotal = Convert.ToDouble(optvalcost, CultureInfo.GetCultureInfo("en-US"))*qty;
                                        cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", optvaltotal);
                                        additionalCosts += optvaltotal;
                                    }
                                }
                                else
                                {
                                    cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvalcost", "0");
                                    cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", "0");
                                }
                            }
                        }
                        lp += 1;
                    }
                }

                if (qty > 0)  // can't devide by zero
                {                   
                    unitcost += (additionalCosts / qty);
                    if (dealercost > 0) dealercost += (additionalCosts / qty); // zero turns off
                    if (saleprice > 0) saleprice += (additionalCosts / qty); // zero turns off
                    sellcost += (additionalCosts / qty);
                }

                var totalcost = qty * sellcost;
                var totaldealercost = qty * dealercost;
                var totalweight = weight * qty;


                if (unitcost != cartItemInfo.GetXmlPropertyDouble("genxml/unitcost") || dealercost != cartItemInfo.GetXmlPropertyDouble("genxml/dealercost") || saleprice != cartItemInfo.GetXmlPropertyDouble("genxml/saleprice"))
                {
                    cartItemInfo.SetXmlPropertyDouble("genxml/unitcost", unitcost);
                    cartItemInfo.SetXmlPropertyDouble("genxml/dealercost", dealercost);
                    cartItemInfo.SetXmlPropertyDouble("genxml/saleprice", saleprice);
                    cartItemInfo.RemoveXmlNode("genxml/productxml");
                    cartItemInfo.AddSingleNode("productxml", prd.Info.XMLData, "genxml");
                }


                cartItemInfo.SetXmlPropertyDouble("genxml/totalweight", totalweight.ToString(""));
                cartItemInfo.SetXmlPropertyDouble("genxml/totalcost", totalcost);
                cartItemInfo.SetXmlPropertyDouble("genxml/totaldealercost", totaldealercost);
                cartItemInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", (totalcost - totaldealercost));

                Double salediscount = 0;
                Double dealerdiscount = 0;
                Double discountcodeamt = 0;
                Double totaldiscount = 0;

                //add update genxml/discountcodeamt
                if (saleprice == 0) // discount codes are only valid for items not on sale
                {
                    var discountcode = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/promocode");
                    cartItemInfo = DiscountCodeInterface.UpdateItemPercentDiscountCode(PortalId, UserId, cartItemInfo, discountcode);
                    discountcodeamt = cartItemInfo.GetXmlPropertyDouble("genxml/discountcodeamt");
                    if (discountcodeamt > 0) PurchaseInfo.SetXmlProperty("genxml/discountprocessed", "False");
                    totaldiscount = discountcodeamt;
                }
                else
                {
                    salediscount = (unitcost - saleprice);
                    totaldiscount = salediscount * qty;
                }

                var totalsalediscount = salediscount * qty;
                var totaldealerdiscount = dealerdiscount * qty;
                cartItemInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);
                cartItemInfo.SetXmlPropertyDouble("genxml/salediscount", totalsalediscount);
                cartItemInfo.SetXmlPropertyDouble("genxml/totaldealerdiscount", totaldealerdiscount);



                cartItemInfo.SetXmlPropertyDouble("genxml/appliedtotalcost", AppliedCost(portalId, userId, totalcost, totaldealercost));
                cartItemInfo.SetXmlPropertyDouble("genxml/appliedcost", AppliedCost(portalId, userId, sellcost, dealercost));

                // calc tax for item
                var taxproviderkey = PurchaseInfo.GetXmlProperty("genxml/hidden/taxproviderkey");
                var taxprov = TaxInterface.Instance(taxproviderkey);
                if (taxprov != null)
                {
                    var nbi = (NBrightInfo)cartItemInfo.Clone();
                    cartItemInfo.SetXmlPropertyDouble("genxml/taxcost", taxprov.CalculateItemTax(nbi));
                }

            }

            cartItemInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartItemAfter, cartItemInfo);

            return cartItemInfo;
        }

        private Double AppliedCost(int portalId, int userId, Double cost, Double dealercost)
        {
            if (cost < 0) cost = 0;
            if (dealercost < 0) dealercost = 0;
            //always return nortmal price for non-registered users
            if (UserController.GetCurrentUserInfo().UserID == -1) return cost;

            var userInfo = UserController.GetUserById(portalId, userId);
            if (userInfo != null)
            {
                if (userInfo.IsInRole(StoreSettings.DealerRole) && StoreSettings.Current.Get("enabledealer") == "True") return dealercost;
            }
            return cost;
        }


        #endregion

    }
}

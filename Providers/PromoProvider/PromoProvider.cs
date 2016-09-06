using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers.PromoProvider
{
    public class GroupPromoScheudler : Components.Interfaces.SchedulerInterface
    {
        public override string DoWork(int portalId)
        {
            return PromoUtils.CalcGroupPromo(portalId);
        }

    }


    public class MultiBuyPromoScheudler : Components.Interfaces.SchedulerInterface
    {
        public override string DoWork(int portalId)
        {
            return PromoUtils.CalcMultiBuyPromo(portalId);
        }

    }

    public class CalcPromo : Components.Interfaces.PromoInterface
    {

        public override string ProviderKey { get; set; }

        public override NBrightInfo CalculatePromotion(int portalId, NBrightInfo cartInfo)
        {
            // loop through cart items
            var rtncartInfo = (NBrightInfo)cartInfo.Clone();
            try
            {

                var cartData = new CartData(cartInfo.PortalId);
                var cartList = cartData.GetCartItemList();

                foreach (var cartItemInfo in cartList)
                {
                    if (cartItemInfo.GetXmlProperty("genxml/productxml/genxml/hidden/promotype") == "PROMOMULTIBUY")
                    {

                        var promoid = cartItemInfo.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid");
                        var objCtrl = new NBrightBuyController();
                        var promoData = objCtrl.GetData(promoid);
                        if (promoData != null)
                        {
                            //NOTE: WE nedd to process disabld promotions so they can be removed from cart

                            var applydiscountto = promoData.GetXmlPropertyInt("genxml/radiobuttonlist/applydiscountto");
                            var buyqty = promoData.GetXmlPropertyInt("genxml/textbox/buyqty");
                            var validfrom = promoData.GetXmlProperty("genxml/textbox/validfrom");
                            var validuntil = promoData.GetXmlProperty("genxml/textbox/validuntil");
                            var propbuygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propbuy");
                            var propapplygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propapply");
                            var amounttype = promoData.GetXmlProperty("genxml/radiobuttonlist/amounttype");
                            var amount = promoData.GetXmlPropertyDouble("genxml/textbox/amount");


                            if (applydiscountto == 1)
                            {
                                // Applied discount to this single cart item
                                if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled") && cartItemInfo.GetXmlPropertyInt("genxml/qty") >= buyqty && Utils.IsDate(validfrom) && Utils.IsDate(validuntil)) // check we have correct qty to activate promo
                                {
                                    var dteF = Convert.ToDateTime(validfrom).Date;
                                    var dteU = Convert.ToDateTime(validuntil).Date;
                                    if (DateTime.Now.Date >= dteF && DateTime.Now.Date <= dteU)
                                    {
                                        // calc discount amount

                                        var cartqty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                                        var qtycount = cartqty;
                                        var unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/basecost");
                                        double discountamt = 0;
                                        while (qtycount > buyqty)
                                        {
                                            if (amounttype == "1")
                                            {
                                                discountamt += (unitcost - amount);
                                            }
                                            else
                                            {
                                                discountamt += ((unitcost / 100) * amount);
                                            }
                                            if (discountamt < 0) discountamt = 0;
                                            
                                            qtycount = (qtycount - (buyqty + 1)); // +1 so we allow for discount 1 in basket.
                                        }

                                        cartInfo.SetXmlPropertyDouble("genxml/items/genxml[./itemcode = '" + cartItemInfo.GetXmlProperty("genxml/itemcode") + "']/promodiscount", discountamt);

                                    }
                                }

                            }

                            if (applydiscountto == 2)
                            {
                                // Add assighned products to cart and Apply discount to these.

                                var alreadtcalc = new List<int>(); // used to keep track of calculated promos, so if we have promo in cart mutliple times the items are not removed when calc 2nd.

                                // delete any cart items existing with this promotion
                                foreach (var ca in cartList)
                                {
                                    if (!alreadtcalc.Contains(promoid) && ca.GetXmlProperty("genxml/productxml/genxml/hidden/promotype") == "PROMOMULTIBUYAPPLY" && ca.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid") == promoid)
                                    {
                                        cartInfo.RemoveXmlNode("genxml/items/genxml[./itemcode='" + ca.GetXmlProperty("genxml/itemcode") + "']");
                                    }
                                }

                                if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled") && cartItemInfo.GetXmlPropertyInt("genxml/qty") >= buyqty && Utils.IsDate(validfrom) && Utils.IsDate(validuntil)) // check we have correct qty to activate promo
                                {
                                    var dteF = Convert.ToDateTime(validfrom).Date;
                                    var dteU = Convert.ToDateTime(validuntil).Date;
                                    if (DateTime.Now.Date >= dteF && DateTime.Now.Date <= dteU)
                                    {

                                        // create a new purchase data, so we can easily create the required XML for adding.
                                        var tmpPurchaseData = new PurchaseData();
                                        tmpPurchaseData.PurchaseInfo = new NBrightInfo(true);
                                        tmpPurchaseData.PopulateItemList();

                                        var gCat = CategoryUtils.GetCategoryData(propapplygroupid, Utils.GetCurrentCulture());
                                        var multibuyAddItems2 = gCat.GetAllArticles();
                                        foreach (var ca in multibuyAddItems2)
                                        {
                                            tmpPurchaseData.AddSingleItem(ca.ParentItemId.ToString(), "defaultpromo", "1", new NBrightInfo());
                                        }

                                        // Loop on the created items and calc the promoprice.
                                        // This seems odd to do this in a sperate loop, but some quirk in XMLDocument force the promoprice element to always be added to last item if done in previous loop.
                                        var tmpList = tmpPurchaseData.GetCartItemList();
                                        foreach (var tmpItem in tmpList)
                                        {
                                            // do calc discount on added item
                                            var unitcost = tmpPurchaseData.PurchaseInfo.GetXmlPropertyDouble("genxml/items/genxml[./productid='" + tmpItem.GetXmlProperty("genxml/productid") + "']/productxml/genxml/models/genxml[1]/textbox/txtunitcost");
                                            Double newamt = 0;
                                            if (amounttype == "1")
                                            {
                                                newamt = unitcost - amount;
                                            }
                                            else
                                            {
                                                newamt = unitcost - ((unitcost/100)*amount);
                                            }
                                            if (newamt < 0) newamt = 0;
                                            tmpPurchaseData.PurchaseInfo.SetXmlPropertyDouble("genxml/items/genxml[./productid='" + tmpItem.GetXmlProperty("genxml/productid") + "']/promoprice", newamt);
                                        }
                                        // addd new promo products to cartlist
                                        tmpList = tmpPurchaseData.GetCartItemList();
                                        foreach (var tmpItem in tmpList)
                                        {
                                            cartInfo.RemoveXmlNode("genxml/items/genxml[./itemcode='" + tmpItem.GetXmlProperty("genxml/itemcode") + "']");
                                            cartInfo.AddXmlNode(tmpItem.XMLData, "genxml", "genxml/items");
                                        }
                                        alreadtcalc.Add(promoid);
                                    }
                                }
                            }
                        }
                    }
                }

                return cartInfo;
            }
            catch (Exception ex)
            {
                var x = ex.ToString();
                return rtncartInfo;
            }
        }

    }


    public class PromoEvents : Components.Interfaces.EventInterface
    {
        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo AfterCartSave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterProductSave(NBrightInfo nbrightInfo)
        {
            var promoid = nbrightInfo.GetXmlPropertyInt("genxml/hidden/promoid");
            if (promoid > 0)
            {
                var prdData = ProductUtils.GetProductData(nbrightInfo.ItemID, nbrightInfo.Lang);
                var objCtrl = new NBrightBuyController();
                var promoData = objCtrl.GetData(promoid);

                var catgroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/catgroupid");
                var propgroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propgroupid");
                var propbuygroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propbuy");
                var propapplygroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propapply");

                var removepromo = true;
                foreach (var c in prdData.GetCategories())
                {
                    if (c.categoryid == catgroupid) removepromo = false;
                    if (c.categoryid == propgroupid) removepromo = false;
                    if (c.categoryid == propbuygroupid) removepromo = false;
                    if (c.categoryid == propapplygroupid) removepromo = false;
                }

                if (removepromo)
                {
                    PromoUtils.RemoveProductPromoData(nbrightInfo.PortalId, nbrightInfo.ItemID,promoid);
                    ProductUtils.RemoveProductDataCache(nbrightInfo.PortalId, nbrightInfo.ItemID);
                }
            }

            return nbrightInfo;
        }

        public override NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }
    }
}

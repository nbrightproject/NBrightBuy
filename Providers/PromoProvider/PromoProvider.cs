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
                            if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled"))
                            {
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

                                    if (cartItemInfo.GetXmlPropertyInt("genxml/qty") >= buyqty && Utils.IsDate(validfrom) && Utils.IsDate(validuntil)) // check we have correct qty to activate promo
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
                                                    newamt = unitcost - ((unitcost / 100) * amount);
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

}

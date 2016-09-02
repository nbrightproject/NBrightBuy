using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Xml;
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

    public class PromoEvents : Components.Interfaces.EventInterface 
    {
        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            // loop through cart items

            var cartData = new CartData(cartInfo.PortalId);
            var cartList = cartData.GetCartItemList();

            var multibuyItems = from c in cartList where c.GetXmlProperty("genxml/productxml/genxml/hidden/promotype") == "PROMOMULTIBUY" select c;

            foreach (var cartItemInfo in multibuyItems)
            {
                var promoid = cartItemInfo.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid");
                var objCtrl = new NBrightBuyController();
                var promoData = objCtrl.GetData(promoid);
                if (promoData != null)
                {
                    if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled"))
                    {
                        var applydiscountto = promoData.GetXmlPropertyInt("genxml/radiobuttonlist/applydiscountto");
                        var buyqty = promoData.GetXmlPropertyInt("genxml/radiobuttonlist/buyqty");
                        var validfrom = promoData.GetXmlProperty("genxml/textbox/validfrom");
                        var validuntil = promoData.GetXmlProperty("genxml/textbox/validuntil");
                        var propbuygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propbuy");
                        var propapplygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propapply");


                        if (applydiscountto == 1)
                        { // Applied discount to this single cart item

                            
                        }
                        if (applydiscountto == 2)
                        { // Add assighned products to cart and Apply discount to these.

                            // delete any cart items existing with this promotion
                            var multibuyAddItems = from c in cartList where c.GetXmlProperty("genxml/productxml/genxml/hidden/promotype") == "PROMOMULTIBUYAPPLY" && cartItemInfo.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid") == promoid select c;
                            foreach (var ca in multibuyAddItems)
                            {
                                cartData.RemoveItem(ca.GetXmlProperty("genxml/itemcode"));
                            }
                            var gCat = CategoryUtils.GetCategoryData(propapplygroupid, Utils.GetCurrentCulture());
                            var multibuyAddItems2 = gCat.GetAllArticles();
                            foreach (var ca in multibuyAddItems2)
                            {
                                cartData.AddSingleItem(ca.ParentItemId.ToString(), "firstdefault", "1", new NBrightInfo());
                            }
                        }


                    }
                }
            }

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

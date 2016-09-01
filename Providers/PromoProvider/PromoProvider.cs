using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
            return cartInfo;
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            var promotype = cartItemInfo.GetXmlProperty("genxml/productxml/genxml/hidden/promotype");
            if (promotype == "PROMOMULTIBUY")
            {
                var promoid = cartItemInfo.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid");
                var objCtrl = new NBrightBuyController();
                var promoData = objCtrl.GetData(promoid);
                if (promoData != null)
                {
                    if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled"))
                    {
                        var applymodel = promoData.GetXmlPropertyInt("genxml/radiobuttonlist/applymodel");
                        var buyqty = promoData.GetXmlPropertyInt("genxml/radiobuttonlist/buyqty");
                        
                    }
                }
            }
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

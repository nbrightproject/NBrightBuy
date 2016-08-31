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

    public class MultiBuyPromoProvider : Components.Interfaces.EventInterface 
    {
        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterCartSave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterProductSave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            throw new NotImplementedException();
        }
    }

}

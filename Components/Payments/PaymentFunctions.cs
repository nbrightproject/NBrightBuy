using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components.Payments
{
    public static class PaymentFunctions
    {

        public static string ProcessCommand(string paramCmd, HttpContext context)
        {
            var strOut = "ORDER - ERROR!! - No Security rights for current user!";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            switch (paramCmd)
            {
                case "payment_manualpayment":
                    strOut = "";
                    var cartInfo = new CartData(PortalSettings.Current.PortalId);
                    if (cartInfo != null)
                    {
                        cartInfo.SaveModelTransQty(); // move qty into trans
                        cartInfo.ConvertToOrder(StoreSettings.Current.DebugMode);
                        var orderData = new OrderData(cartInfo.PurchaseInfo.ItemID);
                        orderData.PaymentProviderKey = ajaxInfo.GetXmlProperty("genxml/hidden/paymentproviderkey").ToLower(); // provider keys should always be lowecase
                        orderData.SavePurchaseData();
                        strOut = PaymentsInterface.Instance(orderData.PaymentProviderKey).RedirectForPayment(orderData);
                    }
                    
                    break;
            }

            return strOut;
        }


    }
}

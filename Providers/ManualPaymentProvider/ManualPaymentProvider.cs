using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using ManualPaymentProvider;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ManualPaymentProvider :Components.Interfaces.PaymentsInterface 
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var info = ProviderUtils.GetProviderSettings("manualpayment");
            var templ = ProviderUtils.GetTemplateData(info.GetXmlProperty("genxml/textbox/checkouttemplate"));

            return templ;
        }

        public override string RedirectForPayment(OrderData orderData)
        {
            orderData.OrderStatus = "020";
            orderData.SavePurchaseData();
            var param = new string[3];
            param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("D");
            param[1] = "status=1";
            return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            var orderid = Utils.RequestQueryStringParam(context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                var orderData = new OrderData(Convert.ToInt32(orderid));

                // 010 = Incomplete, 020 = Waiting for Bank,030 = Cancelled,040 = Payment OK,050 = Payment Not Verified,060 = Waiting for Payment,070 = Waiting for Stock,080 = Waiting,090 = Shipped,010 = Closed,011 = Archived
                if (orderData.OrderStatus == "020")
                {
                    var rtnstatus = Utils.RequestQueryStringParam(context, "status");
                    if (rtnstatus == "1") // status 1 = successful (0 = fail)
                    {
                        orderData.ApplyModelTransQty();
                        orderData.OrderStatus = "060";
                        orderData.SavePurchaseData();
                    }
                }

            }
            return "";
        }

    }
}

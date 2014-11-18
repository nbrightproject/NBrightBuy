using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class DiscountCodesProvider : Components.Interfaces.DiscountCodeInterface 
    {
        public override string ProviderKey { get; set; }

        public override NBrightInfo CalculateItemPercentDiscount(int portalId, int userId, NBrightInfo cartItemInfo,String discountcode)
        {
            if (userId <= 0) return cartItemInfo;
            cartItemInfo.SetXmlPropertyDouble("genxml/discountcodeamt", "0"); // reset discount amount
            if (discountcode == "") return cartItemInfo;
            var clientData = new ClientData(portalId,userId);
            if (clientData.DiscountCodes.Count == 0) return cartItemInfo;

            Double discountcodeamt = 0;
            foreach (var d in clientData.DiscountCodes)
            {
                var validutil = d.GetXmlProperty("genxml/textbox/validuntil");
                var validutildate = DateTime.Today;
                if (Utils.IsDate(validutil)) validutildate = Convert.ToDateTime(validutil);
                if (d.GetXmlProperty("genxml/textbox/coderef").ToLower() == discountcode.ToLower() && validutildate >= DateTime.Today)
                {
                    var usageleft = d.GetXmlPropertyDouble("genxml/textbox/usageleft");
                    var percentage = d.GetXmlPropertyDouble("genxml/textbox/percentage");
                    if (percentage > 0 && usageleft > 0)
                    {
                        var appliedtotalcost = cartItemInfo.GetXmlPropertyDouble("genxml/appliedtotalcost");
                        discountcodeamt = ((appliedtotalcost/100)*percentage);
                    }
                }
                if (discountcodeamt > 0) break;
            }
            cartItemInfo.SetXmlPropertyDouble("genxml/discountcodeamt", discountcodeamt);
            
            return cartItemInfo;
        }

        public override NBrightInfo CalculateVoucherAmount(int portalId, int userId, NBrightInfo cartInfo, string discountcode)
        {
            return cartInfo;
        }

        public override NBrightInfo UpdatePercentUsage(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            if (userId <= 0) return purchaseInfo;
            var discountcode = purchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/promocode");
            if (!purchaseInfo.GetXmlPropertyBool("genxml/discountprocessed"))
            {
                if (discountcode == "") return purchaseInfo;
                var clientData = new ClientData(portalId, userId);
                if (clientData.DiscountCodes.Count == 0) return purchaseInfo;
                var list = clientData.DiscountCodes;
                foreach (var d in list)
                {
                    if (d.GetXmlProperty("genxml/textbox/coderef").ToLower() == discountcode.ToLower())
                    {
                        var usageleft = d.GetXmlPropertyDouble("genxml/textbox/usageleft");
                        var used = d.GetXmlPropertyDouble("genxml/textbox/used");
                        d.SetXmlPropertyDouble("genxml/textbox/usageleft", (usageleft - 1));
                        d.SetXmlPropertyDouble("genxml/textbox/used", (used + 1));
                    }
                }
                clientData.UpdateDiscountCodeList(list);
                clientData.Save();
                purchaseInfo.SetXmlProperty("genxml/discountprocessed", "True");
            }
            return purchaseInfo;
        }


        public override NBrightInfo UpdateVoucherAmount(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            return purchaseInfo;
        }
    }
}

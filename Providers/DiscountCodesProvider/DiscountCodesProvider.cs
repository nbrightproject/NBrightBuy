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
            if (discountcode == "") return cartItemInfo;
            cartItemInfo.SetXmlPropertyDouble("genxml/discountcodeamt", "0"); // reset discount amount
            Double discountcodeamt = 0;
            if (userId > 0)
            {
                var clientData = new ClientData(portalId, userId);
                if (clientData.DiscountCodes.Count > 0)
                {
                    // do client level discount on total cart
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
                }
            }

            if (discountcodeamt == 0) // if no client level, calc any portal level percentage discount
            {
                var objCtrl = new NBrightBuyController();
                var d = objCtrl.GetByGuidKey(portalId, -1, "DISCOUNTCODE", discountcode);
                if (d != null)
                {
                    var validutil = d.GetXmlProperty("genxml/textbox/validuntil");
                    var validutildate = DateTime.Today;
                    if (Utils.IsDate(validutil)) validutildate = Convert.ToDateTime(validutil);
                    if (validutildate >= DateTime.Today && d.GetXmlProperty("genxml/radiobuttonlist/amounttype") == "2")
                    {
                        var usage = d.GetXmlPropertyDouble("genxml/textbox/usage");
                        var usagelimit = d.GetXmlPropertyDouble("genxml/textbox/usagelimit");
                        var percentage = d.GetXmlPropertyDouble("genxml/textbox/amount");
                        if (percentage > 0 && (usagelimit == 0 || usagelimit > usage))
                        {
                            var appliedtotalcost = cartItemInfo.GetXmlPropertyDouble("genxml/appliedtotalcost");
                            discountcodeamt = ((appliedtotalcost / 100) * percentage);
                        }
                    }
                }
            }

            cartItemInfo.SetXmlPropertyDouble("genxml/discountcodeamt", discountcodeamt);
            
            return cartItemInfo;
        }

        public override NBrightInfo CalculateVoucherAmount(int portalId, int userId, NBrightInfo cartInfo, string discountcode)
        {
            if (discountcode == "") return cartInfo;
            cartInfo.SetXmlPropertyDouble("genxml/voucherdiscount", "0"); // reset discount amount
            Double discountcodeamt = 0;
            if (userId > 0)
            {
                var clientData = new ClientData(portalId, userId);
                if (clientData.DiscountCodes.Count > 0)
                {
                    // do client level discount on total cart
                    foreach (var d in clientData.DiscountCodes)
                    {
                        var validutil = d.GetXmlProperty("genxml/textbox/validuntil");
                        var validutildate = DateTime.Today;
                        if (Utils.IsDate(validutil)) validutildate = Convert.ToDateTime(validutil);
                        if (d.GetXmlProperty("genxml/textbox/coderef").ToLower() == discountcode.ToLower() && validutildate >= DateTime.Today)
                        {
                            var usageleft = d.GetXmlPropertyDouble("genxml/textbox/usageleft");
                            var amount = d.GetXmlPropertyDouble("genxml/textbox/amount");
                            if (amount > 0 && usageleft > 0) discountcodeamt = amount;
                        }
                        if (discountcodeamt > 0) break;
                    }
                }
            }

            if (discountcodeamt == 0) // if no client level, calc any portal level percentage discount
            {
                var objCtrl = new NBrightBuyController();
                var d = objCtrl.GetByGuidKey(portalId, -1, "DISCOUNTCODE", discountcode);
                if (d != null)
                {
                    var validutil = d.GetXmlProperty("genxml/textbox/validuntil");
                    var validutildate = DateTime.Today;
                    if (Utils.IsDate(validutil)) validutildate = Convert.ToDateTime(validutil);
                    if (validutildate >= DateTime.Today && d.GetXmlProperty("genxml/radiobuttonlist/amounttype") == "1")
                    {
                        var usage = d.GetXmlPropertyDouble("genxml/textbox/usage");
                        var usagelimit = d.GetXmlPropertyDouble("genxml/textbox/usagelimit");
                        var amount = d.GetXmlPropertyDouble("genxml/textbox/amount");
                        if (amount > 0 && (usagelimit == 0 || usagelimit > usage)) discountcodeamt = amount;
                    }
                }
            }

            cartInfo.SetXmlPropertyDouble("genxml/voucherdiscount", discountcodeamt); // reset discount amount

            return cartInfo;
        }

        public override NBrightInfo UpdatePercentUsage(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            var discountcode = purchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/promocode");
            if (!purchaseInfo.GetXmlPropertyBool("genxml/discountprocessed"))
            {
                if (userId > 0)
                {
                    if (discountcode == "") return purchaseInfo;
                    var clientData = new ClientData(portalId, userId);
                    if (clientData.DiscountCodes.Count > 0)
                    {
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
                }

                var objCtrl = new NBrightBuyController();
                var dis = objCtrl.GetByGuidKey(portalId, -1, "DISCOUNTCODE", discountcode);
                if (dis != null)
                {
                    var usage = dis.GetXmlPropertyDouble("genxml/textbox/usage");
                    dis.SetXmlPropertyDouble("genxml/textbox/usage", (usage + 1));
                    objCtrl.Update(dis);
                    purchaseInfo.SetXmlProperty("genxml/discountprocessed", "True");
                }
            }

            return purchaseInfo;
        }


        public override NBrightInfo UpdateVoucherAmount(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            // the "UpdatePercentUsage" function deals with the vouchers for DISCOUNTCODE, just left the interface here for compatiblity.
            return purchaseInfo;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ShippingProvider :Components.Interfaces.ShippingInterface 
    {
        public override NBrightInfo CalculateShipping(NBrightInfo cartInfo)
        {

            var shipData = new ShippingData(Shippingkey);
            var shipoption = cartInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
            Double rangeValue = 0;
            if (shipData.CalculationUnit == "1")
                rangeValue = cartInfo.GetXmlPropertyDouble("genxml/totalweight"); 
            else
                rangeValue = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
            var countrycode = "";
            var regioncode = "";
            var regionkey = "";
            var total = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
            switch (shipoption)
            {
                case "1":
                    countrycode = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/country");
                    regionkey = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/region");
                    break;
                case "2":
                    countrycode = cartInfo.GetXmlProperty("genxml/shipaddress/genxml/dropdownlist/country");
                    regionkey = cartInfo.GetXmlProperty("genxml/shipaddress/genxml/dropdownlist/region");                    
                    break;
            }

            if (regionkey != "")
            {
                var rl = regionkey.Split(':');
                if (rl.Count() == 2) regioncode = rl[1];
            }

            var shippingcost = shipData.CalculateShipping(countrycode, regioncode, rangeValue, total);
            var shippingdealercost = shippingcost;
            cartInfo.SetXmlPropertyDouble("genxml/shippingcost", shippingcost);
            cartInfo.SetXmlPropertyDouble("genxml/shippingdealercost", shippingdealercost);

            return cartInfo;            
        }

        public override string Shippingkey { get; set; }

        public override string Name()
        {
            return "Standard";
        }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            return "";
        }

        public override string GetDeliveryLabelUrl(NBrightInfo cartInfo)
        {
            return "";
        }

        public override bool IsValid(NBrightInfo cartInfo)
        {
            // check if this provider is valid for the counrty in the checkout
            var shipoption = cartInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
            var countrycode = "";
            switch (shipoption)
            {
                case "1":
                    countrycode = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/country");
                    break;
                case "2":
                    countrycode = cartInfo.GetXmlProperty("genxml/shipaddress/genxml/dropdownlist/country");
                    break;
            }

            var isValid = true;
            var shipData = new ShippingData(Shippingkey);
            var validlist = "," + shipData.Info.GetXmlProperty("genxml/textbox/validcountrycodes") + ",";
            var notvalidlist = "," + shipData.Info.GetXmlProperty("genxml/textbox/notvalidcountrycodes") + ",";
            if (validlist.Trim(',') != "")
            {
                isValid = false;
                if (validlist.Contains("," + countrycode + ",")) isValid = true;
            }
            if (notvalidlist.Trim(',') != "" && notvalidlist.Contains("," + countrycode + ",")) isValid = false;

            return isValid;

        }
    }
}

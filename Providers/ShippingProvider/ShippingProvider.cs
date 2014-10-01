using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ShippingProvider :Components.Interfaces.ShippingInterface 
    {
        public override NBrightInfo CalculateShipping(NBrightInfo cartInfo)
        {
            var shipData = new ShippingData(Shippingkey);
            var shipoption = cartInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
            Double total = 0;
            if (shipData.CalculationUnit == "1")
            {
                var totalweight = 0;

                total = totalweight;
            }
            else
            {
                total = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
            }
            var countrycode = "";
            var regioncode = "";
            var regionkey = "";
            Double rangeValue = 0;
            switch (shipoption)
            {
                case "1":
                    countrycode = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/country");
                    regionkey = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/region");
                    rangeValue = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
                    break;
                case "2":
                    countrycode = cartInfo.GetXmlProperty("genxml/shipaddress/genxml/dropdownlist/country");
                    regionkey = cartInfo.GetXmlProperty("genxml/shipaddress/genxml/dropdownlist/region");                    
                    rangeValue = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
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

        public override string GetTemplate()
        {
            return "";
        }
    }
}

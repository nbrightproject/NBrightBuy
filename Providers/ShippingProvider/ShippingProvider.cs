using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ShippingProvider :Components.Interfaces.ShippingInterface 
    {
        public override NBrightInfo CalculateShipping(NBrightInfo cartInfo)
        {
            var nbi = new NBrightInfo(true);
            var shipData = new ShippingData();
            var shipoption = cartInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
            var total = cartInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
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
                default:
                    nbi.SetXmlPropertyDouble("genxml/totaltest", 0);
                    break;
            }

            if (regionkey != "")
            {
                var rl = regionkey.Split(':');
                if (rl.Count() == 2) regioncode = rl[1];
            }

            nbi.SetXmlPropertyDouble("genxml/totaltest", shipData.CalculateShipping(countrycode, regioncode, rangeValue, total));
            return nbi;            


        }

    }
}

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
            nbi.SetXmlPropertyDouble("genxml/totaltest",0);
            return nbi;
        }
    }
}

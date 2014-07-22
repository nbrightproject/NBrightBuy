using System;
using System.Xml;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using System.Web.UI.WebControls;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class CartProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.CartInterface  
    {

        public override NBrightInfo ValidateCart(NBrightInfo cartInfo)
        {
            var nods = cartInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (nods != null)
            {
                var lp = 1;
                foreach (XmlNode nod in nods)
                {
                    var nbi = new NBrightInfo();
                    nbi.XMLData = nod.OuterXml;
                    nbi = ValidateCartItem(nbi);

                    var xmlNod1 = cartInfo.XMLDoc.SelectSingleNode("genxml/items/genxml[" + lp + "]");
                    if (xmlNod1 != null)
                    {
                        var xmlNod2 = nbi.XMLDoc.SelectSingleNode("genxml");
                        if (xmlNod2 != null)
                        {
                            var newNod = cartInfo.XMLDoc.ImportNode(xmlNod2, true);
                            cartInfo.XMLDoc.ReplaceChild(newNod, xmlNod1);
                        }
                    }
                    lp += 1;
                }
            }

            return cartInfo;
        }

        public override NBrightInfo ValidateCartItem(NBrightInfo cartItemInfo)
        {
            var unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/unitcost");
            var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
            var dealercost = cartItemInfo.GetXmlPropertyDouble("genxml/dealercost");
            var totalcost = qty*unitcost;
            var totaldealercost = qty * dealercost;

            var optNods = cartItemInfo.XMLDoc.SelectNodes("genxml/options/*");
            if (optNods != null)
            {
                var lp = 0;
                foreach (XmlNode nod in optNods)
                {
                    var optvalcostnod = nod.SelectSingleNode("option/optvalcost");
                    if (optvalcostnod != null)
                    {
                        var optvalcost = optvalcostnod.InnerText;
                        if (Utils.IsNumeric(optvalcost))
                        {
                            var optvaltotal = Convert.ToDouble(optvalcost) * qty;
                            cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", optvaltotal);
                            totalcost += optvaltotal;
                            totaldealercost += optvaltotal;
                        }
                    }
                    lp += 1;
                }
            }
            cartItemInfo.SetXmlPropertyDouble("genxml/totalcost", totalcost);
            cartItemInfo.SetXmlPropertyDouble("genxml/totaldealercost", totaldealercost);

            return cartItemInfo;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class DiscountCodesProvider : Components.Interfaces.DiscountCodeInterface 
    {
        public override string ProviderKey { get; set; }

        public override NBrightInfo CalculateItemDiscount(int portalId, int userId, NBrightInfo cartItemInfo,String discountcode)
        {
            var clientData = new ClientData(portalId,userId);
            if (clientData.DiscountCodes.Count == 0) return cartItemInfo;

            foreach (var d in clientData.DiscountCodes)
            {
                if (d.GetXmlProperty("") == discountcode)
                {

                    cartItemInfo.SetXmlPropertyDouble("genxml/discountcodeamt", "99");
                }                
            }
            
            return cartItemInfo;
        }
    }
}

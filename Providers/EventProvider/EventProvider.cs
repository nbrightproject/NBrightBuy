using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class EventProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.EventInterface 
    {

        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }
    }
}

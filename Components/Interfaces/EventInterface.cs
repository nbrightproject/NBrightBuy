
using System.Linq;
using DotNetNuke.Entities.Portals;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Runtime.Remoting;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class EventInterface
	{

        public abstract NBrightInfo ValidateCartBefore(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartAfter(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo);
        public abstract NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo);

	}

}


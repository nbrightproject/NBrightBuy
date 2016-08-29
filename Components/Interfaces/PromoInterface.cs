
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using DotNetNuke.Entities.Portals;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class PromoInterface
	{

        public abstract String ProviderKey { get; set; }

        public abstract string SchedulerPromotionCalc(int portalId);

        public abstract string ProductPromotionCalc(int portalId,int productId);


    }

}


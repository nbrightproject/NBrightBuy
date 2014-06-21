
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


	public abstract class ShippingInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        private static ShippingInterface objProvider = null;
        // constructor
        static ShippingInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{
			ObjectHandle handle = null;
			string[] Prov = null;
			string ProviderName = null;

            ProviderName = StoreSettings.Current.Get("shipping.provider");
            if (String.IsNullOrEmpty(ProviderName)) ProviderName = "NBrightBuy.ShippingProvider,Nevoweb.DNN.NBrightBuy.Providers.ShippingProvider"; 
            if (!string.IsNullOrEmpty(ProviderName))
			{
			    Prov = ProviderName.Split(',');
				handle = Activator.CreateInstance(Prov[0], Prov[1]);
                objProvider = (ShippingInterface)handle.Unwrap();
			}
        }

		// return the provider
        public static new ShippingInterface Instance()
		{
            return objProvider;
		}

		#endregion
        


	}

}


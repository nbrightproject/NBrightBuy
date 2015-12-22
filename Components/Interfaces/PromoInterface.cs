
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


	public abstract class PromoInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        private static PromoInterface objProvider = null;
        // constructor
        static PromoInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{
			ObjectHandle handle = null;
			string[] Prov = null;
			string ProviderName = null;

            ProviderName = StoreSettings.Current.Get("promo.provider");
            if (String.IsNullOrEmpty(ProviderName)) ProviderName = "NBrightBuy.PromoProvider,Nevoweb.DNN.NBrightBuy.Providers.PromoProvider"; 
            if (!string.IsNullOrEmpty(ProviderName))
			{
			    Prov = ProviderName.Split(',');
				handle = Activator.CreateInstance(Prov[0], Prov[1]);
                objProvider = (PromoInterface)handle.Unwrap();
			}
        }

		// return the provider
        public static PromoInterface Instance()
		{
            return objProvider;
		}

		#endregion
        


	}

}


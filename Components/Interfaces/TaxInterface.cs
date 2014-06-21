
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


	public abstract class TaxInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        private static TaxInterface objProvider = null;
        // constructor
        static TaxInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{
			ObjectHandle handle = null;
			string[] Prov = null;
			string ProviderName = null;

            ProviderName = StoreSettings.Current.Get("tax.provider");
            if (String.IsNullOrEmpty(ProviderName)) ProviderName = "NBrightBuy.TaxProvider,Nevoweb.DNN.NBrightBuy.Providers.TaxProvider"; 
            if (!string.IsNullOrEmpty(ProviderName))
			{
			    Prov = ProviderName.Split(',');
				handle = Activator.CreateInstance(Prov[0], Prov[1]);
                objProvider = (TaxInterface)handle.Unwrap();
			}
        }

		// return the provider
        public static new TaxInterface Instance()
		{
            return objProvider;
		}

		#endregion



	}

}


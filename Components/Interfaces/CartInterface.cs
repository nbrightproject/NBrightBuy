
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


	public abstract class CartInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

		private static CartInterface objProvider = null;
		// constructor
        static CartInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{
			ObjectHandle handle = null;
			string[] Prov = null;
			string ProviderName = null;

            ProviderName = StoreSettings.Current.Get("cart.provider");
            if (String.IsNullOrEmpty(ProviderName)) ProviderName = "NBrightBuy.CartProvider,Nevoweb.DNN.NBrightBuy.Providers.CartProvider"; 
            if (!string.IsNullOrEmpty(ProviderName))
			{
			    Prov = ProviderName.Split(',');
				handle = Activator.CreateInstance(Prov[0], Prov[1]);
                objProvider = (CartInterface)handle.Unwrap();
			}
		}

		// return the provider
        public static new CartInterface Instance()
		{
			return objProvider;
		}

		#endregion

	    public abstract NBrightInfo ValidateCart(NBrightInfo cartInfo);


	}

}


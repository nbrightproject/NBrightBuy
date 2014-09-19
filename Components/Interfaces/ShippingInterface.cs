
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

        //private static ShippingInterface objProvider = null;
	    private static Dictionary<String,ShippingInterface> _providerList; 
        // constructor
        static ShippingInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

			string providerName = null;

		    _providerList = new Dictionary<string, ShippingInterface>(); 

            providerName = StoreSettings.Current.Get("shipping.provider");
            if (providerName != "") providerName += ";";
            providerName += "NBrightBuy.ShippingProvider,Nevoweb.DNN.NBrightBuy.Providers.ShippingProvider";

		    var l = providerName.Split(';');
            foreach (var p in l)
            {
                ObjectHandle handle = null;
                string[] prov = null;
                prov = p.Split(',');
                handle = Activator.CreateInstance(prov[0], prov[1]);
                var objProvider = (ShippingInterface) handle.Unwrap();
                _providerList.Add(prov[1], objProvider);
            }

		}

		// return the provider
        public static new ShippingInterface Instance(String key)
		{
            return _providerList[key];
		}

		#endregion

        public abstract NBrightInfo CalculateShipping(NBrightInfo cartInfo);


	}

}


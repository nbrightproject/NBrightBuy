
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

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        private static EventInterface objProvider = null;
        // constructor
        static EventInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{
			ObjectHandle handle = null;
			string[] Prov = null;
			string ProviderName = null;

            ProviderName = StoreSettings.Current.Get("event.provider");
            if (String.IsNullOrEmpty(ProviderName)) ProviderName = "NBrightBuy.EventProvider,Nevoweb.DNN.NBrightBuy.Providers.EventProvider";
            if (!string.IsNullOrEmpty(ProviderName))
			{
			    Prov = ProviderName.Split(',');
				handle = Activator.CreateInstance(Prov[0], Prov[1]);
                objProvider = (EventInterface)handle.Unwrap();
			}
        }

		// return the provider
        public static new EventInterface Instance()
		{
            return objProvider;
		}

		#endregion
        
        public abstract NBrightInfo ValidateCartBefore(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartAfter(NBrightInfo cartInfo);

        public abstract NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo);
        public abstract NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo);


	}

}


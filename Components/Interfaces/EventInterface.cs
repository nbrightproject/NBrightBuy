
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

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        private static Dictionary<String, EventInterface> _providerList;
        // constructor
        static EventInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

            string providerName = null;

            _providerList = new Dictionary<string, EventInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var l = pluginData.GetEventsProviders();

            foreach (var p in l)
            {
                var prov = p.Value;
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                var objProvider = (EventInterface)handle.Unwrap();
                var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                var lp = 1;
                while (_providerList.ContainsKey(ctrlkey))
                {
                    ctrlkey = ctrlkey + lp.ToString("");
                    lp += 1;
                }
                objProvider.Key = ctrlkey;
                _providerList.Add(ctrlkey, objProvider);
            }


        }

		// return the provider
        public static new EventInterface Instance(String ctrlkey)
		{
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            if (_providerList.Count > 0) return _providerList.Values.First();
            return null;
		}

		#endregion

        public abstract String Key { get; set; }

        public abstract NBrightInfo ValidateCartBefore(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartAfter(NBrightInfo cartInfo);

        public abstract NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo);
        public abstract NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo);


	}

}


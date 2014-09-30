
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

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
		    var l = pluginData.GetPluginList();

            foreach (var p in l)
            {
                //UI Only;Shipping;Tax;Promotions;Scheduler;Events;Other
                //01;02;03;04;05;06;07
                if (p.GetXmlProperty("genxml/dropdownlist/providertype") == "02" && p.GetXmlProperty("genxml/checkbox/active") == "True")
                {
                    ObjectHandle handle = null;
                    handle = Activator.CreateInstance(p.GetXmlProperty("genxml/textbox/assembly"),
                        p.GetXmlProperty("genxml/textbox/namespaceclass"));
                    var objProvider = (ShippingInterface) handle.Unwrap();
                    var key = p.GetXmlProperty("genxml/textbox/assembly");
                    var lp = 1;
                    while (_providerList.ContainsKey(key))
                    {
                        key = p.GetXmlProperty("genxml/textbox/assembly") + lp.ToString("D");
                        lp += 1;
                    }
                    _providerList.Add(key, objProvider);
                }
            }

		}

		// return the provider
        public static new ShippingInterface Instance(String key)
		{
            return _providerList[key];
		}

		#endregion

        public abstract NBrightInfo CalculateShipping(NBrightInfo cartInfo);

        public abstract String Name();

        public abstract String Template(NBrightInfo cartInfo);

    }

}


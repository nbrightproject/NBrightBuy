using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using NBrightDNN.controls;
using NEvoWeb.Modules.NB_Store;
using NBrightCore.common;
using System.Xml;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{
	public class NBrightBuyBase : BasePage
	{
		//public NBrightInfo NBSettings;
        public NBrightBuyController ModCtrl;
		public bool DebugMode = false;
		public string ModuleKey = "";
		public string ModuleAppType = "1";
		public string UploadFolder = "";
		public string SelUserId = "";
        public string ThemeFolder = "";
	    public ModSettings ModSettings;
        //public Dictionary<string, string> SettingsDic;

		protected override void OnInit(EventArgs e)
		{

            var obj = new NBrightBuyController();
            base.ObjCtrl = obj;

            // build StoreSettings and place in httpcontext
		    if (HttpContext.Current.Items["StoreSettings"] == null)
		    {
                HttpContext.Current.Items.Add("StoreSettings", new StoreSettings());		        
		    }
            DebugMode = StoreSettings.Current.DebugMode;
                        
		    base.OnInit(e);

			ModCtrl = (NBrightBuyController)base.ObjCtrl;

            #region "Get all Settings for module"
            //get Model Level Settings
            ModSettings = new ModSettings(ModuleId, Settings);
            ModuleKey = ModSettings.Get("modulekey");

            #endregion

            //add template provider to NBright Templating
            NBrightCore.providers.GenXProviderManager.AddProvider("NBrightBuy,Nevoweb.DNN.NBrightBuy.render.GenXmlTemplateExt");
			var pInfo = ModCtrl.GetByGuidKey(PortalId, -1, "PROVIDERS", "NBrightTempalteProviders");
			if (pInfo != null) NBrightCore.providers.GenXProviderManager.AddProvider(pInfo.XMLDoc);
        }


	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.controls;
using NEvoWeb.Modules.NB_Store;
using NBrightCore.common;
using System.Xml;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{
    public class NBrightBuyBase : DotNetNuke.Entities.Modules.PortalModuleBase
	{
        protected NBrightCore.controls.PagingCtrl CtrlPaging;
        public NBrightBuyController ModCtrl;
		public bool DebugMode = false;
		public string ModuleKey = "";
		public string ModuleAppType = "1";
		public string UploadFolder = "";
		public string SelUserId = "";
        public string ThemeFolder = "";
	    public ModSettings ModSettings;
        public Boolean DisablePaging { get; set; } // disable the paging control
        public bool FileHasBeenUploaded = false; // flag to check if file has been uploaded on any form.

        public DotNetNuke.Framework.CDefault BasePage
        {
            get { return (DotNetNuke.Framework.CDefault) this.Page; }
        }

		protected override void OnInit(EventArgs e)
		{

            ModCtrl = new NBrightBuyController();
            DebugMode = StoreSettings.Current.DebugMode;

            NBrightBuyUtils.NotfiyMessage(Context.Request,this);
            
		    base.OnInit(e);

            // Attach events
            GenXmlFunctions.FileHasBeenUploaded += new UploadFileCompleted(OnFileUploaded);

            #region "Get all Settings for module"
            //get Model Level Settings
            ModSettings = new ModSettings(ModuleId, Settings);
            ModuleKey = ModSettings.Get("modulekey");

            #endregion

            if (!DisablePaging)
            {
                CtrlPaging = new NBrightCore.controls.PagingCtrl();
                this.Controls.Add(CtrlPaging);
                CtrlPaging.PageChanged += new RepeaterCommandEventHandler(PagingClick);
            }

            //add template provider to NBright Templating
            NBrightCore.providers.GenXProviderManager.AddProvider("NBrightBuy,Nevoweb.DNN.NBrightBuy.render.GenXmlTemplateExt");
			var pInfo = ModCtrl.GetByGuidKey(PortalId, -1, "PROVIDERS", "NBrightTempalteProviders");
			if (pInfo != null) NBrightCore.providers.GenXProviderManager.AddProvider(pInfo.XMLDoc);
        }

        #region "Display Methods"


        public void DoDetail(Repeater rp1, NBrightInfo obj)
        {
            var l = new List<object> { obj };
            rp1.DataSource = l;
            rp1.DataBind();
        }

        public void DoDetail(Repeater rp1)
        {
            var obj = new NBrightInfo(true);
            var l = new List<object> { obj };
            rp1.DataSource = l;
            rp1.DataBind();
        }

        #endregion

        #region "Events"


        protected virtual void PagingClick(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            EventBeforePageChange(source, e);
        }

        public virtual void EventBeforePageChange(object source, RepeaterCommandEventArgs e)
        {

        }

        public void OnFileUploaded()
        {
            FileHasBeenUploaded = true;
        }

        #endregion


    }
}

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

        public Boolean EnablePaging;

        public DotNetNuke.Framework.CDefault BasePage
        {
            get { return (DotNetNuke.Framework.CDefault) this.Page; }
        }

		protected override void OnInit(EventArgs e)
		{

            ModCtrl = new NBrightBuyController();
            DebugMode = StoreSettings.Current.DebugMode;

		    base.OnInit(e);

            #region "Get all Settings for module"
            //get Model Level Settings
            ModSettings = new ModSettings(ModuleId, Settings);
            ModuleKey = ModSettings.Get("modulekey");

            #endregion

            if (EnablePaging)
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // load the notificationmessage if with have a placeholder control to display it.
            var ctrlMsg = this.FindControl("notifymsg");
            if (ctrlMsg != null)
            {
                var msg = NBrightBuyUtils.GetNotfiyMessage(ModuleId);
                var l = new Literal {Text = msg};
                ctrlMsg.Controls.Add(l);
            }
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

        #endregion


    }
}

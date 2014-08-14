using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditNBrightIndex class is used to manage content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Products : NBrightBuyAdminBase
    {

        private int _eid = 0;

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            if (Utils.IsNumeric(Utils.RequestParam(Context, "eid"))) _eid = Convert.ToInt32(Utils.RequestParam(Context, "eid"));

            base.OnInit(e);

            var t2 = "productadminlist.html";
            if (_eid > 0) t2 = "productadmin.html";

            // Get Display Body
            var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {
            if (UserId > 0) // only logged in users can see data on this module.
            {
                var prodData = ProductUtils.GetProductData(_eid, StoreSettings.Current.EditLanguage);
                base.DoDetail(rpData,prodData.Info);
            }
        }

        #endregion

        #region "Event handlers"

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "save":
                    Update();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "edit":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "return":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;

            }

        }


        #endregion

        private void Update()
        {
            if (_eid > 0)
            {
                var prodData = ProductUtils.GetProductData(_eid, StoreSettings.Current.EditLanguage);
                var strXml = GenXmlFunctions.GetGenXml(rpData);
                prodData.Update(strXml);
                prodData.Save();
            }
        }



    }

}
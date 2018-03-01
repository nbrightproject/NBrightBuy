using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using ManualPaymentProvider;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class AjaxProvider : AjaxInterface
    {
        public override string Ajaxkey { get; set; }

        public override string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var strOut = "manualpayment Ajax Error";
            switch (paramCmd)
            {
                case "manualpaymentajax_savesettings":
                    strOut = ProviderUtils.SaveData(context);
                    break;
            }

            return strOut;

        }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

    }
}

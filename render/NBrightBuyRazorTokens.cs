using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Components;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace NBrightBuy.render
{
    public class NBrightBuyRazorTokens<T> : RazorEngineTokens<T>
    {

        #region "NBS Action tokens"

        public IEncodedString AddToBasketButton(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + "  type='text' />";

            return new RawString(strOut);
        }

        #endregion

        #region "NBS display tokens"

        public IEncodedString ProductName(NBrightInfo info)
        {
            return new RawString(info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"));
        }

        public IEncodedString ProductImage(NBrightInfo info, String width = "150", String height = "0", String idx = "1", String attributes = "")
        {

            var imagesrc = info.GetXmlProperty("genxml/imgs/genxml[" + idx + "]/hidden/imageurl");
            var url = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?src=" + imagesrc + "&w=" + width + "&h=" + height +   attributes;
            return new RawString(url);
        }

        #endregion

    }


}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightCore;
using NBrightCore.TemplateEngine;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using DotNetNuke.Entities.Users;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.render
{
    public class GenXmlTemplateExt : GenXProvider
    {

        private string _rootname = "genxml";
        private string _databindColumn = "XMLData";
        private Dictionary<string, string> _settings = null;

        #region "Override methods"

        // This section overrides the interface methods for the GenX provider.
        // It allows providers to create controls/Literals in the NBright template system.

        public override bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null)
        {
            //remove namespace of token.
            // If the NBrigthCore template system is being used across mutliple modules in the portal (that use a provider interface for tokens),
            // then a namespace should be added to the front of the type attribute, this stops clashes in the tokening system. 
            if (ctrltype.StartsWith("nbb:")) ctrltype = ctrltype.Substring(4);

            _rootname = rootname;
            _databindColumn = databindColum;
            _settings = settings;
            switch (ctrltype)
            {
                case "addqty":
                    CreateQtyField(container, xmlNod);
                    return true;
                case "relatedproducts":
                    CreateRelatedlist(container, xmlNod);
                    return true;
                case "productdoclink":
                    CreateProductDocLink(container, xmlNod);
                    return true;
                case "productdocdesc":
                    CreateProductDocDesc(container, xmlNod);
                    return true;
                case "productimgdesc":
                    CreateProductImageDesc(container, xmlNod);
                    return true;
                case "productdocfilename":
                    CreateProductDocFileName(container, xmlNod);
                    return true;
                case "productoptionname":
                    CreateProductOptionName(container, xmlNod);
                    return true;
                case "productoption":
                    Createproductoptions(container, xmlNod);
                    return true;
                case "modelslist":
                    Createmodelslist(container, xmlNod);
                    return true;
                case "modelsradio":
                    Createmodelsradio(container, xmlNod);
                    return true;
                case "modelsdropdown":
                    Createmodelsdropdown(container, xmlNod);
                    return true;
                case "modeldefault":
                    Createmodeldefault(container, xmlNod);
                    return true;
                case "productname":
                    CreateProductName(container, xmlNod);
                    return true;
                case "manufacturer":
                    CreateManufacturer(container, xmlNod);
                    return true;
                case "summary":
                    CreateSummary(container, xmlNod);
                    return true;
                case "seoname":
                    CreateSEOname(container, xmlNod);
                    return true;
                case "seopagetitle":
                    CreateSEOpagetitle(container, xmlNod);
                    return true;
                case "tagwords":
                    CreateTagwords(container, xmlNod);
                    return true;
                case "description":
                    CreateDescription(container, xmlNod);
                    return true;
                case "currencyisocode":
                    CreateCurrencyIsoCode(container, xmlNod);
                    return true;
                case "price":
                    CreateFromPrice(container, xmlNod);
                    return true;
                case "saleprice":
                    CreateSalePrice(container, xmlNod);
                    return true;
                case "dealerprice":
                    CreateDealerPrice(container, xmlNod);
                    return true;
                case "bestprice":
                    CreateBestPrice(container, xmlNod);
                    return true;
                case "quantity":
                    CreateQuantity(container, xmlNod);
                    return true;
                case "thumbnail":
                    CreateThumbNailer(container, xmlNod);
                    return true;
                case "editlink":
                    CreateEditLink(container, xmlNod);
                    return true;
                case "entrylink":
                    CreateEntryLink(container, xmlNod);
                    return true;
                case "entryurl":
                    CreateEntryUrl(container, xmlNod);
                    return true;                    
                case "returnlink":
                    CreateReturnLink(container, xmlNod);
                    return true;
                case "currenturl":
                    CreateCurrentUrl(container, xmlNod);
                    return true;
                case "hrefpagelink":
                    Createhrefpagelink(container, xmlNod);
                    return true;                    
                case "catdropdown":
                    CreateCatDropDownList(container, xmlNod);
                    return true;
                case "catcheckboxlist":
                    CreateCatCheckBoxList(container, xmlNod);
                    return true;
                case "catbreadcrumb":
                    CreateCatBreadCrumb(container, xmlNod);
                    return true;
                case "catshortcrumb":
                    CreateCatBreadCrumb(container, xmlNod);
                    return true;
                case "catdefaultname":
                    CreateCatDefaultName(container, xmlNod);
                    return true;
                case "catvalueof":
                    CreateCatValueOf(container, xmlNod);
                    return true;
                case "catbreakof":
                    CreateCatBreakOf(container, xmlNod);
                    return true;
                case "cathtmlof":
                    CreateCatHtmlOf(container, xmlNod);
                    return true;
                case "testof":
                    CreateTestOf(container, xmlNod);
                    return true;
                case "if":
                    CreateTestOf(container, xmlNod);
                    return true;
                default:
                    return false;

            }

        }

        public override string GetField(Control ctrl)
        {
            return "";
        }

        public override void SetField(Control ctrl, string newValue)
        {
        }

        public override string GetGenXml(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
            return "";
        }

        public override string GetGenXmlTextBox(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
            return "";
        }

        public override object PopulateGenObject(List<Control> genCtrls, object obj)
        {
            return null;
        }

        #endregion

        #region "create nbb:testof"

        private void CreateTestOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal { Text = xmlNod.OuterXml };
            lc.DataBinding += TestOfDataBinding;
            container.Controls.Add(lc);
        }

        private void TestOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = NBrightGlobal.IsVisible;
                var xmlDoc = new XmlDataDocument();
                string display = "{ON}";
                string displayElse = "{OFF}";
                string dataValue = "";

                xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                var xmlNod = xmlDoc.SelectSingleNode("root/tag");


                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["display"] != null)))
                {
                    display = xmlNod.Attributes["display"].InnerXml;
                }

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["displayelse"] != null)))
                {
                    displayElse = xmlNod.Attributes["displayelse"].InnerXml;
                }
                else
                {
                    if (display == "{ON}") displayElse = "{OFF}";
                    if (display == "{OFF}") displayElse = "{ON}";
                }

                //get test value, set all tests to else
                string output = displayElse;

                if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && xmlNod.Attributes["function"] != null))
                {
                    XmlNode nod;
                    var testValue = "";
                    if ((xmlNod.Attributes["testvalue"] != null)) testValue = xmlNod.Attributes["testvalue"].Value;

                    // check for setting key
                    var settingkey = "";
                    if ((xmlNod.Attributes["key"] != null)) settingkey = xmlNod.Attributes["key"].Value;

                    var role = "";
                    if ((xmlNod.Attributes["role"] != null)) role = xmlNod.Attributes["role"].Value;

                    var index = "";
                    if ((xmlNod.Attributes["index"] != null)) role = xmlNod.Attributes["index"].Value;


                    // do normal xpath test
                    if (xmlNod.Attributes["xpath"] != null)
                    {
                        nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), xmlNod.Attributes["xpath"].InnerXml);
                        if (nod != null)
                        {
                            dataValue = nod.InnerText;
                        }
                    }

                    // do special tests for named fucntions
                    if (xmlNod.Attributes["function"] != null)
                    {
                        switch (xmlNod.Attributes["function"].Value.ToLower())
                        {
                            case "price":
                                dataValue = GetFromPrice((NBrightInfo) container.DataItem);
                                break;
                            case "dealerprice":
                                dataValue = GetDealerPrice((NBrightInfo) container.DataItem);
                                break;
                            case "saleprice":
                                dataValue = GetSalePrice((NBrightInfo) container.DataItem);
                                break;
                            case "imgexists":
                                dataValue = testValue;
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/imgs/genxml[" + dataValue + "]/hidden/imageid");
                                if (nod == null || nod.InnerText == "") dataValue = "FALSE";
                                break;
                            case "modelexists":
                                dataValue = testValue;
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/models/genxml[" + dataValue + "]/hidden/modelid");
                                if (nod == null || nod.InnerText == "") dataValue = "FALSE";
                                break;
                            case "optionexists":
                                dataValue = testValue;
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/options/genxml[" + dataValue + "]/hidden/optionid");
                                if (nod == null || nod.InnerText == "") dataValue = "FALSE";
                                break;
                            case "isinstock":
                                dataValue = "FALSE";
                                if (IsInStock((NBrightInfo) container.DataItem, testValue))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "isinstockcart":
                                dataValue = "FALSE";
                                if (IsInStock((NBrightInfo) container.DataItem, testValue, true))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "inwishlist":
                                var productid = DataBinder.Eval(container.DataItem, "ItemId").ToString();
                                dataValue = "FALSE";
                                if (Utils.IsNumeric(productid))
                                {
                                    if (WishList.IsInWishlist(PortalSettings.Current.PortalId, NBrightBuyV2Utils.GetLegacyProductId(Convert.ToInt32(productid)).ToString("")))
                                    {
                                        dataValue = "TRUE";
                                        testValue = "TRUE";
                                    }
                                }
                                break;
                            case "isonsale":
                                dataValue = "FALSE";
                                var saleprice = GetSalePrice((NBrightInfo) container.DataItem);
                                if ((Utils.IsNumeric(saleprice)) && (Convert.ToDouble(saleprice) > 0))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "hasrelateditems":
                                dataValue = "FALSE";
                                if (NBrightBuyV2Utils.HasRelatedProducts((NBrightInfo)container.DataItem))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "hasdocuments":
                                dataValue = "FALSE";
                                if (NBrightBuyV2Utils.HasDocuments((NBrightInfo)container.DataItem))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "haspurchasedocuments":
                                dataValue = "FALSE";
                                if (NBrightBuyV2Utils.HasPurchaseDocuments((NBrightInfo)container.DataItem))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "isdocpurchased":
                                dataValue = "FALSE";
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/docs/genxml[" + index + "]/hidden/docid");
                                if (nod != null && Utils.IsNumeric(nod.InnerText))
                                {
                                    var uInfo = UserController.GetCurrentUserInfo();
                                    if (NBrightBuyV2Utils.DocHasBeenPurchasedByDocId(uInfo.UserID, Convert.ToInt32(nod.InnerText)))
                                    {
                                        dataValue = "TRUE";
                                        testValue = "TRUE";
                                    }
                                }
                                break;
                            case "hasmodelsoroptions":
                                dataValue = "FALSE";
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/models/genxml[2]/hidden/modelid");
                                if (nod != null && nod.InnerText != "")
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                if (dataValue=="FALSE")
                                {
                                    nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/options/genxml[1]/hidden/optionid");
                                    if (nod != null && nod.InnerText != "")
                                    {
                                        dataValue = "TRUE";
                                        testValue = "TRUE";
                                    }                                                                        
                                }
                                break;
                            case "isproductincart":
                                dataValue = "FALSE";
                                if (NBrightBuyV2Utils.IsInCart((NBrightInfo)container.DataItem))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "settings":
                                dataValue = "FALSE";
                                if (_settings[settingkey] != null && _settings[settingkey] == testValue)
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;
                            case "isinrole":
                                dataValue = "FALSE";
                                if (CmsProviderManager.Default.IsInRole(role))
                                {
                                    dataValue = "TRUE";
                                    testValue = "TRUE";
                                }
                                break;                                
                            default:
                                dataValue = "";
                                break;
                        }
                    }

                    if (testValue == dataValue)
                        output = display;
                    else
                        output = displayElse;

                }


                // If the Visible flag is OFF then keep it off, even if the child test is true
                // This allows nested tests to function correctly, by using the parent result.
                if (!NBrightGlobal.IsVisible)
                {
                    if (output == "{ON}" | output == "{OFF}") NBrightGlobal.IsVisibleList.Add(false); // only add level on {} testof
                }
                else
                {
                    if (output == "{ON}") NBrightGlobal.IsVisibleList.Add(true);
                    if (output == "{OFF}") NBrightGlobal.IsVisibleList.Add(false);
                }

                if (NBrightGlobal.IsVisible && output != "{ON}") lc.Text = output;
                if (output == "{ON}" | output == "{OFF}") lc.Text = ""; // don;t display the test tag
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        #endregion

        #region "create Shortcuts"

        private void CreateProductImageDesc(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBinding;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/imgs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtimagedesc";
                    container.Controls.Add(l);
                }
            }
        }

        private void CreateProductDocDesc(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBinding;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/docs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtdocdesc";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductDocFileName(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBinding;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/docs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtfilename";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductOptionName(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBinding;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/options/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtoptiondesc";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductName(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtproductname";
            container.Controls.Add(l);
        }
        private void CreateManufacturer(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtmanufacturer";
            container.Controls.Add(l);
        }
        private void CreateSummary(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtsummary";
            container.Controls.Add(l);
        }
        private void CreateSEOname(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtseoname";
            container.Controls.Add(l);
        }
        private void CreateTagwords(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txttagwords";
            container.Controls.Add(l);
        }
        private void CreateSEOpagetitle(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtseopagetitle";
            container.Controls.Add(l);
        }
        private void CreateDescription(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBindingHtmlDecode;
            l.Text = "genxml/lang/genxml/edt/description";
            container.Controls.Add(l);
        }
        private void CreateQuantity(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            //l.Text = "genxml/models/genxml/textbox/txtqtyremaining";
            //Get quantity with the lowest unitcost value with xpath
            l.Text = "(genxml/models/genxml/textbox/txtqtyremaining[not(number((.)[1]) > number((../../../genxml/textbox/txtqtyremaining)[1]))][1])[1]";
            container.Controls.Add(l);
        }
        private void ShortcutDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    l.Text = System.Web.HttpUtility.UrlDecode(XmlConvert.DecodeName(nod.InnerText)); // the urldecode is included for filename on documents, which was forced to encoded in v2 so it work correctly. 
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }
        private void ShortcutDataBindingHtmlDecode(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    l.Text = System.Web.HttpUtility.HtmlDecode(XmlConvert.DecodeName(nod.InnerText));
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }
        private void ShortcutDataBindingCurrency(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    Double v = 0;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(nod.InnerText)))
                    {
                        v  = Convert.ToDouble(XmlConvert.DecodeName(nod.InnerText));
                    }
                    l.Text = NBrightBuyV2Utils.FormatToStoreCurrency(v); 
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "Create Thumbnailer"

        private void CreateThumbNailer(Control container, XmlNode xmlNod)
        {
            var l = new Literal();

            var thumbparams = "";
            var imagenum = "1";
            if (xmlNod.Attributes != null)
            {
                foreach (XmlAttribute a in xmlNod.Attributes)
                {
                    if (a.Name.ToLower() != "type")
                    {
                        if (a.Name.ToLower() != "image")
                            thumbparams += "&amp;" + a.Name + "=" + a.Value; // don;t use the type in the params
                        else
                            imagenum = a.Value;
                    }
                }
            }

            l.Text = imagenum + ":" + thumbparams; // pass the attributes to be added

            l.DataBinding += ThumbNailerDataBinding;
            container.Controls.Add(l);
        }

        private void ThumbNailerDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                var imagesrc = "0";
                var imageparams = l.Text.Split(':');

                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/imgs/genxml[" + imageparams[0] + "]/hidden/imageurl");
                if ((nod != null)) imagesrc = nod.InnerText;
                var url = "/DesktopModules/NBright/NBrightBuy/NBrightThumb.ashx?src=" + imagesrc + imageparams[1];
                l.Text = url;
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "create EntryLink/URL control"

        private void CreateEntryLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["tabid"] != null) lk.Attributes.Add("tabid", xmlNod.Attributes["tabid"].InnerText);
                if (xmlNod.Attributes["modkey"] != null) lk.Attributes.Add("modkey", xmlNod.Attributes["modkey"].InnerText);
                if (xmlNod.Attributes["xpath"] != null) lk.Attributes.Add("xpath", xmlNod.Attributes["xpath"].InnerText);
            }
            lk.DataBinding += EntryLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void EntryLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
			try
			{
				//set a default url

                lk.Visible = NBrightGlobal.IsVisible;

				var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));

			    var urlname = "Default";
                if (lk.Attributes["xpath"] != null)
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), lk.Attributes["xpath"]);
                    if ((nod != null)) urlname = nod.InnerText;
                }
                var t = "";
				if (lk.Attributes["tabid"] != null && Utils.IsNumeric(lk.Attributes["tabid"])) t = lk.Attributes["tabid"];
                var c = "";
                if (lk.Attributes["catid"] != null && Utils.IsNumeric(lk.Attributes["catid"])) c = lk.Attributes["catid"];
			    var moduleref = "";
                if ((lk.Attributes["modkey"] != null)) moduleref = lk.Attributes["modkey"];

                var url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, entryid, moduleref, urlname, t);
                lk.NavigateUrl = url;

			}
			catch (Exception ex)
			{
				lk.Text = ex.ToString();
			}
        }

        private void CreateEntryUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            if (xmlNod.Attributes != null)
            {
                // we dont; have any attributes for a literal, so pass data as string (tabid,modulekey,entryname)
                var t = PortalSettings.Current.ActiveTab.TabID.ToString("");
                var mk = "";
                var xp = "";
                if (xmlNod.Attributes["tabid"] != null) t = xmlNod.Attributes["tabid"].InnerText;
                if (xmlNod.Attributes["modkey"] != null) mk = xmlNod.Attributes["modkey"].InnerText;
                if (xmlNod.Attributes["xpath"] != null) xp = xmlNod.Attributes["xpath"].InnerText;

                l.Text = t + '*' + mk + '*' + xp.Replace('*','-');
            }
            l.DataBinding += EntryUrlDataBinding;
            container.Controls.Add(l);
        }

        private void EntryUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                //set a default url

                l.Visible = NBrightGlobal.IsVisible;

                var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));
                var dataIn = l.Text.Split('*'); 
                var urlname = "Default";
                var t = "";
                var moduleref = "";

                if (dataIn.Length == 3)
                {
                    if (Utils.IsNumeric(dataIn[0])) t = dataIn[0];
                    if (Utils.IsNumeric(dataIn[1])) moduleref = dataIn[1];
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), dataIn[2]);
                    if ((nod != null)) urlname = nod.InnerText;
                }

                var url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, entryid, moduleref, urlname, t);
                l.Text = url;

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "create ReturnLink control"

        private void CreateReturnLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["tabid"] != null))
            {
                lk.Attributes.Add("tabid", xmlNod.Attributes["tabid"].InnerText);
            }

            lk.DataBinding += ReturnLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void ReturnLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
            try
            {
                lk.Visible = NBrightGlobal.IsVisible;

                var t = "";
                if (lk.Attributes["tabid"] != null && Utils.IsNumeric(lk.Attributes["tabid"]))
                {
                    t = lk.Attributes["tabid"];
                }

                var url = NBrightBuyUtils.GetReturnUrl(t);
                lk.NavigateUrl = url;

            }
            catch (Exception ex)
            {
                lk.Text = ex.ToString();
            }
        }

        #endregion

        #region "create HrefPageLink control"
        private void Createhrefpagelink(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.Text = "-1";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["moduleid"] != null))
            {
                l.Text = xmlNod.Attributes["moduleid"].InnerXml;
            }
            l.DataBinding += hrefpagelinkbind;
            container.Controls.Add(l);
        }

        private void hrefpagelinkbind(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                var catparam = "";
                var pagename = PortalSettings.Current.ActiveTab.TabName + ".aspx";
                var catid = Utils.RequestParam(HttpContext.Current, "catid");
                if (Utils.IsNumeric(catid))
                {
                    pagename = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(catid)) + ".aspx";
                    catparam = "&catid=" + catid;
                }
                var url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(PortalSettings.Current.ActiveTab, "~/Default.aspx?tabid=" + PortalSettings.Current.ActiveTab.TabID.ToString("") + catparam + "&page=" + Convert.ToString(DataBinder.Eval(container.DataItem, "PageNumber")) + "&pagemid=" + l.Text, pagename);
                l.Text = "<a href=\"" + url + "\">" + Convert.ToString(DataBinder.Eval(container.DataItem, "Text")) + "</a>";
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "create CurrentUrl control"
        private void CreateCurrentUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();

            l.DataBinding += CurrentUrlDataBinding;
            container.Controls.Add(l);
        }

        private void CurrentUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = NBrightGlobal.IsVisible;
                //set a default url
                var url = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.FullUrl;
                l.Text = url;

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region  "category dropdown and checkbox list"

        private void CreateCatCheckBoxList(Control container, XmlNode xmlNod)
        {
            try
            {

                var cbl = new CheckBoxList();
                cbl = (CheckBoxList) GenXmlFunctions.AssignByReflection(cbl, xmlNod);
                var selected = false;
                if (xmlNod.Attributes != null && (xmlNod.Attributes["selected"] != null))
                {
                    if (xmlNod.Attributes["selected"].InnerText.ToLower() == "true") selected = true;
                }

                var tList = GetCatList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");
                    li.Selected = selected;
                    cbl.Items.Add(li);
                }

                cbl.DataBinding += CbListDataBinding;
                container.Controls.Add(cbl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }

        }

        private void CbListDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBoxList)sender;
            var container = (IDataItemContainer)chk.NamingContainer;
            try
            {
                chk.Visible = NBrightGlobal.IsVisible;
                var xmlNod = GenXmlFunctions.GetGenXmLnode(chk.ID, "checkboxlist", (string)DataBinder.Eval(container.DataItem, _databindColumn));
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null)
                        {
                            if (xmlNoda.Attributes.GetNamedItem("data") != null)
                            {
                                var datavalue = xmlNoda.Attributes["data"].Value;
                                //use the data attribute if there
                                if ((chk.Items.FindByValue(datavalue).Value != null))
                                {
                                    chk.Items.FindByValue(datavalue).Selected = Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }

        }

        private void CreateCatDropDownList(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new DropDownList();
                ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

                if (xmlNod.Attributes != null && (xmlNod.Attributes["allowblank"] != null))
                {
                        var li = new ListItem();
                        li.Text = "";
                        li.Value = "";
                        ddl.Items.Add(li);
                }

                var tList = GetCatList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");
                    
                    ddl.Items.Add(li);
                }

                ddl.DataBinding += DdListDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void DdListDataBinding(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;

            try
            {
                ddl.Visible = NBrightGlobal.IsVisible;

                var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));

                if ((ddl.Items.FindByValue(strValue) != null))
                {
                    ddl.SelectedValue = strValue;
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if ((nod.Attributes != null) && (nod.Attributes["selectedtext"] != null))
                    {
                        strValue = XmlConvert.DecodeName(nod.Attributes["selectedtext"].Value);
                        if ((ddl.Items.FindByValue(strValue) != null))
                        {
                            ddl.SelectedValue = strValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private Dictionary<int, string> BuildCatList(int displaylevels = 20, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, String catreflist = "", String prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "")
        {
            var rtnDic = new Dictionary<int, string>();

            var strCacheKey = "NBrightBuy_BuildCatList" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + showHidden.ToString(CultureInfo.InvariantCulture) + "*" + showArchived.ToString(CultureInfo.InvariantCulture) + "*" + parentid + "*" + catreflist + "*" + prefix + "*" + Utils.GetCurrentCulture() + "*" + showEmpty + "*" + displayCount + "*" + groupref;

            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);

            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var d = new Dictionary<int, string>();
                var rtnList = new List<GroupCategoryData>();
                rtnList = grpCatCtrl.GetTreeCategoryList(rtnList, 0, parentid, groupref);
                var strCount = "";
                foreach (var grpcat in rtnList)
                {
                    if (displayCount) strCount = " (" + grpcat.entrycount.ToString("") + ")";

                    if (grpcat.depth < displaylevels)
                    {
                        if (showEmpty || grpcat.entrycount > 0)
                        {

                            if (grpcat.archived == false || showArchived)
                            {
                                if (grpcat.ishidden == false || showHidden)
                                {
                                    var addprefix = new String(' ', grpcat.depth).Replace(" ", prefix);
                                    if (catreflist == "")
                                        rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                    else
                                    {
                                        if (grpcat.categoryref != "" && (catreflist + ",").Contains(grpcat.categoryref + ","))
                                        {
                                            rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnDic);

            }
            else
            {
                rtnDic = (Dictionary<int, string>)objCache;
            }
            return rtnDic;
        }

        private Dictionary<int, string> GetCatList(XmlNode xmlNod)
        {
            var displaylevels = 20;
            var parentref = "";
            var prefix = "..";
            var showhidden = "False";
            var showarchived = "False";
            var showempty = "True";
            var showHidden = false;
            var showArchived = false;
            var catreflist = "";
            var parentid = 0;
            var displaycount = "False";
            var displayCount = false;
            var showEmpty = true;
            var groupref = "";

            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["displaylevels"] != null)
                {
                    if (Utils.IsNumeric(xmlNod.Attributes["displaylevels"].Value)) displaylevels = Convert.ToInt32(xmlNod.Attributes["displaylevels"].Value);
                }

                if (xmlNod.Attributes["parentref"] != null) parentref = xmlNod.Attributes["parentref"].Value;
                if (xmlNod.Attributes["showhidden"] != null) showhidden = xmlNod.Attributes["showhidden"].Value;
                if (xmlNod.Attributes["showarchived"] != null) showarchived = xmlNod.Attributes["showarchived"].Value;
                if (xmlNod.Attributes["showempty"] != null) showempty = xmlNod.Attributes["showempty"].Value;
                if (xmlNod.Attributes["displaycount"] != null) displaycount = xmlNod.Attributes["displaycount"].Value;
                if (xmlNod.Attributes["prefix"] != null) prefix = xmlNod.Attributes["prefix"].Value;
                if (xmlNod.Attributes["groupref"] != null) groupref = xmlNod.Attributes["groupref"].Value;    
                
                if (showhidden == "True") showHidden = true;
                if (showarchived == "True") showArchived = true;
                if (showempty == "False") showEmpty = false;
                if (displaycount == "True") displayCount = true;
                if (xmlNod.Attributes["catreflist"] != null) catreflist = xmlNod.Attributes["catreflist"].Value;
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                if (parentref != "")
                {
                    var p = grpCatCtrl.GetGrpCategoryByRef(parentref);
                    if (p != null) parentid = p.categoryid;                    
                }
            }


            return BuildCatList(displaylevels, showHidden, showArchived, parentid, catreflist, prefix, displayCount, showEmpty, groupref);

        }
        #endregion

        #region "catbreadcrumb"

        private void CreateCatBreadCrumb(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            lc.Text = xmlNod.OuterXml;
            lc.DataBinding += CatBreadCrumbDataBind;
            container.Controls.Add(lc);
        }

        private void CatBreadCrumbDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());

                lc.Visible = NBrightGlobal.IsVisible;
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                if (Utils.IsNumeric(id))
                {
                    var objCInfo = (NBrightDNN.NBrightInfo) container.DataItem;
                    var itemid = objCInfo.ItemID;
                    if (objCInfo.TypeCode == "DATA")
                    {
                        // Is EntryId, so Get default category for entry.
                        var obj = grpCatCtrl.GetCurrentCategoryData(PortalSettings.Current.PortalId, lc.Page.Request,
                            Convert.ToInt32(id));
                        if (obj != null) itemid = obj.categoryid;
                    }
                    var xmlDoc = new XmlDataDocument();
                    xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                    var xmlNod = xmlDoc.SelectSingleNode("root/tag");
                    var intLength = 400;
                    var intShortLength = -1;
                    var IsLink = false;

                    if (xmlNod.Attributes != null)
                    {
                        if (xmlNod.Attributes["length"] != null)
                        {
                            if (Utils.IsNumeric(xmlNod.Attributes["length"].InnerText))
                            {
                                intLength = Convert.ToInt32(xmlNod.Attributes["length"].InnerText);
                            }
                        }
                        if (xmlNod.Attributes["links"] != null) IsLink = true;
                        if (xmlNod.Attributes["short"] != null)
                        {
                            if (Utils.IsNumeric(xmlNod.Attributes["short"].InnerText))
                            {
                                intShortLength = Convert.ToInt32(xmlNod.Attributes["short"].InnerText);
                            }
                        }
                    }

                    var defcatid = itemid;
                    if (IsLink)
                    {
                        if (Utils.IsNumeric(moduleId))
                        {
                            var nbSettings = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId,Convert.ToInt32(moduleId));
                            var defTabId = nbSettings.GetXmlProperty("genxml/dropdownlist/cattabid");
                            lc.Text = grpCatCtrl.GetBreadCrumbWithLinks(defcatid, defTabId, intShortLength);
                        }
                    }
                    else
                    {
                        lc.Text = grpCatCtrl.GetBreadCrumb(defcatid, intShortLength);
                    }

                    if (lc.Text.Length > intLength)
                    {
                        lc.Text = lc.Text.Substring(0, (intLength - 3)) + "...";
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatDefaultName"

        private void CreateCatDefaultName(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && xmlNod.Attributes["default"] != null) lc.Text = xmlNod.Attributes["default"].InnerText;
            lc.DataBinding += CatDefaultNameDataBind;
            container.Controls.Add(lc);
        }

        private void CatDefaultNameDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            lc.Text = "";
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = NBrightGlobal.IsVisible;
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));

                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryData(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.categoryname;
                    }
                }

            }
            catch (Exception ex)
            {
                lc.Text = ex.ToString();
            }
        }

        #endregion

        #region "CreateCatValueOf"

        private void CreateCatValueOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatValueOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatValueOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = NBrightGlobal.IsVisible;
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.GetXmlProperty(lc.Text);
                    }
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatBreakOf"

        private void CreateCatBreakOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatBreakOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatBreakOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = NBrightGlobal.IsVisible;
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.GetXmlProperty(lc.Text);
                    }
                    lc.Text = System.Web.HttpUtility.HtmlEncode(lc.Text);
                    lc.Text = lc.Text.Replace(Environment.NewLine, "<br/>");
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatHtmlOf"

        private void CreateCatHtmlOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatHtmlOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatHtmlOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = NBrightGlobal.IsVisible;
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.GetXmlProperty(lc.Text);
                    }
                    lc.Text = System.Web.HttpUtility.HtmlDecode(lc.Text);
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "Product Options"

        private void Createproductoptions(Control container, XmlNode xmlNod)
        {
            // create all 3 control possible
            var ddl = new DropDownList();
            var chk = new CheckBox();
            var txt = new TextBox();
            // pass wrapper templates using ddl attributes.
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                ddl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
                chk.Attributes.Add("template", xmlNod.Attributes["template"].Value);
                txt.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    ddl.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                    ddl = (DropDownList) GenXmlFunctions.AssignByReflection(ddl, xmlNod);
                    ddl.DataBinding += ProductoptionsDataBind;
                    ddl.Visible = false;
                    ddl.Enabled = false;
                    ddl.ID = "optionddl" + xmlNod.Attributes["index"].Value;
                    container.Controls.Add(ddl);
                    chk.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                    chk = (CheckBox)GenXmlFunctions.AssignByReflection(chk, xmlNod);
                    chk.DataBinding += ProductoptionsDataBind;
                    chk.ID = "optionchk" + xmlNod.Attributes["index"].Value;
                    chk.Visible = false;
                    chk.Enabled = false;
                    container.Controls.Add(chk);
                    txt.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                    txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);
                    txt.DataBinding += ProductoptionsDataBind;
                    txt.ID = "optiontxt" + xmlNod.Attributes["index"].Value;
                    txt.Visible = false;
                    txt.Enabled = false;
                    container.Controls.Add(txt);
                }
            }
        }

        private void ProductoptionsDataBind(object sender, EventArgs e)
        {
            if (NBrightGlobal.IsVisible)
            {
                #region "Init"

                var ctl = (Control) sender;
                var container = (IDataItemContainer) ctl.NamingContainer;
                var objInfo = (NBrightInfo) container.DataItem;
                var useCtrlType = "";
                var index = "1";
                DropDownList ddl = null;
                CheckBox chk = null;
                TextBox txt = null;

                if (ctl is DropDownList)
                {
                    ddl = (DropDownList) ctl;
                    index = ddl.Attributes["index"];
                    ddl.Attributes.Remove("index");
                }
                if (ctl is CheckBox)
                {
                    chk = (CheckBox) ctl;
                    index = chk.Attributes["index"];
                    chk.Attributes.Remove("index");
                }
                if (ctl is TextBox)
                {
                    txt = (TextBox)ctl;
                    index = txt.Attributes["index"];
                    txt.Attributes.Remove("index");
                }

                var optionid = "";
                var optiondesc = "";
                XmlNodeList nodList = null;
                var nod = objInfo.XMLDoc.SelectSingleNode("genxml/options/genxml[" + index + "]/hidden/optionid");
                if (nod != null)
                {
                    optionid = nod.InnerText;
                    var nodDesc = objInfo.XMLDoc.SelectSingleNode("genxml/lang/genxml/options/genxml[" + index + "]/textbox/txtoptiondesc");
                    if (nodDesc != null) optiondesc = nodDesc.InnerText;

                     nodList = objInfo.XMLDoc.SelectNodes("genxml/optionvalues[@optionid='" + optionid + "']/*");
                     if (nodList != null)
                     {
                         switch (nodList.Count)
                         {
                             case 0:
                                 useCtrlType = "TextBox";
                                 break;
                             case 1:
                                 useCtrlType = "CheckBox";
                                 break;
                             default:
                                 useCtrlType = "DropDownList";
                                 break;
                         }
                     }
                }

                #endregion

                if (ddl != null && useCtrlType == "DropDownList")
                {
                    try
                    {
                        ddl.Visible = true;
                        ddl.Enabled = true;
                        if (nodList != null)
                        {
                            foreach (XmlNode nodOptVal in nodList)
                            {
                                var nodVal = nodOptVal.SelectSingleNode("hidden/optionvalueid");
                                if (nodVal != null)
                                {
                                    var optionvalueid = nodVal.InnerText;
                                    var li = new ListItem();
                                    var nodLang = objInfo.XMLDoc.SelectSingleNode("genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml[hidden/optionvalueid='" + optionvalueid + "']/textbox/txtoptionvaluedesc");
                                    if (nodLang != null)
                                    {
                                        li.Text = nodLang.InnerText;
                                        li.Value = optionvalueid;
                                        if (li.Text != "") ddl.Items.Add(li);
                                    }
                                }
                            }
                            if (nodList.Count > 0) ddl.SelectedIndex = 0;
                        }

                    }
                    catch (Exception)
                    {
                        ddl.Visible = false;
                    }
                }

                if (chk != null && useCtrlType == "CheckBox")
                {
                    try
                    {
                        chk.Visible = true;
                        chk.Enabled = true;
                        if (nodList != null)
                        {
                            foreach (XmlNode nodOptVal in nodList)
                            {
                                var nodVal = nodOptVal.SelectSingleNode("hidden/optionvalueid");
                                if (nodVal != null)
                                {
                                    var optionvalueid = nodVal.InnerText;
                                    var nodLang = objInfo.XMLDoc.SelectSingleNode("genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml[hidden/optionvalueid='" + optionvalueid + "']/textbox/txtoptionvaluedesc");
                                    if (nodLang != null)
                                    {
                                        chk.Text = nodLang.InnerText;
                                        chk.Attributes.Add("optionvalueid",optionvalueid);
                                        chk.Attributes.Add("optionid", optionid);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception)
                    {
                        chk.Visible = false;
                    }

                }

                if (txt != null && useCtrlType == "TextBox")
                {
                    txt.Visible = true;
                    txt.Enabled = true;
                    txt.Attributes.Add("optionid", optionid);
                    txt.Attributes.Add("optiondesc", optiondesc);
                }
            }
        }

        #endregion

        #region "Models"

        private void Createmodelslist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
            }
            lc.DataBinding += ModelslistDataBind;
            container.Controls.Add(lc);
        }

        private void ModelslistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = NBrightGlobal.IsVisible;
                if (lc.Visible)
                {

                    var moduleid = _settings["moduleid"];
                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleid) && (templName != ""))
                    {
                        var debugMode = _settings.ContainsKey("debug.mode") && _settings["debug.mode"].ToLower() == "1";
                        var buyCtrl = new NBrightBuyController();
                        var rpTempl = buyCtrl.GetTemplateData(Convert.ToInt32(moduleid), templName, Utils.GetCurrentCulture(), _settings, debugMode); 
                       
                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                        //build models list
                        var objL = BuildModelList((NBrightInfo)container.DataItem,true,true);
                        // render repeater
                        try
                        {
                            strOut = GenXmlFunctions.RenderRepeater(objL, rpTempl, "", "XMLData", "", _settings);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void Createmodelsradio(Control container, XmlNode xmlNod)
        {
            var rbl = new RadioButtonList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                rbl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            rbl = (RadioButtonList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += ModelsradioDataBind;
            rbl.ID = "rblModelsel";
            container.Controls.Add(rbl);
        }

        private void ModelsradioDataBind(object sender, EventArgs e)
        {
            var rbl = (RadioButtonList)sender;
            var container = (IDataItemContainer)rbl.NamingContainer;
            try
            {
                rbl.Visible = NBrightGlobal.IsVisible;
                if (rbl.Visible)
                {
                    var templ = "{name} {price}";
                    if (rbl.Attributes["template"] != null)
                    {
                        templ = rbl.Attributes["template"];
                        rbl.Attributes.Remove("template");
                    }

                    var objL = BuildModelList((NBrightInfo) container.DataItem, true, true);

                    var displayPrice = HasDifferentPrices((NBrightInfo)container.DataItem);

                    foreach (var obj in objL)
                    {
                        var li = new ListItem();
                        li.Text = GetItemDisplay(obj, templ, displayPrice);
                        li.Value = obj.GetXmlProperty("genxml/hidden/modelid");
                        if (li.Text != "") rbl.Items.Add(li);
                    }
                    if (objL.Count > 0) rbl.SelectedIndex = 0;
                }

            }
            catch (Exception)
            {
                rbl.Visible = false;
            }
        }

        private void Createmodelsdropdown(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                rbl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += ModelsdropdownDataBind;
            rbl.ID = "ddlModelsel";
            container.Controls.Add(rbl);
        }

        private void ModelsdropdownDataBind(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = NBrightGlobal.IsVisible;
                if (ddl.Visible)
                {
                    var templ = "{name} {price}";
                    if(ddl.Attributes["template"] != null)
                    {
                        templ = ddl.Attributes["template"];
                        ddl.Attributes.Remove("template");                        
                    }

                    var objL = BuildModelList((NBrightInfo)container.DataItem, true, true);

                    var displayPrice = HasDifferentPrices((NBrightInfo)container.DataItem);

                    foreach (var obj in objL)
                    {
                        var li = new ListItem();
                        li.Text = GetItemDisplay(obj, templ, displayPrice);
                        li.Value = obj.GetXmlProperty("genxml/hidden/modelid");
                        if (li.Text != "") ddl.Items.Add(li);
                    }
                    if (objL.Count > 0) ddl.SelectedIndex = 0;
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }

        private void Createmodeldefault(Control container, XmlNode xmlNod)
        {
            var hf = new HiddenField();
            hf.DataBinding += ModeldefaultDataBind;
            hf.ID = "modeldefault";
            container.Controls.Add(hf);
        }

        private void ModeldefaultDataBind(object sender, EventArgs e)
        {
            var hf = (HiddenField)sender;
            var container = (IDataItemContainer)hf.NamingContainer;
            try
            {
                hf.Visible = NBrightGlobal.IsVisible;
                var obj = (NBrightInfo)container.DataItem;
                if (obj != null) hf.Value = obj.GetXmlProperty("genxml/models/genxml[1]/hidden/modelid");
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private String GetItemDisplay(NBrightInfo obj,String templ,Boolean displayPrices)
        {
            var isDealer = CmsProviderManager.Default.IsInRole(_settings["dealer.role"]);
            var outText = templ;
            var strStockOn = obj.GetXmlProperty("genxml/textbox/txtqtyremaining");
            var strStock = obj.GetXmlProperty("genxml/hidden/calcstock");
            var stock = 0;
            if (Utils.IsNumeric(strStock)) stock = Convert.ToInt32(strStock);
            if (stock > 0 | strStockOn == "-1")
            {
                outText = outText.Replace("{ref}", obj.GetXmlProperty("genxml/textbox/txtmodelref"));
                outText = outText.Replace("{name}", obj.GetXmlProperty("genxml/lang/genxml/textbox/txtmodelname"));
                outText = outText.Replace("{stock}", strStock);

                if (displayPrices)
                {
                    var strprice = obj.GetXmlProperty("genxml/hidden/saleprice");
                    if (strprice == "-1") strprice = obj.GetXmlProperty("genxml/textbox/txtunitcost");
                    Double price = 0;
                    if (Utils.IsNumeric(strprice))
                    {
                        price = Convert.ToDouble(strprice);
                        strprice = SharedFunctions.FormatToStoreCurrency(PortalSettings.Current.PortalId, price);
                    }

                    var strdealerprice = obj.GetXmlProperty("genxml/textbox/txtdealercost");
                    if (isDealer)
                    {
                        if (Utils.IsNumeric(strdealerprice))
                        {
                            var dealerprice = Convert.ToDouble(strdealerprice);
                            strdealerprice = SharedFunctions.FormatToStoreCurrency(PortalSettings.Current.PortalId, dealerprice);
                            if (!outText.Contains("{dealerprice}") && (price > dealerprice)) strprice = strdealerprice;
                        }
                    }
                    else                       
                        strdealerprice = "";

                    outText = outText.Replace("{price}", "(" + strprice + ")");
                    outText = outText.Replace("{dealerprice}", strdealerprice);
                }
                else
                {
                    outText = outText.Replace("{price}", "");
                    outText = outText.Replace("{dealerprice}", "");
                }

                return outText;
            }
            return ""; // no stock so return empty string.
        }

        #endregion

        #region "Docs"

        private void CreateProductDocLink(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var cmd = new LinkButton();
                    cmd = (LinkButton)GenXmlFunctions.AssignByReflection(cmd, xmlNod);
                    cmd.Attributes.Add("index",xmlNod.Attributes["index"].Value);
                    cmd.DataBinding += ProductDocLinkDataBind;
                    container.Controls.Add(cmd);
                }
            }
        }

        private void ProductDocLinkDataBind(object sender, EventArgs e)
        {
            var cmd = (LinkButton)sender;
            var container = (IDataItemContainer)cmd.NamingContainer;
            try
            {
                cmd.Visible = NBrightGlobal.IsVisible;
                if (cmd.Visible)
                {
                    var index = cmd.Attributes["index"];
                    cmd.Attributes.Remove("index");

                    var objInfo = (NBrightInfo) container.DataItem;
                    cmd.CommandName = "DocDownload";
                    if (cmd.Text == "")
                    {
                        var nodDesc = objInfo.XMLDoc.SelectSingleNode("genxml/lang/genxml/docs/genxml[" + index + "]/textbox/txtdocdesc");
                        if (nodDesc != null) cmd.Text = nodDesc.InnerText;
                    }
                    if (cmd.ToolTip == "")
                    {
                        var nodName = objInfo.XMLDoc.SelectSingleNode("genxml/docs/genxml[" + index + "]/textbox/txtfilename");
                        if (nodName != null) cmd.ToolTip = nodName.InnerText;
                    }

                    cmd.Visible = true;
                    var nodDocId = objInfo.XMLDoc.SelectSingleNode("genxml/docs/genxml[" + index + "]/hidden/docid");
                    if (nodDocId != null && Utils.IsNumeric(nodDocId.InnerText))
                    {
                        if (NBrightBuyV2Utils.DocIsPurchaseOnlyByDocId(Convert.ToInt32(nodDocId.InnerText)))
                        {
                            cmd.Visible = false;
                            var role = "Manager";
                            if (!String.IsNullOrEmpty(_settings["manager.role"])) role = _settings["manager.role"];
                            var uInfo = UserController.GetCurrentUserInfo();
                            if (NBrightBuyV2Utils.DocHasBeenPurchasedByDocId(uInfo.UserID, Convert.ToInt32(nodDocId.InnerText)) || CmsProviderManager.Default.IsInRole(role)) cmd.Visible = true;
                        }
                    }
                }

            }
            catch (Exception)
            {
                cmd.Visible = false;
            }
        }


        #endregion

        #region "Related Products"

        private void CreateRelatedlist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
            }
            lc.DataBinding += RelatedlistDataBind;
            container.Controls.Add(lc);
        }

        private void RelatedlistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = NBrightGlobal.IsVisible;
                if (lc.Visible)
                {

                    var moduleid = _settings["moduleid"];
                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleid) && (templName != ""))
                    {
                        var debugMode = _settings.ContainsKey("debug.mode") && _settings["debug.mode"].ToLower() == "1";
                        var modCtrl = new NBrightBuyController();
                        var rpTempl = modCtrl.GetTemplateData(Convert.ToInt32(moduleid), templName, Utils.GetCurrentCulture(), _settings, debugMode); 

                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains('"' + templName + '"')) rpTempl = rpTempl.Replace(templName, "");
                        //build list
                        var objInfo = (NBrightInfo)container.DataItem;

                        List<NBrightInfo> objL = null;
                        var strCacheKey = Utils.GetCurrentCulture() + "*" + objInfo.ItemID;
                        if (!debugMode) objL = (List<NBrightInfo>)Utils.GetCache(strCacheKey);
                        if (objL == null)
                        {
                            objL = NBrightBuyV2Utils.GetRelatedProducts(objInfo);
                            if (!debugMode) NBrightBuyUtils.SetModCache(Convert.ToInt32(moduleid), strCacheKey, objL);                            
                        }
                        // render repeater
                        try
                        {
                            strOut = GenXmlFunctions.RenderRepeater(objL, rpTempl, "", "XMLData", "", _settings);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        #endregion

        #region "Qty Field"

        private void CreateQtyField(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox();
            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);
            txt.ID = "selectedaddqty";
            txt.DataBinding += QtyFieldDataBind;
            container.Controls.Add(txt);
        }

        private void QtyFieldDataBind(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            txt.Visible = NBrightGlobal.IsVisible;
        }

        #endregion

        #region "create EditLink control"

        private void CreateEditLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            // if we are using xsl then we might not have a databind ItemId (if the xsl is in the header loop).  So pass it in here, via the xsl, so we can use it in the link. 
            if (xmlNod.Attributes != null && (xmlNod.Attributes["itemid"] != null))
            {
                lk.NavigateUrl = xmlNod.Attributes["itemid"].InnerXml;
            }
            else
            {
                lk.NavigateUrl = "";
            }

            lk.DataBinding += EditLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void EditLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
            try
            {
                lk.Visible = NBrightGlobal.IsVisible;

                var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));

                if (lk.NavigateUrl != "") entryid = lk.NavigateUrl; // use the itemid passed in (XSL loop in display header)

                //[TODO: change for v3 BO]
                var url = "Unable to find NB_Store_BackOffice module";
                var mCtrl = new ModuleController();
                var mInfo = mCtrl.GetModuleByDefinition(PortalSettings.Current.PortalId, "NB_Store_BackOffice");
                if (mInfo != null)
                {
                    var paramlist = new string[4];
                    paramlist[0] = "mid=" + mInfo.ModuleID.ToString("");
                    paramlist[1] = "ProdID=" + NBrightBuyV2Utils.GetLegacyProductId(Convert.ToInt32(entryid));
                    paramlist[2] = "RtnTab=" + PortalSettings.Current.ActiveTab.TabID.ToString("");
                    paramlist[3] = "rtnmid=" + _settings["moduleid"];
                    var urlpage = Utils.RequestParam(HttpContext.Current, "page");
                    if (urlpage.Trim() != "")
                    {
                        IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "PageIndex=" + urlpage.Trim();
                    }
                    var urlcatid = Utils.RequestParam(HttpContext.Current, "catid");
                    if (urlcatid.Trim() != "")
                    {
                        IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "CatID=" + urlcatid.Trim();
                    }

                    url = Globals.NavigateURL(mInfo.TabID, "AdminProduct", paramlist);
                }
                lk.NavigateUrl = url;
            }
            catch (Exception ex)
            {
                lk.Text = ex.ToString();
            }
        }

        #endregion

        #region "Sale Price"

        private void CreateSalePrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += SalePriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void SalePriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = NBrightGlobal.IsVisible;
                var sp = GetSalePrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyV2Utils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Dealer Price"

        private void CreateDealerPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += DealerPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void DealerPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = NBrightGlobal.IsVisible;
                var sp = GetDealerPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyV2Utils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "CreateCurrencyIsoCode"

        private void CreateCurrencyIsoCode(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += CreateCurrencyIsoCodeDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void CreateCurrencyIsoCodeDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = NBrightGlobal.IsVisible;
                l.Text = NBrightBuyV2Utils.GetCurrencyIsoCode();
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "From Price"

        private void CreateFromPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += FromPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void FromPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = NBrightGlobal.IsVisible;
                var sp = GetFromPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyV2Utils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Best Price"

        private void CreateBestPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += BestPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void BestPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = NBrightGlobal.IsVisible;
                var sp = GetBestPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyV2Utils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Functions"

        private List<NBrightInfo> BuildModelList(NBrightInfo dataItemObj, Boolean addCartStock = false, Boolean addSalePrices = false)
        {
            //build models list
            var objL = new List<NBrightInfo>();
            var nodList = dataItemObj.XMLDoc.SelectNodes("genxml/models/*");
            if (nodList != null)
            {

                #region "Init"
                var isDealer = CmsProviderManager.Default.IsInRole(_settings["dealer.role"]);


                #endregion

                foreach (XmlNode nod in nodList)
                {
                    // check if Deleted
                    var selectDeletedFlag = nod.SelectSingleNode("checkbox/chkdeleted");
                    if ((selectDeletedFlag != null) && (selectDeletedFlag.InnerText == "False"))
                    {
                        // check if dealer
                        var selectDealerFlag = nod.SelectSingleNode("checkbox/chkdealeronly");
                        if (((selectDealerFlag != null) && (!isDealer && (selectDealerFlag.InnerText == "False"))) | isDealer)
                        {
                            // get modelid
                            var nodModelId = nod.SelectSingleNode("hidden/modelid");
                            var modelId = -1;
                            if (nodModelId != null && Utils.IsNumeric(nodModelId.InnerText)) modelId = Convert.ToInt32(nodModelId.InnerText);

                            //Build NBrightInfo class for model
                            var o = new NBrightInfo();
                            o.XMLData = nod.OuterXml;
                            if (modelId >= 0)
                            {
                                #region "Add Lanaguge Data"

                                var nodLang = dataItemObj.XMLDoc.SelectSingleNode("genxml/lang/genxml/models/genxml[hidden/modelid='" + modelId.ToString("") + "']");
                                if (nodLang != null)
                                {
                                    o.AddSingleNode("lang", "", "genxml");
                                    o.AddXmlNode(nodLang.OuterXml, "genxml", "genxml/lang");
                                }

                                #endregion

                                #region "stock calcs"

                                if (addCartStock)
                                {
                                    // Get the stock levels that exists in the cart. (this could also be for all carts if the "lockstockoncart" setting is set)
                                    var nodModelStock = nod.SelectSingleNode("textbox/txtqtyremaining");
                                    if (nodModelStock != null)
                                    {
                                        var modelStock = nodModelStock.InnerText;
                                        // only create nodes if stock is active (modelstock of -1, stock is turned off)                                       
                                        if (Utils.IsNumeric(modelStock) && Convert.ToInt32(modelStock) >= 0)
                                        {
                                            var cartStock = 0;
                                            cartStock = CurrentCart.GetCartStockInModel(PortalSettings.Current.PortalId, modelId);

                                            o.AddSingleNode("stockincart", cartStock.ToString(""), "genxml/hidden");
                                            var actualStock = Convert.ToInt32(modelStock);
                                            actualStock = actualStock - cartStock;
                                            if (actualStock < 0) actualStock = 0;
                                            o.AddSingleNode("calcstock", actualStock.ToString(""), "genxml/hidden");
                                        }
                                    }
                                }

                                #endregion

                                #region "Prices"

                                if (addSalePrices)
                                {
                                    var uInfo = UserController.GetCurrentUserInfo();
                                    if (uInfo != null)
                                    {
                                        var objPromoCtrl = new PromoController();
                                        var objPCtrl = new ProductController();
                                        var objM = objPCtrl.GetModel(modelId, Utils.GetCurrentCulture());
                                        var salePrice = objPromoCtrl.GetSalePrice(objM, uInfo);
                                        o.AddSingleNode("saleprice", salePrice.ToString(""), "genxml/hidden");
                                    }
                                }

                                #endregion

                            }
                            objL.Add(o);
                        }
                    }
                }
            }
            return objL;
        }

        private String GetSalePrice(NBrightInfo dataItemObj)
        {
            var saleprice = "-1";
            var l = BuildModelList(dataItemObj, false, true);
            foreach (var m in l)
            {
                var s = m.GetXmlProperty("genxml/hidden/saleprice");
                if (Utils.IsNumeric(s))
                {
                    if ((Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(saleprice, CultureInfo.GetCultureInfo("en-US"))) | (saleprice == "-1")) saleprice = s;
                }
            }
            return saleprice;
        }

        private String GetDealerPrice(NBrightInfo dataItemObj)
        {
            var dealprice = "-1";
            var l = BuildModelList(dataItemObj, false, false);
            foreach (var m in l)
            {
                var s = m.GetXmlProperty("genxml/textbox/txtdealercost");
                if (Utils.IsNumeric(s))
                {
                    if ((Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(dealprice, CultureInfo.GetCultureInfo("en-US"))) | (dealprice == "-1")) dealprice = s;
                }
            }
            return dealprice;
        }

        private String GetFromPrice(NBrightInfo dataItemObj)
        {
            var price = "-1";
            var l = BuildModelList(dataItemObj, false, false);
            foreach (var m in l)
            {
                var s = m.GetXmlProperty("genxml/textbox/txtunitcost");
                if (Utils.IsNumeric(s))
                {
                    // NBrightBuy numeric always stored in en-US format.
                    if ((Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(price, CultureInfo.GetCultureInfo("en-US"))) | (price == "-1")) price = s;
                }
            }
            return price;
        }

        private String GetBestPrice(NBrightInfo dataItemObj)
        {
            var fromprice = Convert.ToDouble(GetFromPrice(dataItemObj));
            if (fromprice < 0) fromprice = 0; // make sure we have a valid price
            var saleprice = Convert.ToDouble(GetSalePrice(dataItemObj));
            if (saleprice < 0) saleprice = fromprice; // sale price might not exists.

            var role = "Dealer";
            if (!String.IsNullOrEmpty(_settings["dealer.role"])) role = _settings["dealer.role"];
            if (CmsProviderManager.Default.IsInRole(role))
            {
                var dealerprice = Convert.ToDouble(GetDealerPrice(dataItemObj));
                if (dealerprice <= 0) dealerprice = fromprice; // check for valid dealer price.
                if (fromprice < dealerprice)
                {
                    if (fromprice < saleprice) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                    return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                }
                if (dealerprice < saleprice) return dealerprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
            if (fromprice < saleprice) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));                
        }

        private Boolean HasDifferentPrices(NBrightInfo dataItemObj)
        {
            var nodList = dataItemObj.XMLDoc.SelectNodes("genxml/models/*");
            if (nodList != null)
            {
                //check if we really need to add prices (don't if all the same)
                var holdPrice = "";
                var holdDealerPrice = "";
                var isDealer = CmsProviderManager.Default.IsInRole(_settings["dealer.role"]);
                foreach (XmlNode nod in nodList)
                {
                    var mPrice = nod.SelectSingleNode("textbox/txtunitcost");
                    if (mPrice != null)
                    {
                        if (holdPrice != "" && mPrice.InnerText != holdPrice)
                        {
                            return true;
                        }
                        holdPrice = mPrice.InnerText;
                    }
                    if (isDealer)
                    {
                        var mDealerPrice = nod.SelectSingleNode("textbox/txtdealercost");
                        if (mDealerPrice != null)
                        {
                            if (holdDealerPrice != "" && mDealerPrice.InnerText != holdDealerPrice) return true;
                            holdDealerPrice = mDealerPrice.InnerText;
                        }                        
                    }
                }
            }
            return false;
        }

        public static void IncreaseArray(ref string[] values, int increment)
        {
            var array = new string[values.Length + increment];
            values.CopyTo(array, 0);
            values = array;
        }

        private String SumQtyinStock(NBrightInfo dataItemObj)
        {
            var stock = -1;  // -1 passed back if stock turned off for all models.
            var l = BuildModelList(dataItemObj,true);
            foreach (var m in l)
            {
                var s = m.GetXmlProperty("genxml/hidden/calcstock");
                if (Utils.IsNumeric(s))
                {
                    // add only if stock is turn on for the model.
                    if (Convert.ToInt32(s) > 0 ) stock += Convert.ToInt32(s);
                }
            }
            return stock.ToString("");
        }

        private Boolean IsInStock(NBrightInfo dataItem,String qtyTestAmt = "0",Boolean includeCart = false)
        {
            var amtTest = 0;

            if (Utils.IsNumeric(qtyTestAmt)) amtTest = Convert.ToInt32(qtyTestAmt);
            var nodList = BuildModelList(dataItem, includeCart);
            var stockcount = 0;
            var stockOn = true;
            foreach (var obj in nodList)
            {
                var strStockOn = obj.GetXmlProperty("genxml/textbox/txtqtyremaining");
                if (strStockOn == "-1")
                { // stock must be availble, if stock turned off
                    stockOn = false;
                    break;
                }
                var stock = "0";
                if (includeCart)
                    stock = obj.GetXmlProperty("genxml/hidden/calcstock");
                else
                    stock = obj.GetXmlProperty("genxml/textbox/txtqtyremaining");

                if (Utils.IsNumeric(stock)) stockcount += Convert.ToInt32(stock);
            }
            if (stockOn && (stockcount > amtTest)) return true;
            if (!stockOn) return true;
            return false;
        }

        #endregion
    }
}

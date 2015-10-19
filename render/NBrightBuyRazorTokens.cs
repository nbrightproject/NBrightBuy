using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using NBrightCore.common;
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

        #region "NBS display tokens"

        #region "products"

        /// <summary>
        /// Product Name
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <returns>Product name text</returns>
        public IEncodedString ProductName(NBrightInfo info)
        {
            return new RawString(info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"));
        }

        /// <summary>
        /// Thumbnail image
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="idx">index of the image to display</param>
        /// <param name="attributes">free text added onto end of url parameters</param>
        /// <returns>Thumbnailer url</returns>
        public IEncodedString ProductImage(NBrightInfo info, String width = "150", String height = "0", String idx = "1", String attributes = "")
        {
            var imagesrc = info.GetXmlProperty("genxml/imgs/genxml[" + idx + "]/hidden/imageurl");
            var url = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?src=" + imagesrc + "&w=" + width + "&h=" + height +   attributes;
            return new RawString(url);
        }

        /// <summary>
        /// Edit Product url
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <param name="model">Razor Model class (NBrightRazor)</param>
        /// <returns>Url to edit product</returns>
        public IEncodedString EditUrl(NBrightInfo info, NBrightRazor model)
        {
            var entryid = info.ItemID;
            var url = "Unable to find BackOffice Setting, go into Back Office settings and save.";
            if (entryid > 0 && StoreSettings.Current.GetInt("backofficetabid") > 0)
            {
                var param = new List<String>();

                param.Add("eid=" + entryid.ToString(""));
                param.Add("ctrl=products");
                param.Add("rtntab=" + PortalSettings.Current.ActiveTab.TabID.ToString());
                if (model.GetSetting("moduleid") != "") param.Add("rtnmid=" + model.GetSetting("moduleid").Trim());
                if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
                if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());

                var paramlist = new string[param.Count];
                for (int lp = 0; lp < param.Count; lp++)
                {
                    paramlist[lp] = param[lp];
                }
                
                url = Globals.NavigateURL(StoreSettings.Current.GetInt("backofficetabid"), "", paramlist);
            }
            return new RawString(url);
        }

        public IEncodedString ModelsRadio(NBrightInfo info, String attributes = "", String template = "{name} ({bestprice})", Int32 defaultIndex = 0, Boolean displayprice = false)
        {
            var strOut = "";
            var objL = NBrightBuyUtils.BuildModelList(info, true);

            if (!displayprice)
            {
                displayprice = NBrightBuyUtils.HasDifferentPrices(info);
            }

            var c = 0;
            var id = info.ItemID + "_rblmodelsel";
            var s = "";
            var v = "";
            foreach (var obj in objL)
            {
                var text = NBrightBuyUtils.GetItemDisplay(obj, template, displayprice);
                var value = obj.GetXmlProperty("genxml/hidden/modelid");
                if (value == v || (v == "" && defaultIndex == c))
                    s = "checked";
                else
                    s = "";
                strOut += "<div " + attributes + "><input id='" + id + "_" + c.ToString("") + "' update='save' name='" + id + "' type='radio' value='" + value + "'  " + s + "/><label>" + text + "</label></div>";
                c += 1;

            }
            return new RawString(strOut);
        }

        public IEncodedString ModelsDropDown(NBrightInfo info, String attributes = "", String template = "{name} ({bestprice})", Int32 defaultIndex = 0, Boolean displayprice = false)
        {
            var strOut = "";
            var objL = NBrightBuyUtils.BuildModelList(info, true);

            if (!displayprice)
            {
                displayprice = NBrightBuyUtils.HasDifferentPrices(info);
            }

            var c = 0;
            var id = info.ItemID + "_ddlmodelsel";
            var s = "";
            var v = "";
            strOut = "<select id='" + id + "' update='save' " + attributes + ">";
            foreach (var obj in objL)
            {
                var text = NBrightBuyUtils.GetItemDisplay(obj, template, displayprice);
                var value = obj.GetXmlProperty("genxml/hidden/modelid");
                if (value == v || (v == "" && defaultIndex == c))
                    s = "selected";
                else
                    s = "";

                strOut += "    <option value='" + value + "' " + s + ">" + text + "</option>";
                c += 1;
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString ProductOption(ProductData productdata, int index, String attributes = "")
        {
            var strOut = "";

            var objL = productdata.Options;

            if (objL.Count > index)
            {
                var obj = objL[index];
                var optid = obj.GetXmlProperty("genxml/hidden/optionid");
                var optvalList = productdata.GetOptionValuesById(optid);

                strOut += "<div  class='option option" + (index + 1) + "' " + attributes + ">";
                strOut += "<span class='optionname optionname" + (index + 1) + "'>" + obj.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</span>";
                strOut += "<span class='optionvalue optionvalue" + (index + 1) + "'>";

                if (optvalList.Count > 1)
                {
                    //dropdown
                    strOut += "<select id='optionddl" + (index + 1) + "' update='save'>";
                    foreach (var optval in optvalList)
                    {
                        strOut += "    <option value='" + optval.GetXmlProperty("genxml/hidden/optionvalueid") + "'>" + optval.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</option>";
                    }
                    strOut += "</select>";
                }
                if (optvalList.Count == 1)
                {
                    //checkbox
                    foreach (var optval in optvalList)
                    {
                        strOut += "    <input id='optionchk" + (index + 1) + "' type='checkbox' " + attributes + " update='save' /><label>" + optval.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</label>";
                    }
                }
                if (optvalList.Count == 0)
                {
                    // textbox
                    strOut += "<input id='optiontxt" + (index + 1) + "' update='save' type='text' />";
                }
                strOut += "<input id='optionid" + (index + 1) + "' update='save' type='hidden' value='" + optid + "' />";
                strOut += "</span>";
                strOut += "</div>";

            }


            return new RawString(strOut);
        }

        public IEncodedString ProductOptions(ProductData  productdata, String attributes = "")
        {
            var strOut = "";

            var objL = productdata.Options;
            var c = objL.Count;
            for (int i = 0; i < c; i++)
            {
                strOut += ProductOption(productdata, i);
            }

            return new RawString(strOut);
        }

        #endregion

        #region "categories"

        public IEncodedString Category(String fieldname)
        {
            var strOut = "";
            try
            {
                // if we have no catid in url, we're going to need a default category from module.
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetGrpCategory(StoreSettings.Current.ActiveCatId);
                if (objCInfo != null)
                {
                    GroupCategoryData objPcat;
                    switch (fieldname.ToLower())
                    {
                        case "categorydesc":
                            strOut = objCInfo.categorydesc;
                            break;
                        case "message":
                            strOut = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                            break;
                        case "archived":
                            strOut = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "breadcrumb":
                            strOut = objCInfo.breadcrumb;
                            break;
                        case "categoryid":
                            strOut = objCInfo.categoryid.ToString("");
                            break;
                        case "categoryname":
                            strOut = objCInfo.categoryname;
                            break;
                        case "categoryref":
                            strOut = objCInfo.categoryref;
                            break;
                        case "depth":
                            strOut = objCInfo.depth.ToString("");
                            break;
                        case "disabled":
                            strOut = objCInfo.disabled.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "entrycount":
                            strOut = objCInfo.entrycount.ToString("");
                            break;
                        case "grouptyperef":
                            strOut = objCInfo.grouptyperef;
                            break;
                        case "imageurl":
                            strOut = objCInfo.imageurl;
                            break;
                        case "ishidden":
                            strOut = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "isvisible":
                            strOut = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "metadescription":
                            strOut = objCInfo.metadescription;
                            break;
                        case "metakeywords":
                            strOut = objCInfo.metakeywords;
                            break;
                        case "parentcatid":
                            strOut = objCInfo.parentcatid.ToString("");
                            break;
                        case "parentcategoryname":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryname;
                            break;
                        case "parentcategoryref":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryref;
                            break;
                        case "parentcategorydesc":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categorydesc;
                            break;
                        case "parentcategorybreadcrumb":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.breadcrumb;
                            break;
                        case "parentcategoryguidkey":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryrefGUIDKey;
                            break;
                        case "recordsortorder":
                            strOut = objCInfo.recordsortorder.ToString("");
                            break;
                        case "seoname":
                            strOut = objCInfo.seoname;
                            if (strOut == "") strOut = objCInfo.categoryname;
                            break;
                        case "seopagetitle":
                            strOut = objCInfo.seopagetitle;
                            break;
                        case "url":
                            strOut = objCInfo.url;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString CategoryBreadCrumb(Boolean includelinks, Boolean aslist = true, int tabRedirect = -1, String separator = "", int wordlength = -1, int maxlength = 400)
        {
            var strOut = "";

            try
            {
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetGrpCategory(StoreSettings.Current.ActiveCatId);
                if (objCInfo != null)
                {

                    if (StoreSettings.Current.ActiveCatId > 0) // check we have a catid
                    {
                        if (includelinks)
                        {
                            if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                            strOut = grpCatCtrl.GetBreadCrumbWithLinks(StoreSettings.Current.ActiveCatId, tabRedirect, wordlength, separator, aslist);
                        }
                        else
                        {
                            strOut = grpCatCtrl.GetBreadCrumb(StoreSettings.Current.ActiveCatId, wordlength, separator, aslist);
                        }

                        if ((strOut.Length > maxlength) && (!aslist))
                        {
                            strOut = strOut.Substring(0, (maxlength - 3)) + "...";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }


            return new RawString(strOut);
        }

        #endregion

        #region "properties"

        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="productdata">productdata class</param>
        /// <param name="propertytype">type of property using propertygroup ref  (e.g. "man" = manufacturer)</param>
        /// <param name="fieldname">field name of data to return</param>
        /// <param name="propertyref">property ref to return</param>
        /// <returns></returns>
        public IEncodedString PropertyValue(ProductData productdata, String propertytype, String fieldname, String propertyref)
        {
            var strOut = "";
            try
            {
                var l = productdata.GetCategories(propertytype);
                foreach (var i in l)
                {
                    if (i.categoryref == propertyref)
                    {
                        return PropertyValue(i, fieldname);
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }
        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="productdata">productdata class</param>
        /// <param name="propertytype">type of property using propertygroup ref  (e.g. "man" = manufacturer)</param>
        /// <param name="fieldname">field name of data to return</param>
        /// <param name="index">zero based index of the property record to return</param>
        /// <returns></returns>
        public IEncodedString PropertyValue(ProductData productdata,String propertytype,String fieldname, int index = 0)
        {
            var strOut = "";
            try
            {
                var l = productdata.GetCategories(propertytype);
                if (l.Count > index)
                {                    
                var objCInfo = l[index];
                    return PropertyValue(objCInfo, fieldname);
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString PropertyUrl(ProductData productdata, String propertytype, String propertyref, int tabRedirect = -1)
        {
            var strOut = "";
            try
            {
                var objGCC = new GrpCatController(productdata.Info.Lang);
                var l = productdata.GetProperties(propertytype);
                foreach (var i in l)
                {
                    if (i.categoryref == propertyref)
                    {
                        if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                        return new RawString(objGCC.GetCategoryUrl(i, tabRedirect));
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }
            return new RawString(strOut);
        }

        public IEncodedString PropertyUrl(ProductData productdata, String propertytype, int index = 0, int tabRedirect = -1)
        {
            var strOut = "";
            try
            {
                var objGCC = new GrpCatController(productdata.Info.Lang);
                var l = productdata.GetCategories(propertytype);
                if (l.Count > index)
                {
                    if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                    return new RawString(objGCC.GetCategoryUrl(l[index], tabRedirect));
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }
            return new RawString(strOut);
        }


        public IEncodedString PropertyValue(GroupCategoryData groupCategopryData, String fieldname)
        {
            var strOut = "";
            try
            {

                var objCInfo = groupCategopryData;
                if (objCInfo != null)
                {
                    switch (fieldname.ToLower())
                    {
                        case "propertydesc":
                            strOut = objCInfo.categorydesc;
                            break;
                        case "message":
                            strOut = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                            break;
                        case "archived":
                            strOut = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "breadcrumb":
                            strOut = objCInfo.breadcrumb;
                            break;
                        case "propertyid":
                            strOut = objCInfo.categoryid.ToString("");
                            break;
                        case "propertyname":
                            strOut = objCInfo.categoryname;
                            break;
                        case "propertyref":
                            strOut = objCInfo.categoryref;
                            break;
                        case "depth":
                            strOut = objCInfo.depth.ToString("");
                            break;
                        case "disabled":
                            strOut = objCInfo.disabled.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "entrycount":
                            strOut = objCInfo.entrycount.ToString("");
                            break;
                        case "grouptyperef":
                            strOut = objCInfo.grouptyperef;
                            break;
                        case "imageurl":
                            strOut = objCInfo.imageurl;
                            break;
                        case "ishidden":
                            strOut = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "isvisible":
                            strOut = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "metadescription":
                            strOut = objCInfo.metadescription;
                            break;
                        case "metakeywords":
                            strOut = objCInfo.metakeywords;
                            break;
                        case "parentcatid":
                            strOut = objCInfo.parentcatid.ToString("");
                            break;
                        case "recordsortorder":
                            strOut = objCInfo.recordsortorder.ToString("");
                            break;
                        case "seoname":
                            strOut = objCInfo.seoname;
                            if (strOut == "") strOut = objCInfo.categoryname;
                            break;
                        case "seopagetitle":
                            strOut = objCInfo.seopagetitle;
                            break;
                        case "url":
                            strOut = objCInfo.url;
                            break;
                    }
                }
            }
            catch
                (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        #endregion

        #region "Functional"

        public IEncodedString EntryUrl(NBrightInfo info, NBrightRazor model, Boolean relative = true, String categoryref = "")
        {
            var url = "";
            try
            {
                var urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (urlname == "") urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");

                    // see if we've injected a categoryid into the data class, this is done in the case of the categorymenu when displaying products.
                var categoryid = info.GetXmlProperty("genxml/categoryid");
                if (categoryid == "") categoryid = StoreSettings.Current.ActiveCatId.ToString();
                if (categoryid == "0") categoryid = ""; // no category active if zero

                url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, info.ItemID.ToString(), model.GetSetting("detailmodulekey"), urlname, model.GetSetting("ddldetailtabid"), categoryid, categoryref);
                if (relative) url = Utils.GetRelativeUrl(url);

            }
            catch (Exception ex)
            {
                url = ex.ToString();
            }

            return new RawString(url);
        }

        public IEncodedString EntryReturnUrl(NBrightRazor model)
        {
            var product = (ProductData)model.List.First();
            var entryid = product.Info.ItemID;
            var url = "";

            var param = new List<String>();
            if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
            if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());
            var listtab = model.GetUrlParam("rtntab");
            if (listtab == "") listtab = StoreSettings.Current.Get("productlisttab");
            var intlisttab = StoreSettings.Current.ActiveCatId;
            if (Utils.IsNumeric(listtab)) intlisttab = Convert.ToInt32(listtab);

            var paramlist = new string[param.Count];
            for (int lp = 0; lp < param.Count; lp++)
            {
                paramlist[lp] = param[lp];
            }

            url = Globals.NavigateURL(intlisttab, "", paramlist);
            return new RawString(url);
        }

        public IEncodedString CurrencyOf(Double x)
        {
            var strOut = NBrightBuyUtils.FormatToStoreCurrency(x);
            return new RawString(strOut);
        }

        public IEncodedString SortOrderDropDownList(String datatext, String modRef, String attributes = "")
        {
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, modRef);

            var strOut = "";
            var datat = datatext.Split(',');
                strOut = "<select " + attributes + ">";
                var c = 0;
                var s = "";
                foreach (var t in datat)
                {
                    s = "";
                    if (("orderby" + c) == navigationdata.OrderByIdx.ToLower()) s = "selected"; 
                    strOut += "    <option value='orderby" + c + "' " + s + " >" + t + "</option>";
                    c += 1;
                }
                strOut += "</select>";
            return new RawString(strOut);
        }


        #endregion

        #endregion

    }


}

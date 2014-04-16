using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    /// <summary>
    /// Class to deal with search cookie data.
    /// </summary>
    public class NavigationData
    {

        private int _portalId;
        private string _cookieName;
        private string _cookieNameXml;
        private string _encryptkey;
        private DataStorageType _storageType;

        /// <summary>
        /// Populate class with cookie data
        /// </summary>
        /// <param name="portalId"> </param>
        /// <param name="tabid"> </param>
        /// <param name="moduleKey"> </param>
        /// <param name="storageType"> Select data storgae type "SessionMemory" or "Cookie" (Default Cookie) </param>
        /// <param name="nameAppendix">specifiy Unique key for search data</param>
        public NavigationData(int portalId, int tabid, String moduleKey, String storageType = "Cookie", string nameAppendix = "")
        {
            if (storageType != null)
                if (storageType.ToLower() == "sessionmemory")
                    _storageType = DataStorageType.SessionMemory;
                else
                    _storageType = DataStorageType.Cookie;
            else
                _storageType = DataStorageType.Cookie;

            Exists = false;
            _portalId = portalId;
            _cookieName = "NBrightBuyNav" + "*" + tabid.ToString("") + "*" + moduleKey + nameAppendix;
            _cookieNameXml = "NBrightBuyNavXml" + "*" + tabid.ToString("") + "*" + moduleKey + nameAppendix;
            _encryptkey = "NBrightBuyNav";
            Get();
        }

        /// <summary>
        /// Build the SQL criteria form the xml field input and the template meta data
        /// </summary>
        public void Build(String xmlData, GenXmlTemplate templ)
        {
            Criteria = "";
            var obj = new NBrightInfo();
            try
            {
                obj.XMLData = xmlData;
            }
            catch
            {
                //Just jump out without search.
            }

            // get any disable controls, we dont; want to process SQl for these.
            var disabledtokens = obj.GetXmlProperty("genxml/hidden/disabledsearchtokens") + ";";

            //Get only search tags
            var searchTags = new List<String>();
            foreach (var mta in templ.MetaTags)
            {
                var orderId = GenXmlFunctions.GetGenXmlValue(mta, "tag/@id");
                var active = GenXmlFunctions.GetGenXmlValue(mta, "tag/@active");
                if (active != "False" && orderId.ToLower().StartsWith("search") && !disabledtokens.Contains(orderId + ";"))
                {
                    searchTags.Add(mta);
                }
            }

            if (searchTags.Count > 0)
            {
                Criteria += " and ( "; // Always use "and", otherwise the portalid and moduleid criteria will always select all.
                var lp = 0;
                foreach (var mt in searchTags)
                {
                    lp += 1;
                    var action = GenXmlFunctions.GetGenXmlValue(mt, "tag/@action");
                    var search = GenXmlFunctions.GetGenXmlValue(mt, "tag/@search");
                    var sqlfield = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqlfield");
                    var sqlcol = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqlcol");
                    var searchfrom = GenXmlFunctions.GetGenXmlValue(mt, "tag/@searchfrom");
                    var searchto = GenXmlFunctions.GetGenXmlValue(mt, "tag/@searchto");
                    var sqltype = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqltype");
                    var sqloperator = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqloperator");

                    if (sqlfield == "") sqlfield = GenXmlFunctions.GetGenXmlValue(mt, "tag/@xpath");  //check is xpath of node ha been used.

                    if (lp == 1) sqloperator = ""; // use the "and" sepcified above for the first criteria.

                    // the < sign cannot be used in a XML attribute (it's illegal), so to do a xpath minimum value, we use a {LessThan} token.
                    // Ummm!!!. http://msdn.microsoft.com/en-us/library/ms748250(v=vs.110).aspx
                    if (sqlfield.Contains("{LessThan}")) sqlfield = sqlfield.Replace("{LessThan}", "<");
                    if (sqlfield.Contains("{GreaterThan}")) sqlfield = sqlfield.Replace("{GreaterThan}", ">"); // to keep it consistant

                    if (sqltype == "") sqltype = "nvarchar(max)";

                    if (sqlcol == "") sqlcol = "XMLData";

                    var searchVal = obj.GetXmlProperty(search);
                    if (searchVal == "") searchVal = GenXmlFunctions.GetGenXmlValue(mt, "tag/@static");

                    var searchValFrom = obj.GetXmlProperty(searchfrom);
                    var searchValTo = obj.GetXmlProperty(searchto);
                    switch (action.ToLower())
                    {
                        case "open":
                            Criteria += sqloperator + " ( ";
                            break;
                        case "close":
                            Criteria += " ) ";
                            break;
                        case "equal":
                            Criteria += " " + sqloperator + " " +
                                        GenXmlFunctions.GetSqlFilterText(sqlfield, sqltype, searchVal, sqlcol);
                            break;
                        case "like":
                            if (searchVal == "")
                                searchVal = "NORESULTSnbright";
                            // for "like", build the sql so we have valid value, but add a fake search so the result is nothing for no selection values
                            Criteria += " " + sqloperator + " " +
                                        GenXmlFunctions.GetSqlFilterLikeText(sqlfield, sqltype, searchVal, sqlcol);
                            break;
                        case "range":
                            // We always need to return a value, otherwise we get an error, so range select cannot be empty. (we'll default here to 9999999)
                            if (searchValFrom == "") searchValFrom = "0";
                            if (searchValTo == "") searchValTo = "9999999";
                            Criteria += " " + sqloperator + " " +
                                        GenXmlFunctions.GetSqlFilterRange(sqlfield, sqltype, searchValFrom, searchValTo, sqlcol);
                            break;
                        case "cats":
                            Criteria += " " + sqloperator + " ";
                            var selectoperator = GenXmlFunctions.GetGenXmlValue(mt, "tag/@selectoperator");
                            Criteria += BuildCategorySearch(search, obj, selectoperator);
                            break;
                    }
                }
                Criteria += " ) ";
            }
        }

        private String BuildCategorySearch(String search, NBrightInfo searchData, String selectoperator)
        {
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

            // get list of selected categories.
            var catlist = new List<string>();
            var xmlNod = GenXmlFunctions.GetGenXmLnode(searchData.XMLData, search);
            if (xmlNod != null)
            {
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    if (xmlNodeList.Count == 0)
                    {//dropdown list
                        catlist.Add(xmlNod.InnerText);
                    }
                    else
                    {// checkbox list
                        foreach (XmlNode xmlNoda in xmlNodeList)
                        {
                            if (xmlNoda.Attributes != null && xmlNoda.Attributes["value"] != null && xmlNoda.Attributes["data"] != null)
                            {
                                if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                                {
                                    catlist.Add(xmlNoda.Attributes["data"].Value);
                                }
                            }
                        }
                    }
                }
            }
            //build SQL
            var strRtn = "";
            if (catlist.Count > 0)
            {
                var categorylist = "";
                for (int i = 0; i < catlist.Count; i++)
                {
                    categorylist += catlist[i] + ",";
                }
                categorylist = categorylist.TrimEnd(',');

                if (selectoperator.ToLower() == "and")
                {
                    strRtn += " (select count(parentitemid) from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and parentitemid = NB1.[ItemId] and XrefItemId in (" + categorylist + ")) = " + catlist.Count + " ";
                }
                else
                {
                    if (selectoperator.ToLower() == "cascade")
                    {
                        strRtn += "NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId in (" + categorylist + ")) ";
                    }
                    else
                    {
                        strRtn += "NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId in (" + categorylist + ")) ";
                    }
                }
            }
            else
            {
                // no categories selected, so add sql to stop display 
                strRtn += "NB1.[ItemId] = -1";
            }

            return strRtn;
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void Save()
        {
            if (_storageType == DataStorageType.SessionMemory)
            {
                // save data to cache
                HttpContext.Current.Session[_cookieName + "Criteria"] = Criteria;
                HttpContext.Current.Session[_cookieName +  "PageModuleId"] = PageModuleId;
                HttpContext.Current.Session[_cookieName + "PageNumber"] = PageNumber;
                HttpContext.Current.Session[_cookieName + "PageName"] = PageName;
                HttpContext.Current.Session[_cookieName + "OrderBy"] =  OrderBy;
                HttpContext.Current.Session[_cookieName + "CategoryId"] = CategoryId;

                // could be large, use with care.           
                HttpContext.Current.Session[_cookieNameXml + "XmlData"] =  XmlData;                
                
            }
            else
            {
                // save data to cache
                Cookie.SetCookieValue(_portalId, _cookieName, "Criteria", Criteria, 1, _encryptkey);
                Cookie.SetCookieValue(_portalId, _cookieName, "PageModuleId", PageModuleId, 1, _encryptkey);
                Cookie.SetCookieValue(_portalId, _cookieName, "PageNumber", PageNumber, 1, _encryptkey);
                Cookie.SetCookieValue(_portalId, _cookieName, "PageName", PageName, 1, _encryptkey);
                Cookie.SetCookieValue(_portalId, _cookieName, "OrderBy", OrderBy, 1, _encryptkey);
                Cookie.SetCookieValue(_portalId, _cookieName, "CategoryId", CategoryId, 1, _encryptkey);

                // could make a large cookie, use with care.           
                Cookie.SetCookieValue(_portalId, _cookieNameXml, "XmlData", XmlData, 1, _encryptkey);                
            }

            Exists = true;
        }

        /// <summary>
        /// Get the cookie data from the client.
        /// </summary>
        /// <returns></returns>
        public NavigationData Get()
        {
            ClearData();

            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "Criteria"] != null) Criteria = (String)HttpContext.Current.Session[_cookieName + "Criteria"];
                if (HttpContext.Current.Session[_cookieName + "PageModuleId"] != null) PageModuleId = (String)HttpContext.Current.Session[_cookieName + "PageModuleId"];
                if (HttpContext.Current.Session[_cookieName + "PageNumber"] != null) PageNumber = (String)HttpContext.Current.Session[_cookieName + "PageNumber"];
                if (HttpContext.Current.Session[_cookieName + "PageName"] != null) PageName = (String)HttpContext.Current.Session[_cookieName + "PageName"];
                if (HttpContext.Current.Session[_cookieName + "OrderBy"] != null) OrderBy = (String)HttpContext.Current.Session[_cookieName + "OrderBy"];
                if (HttpContext.Current.Session[_cookieName + "CategoryId"] != null) CategoryId = (String)HttpContext.Current.Session[_cookieName + "CategoryId"];
                if (HttpContext.Current.Session[_cookieNameXml + "XmlData"] != null) XmlData = (String)HttpContext.Current.Session[_cookieNameXml + "XmlData"];
            }
            else
            {
                Criteria = Cookie.GetCookieValue(_portalId, _cookieName, "Criteria", _encryptkey);
                PageModuleId = Cookie.GetCookieValue(_portalId, _cookieName, "PageModuleId", _encryptkey);
                PageNumber = Cookie.GetCookieValue(_portalId, _cookieName, "PageNumber", _encryptkey);
                PageName = Cookie.GetCookieValue(_portalId, _cookieName, "PageName", _encryptkey);
                OrderBy = Cookie.GetCookieValue(_portalId, _cookieName, "OrderBy", _encryptkey);
                CategoryId = Cookie.GetCookieValue(_portalId, _cookieName, "CategoryId", _encryptkey);
                XmlData = Cookie.GetCookieValue(_portalId, _cookieNameXml, "XmlData", _encryptkey);
            }

            if (Criteria == "" && XmlData == "") // "Exist" property not used for paging data
                Exists = false;
            else
                Exists = true;

            return this;
        }

        /// <summary>
        /// Delete cookie from client
        /// </summary>
        public void Delete()
        {
            ClearData();

            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "Criteria"] != null) HttpContext.Current.Session.Remove(_cookieName + "Criteria");
                if (HttpContext.Current.Session[_cookieName + "PageModuleId"] != null) HttpContext.Current.Session.Remove(_cookieName + "PageModuleId");
                if (HttpContext.Current.Session[_cookieName + "PageNumber"] != null) HttpContext.Current.Session.Remove(_cookieName + "PageNumber");
                if (HttpContext.Current.Session[_cookieName + "PageName"] != null) HttpContext.Current.Session.Remove(_cookieName + "PageName");
                if (HttpContext.Current.Session[_cookieName + "OrderBy"] != null) HttpContext.Current.Session.Remove(_cookieName + "OrderBy");
                if (HttpContext.Current.Session[_cookieName + "CategoryId"] != null) HttpContext.Current.Session.Remove(_cookieName + "CategoryId");
                if (HttpContext.Current.Session[_cookieNameXml + "XmlData"] != null) HttpContext.Current.Session.Remove(_cookieNameXml + "XmlData");
            }
            else
            {
                Cookie.RemoveCookie(_portalId, _cookieName);
                Cookie.RemoveCookie(_portalId, _cookieNameXml);
            }
            Exists = false;
        }

        private void ClearData()
        {
            Criteria = "";
            PageModuleId = "";
            PageNumber = "";
            PageName = "";
            OrderBy = "";
            XmlData = "";
            CategoryId = "";
        }

        /// <summary>
        /// Set to true if cookie exists
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Search Criteria, partial SQL String
        /// </summary>
        public string Criteria { get; set; }

        /// <summary>
        /// selected page
        /// </summary>
        public string PageNumber { get; set; }

        /// <summary>
        /// selected pagemid
        /// </summary>
        public string PageModuleId { get; set; }

        /// <summary>
        /// Page Name, used to return to page with correct page name 
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Save the sort order of the last required
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Save form xml data (this could be large, be careful on the cookie size)
        /// </summary>
        public string XmlData { get; set; }

        /// <summary>
        /// CategoryId Selected
        /// </summary>
        public string CategoryId { get; set; }

    }

}

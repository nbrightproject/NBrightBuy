using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Search.Entities;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;


namespace Nevoweb.DNN.NBrightBuy.Components
{

    public class NBrightBuyController : DataCtrlInterfaceNBrightBuy, IPortable
    {

        #region "NBrightBuy override DB Public Methods"

        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="itemId"></param>
        public override void Delete(int itemId)
        {
            DataProvider.Instance().Delete(itemId);
        }

        /// <summary>
        /// override for Database Function
        /// </summary>
        public override void CleanData()
        {
            DataProvider.Instance().CleanData();
        }

        /// <summary>
        /// This method return the data item with the lang node merged if the lang param is past.  NOTE: The typecodeLang Param is redundant
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="typeCodeLang">Redundant, set to ""</param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public override NBrightInfo Get(int itemId, string typeCodeLang = "", string lang = "")
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().Get(itemId, typeCodeLang, lang));
        }

        /// <summary>
        /// Return ONLY the data record
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public override NBrightInfo GetData(int itemId)
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().GetData(itemId));
        }
        /// <summary>
        /// Returns only the LANG data record, by using no language recored itemid (parentitemid)
        /// </summary>
        /// <param name="parentItemId"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public override NBrightInfo GetDataLang(int parentItemId, string lang)
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().GetDataLang(parentItemId, lang));
        }


        public override List<NBrightInfo> GetListCustom(int portalId, int moduleId, string SPROCname, int pageNumber = 0, string lang = "", string extraParam = "")
        {
            return CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetListCustom(portalId, moduleId, SPROCname, pageNumber, lang, extraParam));
        }

        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="typeCode"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <param name="sqlOrderBy"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="typeCodeLang"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public override List<NBrightInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string typeCodeLang = "", string lang = "")
        {
            return CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(portalId, moduleId, typeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, typeCodeLang, lang));
        }

	    /// <summary>
	    /// override for Database Function
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="typeCode"></param>
	    /// <param name="sqlSearchFilter"></param>
	    /// <param name="typeCodeLang"></param>
	    /// <param name="lang"></param>
	    /// <returns></returns>
	    public override int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "")
        {
            return DataProvider.Instance().GetListCount(portalId, moduleId, typeCode, sqlSearchFilter, typeCodeLang, lang);
        }

        /// <summary>
        /// Get DNN user list
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public override List<NBrightInfo> GetDnnUsers(int portalId, string sqlSearchFilter = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            return CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetDnnUsers(portalId, sqlSearchFilter, returnLimit, pageNumber, pageSize, recordCount));
        }

        /// <summary>
        /// Get DNN user list
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public override List<NBrightInfo> GetDnnUserProductClient(int portalId, int productid)
        {
            return CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetDnnUserProductClient(portalId, productid));
        }
        

        /// <summary>
        /// Get full record count for user search. (Needed for paging)
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <returns></returns>
        public override int GetDnnUsersCount(int portalId, string sqlSearchFilter = "")
        {
            return DataProvider.Instance().GetDnnUsersCount(portalId, sqlSearchFilter);
        }

        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="objInfo"></param>
        /// <returns></returns>
        public override int Update(NBrightInfo objInfo)
        {
            // clear any cache data that might be there
            var strCacheKey = "GetByGudKey*" + objInfo.ModuleId.ToString("") + "*" + objInfo.PortalId.ToString("") + "*" + objInfo.TypeCode + "*" + objInfo.UserId + "*" + objInfo.GUIDKey;
            Utils.RemoveCache(strCacheKey);
            strCacheKey = "GetByType*" + objInfo.ModuleId.ToString("") + "*" + objInfo.PortalId.ToString("") + "*" + objInfo.TypeCode + "*" + objInfo.UserId + "*" + objInfo.Lang;
            Utils.RemoveCache(strCacheKey);

            // do update
            objInfo.ModifiedDate = DateTime.Now;
            return DataProvider.Instance().Update(objInfo.ItemID, objInfo.PortalId, objInfo.ModuleId, objInfo.TypeCode, objInfo.XMLData, objInfo.GUIDKey, objInfo.ModifiedDate, objInfo.TextData, objInfo.XrefItemId, objInfo.ParentItemId, objInfo.UserId, objInfo.Lang);
        }

        /// <summary>
        /// Gte a single record from the Database using the EntityTypeCode.  This is usually used to fetch settings data "SETTINGS", where only 1 record will exist for the module.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="selUserId"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public NBrightInfo GetByType(int portalId, int moduleId, string entityTypeCode, string selUserId = "", string entityTypeCodeLang = "", string lang = "")
        {
            var strCacheKey = "GetByType*" + moduleId.ToString("") + "*" + portalId.ToString("") + "*" + entityTypeCode + "*" + selUserId + "*" + lang;
            var obj = (NBrightInfo)Utils.GetCache(strCacheKey);
            if (obj != null && StoreSettings.Current.DebugMode == false) return obj;

            var strFilter = "";
            if (selUserId != "")
            {
                strFilter += " and UserId = " + selUserId + " ";
            }

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1, 1, 1, 1, entityTypeCodeLang, lang));
            if (l.Count >= 1)
            {
                Utils.SetCache(strCacheKey, l[0]);
                return l[0];
            }
            return null;
        }

        /// <summary>
        /// Get a single record back from the database, using the guyidkey (The seluserid is used to confirm the correct user.)
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="guidKey"></param>
        /// <param name="selUserId"></param>
        /// <returns></returns>
        public NBrightInfo GetByGuidKey(int portalId, int moduleId, string entityTypeCode, string guidKey, string selUserId = "")
        {

            var strCacheKey = "GetByGudKey*" + moduleId.ToString("") + "*" + portalId.ToString("") + "*" + entityTypeCode + "*" + selUserId + "*" + guidKey;
            var obj = (NBrightInfo)Utils.GetCache(strCacheKey);
            if (obj != null) return obj;
            
            var strFilter = " and GUIDKey = '" + guidKey + "' ";
            if (selUserId != "")
            {
                strFilter += " and UserId = " + selUserId + " ";
            }

            var l = GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1); // only return 1, not a real guidkey, so m,ay be duplicates.
            if (l.Count == 0) return null;
            Utils.SetCache(strCacheKey, l[0]);
            return l[0];
        }

        /// <summary>
        /// This method return the data item with the lang node merged if the lang param is past. (with cache option)  NOTE: The typecodeLang Param is redundant
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="typeCodeLang">Redundant, set to ""</param>
        /// <param name="lang"></param>
        /// <param name="debugMode"></param>
        /// <returns></returns>
        public NBrightInfo GetData(int itemId, string typeCodeLang, string lang = "",bool debugMode = false)
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            // get cache data
            var strCacheKey = itemId.ToString("") + "*" + typeCodeLang + "*" + "*" + lang;
            NBrightInfo rtnInfo = null;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtnInfo = (NBrightInfo)obj;
            }

            if (rtnInfo == null)
            {
                rtnInfo = CBO.FillObject<NBrightInfo>(DataProvider.Instance().Get(itemId, typeCodeLang, lang)); 
                if (debugMode == false) Utils.SetCache(strCacheKey, rtnInfo);
            }
            return rtnInfo;
        }

        /// <summary>
        /// get XML data using SQL command text.
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public string GetSqlxml(string commandText)
        {
            return DataProvider.Instance().GetSqlxml(commandText);
        }

        /// <summary>
        /// exec sql
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public string ExecSql(string commandText)
        {
            return DataProvider.Instance().ExecSql(commandText);
        }


        /* *********************  list Data Gets ********************** */

	    /// <summary>
	    /// Get data list count with caching
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="typeCode"></param>
	    /// <param name="sqlSearchFilter"></param>
	    /// <param name="typeCodeLang"></param>
	    /// <param name="lang"></param>
	    /// <param name="debugMode"></param>
	    /// <param name="visibleOnly"> </param>
	    /// <returns></returns>
	    public int GetDataListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "", Boolean debugMode = false, Boolean visibleOnly = true)
        {
            // get cache data
            var strCacheKey = portalId.ToString("") + "*" + moduleId.ToString("") + "*" + typeCode + "*" + "*filter:" + sqlSearchFilter.Replace(" ", "") + "*" + lang + "*" + visibleOnly.ToString(CultureInfo.InvariantCulture);
            var rtncount = -1;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtncount = (int)obj;
            }

            if (rtncount == -1)
            {
                if (visibleOnly) sqlSearchFilter += " and (NB3.Visible = 1) ";
                rtncount = DataProvider.Instance().GetListCount(portalId, moduleId, typeCode, sqlSearchFilter, typeCodeLang, lang);
                if (debugMode == false) NBrightBuyUtils.SetModCache(moduleId, strCacheKey, rtncount);
            }
            return rtncount;
        }

        /// <summary>
        /// Data Get, used to call the Database provider and applies caching. Plus the option of taking filter and order information from the meta fields of the repeater template 
        /// </summary>
        /// <param name="rp1"></param>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="cultureCode"></param>
        /// <param name="debugMode"></param>
        /// <param name="selUserId"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<NBrightInfo> GetDataList(Repeater rp1, int portalId, int moduleId, string entityTypeCode, string entityTypeCodeLang, string cultureCode, bool debugMode = false, string selUserId = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            var strFilters = GenXmlFunctions.GetSqlSearchFilters(rp1);
            var strOrderBy = GenXmlFunctions.GetSqlOrderBy(rp1);
            //Default orderby if not set
            if (String.IsNullOrEmpty(strOrderBy)) strOrderBy = " Order by ModifiedDate DESC ";
            return GetDataList(portalId, moduleId, entityTypeCode, entityTypeCodeLang, Utils.GetCurrentCulture(), strFilters, strOrderBy, debugMode, selUserId, returnLimit, pageNumber, pageSize, recordCount);
        }


	    /// <summary>
	    /// Data Get, used to call the Database provider and applies caching. Plus the option of adding user to the filter.
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="entityTypeCode"></param>
	    /// <param name="entityTypeCodeLang"></param>
	    /// <param name="cultureCode"></param>
	    /// <param name="strFilters"></param>
	    /// <param name="strOrderBy"></param>
	    /// <param name="debugMode"></param>
	    /// <param name="selUserId"></param>
	    /// <param name="returnLimit"></param>
	    /// <param name="pageNumber"></param>
	    /// <param name="pageSize"></param>
	    /// <param name="recordCount"></param>
	    /// <returns></returns>
	    public List<NBrightInfo> GetDataList(int portalId, int moduleId, string entityTypeCode, string entityTypeCodeLang, string cultureCode, string strFilters, string strOrderBy, bool debugMode = false, string selUserId = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            if (selUserId != "")
            {
                strFilters += " and UserId = " + selUserId + " ";
            }

            List<NBrightInfo> l = null;

            // get cache template 
            var strCacheKey = portalId.ToString("") + "*" + moduleId.ToString("") + "*" + entityTypeCode + "*" + "*filter:" + strFilters.Replace(" ", "") + "*orderby:" + strOrderBy.Replace(" ", "") + "*" + returnLimit.ToString("") + "*" + pageNumber.ToString("") + "*" + pageSize.ToString("") + "*" + recordCount.ToString("") + "*" + entityTypeCodeLang + "*" + Utils.GetCurrentCulture();
            if (debugMode == false)
            {
                l = (List<NBrightInfo>)Utils.GetCache(strCacheKey);
            }

            if (l == null)
            {
                l = GetList(portalId, moduleId, entityTypeCode, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, entityTypeCodeLang, cultureCode);
                //add rowcount, so we can use databind RowCount in the templates
                foreach (var i in l)
                {
                    i.RowCount = l.IndexOf(i) + 1;
                }
                if (debugMode == false) NBrightBuyUtils.SetModCache(moduleId, strCacheKey, l);
            }
            return l;
        }

        /// <summary>
        /// Build cachekey for razor, so we get the same results from cache for DB and razor.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="cultureCode"></param>
        /// <param name="strFilters"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="debugMode"></param>
        /// <param name="selUserId"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public String GetDataListCacheKey(int portalId, int moduleId, string entityTypeCode, string entityTypeCodeLang, string cultureCode, string strFilters, string strOrderBy, bool debugMode = false, string selUserId = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            if (selUserId != "") strFilters += " and UserId = " + selUserId + " ";
            return portalId.ToString("") + "*" + moduleId.ToString("") + "*" + entityTypeCode + "*" + "*filter:" + strFilters.Replace(" ", "") + "*orderby:" + strOrderBy.Replace(" ", "") + "*" + returnLimit.ToString("") + "*" + pageNumber.ToString("") + "*" + pageSize.ToString("") + "*" + recordCount.ToString("") + "*" + entityTypeCodeLang + "*" + Utils.GetCurrentCulture();
        }

        #endregion

        #region "NBrightBuy Control functions"

        public string GetTemplateData(ModSettings modSettings, string templatename, string lang, bool debugMode = false)
	    {
	        return GetTemplateData(modSettings.Moduleid, templatename, lang, modSettings.Settings(), debugMode);
	    }

	    public string GetTemplateData(int moduleId, string templatename, string lang, Dictionary<string,string> settings, bool debugMode = false)
	    {
	        if (lang == "") lang = Utils.GetCurrentCulture();
            string templ = null;
            var strCacheKey = templatename + "*" + moduleId.ToString("") + "*" + lang + "*" + PortalSettings.Current.PortalId.ToString("");
            
            if (debugMode == false) templ = (String)Utils.GetCache(strCacheKey);

            if (templ == null)
            {
                var themeFolder = "";
                if (settings != null && settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                templ = templCtrl.GetTemplateData(templatename, lang, true, true, true, settings);

                // WARNING!! do not inject text here, it will cause a loop on the GetMenuTemplates function.

                if (debugMode == false) NBrightBuyUtils.SetModCache(-1, strCacheKey, templ);
            }
            // always replace url tokens after cache, they could be different per url, not by cache key
            templ = Utils.ReplaceUrlTokens(templ);

            return templ;
        }

        public string GetTemplate(string templatename, string lang, string themeFolder, bool debugMode = false)
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            string templ = null;
            var strCacheKey = templatename + "*" + lang + "*" + PortalSettings.Current.PortalId.ToString("");

            if (debugMode == false) templ = (String)Utils.GetCache(strCacheKey);

            if (templ == null)
            {
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                templ = templCtrl.GetTemplateData(templatename, lang, true, true, true, StoreSettings.Current.Settings());

                if (debugMode == false) NBrightBuyUtils.SetModCache(-1, strCacheKey, templ);
            }
            // always replace url tokens after cache, they could be different per url, not by cache key
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }



	    /// <summary>
        /// Ouputs the module data in XML fomrat 
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="strFilters"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="lang"> </param>
        /// <param name="context">Allows the context data to be included in the xml output</param>
        /// <returns></returns>
        public String GetXml(int portalId, int moduleId, string entityTypeCode, string strFilters, string strOrderBy, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string entityTypeCodeLang = "", string lang = "", HttpContext context = null)
        {
            var strXml = "<root>";

            if (context != null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    strXml += "<current>";
                    foreach (var paramName in context.Request.QueryString.AllKeys)
                    {
                        strXml += "<" + paramName.ToLower() + ">";
                        strXml += context.Request.QueryString[paramName];
                        strXml += "</" + paramName.ToLower() + ">";
                    }
                    strXml += "<lang>";
                    strXml += lang;
                    strXml += "</lang>";
                    strXml += "</current>";
                }
            }

            var objS = GetByType(portalId, moduleId, "SETTINGS");
            if (objS != null)
            {
                strXml += "<module key = \"" + objS.GetXmlProperty("genxml/textbox/txtmodulekey") + "\">";
                strXml += "<settings portalid=\"" + objS.PortalId.ToString("") + "\" moduleid=\"" + objS.ModuleId.ToString("") + "\" now=\"" + DateTime.Now.ToString("s") + "\"  >";
                strXml += objS.XMLData;
                strXml += "</settings>";
            }
            else
            {
                strXml += "<module><settings><msg>NO SETTINGS FOUND</msg></settings>";
            }
            strXml += "<select>";
            strXml += "<entityTypeCode>" + entityTypeCode + "</entityTypeCode>";
            strXml += "<lang>" + lang + "</lang>";
            strXml += "<entityTypeCodeLang>" + entityTypeCodeLang + "</entityTypeCodeLang>";
            strXml += "<strFilters>" + strFilters + "</strFilters>";
            strXml += "<strOrderBy>" + strOrderBy + "</strOrderBy>";
            strXml += "<returnLimit>" + lang + "</returnLimit>";
            strXml += "<pageNumber>" + lang + "</pageNumber>";
            strXml += "<pageSize>" + lang + "</pageSize>";
            strXml += "<recordCount>" + recordCount + "</recordCount>";
            strXml += "</select>";
            var l = GetList(portalId, moduleId, entityTypeCode, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, entityTypeCodeLang, lang);
            strXml += NBrightBuyUtils.FormatListtoXml(l);
            strXml += "</module>";
            strXml += "</root>";
            return strXml;
        }

        /// <summary>
        /// Update any empty language fields to the same as the base language data
        /// </summary>
        /// <param name="baseParentItemId">Itemid of the data record (ParentItemId of hte base language record)</param>
        /// <param name="baseLang">Base Langauge culture code</param>
        /// <param name="extraXmlNodes">List of extra nodes to be updated, this is for custom data. NBS defaults are already included. (imgs,docs,options,optionvalues,models)</param>
        public void FillEmptyLanguageFields(int baseParentItemId, String baseLang, List<String> extraXmlNodes = null)
        {
            var baseInfo = GetDataLang(baseParentItemId, baseLang);
            if (baseInfo != null)
            {
                foreach (var toLang in DnnUtils.GetCultureCodeList(baseInfo.PortalId))
                {
                    if (toLang != baseInfo.Lang)
                    {
                        var dlang = GetDataLang(baseParentItemId, toLang);
                        if (dlang != null)
                        {
                            var nodList = baseInfo.XMLDoc.SelectNodes("genxml/textbox/*");
                            if (nodList != null)
                            {
                                foreach (XmlNode nod in nodList)
                                {
                                    if (nod.InnerText.Trim() != "")
                                    {
                                        if (dlang.GetXmlProperty("genxml/textbox/" + nod.Name) == "")
                                        {
                                            dlang.SetXmlProperty("genxml/textbox/" + nod.Name, nod.InnerText);
                                        }
                                    }
                                }
                            }

                            dlang = UpdateLangNodeFields("imgs",baseInfo,dlang);
                            dlang = UpdateLangNodeFields("docs", baseInfo, dlang);
                            dlang = UpdateLangNodeFields("options", baseInfo, dlang);
                            dlang = UpdateLangNodeFields("optionvalues", baseInfo, dlang);
                            dlang = UpdateLangNodeFields("models", baseInfo, dlang);

                            if (extraXmlNodes != null)
                            {
                                foreach (var xmlname in extraXmlNodes)
                                {
                                    dlang = UpdateLangNodeFields(xmlname, baseInfo, dlang);
                                }
                            }

                            Update(dlang);
                        }
                    }
                }
            }
        }


        #endregion

            #region "static methods"

        private static NBrightInfo UpdateLangNodeFields(String xmlname, NBrightInfo baseInfo, NBrightInfo dlang)
        {
            var nodList3I = baseInfo.XMLDoc.SelectNodes("genxml/" + xmlname + "/genxml");
            if (nodList3I != null)
            {
                for (int i = 1; i <= nodList3I.Count; i++)
                {
                    var nodList3 = baseInfo.XMLDoc.SelectNodes("genxml/" + xmlname + "/genxml[" + i + "]/textbox/*");
                    if (nodList3 != null)
                    {
                        foreach (XmlNode nod in nodList3)
                        {
                            if (nod.InnerText.Trim() != "")
                            {
                                if (dlang.GetXmlProperty("genxml/" + xmlname + "/genxml[" + i + "]/textbox/" + nod.Name) == "")
                                {
                                    if (dlang.XMLDoc.SelectSingleNode("genxml/" + xmlname + "/genxml[" + i + "]") == null)
                                    {
                                        var baseXml = baseInfo.XMLDoc.SelectSingleNode("genxml/" + xmlname + "/genxml[" + i + "]");
                                        if (baseXml != null)
                                        {
                                            if (dlang.XMLDoc.SelectSingleNode("genxml/" + xmlname) == null)
                                            {
                                                dlang.AddSingleNode(xmlname, "", "genxml");
                                            }
                                            dlang.AddXmlNode(baseXml.OuterXml, "genxml", "genxml/" + xmlname);
                                        }
                                    }

                                    dlang.SetXmlProperty("genxml/" + xmlname + "/genxml[" + i + "]/textbox/" + nod.Name, nod.InnerText);
                                }
                            }
                        }
                    }
                }
            }
            return dlang;
        }

        /// <summary>
            /// Get current portal StoreSettings
            /// </summary>
            /// <returns></returns>
        public static StoreSettings GetCurrentPortalData()
	    {
            StoreSettings objPortalSettings = null;
            if (HttpContext.Current != null)
            {
                // build StoreSettings and place in httpcontext
                if (HttpContext.Current.Items["NBBStoreSettings" + PortalSettings.Current.PortalId.ToString("")] == null)
                {
                    HttpContext.Current.Items.Add("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""), GetStaticStoreSettings(PortalSettings.Current.PortalId));
                }
                objPortalSettings = (StoreSettings)HttpContext.Current.Items["NBBStoreSettings" + PortalSettings.Current.PortalId.ToString("")];
            }
            return objPortalSettings;
	    }

        /// <summary>
        /// Cache the current store settings
        /// </summary>
        /// <returns></returns>
        private static StoreSettings GetStaticStoreSettings(int portalId)
        {
            var objSs = (StoreSettings)Utils.GetCache("NBBStoreSettings" + portalId.ToString(""));
            if (objSs == null)
            {
                objSs = new StoreSettings(portalId);
                Utils.SetCache("NBBStoreSettings" + portalId.ToString(""), objSs);
            }
            return objSs;
        }
        public StoreSettings GetStoreSettings(int portalId)
        {
            var objSs = (StoreSettings)Utils.GetCache("NBBStoreSettings" + portalId.ToString(""));
            if (objSs == null)
            {
                objSs = new StoreSettings(portalId);
                Utils.SetCache("NBBStoreSettings" + portalId.ToString(""), objSs);
            }
            return objSs;
        }

        #endregion


        #region Optional Interfaces

        #region IPortable Members

        /// -----------------------------------------------------------------------------
		/// <summary>
		///   ExportModule implements the IPortable ExportModule Interface
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name = "moduleId">The Id of the module to be exported</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------
		public string ExportModule(int ModuleId)
		{
			var objModCtrl = new ModuleController();
			var xmlOut = "";

			var objModInfo = objModCtrl.GetModule(ModuleId);

			if (objModInfo != null)
			{
				var portalId = objModInfo.PortalID;
                var moduleSettings = NBrightBuyUtils.GetSettings(portalId, ModuleId, "", false);

			    xmlOut += moduleSettings.ToXmlItem();
			}

			return xmlOut;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		///   ImportModule implements the IPortable ImportModule Interface
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name = "ModuleID">The ID of the Module being imported</param>
		/// <param name = "Content">The Content being imported</param>
		/// <param name = "Version">The Version of the Module Content being imported</param>
		/// <param name = "UserId">The UserID of the User importing the Content</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------

		public void ImportModule(int ModuleID, string Content, string Version, int UserId)
		{
			var xmlDoc = new XmlDocument();
			var objModCtrl = new ModuleController();
			var objModInfo = objModCtrl.GetModule(ModuleID);
			if (objModInfo != null)
			{
			    var objImp = new NBrightInfo(false);
                objImp.FromXmlItem(Content);
			    objImp.ModuleId = ModuleID;
			    objImp.PortalId = objModInfo.PortalID;
			    objImp.ItemID = -1; // create new record

                //delete the old setting record.
                var moduleSettings = NBrightBuyUtils.GetSettings(objModInfo.PortalID, ModuleID, "", false);
                if (moduleSettings.ItemID > -1) Delete(moduleSettings.ItemID);

			    Update(objImp);

                NBrightBuyUtils.RemoveModCache(ModuleID);

			}

		}

        #endregion


		#endregion

	}

}

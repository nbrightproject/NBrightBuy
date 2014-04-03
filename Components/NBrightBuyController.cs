using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.DNN.NBrightBuy.Components
{

	public class NBrightBuyController : DataCtrlInterface, IPortable, ISearchable
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
        /// override for Database Function
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="typeCodeLang"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public override NBrightInfo Get(int itemId, string typeCodeLang = "", string lang = "")
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().Get(itemId, typeCodeLang, lang));
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
        /// override for Database Function
        /// </summary>
        /// <param name="objInfo"></param>
        /// <returns></returns>
        public override int Update(NBrightInfo objInfo)
        {
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
            var strFilter = "";
            if (selUserId != "")
            {
                strFilter += " and UserId = " + selUserId + " ";
            }

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1, 1, 1, 1, entityTypeCodeLang, lang));
            if (l.Count >= 1)
            {
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
            var strFilter = " and GUIDKey = '" + guidKey + "' ";
            if (selUserId != "")
            {
                strFilter += " and UserId = " + selUserId + " ";
            }

            var l = GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1);
            if (l.Count == 0) return null;
            if (l.Count > 1)
            {
                for (int i = 1; i < l.Count; i++)
                {
                    // remove invalid DB entries
                    Delete(l[i].ItemID);
                }
            }
            return l[0];
        }


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
        /// <returns></returns>
        public int GetDataListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "", Boolean debugMode = false)
        {
            // get cache data
            var strCacheKey = portalId.ToString("") + "*" + moduleId.ToString("") + "*" + typeCode + "*" + "*filter:" + sqlSearchFilter.Replace(" ", "") + "*" + lang;
            var rtncount = -1;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtncount = (int)obj;
            }

            if (rtncount == -1)
            {
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

        #endregion

        #region "NBrightBuy Control functions"

	    public string GetTemplateData(ModSettings modSettings, string templatename, string lang, bool debugMode = false)
	    {
	        return GetTemplateData(modSettings.Moduleid, templatename, lang, modSettings.Settings(), debugMode);
	    }

	    public string GetTemplateData(int moduleId, string templatename, string lang, Dictionary<string,string> settings, bool debugMode = false)
        {
            string templ = null;
            var strCacheKey = templatename + "*" + moduleId.ToString("") + "*" + lang + "*" + PortalSettings.Current.PortalId.ToString("");
            
            if (debugMode == false) templ = (String)Utils.GetCache(strCacheKey);

            if (templ == null)
            {
                var themeFolder = "";
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());

                templ = Utils.ReplaceSettingTokens(templ, settings);
                templ = Utils.ReplaceUrlTokens(templ);
                if (debugMode == false) NBrightBuyUtils.SetModCache(-1, strCacheKey, templ);
            }
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

        #endregion

        #region "static methods"

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
                if (HttpContext.Current.Items["NBBStoreSettings"] == null)
                {
                    HttpContext.Current.Items.Add("NBBStoreSettings", GetStoreSettings());
                }
                objPortalSettings = (StoreSettings)HttpContext.Current.Items["NBBStoreSettings"];
            }
            return objPortalSettings;
	    }

        /// <summary>
        /// Cache the current store settings
        /// </summary>
        /// <returns></returns>
        private static StoreSettings GetStoreSettings()
        {
            var objSs = (StoreSettings)Utils.GetCache("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""));
            if (objSs == null)
            {
                objSs = new StoreSettings();
                Utils.SetCache("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""), objSs);
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

				xmlOut += "<root>";

				xmlOut += "</root>";
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
			var xmlDoc = new XmlDataDocument();
			var objModCtrl = new ModuleController();
			var objModInfo = objModCtrl.GetModule(ModuleID);
			if (objModInfo != null)
			{

				xmlDoc.LoadXml(Content);

			}

		}

		#endregion


		#region ISearchable Members

		/// -----------------------------------------------------------------------------
		/// <summary>
		///   GetSearchItems implements the ISearchable Interface
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name = "ModInfo">The ModuleInfo for the module to be Indexed</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------
		public SearchItemInfoCollection GetSearchItems(ModuleInfo ModInfo)
		{
			var searchItemCollection = new SearchItemInfoCollection();
			return searchItemCollection;
		}

		#endregion


		#endregion

	}

}

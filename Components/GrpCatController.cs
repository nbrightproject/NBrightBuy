using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class GrpCatController
    {

        public List<GroupCategoryData> GrpCategoryList;
        public List<GroupCategoryData> CategoryList;

        public GrpCatController(String lang)
        {
            // build group category list
            var strCacheKey = "NBS_GrpCategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
            GrpCategoryList = (List<GroupCategoryData>)NBrightBuyUtils.GetModCache(strCacheKey);
            if (GrpCategoryList == null)
            {
                GrpCategoryList = GetGrpCatListFromDatabase(lang);
                NBrightBuyUtils.SetModCache(-1, strCacheKey,GrpCategoryList);
            }

            // build cateogry list for navigation from group category list
            strCacheKey = "NBS_CategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
            CategoryList = (List<GroupCategoryData>)NBrightBuyUtils.GetModCache(strCacheKey);
            if (CategoryList == null)
            {
                var lenum = from i in GrpCategoryList where i.grouptyperef == "cat" select i;
                CategoryList = lenum.ToList();
                NBrightBuyUtils.SetModCache(-1, strCacheKey, CategoryList);
            }

        }

        #region "base methods"


        public GroupCategoryData GetGrpCategory(int categoryid)
        {
            var lenum = from i in GrpCategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        public GroupCategoryData GetCategory(int categoryid)
        {
            var lenum = from i in CategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        public List<GroupCategoryData> GetGrpCategories(int parentcategoryid,string groupref = "")
        {
                var lenum = from i in GrpCategoryList where i.parentcatid == parentcategoryid select i;
                if (groupref != "")
                {
                    var lenum2 = from i2 in lenum where i2.grouptyperef == groupref select i2;
                    return lenum2.ToList();
                }
                return lenum.ToList();                
        }

        public List<GroupCategoryData> GetCategories(int parentcategoryid)
        {
            var lenum = from i in CategoryList where i.parentcatid == parentcategoryid select i;
            var l = lenum.ToList();
            return l;
        }

        public List<GroupCategoryData> GetCategoriesWithUrl(int parentcategoryid, int tabid)
        {
            var l = GetCategories(parentcategoryid);
            foreach (var c in l)
            {
                c.url = GetCategoryUrl(c, tabid);
            }
            return l;
        }

        public GroupCategoryData GetGrpCategoryByRef(string categoryref)
        {
            var lenum = from i in GrpCategoryList where i.categoryref == categoryref select i;
            var l = lenum.ToList();
            if (l.Count == 0) return null;
            return l[0];
        }

        #endregion

        #region "Special methods"


        public string GetCategoryUrl(GroupCategoryData groupCategoryInfo, int tabid)
        {

            //set a default url
            var url = "?catid=" + groupCategoryInfo.categoryid.ToString("");
            // get friendly url if possible
                if (groupCategoryInfo.categoryname != "")
                {
                    var newBaseName = groupCategoryInfo.seoname;
                    if (newBaseName == "") newBaseName = groupCategoryInfo.categoryname;
                    var tab = CBO.FillObject<DotNetNuke.Entities.Tabs.TabInfo>(DotNetNuke.Data.DataProvider.Instance().GetTab(tabid));
                    if (tab != null)
                    {
                        url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(tab, "~/Default.aspx?TabId=" + tab.TabID.ToString("") + "&catid=" + groupCategoryInfo.categoryid.ToString(""), newBaseName.Replace(" ", "-") + ".aspx");
                        url = url.Replace("[catid]/", ""); // remove the injection token from the url, if still there. (Should be removed redirected to new page)
                    }
                }
            return url;
        }

        /// <summary>
        /// Get the default category for the product 
        /// </summary>
        /// <param name="productid"></param>
        /// <returns></returns>
        public int GetDefaultCatId(int productid)
        {
            var defId = 0;
            var objCtrl = new NBrightBuyController();
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
            var strFilter = " and NB1.parentitemid = " + productid.ToString("") + " ";
            var objInfo = objCtrl.Get(productid);

            if (objInfo != null)
            {

                var l = objCtrl.GetList(objInfo.PortalId, objInfo.ModuleId, "CATXREF", strFilter);

                var catIds = new HashSet<int>(CategoryList.Select(x => x.categoryid));
                l.RemoveAll(x => !catIds.Contains(x.XrefItemId));


                foreach (var e in l)
                {
                    if (e.GetXmlProperty("genxml/hidden/defaultcat").ToLower() == "true")
                    {
                        defId = e.XrefItemId;
                        break;
                    }
                }

                if (defId == 0)
                {
                    foreach (var e in l)
                    {
                        if ((e.GetXmlProperty("genxml/checkbox/chkishidden").ToLower() == "false") && (e.GetXmlProperty("genxml/checkbox/chkarchived").ToLower() == "false"))
                        {
                            defId = e.XrefItemId;
                            break;
                        }
                    }
                }


                if (defId == 0)
                {
                    if (l.Count > 0)
                    {
                        defId = l[0].XrefItemId;
                    }
                }
            }

            return defId;
        }

        public GroupCategoryData GetCurrentCategoryData(int portalId, System.Web.HttpRequest request, int entryId = 0, Dictionary<string, string> settings = null, String targetModuleKey = "")
        {

            var defcatid = 0;
            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(entryId) && entryId > 0) defcatid = GetDefaultCatId(entryId);

            if (defcatid == 0 && Utils.IsNumeric(qrycatid)) defcatid = Convert.ToInt32(qrycatid);

            if (targetModuleKey != "")
            {
                var navigationdata = new NavigationData(portalId, targetModuleKey, StoreSettings.Current.Get("DataStorageType"));
                if (Utils.IsNumeric(navigationdata.CategoryId) && navigationdata.FilterMode) defcatid = Convert.ToInt32(navigationdata.CategoryId);
            }

            if (defcatid == 0)
            {
                if (settings != null && settings["defaultcatid"] != null)
                {
                    var setcatid = settings["defaultcatid"];
                    if (Utils.IsNumeric(setcatid)) defcatid = Convert.ToInt32(setcatid);
                }

                if (Utils.IsNumeric(portalId) && defcatid == 0)
                {
                    var nbSettings = NBrightBuyUtils.GetGlobalSettings(Convert.ToInt32(portalId));
                    var setcatid = nbSettings.GetXmlProperty("genxml/hidden/defaultcatid");
                    if (Utils.IsNumeric(setcatid)) defcatid = Convert.ToInt32(setcatid);
                }
            }

            return GetCategory(defcatid);
        }

        public NBrightInfo GetCurrentCategoryInfo(int portalId, System.Web.HttpRequest request, int entryId = 0)
        {

            var defcatid = 0;
            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(entryId) && entryId > 0) defcatid = GetDefaultCatId(entryId);

            if (defcatid == 0 && Utils.IsNumeric(qrycatid)) defcatid = Convert.ToInt32(qrycatid);

            if (defcatid == 0)
            {
                if (Utils.IsNumeric(portalId))
                {
                    var nbSettings = NBrightBuyUtils.GetGlobalSettings(Convert.ToInt32(portalId));
                    var setcatid = nbSettings.GetXmlProperty("genxml/hidden/defaultcatid");
                    if (Utils.IsNumeric(setcatid)) defcatid = Convert.ToInt32(setcatid);
                }
            }
            var objCtrl = new NBrightBuyController();
            return objCtrl.Get(defcatid);
        }

        public List<GroupCategoryData> GetTreeCategoryList(List<GroupCategoryData> rtnList, int level, int parentid, string groupref,string breadcrumbseparator)
        {
            if (level > 20) return rtnList; // stop infinate loop

            var levelList = GetGrpCategories(parentid, groupref);
            foreach (GroupCategoryData tInfo in levelList)
            {
                var nInfo = tInfo;
                nInfo.breadcrumb = GetBreadCrumb(nInfo.categoryid, 7, breadcrumbseparator,false);
                nInfo.depth = level;
                rtnList.Add(nInfo);
                GetTreeCategoryList(rtnList, level + 1, tInfo.categoryid, groupref, breadcrumbseparator);
            }

            return rtnList;
        }

        #endregion

        #region "breadcrumbs"

        public String GetBreadCrumb(int categoryid, int shortLength, string separator, bool aslist)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (aslist)
                            strOut = "<li>" + separator + crumbText + "</li>" + breadCrumb;
                        else
                            strOut = separator + crumbText + breadCrumb;

                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist) breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>";
                return breadCrumb;
            }
            return "";
        }

        public String GetBreadCrumbWithLinks(int categoryid, int tabId, int shortLength, string separator, bool aslist)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (aslist)
                            strOut = "<li>" + separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + "</li>" + breadCrumb;
                        else
                            strOut = separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + breadCrumb;
                        
                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist) breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>"; 
                return breadCrumb;
            }
            return "";
        }

        #endregion

        #region "private methods"

        private NBrightInfo GetLangData(List<NBrightInfo> langList,int categoryid)
        {
            var lenum = from i in langList where i.ParentItemId == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        private int GetEntryCount(List<NBrightInfo> xrefList, int categoryid)
        {
            var lenum = from i in xrefList where i.XrefItemId == categoryid select i;
            return lenum.Count();
        }


        private List<NBrightInfo> GetParentList(List<NBrightInfo> catList, int categoryid)
        {
            var rtnList = new List<NBrightInfo>();
            var startCat = from i in catList where i.ItemID == categoryid select i;
            if (startCat.Any())
            {
                categoryid = startCat.ToList()[0].ParentItemId;
                var c = 1;
                while (true)
                {
                    var l = from i in catList where i.ItemID == categoryid select i;
                    if (l.Any())
                    {
                        rtnList.Add(l.ToList()[0]);
                        categoryid = rtnList.Last().ParentItemId;
                    }
                    else
                        break;
                    c += 1;
                    if (c > 50) break; //stop possible infinate loop
                }
            }
            return rtnList;
        }

        private List<GroupCategoryData> GetGrpCatListFromDatabase(String lang = "")
        {

            var objCtrl = new NBrightBuyController();
            const string strOrderBy = " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','int') ";
            var grpcatList = new List<GroupCategoryData>();

            var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY", "", strOrderBy, 0, 0, 0, 0, "", "");
            var lg = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORYLANG", "and NB1.lang = '" + lang + "'", "", 0, 0, 0, 0, "", "");
            var lx = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATCASCADE", "", "", 0, 0, 0, 0, "", "");
            var lx2 = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", "", "", 0, 0, 0, 0, "", "");
            lx.AddRange(lx2);
            foreach (var i in l)
            {
                var grpcat = new GroupCategoryData();
                grpcat.categoryid = i.ItemID;
                grpcat.recordsortorder = i.GetXmlPropertyInt("genxml/hidden/recordsortorder");
                grpcat.imageurl = i.GetXmlProperty("genxml/hidden/imageurl");
                grpcat.categoryref = i.GetXmlProperty("genxml/textbox/txtcategoryref");
                grpcat.archived = i.GetXmlPropertyBool("genxml/checkbox/chkarchived");
                grpcat.ishidden = i.GetXmlPropertyBool("genxml/checkbox/chkishidden");
                grpcat.grouptyperef = i.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                grpcat.parentcatid = i.ParentItemId;
                grpcat.entrycount = GetEntryCount(lx, grpcat.categoryid);
                
                // get the language data
                var langItem =  GetLangData(lg,grpcat.categoryid);
                if (langItem != null)
                {
                    grpcat.categoryname = langItem.GetXmlProperty("genxml/textbox/txtcategoryname");
                    grpcat.categorydesc = langItem.GetXmlProperty("genxml/textbox/txtcategorydesc");
                    grpcat.seoname = langItem.GetXmlProperty("genxml/textbox/txtseoname");
                    grpcat.metadescription = langItem.GetXmlProperty("genxml/textbox/txtmetadescription");
                    grpcat.metakeywords = langItem.GetXmlProperty("genxml/textbox/txtmetakeywords");
                    grpcat.seopagetitle = langItem.GetXmlProperty("genxml/textbox/txtseopagetitle");
                }

                //get parents
                var p = GetParentList(l,grpcat.categoryid);
                foreach (var pi in p)
                    grpcat.Parents.Add(pi.ItemID);

                grpcatList.Add(grpcat);
            }

            return grpcatList;

        }

        #endregion

    }
}

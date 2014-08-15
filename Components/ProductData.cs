using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ProductData
    {
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;

        public List<NBrightInfo> Models;
        public List<NBrightInfo> Options;
        public List<NBrightInfo> Imgs;
        public List<NBrightInfo> Docs;

        private String _lang = ""; // needed for webservice

        /// <summary>
        /// Populate the ProductData in this class
        /// </summary>
        /// <param name="productId">productid</param>
        /// <param name="lang">langauge to populate</param>
        /// <param name="hydrateLists">populate the sub data into lists</param>
        public ProductData(int productId, String lang, Boolean hydrateLists = true)
        {
            _lang = lang;
            if (_lang == "") _lang = StoreSettings.Current.EditLanguage;
            LoadData(productId, hydrateLists);
        }

        #region "public functions/interface"

        /// <summary>
        /// Set to true if product exists
        /// </summary>
        public bool Exists { get; private set; }

        public String SEOName
        {
            get
            {
                var seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (seoname == "") seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");
                return seoname;                
            }
        }

        public String SEOTitle
        {
            get{return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");}
        }

        public String SEOTagwords
        {
            get{return Info.GetXmlProperty("genxml/lang/genxml/textbox/txttagwords");}
        }

        public String SEODescription
        {
            get{return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary");}
        }

        public List<NBrightInfo> GetOptionValues(int optionid)
        {
            var l = new List<NBrightInfo>();
            if (Info != null)
            {
                var xmlNodList = Info.XMLDoc.SelectNodes("genxml/optionvalues[@optionid='" + optionid.ToString("") + "']/*");
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    var lp = 1;
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        var nodLang = "<genxml>" + Info.GetXmlNode("genxml/lang/genxml/optionvalues[@optionid='" + optionid.ToString("") + "']/genxml[" + lp + "]") + "</genxml>";
                        if (nodLang != "")
                        {
                            obj.SetXmlProperty("genxml/hidden/productid", Info.ItemID.ToString(""));
                            obj.SetXmlProperty("genxml/hidden/lang", Info.Lang);
                            obj.SetXmlProperty("genxml/hidden/optionid", optionid.ToString(""));
                            obj.AddSingleNode("lang", "", "genxml");
                            obj.AddXmlNode(nodLang, "genxml", "genxml/lang");
                        }
                        obj.ParentItemId = Info.ItemID;
                        l.Add(obj);
                        lp += 1;
                    }
                }
            }
            return l;
        }

        public List<GroupCategoryData> GetCategories()
        {
            var objGrpCtrl = new GrpCatController(_lang);
            return objGrpCtrl.GetProductCategories(Info.ItemID);
        }

        public List<NBrightInfo> GetRelatedProducts()
        {
            var objCtrl = new NBrightBuyController();
            var strSelectedIds = "";
            var arylist = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRDXREF"," and NB1.parentitemid = " + Info.ItemID.ToString(""));
            foreach (var obj in arylist)
            {
                strSelectedIds += obj.XrefItemId.ToString("") + ",";
            }
            var relList = new List<NBrightInfo>();
            if (strSelectedIds.TrimEnd(',') != "")
            {
                var strFilter = " and NB1.[ItemId] in (" + strSelectedIds.TrimEnd(',') + ") ";
                relList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "PRD", "PRDLANG", _lang, strFilter, "");
            }
            return relList;
        }

        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);
            ResetCache();
        }

        public void ClearCache()
        {
            var cacheKey = "NBSProductData*" + Info.ItemID.ToString("") + "*" + _lang;
            Utils.RemoveCache(cacheKey); 
        }

        public void ResetCache()
        {
            LoadData(Info.ItemID);
            var cacheKey = "NBSProductData*" + Info.ItemID.ToString("") + "*" + _lang;
            Utils.SetCache(cacheKey,this);
        }

        public void Update(String xmlData)
        {
            var info = new NBrightInfo();
            info.ItemID = -1;
            info.TypeCode = "UPDATEDATA";
            info.XMLData = xmlData;

            var localfields = info.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
            foreach (var f in localfields)
            {
                if (f != "")
                {
                    if (f == "genxml/edt/description")
                    {
                        // special processing for editor, to place code in standard place.
                        if (DataLangRecord.XMLDoc.SelectSingleNode("genxml/edt") == null) DataLangRecord.AddSingleNode("edt", "", "genxml");
                        if (info.GetXmlProperty("genxml/textbox/description") == "")
                            DataLangRecord.SetXmlProperty(f, info.GetXmlProperty("genxml/edt/description"));
                        else
                            DataLangRecord.SetXmlProperty(f, info.GetXmlProperty("genxml/textbox/description")); // ajax on ckeditor (Ajax doesn't work for telrik)
                    }
                    else
                        DataLangRecord.SetXmlProperty(f, info.GetXmlProperty(f));

                    DataRecord.RemoveXmlNode(f);                    
                }
            }

            var fields = info.GetXmlProperty("genxml/hidden/fields").Split(',');
            foreach (var f in fields)
            {
                if (f != "")
                {
                    DataRecord.SetXmlProperty(f, info.GetXmlProperty(f));
                    // if we have a image field then we need to create the imageurl field
                    if (info.GetXmlProperty(f.Replace("textbox/", "hidden/hidinfo")) == "Img=True")
                        DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "url", StoreSettings.Current.FolderImages + "/" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));

                    DataLangRecord.RemoveXmlNode(f);
                }
            }

            // update Models
            var strXml = info.GetXmlProperty("genxml/hidden/xmlupdatemodeldata");
            strXml = GenXmlFunctions.DecodeCDataTag(strXml);
            UpdateModels(strXml);
            // update Options
            strXml = info.GetXmlProperty("genxml/hidden/xmlupdateproductoptions");
            strXml = GenXmlFunctions.DecodeCDataTag(strXml);
            UpdateOptions(strXml);
            // update Options
            strXml = info.GetXmlProperty("genxml/hidden/xmlupdateproductoptionvalues");
            strXml = GenXmlFunctions.DecodeCDataTag(strXml);
            UpdateOptionValues(strXml);
            

        }

        public void UpdateModels(String xmlData)
        {
            var modelList = NBrightBuyUtils.GetGenXmlListByAjax(xmlData, "");

            // build xml for data records
            var strXml = "<genxml><models>";
            var strXmlLang = "<genxml><models>";
            foreach (var modelInfo in modelList)
            {
                var objInfo = new NBrightInfo(true);
                var objInfoLang = new NBrightInfo(true);

                var localfields = modelInfo.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
                foreach (var f in localfields.Where(f => f != ""))
                {
                    objInfoLang.SetXmlProperty(f,modelInfo.GetXmlProperty(f));
                }
                strXmlLang += objInfoLang.XMLData;

                var fields = modelInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    objInfo.SetXmlProperty(f, modelInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</models></genxml>";
            strXmlLang += "</models></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/models", "genxml");
            DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/models", "genxml");
        }

        public void UpdateOptions(String xmlData)
        {
            var objList = NBrightBuyUtils.GetGenXmlListByAjax(xmlData, "");

            // build xml for data records
            var strXml = "<genxml><options>";
            var strXmlLang = "<genxml><options>";
            foreach (var objDataInfo in objList)
            {
                var objInfo = new NBrightInfo(true);
                var objInfoLang = new NBrightInfo(true);

                var localfields = objDataInfo.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
                foreach (var f in localfields.Where(f => f != ""))
                {
                    objInfoLang.SetXmlProperty(f, objDataInfo.GetXmlProperty(f));
                }
                strXmlLang += objInfoLang.XMLData;

                var fields = objDataInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    objInfo.SetXmlProperty(f, objDataInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</options></genxml>";
            strXmlLang += "</options></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/options", "genxml");
            DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/options", "genxml");
        }

        public void UpdateOptionValues(String xmlData)
        {
            var objList = NBrightBuyUtils.GetGenXmlListByAjax(xmlData, "");
            if (objList.Count > 0)
            {
                // get optionid for attr on node, for linking to option.  (All optionvalues should have the optionid included, so just take the first)
                var optid = objList[0].GetXmlProperty("genxml/hidden/optionid");
                // build xml for data records
                var strXml = "<genxml><optionvalues optionid='" + optid + "'>";
                var strXmlLang = "<genxml><optionvalues optionid='" + optid + "'>";
                foreach (var objDataInfo in objList)
                {
                    var objInfo = new NBrightInfo(true);
                    var objInfoLang = new NBrightInfo(true);

                    var localfields = objDataInfo.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
                    foreach (var f in localfields.Where(f => f != ""))
                    {
                        objInfoLang.SetXmlProperty(f, objDataInfo.GetXmlProperty(f));
                    }
                    strXmlLang += objInfoLang.XMLData;

                    var fields = objDataInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                    foreach (var f in fields.Where(f => f != ""))
                    {
                        objInfo.SetXmlProperty(f, objDataInfo.GetXmlProperty(f));
                    }
                    strXml += objInfo.XMLData;
                }
                strXml += "</optionvalues></genxml>";
                strXmlLang += "</optionvalues></genxml>";

                // replace  xml 
                DataRecord.ReplaceXmlNode(strXml, "genxml/optionvalues[@optionid='" + optid + "']", "genxml");
                DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/optionvalues[@optionid='" + optid + "']", "genxml");
                
                // tidy up any invlid option values (usually created in a migration phase)
                var nodList = DataRecord.XMLDoc.SelectNodes("genxml/options/genxml");
                var optionids = new Dictionary<String,String>();
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        var selectSingleNode = nod.SelectSingleNode("hidden/optionid");
                        if (selectSingleNode != null) optionids.Add(selectSingleNode.InnerText, selectSingleNode.InnerText);
                    }
                nodList = DataRecord.XMLDoc.SelectNodes("genxml/optionvalues");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        if (nod.Attributes != null && nod.Attributes["optionid"] != null)
                        {
                            if (!optionids.ContainsKey(nod.Attributes["optionid"].InnerText))
                            {
                                DataRecord.RemoveXmlNode("genxml/optionvalues[@optionid='" + nod.Attributes["optionid"].InnerText + "']");
                                DataLangRecord.RemoveXmlNode("genxml/optionvalues[@optionid='" + nod.Attributes["optionid"].InnerText + "']");
                            }
                        }
                    }
            }
        }

        #endregion

        #region " private functions"

        private void LoadData(int productId, Boolean hydrateLists = true)
        {
            Exists = false;
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(productId,"PRDLANG",_lang);
            if (Info != null)
            {
                Exists = true;
                if (hydrateLists)
                {
                    //build model list
                    Models = GetEntityList("models");
                    Options = GetEntityList("options");
                    Imgs = GetEntityList("imgs");
                    Docs = GetEntityList("docs");
                }
                Exists = true;
                DataRecord = objCtrl.GetData(productId);
                DataLangRecord = objCtrl.GetDataLang(productId, _lang);
            }
        }

        private List<NBrightInfo> GetEntityList(String entityName)
        {
            var l = new List<NBrightInfo>();
            if (Info != null)
            {
                var xmlNodList = Info.XMLDoc.SelectNodes("genxml/" + entityName + "/*");
                // build generic list to bind to rpModelsLang List
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    var lp = 1;
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        obj.ItemID = lp;
                        obj.Lang = Info.Lang;                        
                        var nodLang = "<genxml>" + Info.GetXmlNode("genxml/lang/genxml/" + entityName + "/genxml[" + lp.ToString("") + "]") + "</genxml>";
                        if (nodLang != "")
                        {
                            obj.AddSingleNode("lang", "", "genxml");
                            obj.AddXmlNode(nodLang, "genxml", "genxml/lang");
                        }
                        obj.ParentItemId = Info.ItemID;
                        l.Add(obj);
                        lp += 1;
                    }
                }
            }
            return l;
        }

        #endregion
    }
}

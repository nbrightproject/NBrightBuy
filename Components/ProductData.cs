using System;
using System.Collections.Generic;
using System.IO;
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
        public List<NBrightInfo> OptionValues;
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
            get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle"); }
        }

        public String SEOTagwords
        {
            get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/txttagwords"); }
        }

        public String SEODescription
        {
            get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary"); }
        }

        public List<NBrightInfo> GetOptionValuesById(String optionid)
        {
            var l = new List<NBrightInfo>();
            if (Info != null)
            {
                var xmlNodList = Info.XMLDoc.SelectNodes("genxml/optionvalues[@optionid='" + optionid + "']/*");
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    var lp = 1;
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        var nodLang = "<genxml>" + Info.GetXmlNode("genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml[" + lp + "]") + "</genxml>";
                        if (nodLang != "")
                        {
                            obj.SetXmlProperty("genxml/hidden/productid", Info.ItemID.ToString(""));
                            obj.SetXmlProperty("genxml/hidden/lang", Info.Lang.Trim());
                            obj.SetXmlProperty("genxml/hidden/optionid", optionid);
                            var selectSingleNode = xNod.SelectSingleNode("hidden/optionvalueid");
                            if (selectSingleNode != null) obj.SetXmlProperty("genxml/hidden/optionvalueid", selectSingleNode.InnerText);                                
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

        public NBrightInfo GetModel(String modelid)
        {
            var obj = Models.Where(i => i.GetXmlProperty("genxml/hidden/modelid") == modelid);
            return obj.First();
        }

        public NBrightInfo GetOption(String optionid)
        {
            var obj = Options.Where(i => i.GetXmlProperty("genxml/hidden/optionid") == optionid);
            return obj.First();
        }

        public NBrightInfo GetOptionValue(String optionid, String optionvalueid)
        {
            var obj = OptionValues.Where(i => i.GetXmlProperty("genxml/hidden/optionid") == optionid && i.GetXmlProperty("genxml/hidden/optionvalueid") == optionvalueid);
            return obj.First();
        }

        public List<GroupCategoryData> GetCategories(String groupref = "")
        {
            var objGrpCtrl = new GrpCatController(_lang);
            return objGrpCtrl.GetProductCategories(Info.ItemID, groupref);
        }

        public List<NBrightInfo> GetRelatedProducts()
        {
            var objCtrl = new NBrightBuyController();
            var strSelectedIds = "";
            var arylist = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRDXREF", " and NB1.parentitemid = " + Info.ItemID.ToString(""));
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
            Utils.SetCache(cacheKey, this);
        }

        public void Update(String xmlData)
        {
            var info = new NBrightInfo();
            info.ItemID = -1;
            info.TypeCode = "UPDATEDATA";
            info.XMLData = xmlData;

            //check we have valid strutre for XML
            DataRecord.ValidateXmlFormat();
            DataLangRecord.ValidateXmlFormat();


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
            // update images
            UpdateImages(info);
            // update docs
            UpdateDocs(info);
        }

        public void UpdateDocs(NBrightInfo info)
        {
            var strAjaxXml = info.GetXmlProperty("genxml/hidden/xmlupdateproductdocs");
            strAjaxXml = GenXmlFunctions.DecodeCDataTag(strAjaxXml);
            var imgList = NBrightBuyUtils.GetGenXmlListByAjax(strAjaxXml, "");

            // build xml for data records
            var strXml = "<genxml><docs>";
            var strXmlLang = "<genxml><docs>";
            foreach (var imgInfo in imgList)
            {
                var objInfo = new NBrightInfo(true);
                var objInfoLang = new NBrightInfo(true);

                var localfields = imgInfo.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
                foreach (var f in localfields.Where(f => f != ""))
                {
                    objInfoLang.SetXmlProperty(f, imgInfo.GetXmlProperty(f));
                }
                strXmlLang += objInfoLang.XMLData;

                var fields = imgInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    objInfo.SetXmlProperty(f, imgInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</docs></genxml>";
            strXmlLang += "</docs></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/docs", "genxml");
            DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/docs", "genxml");

            //add new doc if uploaded
            var docFile = info.GetXmlProperty("genxml/hidden/hiddocument");
            if (docFile != "")
            {
                var postedFile = info.GetXmlProperty("genxml/hidden/posteddocumentname");
                AddNewDoc(StoreSettings.Current.FolderDocumentsMapPath.TrimEnd('\\') + "\\" + docFile, postedFile);
            }

        }

        public void UpdateImages(NBrightInfo info)
        {
            var strAjaxXml = info.GetXmlProperty("genxml/hidden/xmlupdateproductimages");
            strAjaxXml = GenXmlFunctions.DecodeCDataTag(strAjaxXml);
            var imgList = NBrightBuyUtils.GetGenXmlListByAjax(strAjaxXml, "");

            // build xml for data records
            var strXml = "<genxml><imgs>";
            var strXmlLang = "<genxml><imgs>";
            foreach (var imgInfo in imgList)
            {
                var objInfo = new NBrightInfo(true);
                var objInfoLang = new NBrightInfo(true);

                var localfields = imgInfo.GetXmlProperty("genxml/hidden/localizedfields").Split(',');
                foreach (var f in localfields.Where(f => f != ""))
                {
                    objInfoLang.SetXmlProperty(f, imgInfo.GetXmlProperty(f));
                }
                strXmlLang += objInfoLang.XMLData;

                var fields = imgInfo.GetXmlProperty("genxml/hidden/fields").Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    objInfo.SetXmlProperty(f, imgInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</imgs></genxml>";
            strXmlLang += "</imgs></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/imgs", "genxml");
            DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/imgs", "genxml");

            //add new image if uploaded
            var imgFile = info.GetXmlProperty("genxml/hidden/hidimage");
            if (imgFile != "")
            {
                AddNewImage(StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + imgFile, StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + imgFile);
            }

        }

        public void UpdateModels(String xmlAjaxData)
        {
            var modelList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");

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
                    objInfoLang.SetXmlProperty(f, modelInfo.GetXmlProperty(f));
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

        public void UpdateOptions(String xmlAjaxData)
        {
            var objList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");

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

        public void UpdateOptionValues(String xmlAjaxData)
        {
            var objList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");
            if (objList != null)
            {
                if (objList.Count == 0)
                {
                    // no optionvalues, so remove all 
                    var nodList = DataRecord.XMLDoc.SelectNodes("genxml/optionvalues");
                    if (nodList != null)
                        for (int i = nodList.Count - 1; i >= 0; i--)
                        {
                            var parentNode = nodList[i].ParentNode;
                            if (parentNode != null) parentNode.RemoveChild(nodList[i]);
                        }
                    nodList = DataLangRecord.XMLDoc.SelectNodes("genxml/optionvalues");
                    if (nodList != null)
                        for (int i = nodList.Count - 1; i >= 0; i--)
                        {
                            var parentNode = nodList[i].ParentNode;
                            if (parentNode != null) parentNode.RemoveChild(nodList[i]);
                        }
                }
                else
                {

                    // get a list of optionid that need to be processed
                    var distinctOptionIds = new Dictionary<String, String>();
                    foreach (var o in objList)
                    {
                        if (!distinctOptionIds.ContainsKey(o.GetXmlProperty("genxml/hidden/optionid")))
                            distinctOptionIds.Add(o.GetXmlProperty("genxml/hidden/optionid"),
                                o.GetXmlProperty("genxml/hidden/optionid"));
                    }

                    foreach (var optid in distinctOptionIds.Keys)
                    {
                        // build xml for data records
                        var strXml = "<genxml><optionvalues optionid='" + optid + "'>";
                        var strXmlLang = "<genxml><optionvalues optionid='" + optid + "'>";
                        foreach (var objDataInfo in objList)
                        {
                            if (objDataInfo.GetXmlProperty("genxml/hidden/optionid") == optid)
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
                        }
                        strXml += "</optionvalues></genxml>";
                        strXmlLang += "</optionvalues></genxml>";

                        // replace  xml 
                        DataRecord.ReplaceXmlNode(strXml, "genxml/optionvalues[@optionid='" + optid + "']", "genxml");
                        DataLangRecord.ReplaceXmlNode(strXmlLang, "genxml/optionvalues[@optionid='" + optid + "']",
                            "genxml");
                    }

                    // tidy up any invlid option values (usually created in a migration phase)
                    var nodList = DataRecord.XMLDoc.SelectNodes("genxml/options/genxml");
                    var optionids = new Dictionary<String, String>();
                    if (nodList != null)
                        foreach (XmlNode nod in nodList)
                        {
                            var selectSingleNode = nod.SelectSingleNode("hidden/optionid");
                            if (selectSingleNode != null)
                                optionids.Add(selectSingleNode.InnerText, selectSingleNode.InnerText);
                        }
                    nodList = DataRecord.XMLDoc.SelectNodes("genxml/optionvalues");
                    if (nodList != null)
                        foreach (XmlNode nod in nodList)
                        {
                            if (nod.Attributes != null && nod.Attributes["optionid"] != null)
                            {
                                if (!optionids.ContainsKey(nod.Attributes["optionid"].InnerText))
                                {
                                    DataRecord.RemoveXmlNode("genxml/optionvalues[@optionid='" +
                                                             nod.Attributes["optionid"].InnerText + "']");
                                    DataLangRecord.RemoveXmlNode("genxml/optionvalues[@optionid='" +
                                                                 nod.Attributes["optionid"].InnerText + "']");
                                }
                            }
                        }
                }
            }
        }

        public void AddNewOptionValue(String optionid)
        {
            var strXml = "<genxml><optionvalues optionid='" + optionid + "'><genxml><hidden><optionid>" + optionid + "</optionid><optionvalueid>" + NBrightBuyUtils.GetUniqueKey() + "</optionvalueid></hidden></genxml></optionvalues></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/optionvalues[@optionid='" + optionid + "']") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/optionvalues[@optionid='" + optionid + "']", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/optionvalues[@optionid='" + optionid + "']/genxml", "genxml/optionvalues[@optionid='" + optionid + "']");
            }
        }

        public void AddNewOption()
        {
            var strXml = "<genxml><options><genxml><hidden><optionid>" + NBrightBuyUtils.GetUniqueKey() + "</optionid></hidden></genxml></options></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/options") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/options", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/options/genxml", "genxml/options");
            }
        }

        public void AddNewModel()
        {
            var strXml = "<genxml><models><genxml><hidden><modelid>" + NBrightBuyUtils.GetUniqueKey() + "</modelid></hidden></genxml></models></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/models") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/models", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/models/genxml", "genxml/models");
            }
        }

        public void AddNewImage(String imageurl, String imagepath)
        {
            var strXml = "<genxml><imgs><genxml><hidden><imagepath>" + imagepath + "</imagepath><imageurl>" + imageurl + "</imageurl></hidden></genxml></imgs></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/imgs") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/imgs", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/imgs/genxml", "genxml/imgs");
            }
        }

        public void AddNewDoc(String docpath, String postedFileName)
        {

            var strXml = "<genxml><docs><genxml><hidden><docid>" + NBrightBuyUtils.GetUniqueKey() + "</docid><docpath>" + docpath + "</docpath><fileext>" + Path.GetExtension(postedFileName) + "</fileext></hidden><textbox><txtfilename>" + postedFileName + "</txtfilename></textbox></genxml></docs></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/docs") == null)
            {
                DataRecord.AddXmlNode(strXml, "genxml/docs", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(strXml, "genxml/docs/genxml", "genxml/docs");
            }
        }

        public void AddCategory(int categoryid)
        {
            var strGuid = categoryid.ToString("") + "x" + Info.ItemID.ToString("");
            var objCtrl = new NBrightBuyController();
            var nbi = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATXREF", strGuid);
            if (nbi == null)
            {
                nbi = new NBrightInfo();
                nbi.ItemID = -1;
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.ModuleId = -1;
                nbi.TypeCode = "CATXREF";
                nbi.XrefItemId = categoryid;
                nbi.ParentItemId = Info.ItemID;
                nbi.XMLData = null;
                nbi.TextData = null;
                nbi.Lang = null;
                nbi.GUIDKey = strGuid;
                objCtrl.Update(nbi);
                //add all cascade xref 
                var objGrpCtrl = new GrpCatController(_lang, true);
                var parentcats = objGrpCtrl.GetCategory(categoryid);
                if (parentcats != null)
                {
                    foreach (var p in parentcats.Parents)
                    {
                        strGuid = p.ToString("") + "x" + Info.ItemID.ToString("");
                        var obj = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATCASCADE", strGuid);
                        if (obj == null)
                        {
                            nbi.XrefItemId = p;
                            nbi.TypeCode = "CATCASCADE";
                            nbi.GUIDKey = strGuid;
                            objCtrl.Update(nbi);
                        }
                    }
                }
            }
        }

        public void RemoveCategory(int categoryid)
        {
            var parentitemid = Info.ItemID.ToString("");
            var xrefitemid = categoryid.ToString("");
            var objCtrl = new NBrightBuyController();
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
            var stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + xrefitemid + " and parentitemid = " + parentitemid;
            objCtrl.GetSqlxml(stmt);
            //remove all cascade xref 
            var objGrpCtrl = new GrpCatController(_lang, true);
            var parentcats = objGrpCtrl.GetCategory(Convert.ToInt32(xrefitemid));
            if (parentcats != null)
            {
                foreach (var p in parentcats.Parents)
                {
                    var xreflist = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", " and NB1.parentitemid = " + parentitemid);
                    if (xreflist != null)
                    {
                        var deleterecord = true;
                        foreach (var xref in xreflist)
                        {
                            var catid = xref.XrefItemId;
                            var xrefparentcats = objGrpCtrl.GetCategory(Convert.ToInt32(catid));
                            if (xrefparentcats != null && xrefparentcats.Parents.Contains(p))
                            {
                                deleterecord = false;
                                break;
                            }
                        }
                        if (deleterecord)
                        {
                            stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATCASCADE' and XrefItemId = " + p.ToString("") + " and parentitemid = " + parentitemid;
                            objCtrl.GetSqlxml(stmt);
                        }

                    }
                }
            }

        }

        public void AddRelatedProduct(int productid)
        {
            if (productid!= Info.ItemID)  //cannot be related to itself
            {
                var strGuid = productid.ToString("") + "x" + Info.ItemID.ToString("");
                var objCtrl = new NBrightBuyController();
                var nbi = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PRDXREF", strGuid);
                if (nbi == null)
                {
                    nbi = new NBrightInfo();
                    nbi.ItemID = -1;
                    nbi.PortalId = PortalSettings.Current.PortalId;
                    nbi.ModuleId = -1;
                    nbi.TypeCode = "PRDXREF";
                    nbi.XrefItemId = productid;
                    nbi.ParentItemId = Info.ItemID;
                    nbi.XMLData = null;
                    nbi.TextData = null;
                    nbi.Lang = null;
                    nbi.GUIDKey = strGuid;
                    objCtrl.Update(nbi);
                }                
            }
        }

        public void RemoveRelatedProduct(int productid)
        {
            var parentitemid = Info.ItemID.ToString("");
            var xrefitemid = productid.ToString("");
            var objCtrl = new NBrightBuyController();
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
            var stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'PRDXREF' and XrefItemId = " + xrefitemid + " and parentitemid = " + parentitemid;
            objCtrl.GetSqlxml(stmt);
        }


        public int CreateNew()
        {

            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = "PRD";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            nbi.SetXmlProperty("genxml/checkbox/chkishidden", "True");
            var objCtrl = new NBrightBuyController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.TypeCode = "PRDLANG";
                nbi.ModuleId = -1;
                nbi.ItemID = -1;
                nbi.Lang = lang;
                nbi.ParentItemId = itemId;
                objCtrl.Update(nbi);
            }

            LoadData(itemId);
            return itemId;
        }

        public int Validate()
        {
            var errorcount = 0;

            DataRecord.ValidateXmlFormat();
            DataLangRecord.ValidateXmlFormat();


            //Fix image paths
            foreach (var i in Imgs)
            {
                if (!i.GetXmlProperty("genxml/hidden/imageurl").StartsWith(StoreSettings.Current.FolderImages))
                {
                    var iname = Path.GetFileName(i.GetXmlProperty("genxml/hidden/imagepath"));
                    i.SetXmlProperty("genxml/hidden/imageurl", StoreSettings.Current.FolderImages.TrimEnd('\\') + "\\" + iname);
                    errorcount += 1;
                }
                if (!i.GetXmlProperty("genxml/hidden/imagepath").StartsWith(StoreSettings.Current.FolderImagesMapPath))
                {
                    var iname = Path.GetFileName(i.GetXmlProperty("genxml/hidden/imagepath"));
                    i.SetXmlProperty("genxml/hidden/imagepath", StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + iname);
                    errorcount += 1;
                }                
            }

            //Fix document paths
            foreach (var d in Docs)
            {
                if (!d.GetXmlProperty("genxml/hidden/filepath").StartsWith(StoreSettings.Current.FolderDocumentsMapPath))
                {
                    d.SetXmlProperty("genxml/hidden/imagepath", StoreSettings.Current.FolderDocumentsMapPath.TrimEnd('\\') + "\\" + d.GetXmlProperty("genxml/textbox/txtfilename"));
                    errorcount += 1;
                }
            }

            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var objCtrl = new NBrightBuyController();
                var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRDLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                if (l.Count == 0 && DataLangRecord != null)
                {
                    var nbi = (NBrightInfo)DataLangRecord.Clone();
                    nbi.ItemID = -1;
                    nbi.Lang = lang;
                    objCtrl.Update(nbi);
                    errorcount += 1;
                }
            }

            return errorcount;
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

                    OptionValues = new List<NBrightInfo>();
                    foreach (var o in Options)
                    {
                        var l = GetOptionValuesById(o.GetXmlProperty("genxml/hidden/optionid"));
                        OptionValues.AddRange(l);   
                    }
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

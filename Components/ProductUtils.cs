using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Xml;
using NBrightCore.TemplateEngine;
using NBrightCore.render;
using NBrightDNN;
using NBrightCore.common;

namespace Nevoweb.DNN.NBrightBuy.Components
{
	public class ProductUtils
	{



        #region "Entities"
        /// <summary>
        /// Entities are XML strucutred objects addtached the to the product data. ("models" on products,"optionval" on options ) 
        /// 
        /// NOTE: This is a little complicated using our XML data strucutre, but the pain here should be repaid when we come to a running system.
        ///       i.e. The entities(e.g. models) can become part of the order and cart xml and hence be independant of the product data,
        ///       so we have a single DB record to deal with for export/display, plus when we need to remove a model from a product
        ///       there is no need to check the foreign key for model usage elsewhere in the system and hence models can be removed from products without error.
        /// </summary>
        public static List<NBrightInfo> GetEntityList(NBrightInfo objInfo, String entityName)
		{
			var l = new List<NBrightInfo>();

            var xmlNodList = objInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
			if (xmlNodList != null)
			{
				//--------------------------------------------------
				//do non-langauge display
				//--------------------------------------------------
				// build generic list to bind to rpModels List
				foreach (XmlNode xNod in xmlNodList)
				{
					var obj = new NBrightInfo();
					// add the models data to this temp obj
					obj.XMLData = xNod.OuterXml;
					obj.ItemID = objInfo.ItemID;
					l.Add(obj);
				}
			}
			return l;
		}

        public static List<NBrightInfo> GetEntityLangList(NBrightInfo objInfo, NBrightInfo objLangInfo, String entityName)
		{

			var l = new List<NBrightInfo>();
			//--------------------------------------------------
			//do langauge display
			//--------------------------------------------------
			if (objLangInfo != null && objInfo != null)
			{
				var xmlNodList = objInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
				// build generic list to bind to rpModelsLang List
                var xmlNodListLang = objLangInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
				if (xmlNodList != null && xmlNodList.Count > 0)
				{
					var lp = 0;
					foreach (XmlNode xNod in xmlNodList)
					{
						var obj = new NBrightInfo();
						// add the models data to this temp obj
						if (xmlNodListLang != null && xmlNodListLang[lp] != null)
						{
							obj.XMLData = xmlNodListLang[lp].OuterXml;
						}
						obj.ItemID = objLangInfo.ParentItemId;
						l.Add(obj);
						lp = lp + 1;
					}
				}
			}
			return l;
		}


        public static NBrightInfo AddEntity(NBrightInfo objInfo, String entityName, int numberToAdd = 1, String genxmlData = "<genxml></genxml>")
        {
            var xNod = objInfo.XMLDoc.SelectSingleNode("genxml/" + entityName.ToLower());
            if (xNod != null)
            {
                var strModelXml = "";
                for (int i = 0; i < numberToAdd; i++)
                {
                    strModelXml += genxmlData;
                }
                // Create a document fragment to contain the XML to be inserted. 
                var docFrag = objInfo.XMLDoc.CreateDocumentFragment();
                // Set the contents of the document fragment. 
                docFrag.InnerXml = strModelXml;
                //Add new model data
                xNod.AppendChild(docFrag);
                objInfo.XMLData = objInfo.XMLDoc.OuterXml;
            }
            return objInfo;
        }

        public static NBrightInfo InsertEntityData(NBrightInfo objInfo, Repeater rpEntity, String entityName, String folderMapPath = "")
		{
			var strModelXml = "<" + entityName + ">";
            foreach (RepeaterItem i in rpEntity.Items)
			{
                if (GenXmlFunctions.GetField(rpEntity, "chkDelete", i.ItemIndex) == "False")
                {
                    GenXmlFunctions.SetField(rpEntity, "entityindex", i.ItemIndex.ToString(CultureInfo.InvariantCulture), i.ItemIndex);
					strModelXml += GenXmlFunctions.GetGenXml(i, "", folderMapPath);
                }
			}
            strModelXml += "</" + entityName + ">";

			// Create a document fragment to contain the XML to be inserted. 
			var docFrag = objInfo.XMLDoc.CreateDocumentFragment();
			// Set the contents of the document fragment. 
			docFrag.InnerXml = strModelXml;
			//Add new data
			if (objInfo.XMLDoc.DocumentElement != null) objInfo.XMLDoc.DocumentElement.AppendChild(docFrag);
			objInfo.XMLData = objInfo.XMLDoc.OuterXml;
			return objInfo;
		}

        public static NBrightInfo InsertEntityLangData(NBrightInfo objLangInfo, Repeater rpEntity, Repeater rpEntityLang, String entityName, String folderMapPath = "")
		{

            var strModelXML = "<" + entityName + ">";
            foreach (RepeaterItem i in rpEntityLang.Items)
			{
                if (GenXmlFunctions.GetField(rpEntity, "chkDelete", i.ItemIndex) == "False")
                {
                    GenXmlFunctions.SetField(rpEntityLang, "entityindex", i.ItemIndex.ToString(CultureInfo.InvariantCulture), i.ItemIndex);
                    strModelXML += GenXmlFunctions.GetGenXml(i, "", folderMapPath);
                }
            }
            strModelXML += "</" + entityName + ">";

			// Create a document fragment to contain the XML to be inserted. 
			var docFrag = objLangInfo.XMLDoc.CreateDocumentFragment();
			// Set the contents of the document fragment. 
			docFrag.InnerXml = strModelXML;
			//Add new model data
			if (objLangInfo.XMLDoc.DocumentElement != null) objLangInfo.XMLDoc.DocumentElement.AppendChild(docFrag);
			objLangInfo.XMLData = objLangInfo.XMLDoc.OuterXml;

			return objLangInfo;
		}

		#endregion

        #region "xref links to product"

        public static string GetRelatedProducts(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdxref", "PRDLANG", controlMapPath);
        }

        public static string GetProductImgs(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdimg", "IMGLANG", controlMapPath);
        }

        public static string GetProductDocs(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prddoc", "DOCLANG", controlMapPath);
        }

        public static string GetProductOpts(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdopt", "OPTLANG", controlMapPath);
        }

        public static string GetRelatedXref(int portalId, string parentItemId, string lang, string templatePrefix, string nodeName, string entityTypeCodeLang, string controlMapPath)
        {
            var strOut = "";
            if (Utils.IsNumeric(parentItemId))
            {

                var objCtrl = new NBrightBuyController();

                var templCtrl = new TemplateController(controlMapPath);

                var hTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedheader + ".html", Utils.GetCurrentCulture());
                var bTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedbody + ".html", Utils.GetCurrentCulture());
                var fTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedfooter + ".html", Utils.GetCurrentCulture());

                // replace tags for ajax to work.
                hTempl = Utils.ReplaceUrlTokens(hTempl);
                bTempl = Utils.ReplaceUrlTokens(bTempl);
                fTempl = Utils.ReplaceUrlTokens(fTempl);

                var objPInfo = objCtrl.Get(Convert.ToInt32(parentItemId));
                if (objPInfo != null)
                {
                    var nodList = objPInfo.XMLDoc.SelectNodes("genxml/" + nodeName + "/id");
                    var objList = new List<NBrightInfo>();

                    foreach (XmlNode xNod in nodList)
                    {
                        if (xNod != null && Utils.IsNumeric(xNod.InnerText))
                        {
                            var o = objCtrl.Get(Convert.ToInt32(xNod.InnerText), lang, entityTypeCodeLang);
                            if (o != null)
                            {
                                objList.Add(o);
                            }
                        }
                    }

                    var obj = new NBrightInfo();
                    strOut += GenXmlFunctions.RenderRepeater(obj, hTempl);
                    strOut += GenXmlFunctions.RenderRepeater(objList, bTempl);
                    strOut += GenXmlFunctions.RenderRepeater(obj, fTempl);
                }
            }

            return strOut;
        }

        public static void RemoveAllXref(string itemId)
        {
            if (Utils.IsNumeric(itemId))
            {
                var xrefList = new List<string>();
                xrefList.Add("prdimg");
                xrefList.Add("prdxref");
                xrefList.Add("prddoc");
                xrefList.Add("prdopt");

                foreach (var xrefName in xrefList)
                {
                    var objCtrl = new NBrightBuyController();
                    var objPInfo = objCtrl.Get(Convert.ToInt32(itemId));
                    if (objPInfo != null)
                    {
                        var xrefIdList = objPInfo.GetXrefList(xrefName);
                        foreach (var xrefid in xrefIdList)
                        {
                            if (Utils.IsNumeric(xrefid))
                            {
                                var objRef = objCtrl.Get(Convert.ToInt32(xrefid));
                                if (objRef != null)
                                {
                                    objRef.RemoveXref(xrefName, itemId);
                                    objCtrl.Update(objRef);
                                }
                            }
                            objPInfo.RemoveXref(xrefName, xrefid);
                            objCtrl.Update(objPInfo);
                        }

                    }
                }
            }
        }
        #endregion


        public static string GetRelatedCats(int portalId, string parentItemId, string cultureCode, string templatePrefix, string controlMapPath, Boolean AllowCache = true)
        {
            var strOut = "";
            if (Utils.IsNumeric(parentItemId))
            {
                if (!AllowCache)
                {
                    //Remove any cache for the module -1, we don't want any cache in BO
                    //All xref records are portal wide, hence -1 in cahce key.
                    NBrightBuyUtils.RemoveModCache(-1);
                }

                var objCtrl = new NBrightBuyController();

                var templCtrl = new TemplateController(controlMapPath);

                var hTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedheader + ".html", Utils.GetCurrentCulture());
                var bTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedbody + ".html", Utils.GetCurrentCulture());
                var fTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedfooter + ".html", Utils.GetCurrentCulture());

                // replace Settings tags for ajax to work.
                hTempl = Utils.ReplaceUrlTokens(hTempl);
                bTempl = Utils.ReplaceUrlTokens(bTempl);
                fTempl = Utils.ReplaceUrlTokens(fTempl);

                var strFilter = " and parentitemid = " + parentItemId;
                var strOrderBy = GenXmlFunctions.GetSqlOrderBy(hTempl);
                if (strOrderBy == "")
                {
                    strOrderBy = GenXmlFunctions.GetSqlOrderBy(bTempl);
                }

                var l = objCtrl.GetList(portalId, -1, "CATXREF", strFilter, strOrderBy);
                var objList = new List<NBrightInfo>();

                foreach (var objXref in l)
                {
                    var o = objCtrl.Get(objXref.XrefItemId, "CATEGORYLANG", cultureCode);
                    if (o != null)
                    {
                        if (objXref.GetXmlProperty("genxml/hidden/defaultcat") != "")
                        {
                            // set the default flag in the category, for display in the entry only.
                            o.SetXmlProperty("genxml/hidden/defaultcat", "True");
                        }
                        o.GUIDKey = objXref.ItemID.ToString(); // overwrite with xref itemid for delete ajax action.
                        o.TextData = o.GetXmlProperty("genxml/lang/genxml/textbox/txtname"); // set for sort
                        o.Lang = cultureCode; // set lang so the GenXmlTemplateExt can pickup the edit langauge.
                        objList.Add(o);
                        objList.Sort(delegate(NBrightInfo p1, NBrightInfo p2) { return p1.TextData.CompareTo(p2.TextData); });
                    }
                }

                var obj = new NBrightInfo();
                strOut += GenXmlFunctions.RenderRepeater(obj, hTempl);
                strOut += GenXmlFunctions.RenderRepeater(objList, bTempl);
                strOut += GenXmlFunctions.RenderRepeater(obj, fTempl);

            }

            return strOut;
        }

        public static NBrightInfo CalculateModels(NBrightInfo objInfo,String controlMapPath)
        {
            var objCtrl = new NBrightBuyController();
            var optList = new List<NBrightInfo>();

            // get list of active options for product models
            var xmlOptList = objInfo.XMLDoc.SelectNodes("genxml/prdopt/id");
            if (xmlOptList != null)
            {
                foreach (XmlNode xNod in xmlOptList)
                {
                    if (Utils.IsNumeric(xNod.InnerText))
                    {
                        var objOpt = objCtrl.Get(Convert.ToInt32(xNod.InnerText));
                        if (objOpt != null) optList.Add(objOpt);
                    }
                }                
            }

            //sort into ItemId order so we get the same modelcode created.
            optList.Sort(delegate(NBrightInfo p1, NBrightInfo p2)
            {
                return p1.ItemID.CompareTo(p2.ItemID);
            });

            //Build modelCode list
            int lp1 = 0;
            var mcList = new List<string>();
            lp1 = 0;
            if (optList.Count == 1)
            {
                // only 1 option with stock, so no need to do a recursive build.
                var xmlNodList2 = optList[0].XMLDoc.SelectNodes("genxml/optionval/genxml");
                if (xmlNodList2 != null)
                    foreach (XmlNode xNod2 in xmlNodList2)
                    {
                        var xNod = xNod2.SelectSingleNode("textbox/txtoptionvalue");
                        if (xNod != null) mcList.Add(xNod.InnerText);
                    }
            }
            else
            {
                // do recursive build on options.
                while (lp1 < (optList.Count - 1))
                {
                    mcList = BuildModelCodes(optList, lp1, "", "", mcList);
                    lp1++;
                }
            }


            //Merge with existing models
            var templCtrl = new TemplateGetter(controlMapPath,controlMapPath);
            Repeater rpEntity;
            var strTemplate = templCtrl.GetTemplateData("AdminProducts_Models.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
            
            // remove models no longer needed
            XmlNodeList nodes = objInfo.XMLDoc.SelectNodes("genxml/models/genxml");
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var mCode = nodes[i].SelectSingleNode("hidden/modelcode");
                if (mCode != null)
                {
                    if (!mcList.Contains(mCode.InnerText))
                    {
                        var parentNode = nodes[i].ParentNode;
                        if (parentNode != null) parentNode.RemoveChild(nodes[i]);
                    }
                }
                else
                {
                    // no modelcode, invalid, so remove
                    var parentNode = nodes[i].ParentNode;
                    if (parentNode != null) parentNode.RemoveChild(nodes[i]);
                }
            }
            
            // save changes back to the product object
            objInfo.XMLData = objInfo.XMLDoc.OuterXml;

            // add new models
            var idx = 0;
            foreach (var modelCode in mcList)
            {
                if (objInfo.XMLDoc.SelectSingleNode("genxml/models/genxml/hidden/modelcode[.='" + modelCode + "']") == null)
                {
                    var obj = new NBrightInfo();
                    rpEntity = GenXmlFunctions.InitRepeater(obj, strTemplate);
                    GenXmlFunctions.SetHiddenField(rpEntity.Items[0], "modelcode", modelCode);
                    GenXmlFunctions.SetHiddenField(rpEntity.Items[0], "entityindex", idx.ToString(CultureInfo.InvariantCulture));
                    var strXml = GenXmlFunctions.GetGenXml(rpEntity, 0);
                    objInfo = AddEntity(objInfo, "models", 1, strXml);
                    idx += 1;
                }
            }

            return objInfo;
        }

        private static List<string> BuildModelCodes(List<NBrightInfo> optList, int lpPos, String pmodelCode, String modelCode, List<string> mcList)
        {
            // COMMENT(Dave Lee): Dave, if your looking at this in the future... 
            // YES!!! you did stuggle to pop this out of your tiny mad mind...
            // and YES!! your still just as stupid now as then! 
            var xmlNodList2 = optList[lpPos].XMLDoc.SelectNodes("genxml/optionval/genxml");
            if (xmlNodList2 != null)
            {
                foreach (XmlNode xNod2 in xmlNodList2)
                {
                    if ((pmodelCode == "") | (pmodelCode != modelCode)) // only want to add the first value to the modelcode
                    {
                        var selectSingleNode = xNod2.SelectSingleNode("textbox/txtoptionvalue");
                        if (selectSingleNode != null) modelCode = pmodelCode + selectSingleNode.InnerText + "-";
                        if (lpPos == (optList.Count - 1)) // only add the last child of the tree
                        {
                            mcList.Add(modelCode.TrimEnd('-'));
                        }
                        if (lpPos < (optList.Count - 1)) // end recussive loop on last child.
                        {
                            mcList = BuildModelCodes(optList, lpPos + 1, modelCode, "", mcList);
                        }
                    }
                }
            }
            return mcList;
        }

        public static ProductData GetProductData(String productid, String lang, Boolean debugMode = false)
        {
            if (Utils.IsNumeric(productid)) return GetProductData(Convert.ToInt32(productid), lang, debugMode);
            return null;
        }

	    public static ProductData GetProductData(int productid, String lang, Boolean debugMode = false)
	    {
	        if (debugMode) return new ProductData(productid, lang, true);
	        var cacheKey = "NBSProductData*" + productid.ToString("") + "*" + lang;
	        var prodData = (ProductData)Utils.GetCache(cacheKey);
	        if (prodData == null) prodData = new ProductData(productid, lang, true);
	        return prodData;
	    }
	}
}
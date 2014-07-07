using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ProductData
    {
        public NBrightInfo Info;

        public List<NBrightInfo> Models;
        public List<NBrightInfo> Options;
        public List<NBrightInfo> OptionValues;
        public List<NBrightInfo> Imgs;
        public List<NBrightInfo> Docs;

        /// <summary>
        /// Populate the ProductData in this class
        /// </summary>
        /// <param name="productId">productid</param>
        /// <param name="hydrateLists">populate the sub data into lists</param>
        public ProductData(String productId, Boolean hydrateLists = true)
        {
            if (Utils.IsNumeric(productId)) LoadData(Convert.ToInt32(productId), hydrateLists);
        }

        /// <summary>
        /// Populate the ProductData in this class
        /// </summary>
        /// <param name="productId">productid</param>
        /// <param name="hydrateLists">populate the sub data into lists</param>
        public ProductData(int productId, Boolean hydrateLists = true)
        {
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


        #endregion

        #region " private functions"

        private void LoadData(int productId, Boolean hydrateLists = true)
        {
            Exists = false;
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(productId,"PRDLANG",Utils.GetCurrentCulture());
            if (Info != null)
            {
                Exists = true;
                if (hydrateLists)
                {
                    //build model list
                    Models = GetEntityList("models", "modelid");
                    Options = GetEntityList("options", "optionid");
                    OptionValues = GetEntityList("optionvalues", "optionvalueid");
                    Imgs = GetEntityList("imgs", "imageid");
                    Docs = GetEntityList("docs", "docid");                    
                }
            }
        }

        private List<NBrightInfo> GetEntityList(String entityName,String entityIdName)
        {
            var l = new List<NBrightInfo>();
            if (Info != null)
            {
                var xmlNodList = Info.XMLDoc.SelectNodes("genxml/" + entityName + "/*");
                // build generic list to bind to rpModelsLang List
                var xmlNodListLang = Info.XMLDoc.SelectNodes("genxml/lang/genxml/" + entityName + "/*");
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        var entityId = obj.GetXmlProperty("genxml/hidden/" + entityIdName);
                        if (Utils.IsNumeric(entityId))
                        {
                            obj.ItemID = Convert.ToInt32(entityId);
                            var nodLang = "<genxml>" + Info.GetXmlNode("genxml/lang/genxml/" + entityName + "/genxml[./hidden/" + entityIdName + "/text()='" + entityId + "']") + "</genxml>";
                            if (nodLang != "")
                            {
                                obj.AddSingleNode("lang","","genxml");
                                obj.AddXmlNode(nodLang, "genxml", "genxml/lang");
                            }
                        }
                        obj.ParentItemId = Info.ItemID;
                        l.Add(obj);
                    }
                }
            }
            return l;
        }

        #endregion
    }
}

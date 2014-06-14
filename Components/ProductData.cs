using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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

        public ProductData(int productId)
        {
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(productId,"PRDLANG",Utils.GetCurrentCulture());

            //build model list
            Models = GetEntityList("models", "modelid");
            Options = GetEntityList("options", "optionid");
            OptionValues = GetEntityList("optionvalues", "optionvalueid");
            Imgs = GetEntityList("imgs", "imageid");
            Docs = GetEntityList("docs", "docid");

        }

        #region "functions"

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

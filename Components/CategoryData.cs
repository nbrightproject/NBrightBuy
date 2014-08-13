using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CategoryData
    {
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;

        private Boolean _doCascadeIndex;
        private int _oldcatcascadeid = 0;
        private String _lang = ""; // needed for webservice

        /// <summary>
        /// Populate the ProductData in this class
        /// </summary>
        /// <param name="categoryId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public CategoryData(String categoryId,String lang)
        {
            _lang = lang;
            if (_lang == "") _lang = StoreSettings.Current.EditLanguage;
            if (Utils.IsNumeric(categoryId)) LoadData(Convert.ToInt32(categoryId));
        }

        /// <summary>
        /// Populate the CategoryData in this class
        /// </summary>
        /// <param name="categoryId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public CategoryData(int categoryId, String lang)
        {
            _lang = lang;
            if (_lang == "") _lang = StoreSettings.Current.EditLanguage;
            LoadData(categoryId);
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
                if (Exists)
                {
                    var seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                    if (seoname == "") seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");
                    return seoname;                                    
                }
                return "";
            }
        }

        public String SEOTitle
        {
            get { if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
            return "";
            }
        }

        public String SEOTagwords
        {
            get { if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txttagwords");
            return "";
            }
        }

        public String SEODescription
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtsummary");
                return "";
            }
        }

        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);
            
            //do reindex of cascade records.
            if (_doCascadeIndex)
            {
                var objGrpCtrl = new GrpCatController(_lang);
                objGrpCtrl.ReIndexCascade(_oldcatcascadeid); // reindex form parnet and parents
                objGrpCtrl.ReIndexCascade(DataRecord.ItemID); // reindex self
                objGrpCtrl.Reload();
            }
        }

        public void Update(NBrightInfo info)
        {
            var localfields = info.GetXmlProperty("genxml/hidden/localizedfields").Split(',');

            foreach (var f in localfields)
            {
                if (f == "genxml/edt/message")
                {
                    // special processing for editor, to place code in standard place.
                    if (DataLangRecord.XMLDoc.SelectSingleNode("genxml/edt") == null) DataLangRecord.AddSingleNode("edt", "", "genxml");
                    if (info.GetXmlProperty("genxml/textbox/message") == "")
                        DataLangRecord.SetXmlProperty(f, info.GetXmlProperty("genxml/edt/message"));
                    else
                        DataLangRecord.SetXmlProperty(f, info.GetXmlProperty("genxml/textbox/message")); // ajax on ckeditor (Ajax diesn't work for telrik)
                }
                else
                    DataLangRecord.SetXmlProperty(f, info.GetXmlProperty(f));

                DataRecord.RemoveXmlNode(f);
            }
            var fields = info.GetXmlProperty("genxml/hidden/fields").Split(',');

            foreach (var f in fields)
            {
                DataRecord.SetXmlProperty(f, info.GetXmlProperty(f));
                // if we have a image field then we need to create the imageurl field
                if (info.GetXmlProperty(f.Replace("textbox/", "hidden/hidinfo")) == "Img=True")
                    DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "url", StoreSettings.Current.FolderImages + "/" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));
                if (f == "genxml/dropdownlist/ddlparentcatid")
                {
                    var parentitemid = info.GetXmlProperty(f);
                    if (!Utils.IsNumeric(parentitemid)) parentitemid = "0";
                    if (DataRecord.ParentItemId != Convert.ToInt32(parentitemid))
                    {
                        _oldcatcascadeid = DataRecord.ParentItemId;
                        _doCascadeIndex = true;
                        DataRecord.ParentItemId = Convert.ToInt32(parentitemid);
                    }
                }
                DataLangRecord.RemoveXmlNode(f);
                
            }
        }

        #endregion



        #region " private functions"

        private void LoadData(int categoryId)
        {
            Exists = false;
            if (categoryId == -1) AddNew(); // add new record if -1 is used as id.
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(categoryId, "CATEGORYLANG", _lang);
            if (Info != null)
            {
                Exists = true;
                DataRecord = objCtrl.GetData(categoryId);
                DataLangRecord = objCtrl.GetDataLang(categoryId, _lang);
            }
        }

        private void AddNew()
        {
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = "CATEGORY";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            var objCtrl = new NBrightBuyController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.TypeCode = "CATEGORYLANG";
                nbi.ModuleId = -1;
                nbi.ItemID = -1;
                nbi.Lang = lang;
                nbi.ParentItemId = itemId;
                objCtrl.Update(nbi);
            }

        }


        #endregion
    }
}

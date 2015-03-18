using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        private int _portalId = -1; // for shared products.

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

        public String GroupType {
            get
            {
                return DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
            }
            set
            {
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", value);
            }
        }

        public int ParentItemId
        {
            get
            {
                return DataRecord.ParentItemId;
            }
            set
            {
                DataRecord.ParentItemId = value;
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", value.ToString(""));
            }
        }


        public String SEOName
        {
            get
            {
                if (Exists)
                {
                    var seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                    if (seoname == "") seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                    return seoname;                                    
                }
                return "";
            }
        }

        public String SEOTitle
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
            return "";
            }
        }

        public String SEOTagwords
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
            return "";
            }
        }

        public String SEODescription
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                return "";
            }
        }

        public String CategoryRef
        {
            get
            {
                return DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");
            }
        }

        public int CategoryId
        {
            get
            {
                return DataRecord.ItemID;
            }
        }


        public Boolean IsHidden
        {
            get
            {
                return DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden");
            }
            set
            {
                DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", value.ToString());
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
            NBrightBuyUtils.RemoveModCachePortalWide(_portalId);
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
                {
                    DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "url", StoreSettings.Current.FolderImages + "/" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));
                    DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "path", StoreSettings.Current.FolderImagesMapPath  + "\\" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));
                }
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

        public void ResetLanguage(String resetToLang)
        {
            if (resetToLang != DataLangRecord.Lang)
            {
                var resetToLangData = new CategoryData(DataRecord.ItemID, resetToLang);
                var objCtrl = new NBrightBuyController();
                DataLangRecord.XMLData = resetToLangData.DataLangRecord.XMLData;
                objCtrl.Update(DataLangRecord);
            }
        }

        public int Validate()
        {
            var errorcount = 0;
            var objCtrl = new NBrightBuyController();

            // default any undefined group type as category (I think quickcategory v1.0.0 plugin causes this)
            if (DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype") == "")
            {
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", "cat");
                Save();
            }

            DataRecord.ValidateXmlFormat();
            if (DataLangRecord == null)
            {
                // we have no datalang record for this language, so get an existing one and save it.
                var l = objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
                if (l.Count > 0)
                {
                    DataLangRecord = (NBrightInfo)l[0].Clone();
                    DataLangRecord.ItemID = -1;
                    DataLangRecord.Lang = _lang;
                    DataLangRecord.ValidateXmlFormat();
                    objCtrl.Update(DataLangRecord);
                }
            }
            
            // fix image
            var imgpath = DataRecord.GetXmlProperty("genxml/hidden/imagepath");
            var imgurl = DataRecord.GetXmlProperty("genxml/hidden/imageurl");
            var imagefilename = Path.GetFileName(imgpath);
            if (!imgpath.StartsWith(StoreSettings.Current.FolderImagesMapPath))
            {
                    DataRecord.SetXmlProperty("genxml/hidden/imagepath", StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + imagefilename);
                errorcount += 1;
            }
            if (imagefilename == "")
            {
                DataRecord.SetXmlProperty("genxml/hidden/imagepath", "");
                errorcount += 1;
            }
            if (!imgurl.StartsWith(StoreSettings.Current.FolderImages))
            {
                    DataRecord.SetXmlProperty("genxml/hidden/imageurl", StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + imagefilename);
                errorcount += 1;
            }
            if (imagefilename == "")
            {
                DataRecord.SetXmlProperty("genxml/hidden/imageurl", "");
                errorcount += 1;
            }

            // check guidkey is correct
            if (DataRecord.GUIDKey != CategoryRef)
            {
                DataRecord.GUIDKey = CategoryRef;
                errorcount += 1;
            }

            if (errorcount > 0) objCtrl.Update(DataRecord); // update if we find a error

            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var l = objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                if (l.Count == 0 && DataLangRecord != null)
                {
                    var nbi = (NBrightInfo)DataLangRecord.Clone();
                    nbi.ItemID = -1;
                    nbi.Lang = lang;
                    objCtrl.Update(nbi);
                    errorcount += 1;
                }
                if (l.Count > 1)
                {
                    // we have more records than shoudl exists, remove any old ones.
                    var l2 = objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'", "order by Modifieddate desc");
                    var lp = 1;
                    foreach (var i in l2)
                    {
                      if (lp >=2) objCtrl.Delete(i.ItemID);
                      lp += 1;
                    }
                }
            }

            // fix groups with mismatching ddlgrouptype
            if (GroupType != "cat")
            {
                var grp = objCtrl.Get(DataRecord.ParentItemId, "GROUP");
                if (grp != null)
                {
                    if (grp.GUIDKey != GroupType)
                    {
                        DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", grp.GUIDKey);
                        objCtrl.Update(DataRecord);
                        errorcount += 1;
                    }
                }
            }


            return errorcount;
        }

        #endregion



        #region " private functions"

        private void LoadData(int categoryId)
        {
            Exists = false;
            if (categoryId == -1) categoryId = AddNew(); // add new record if -1 is used as id.
            var objCtrl = new NBrightBuyController();
            if (_lang == "") _lang = Utils.GetCurrentCulture();
            Info = objCtrl.Get(categoryId, "CATEGORYLANG", _lang);
            if (Info != null)
            {
                Exists = true;
                _portalId = Info.PortalId;
                DataRecord = objCtrl.GetData(categoryId);
                DataLangRecord = objCtrl.GetDataLang(categoryId, _lang);
                if (DataLangRecord == null) // rebuild langauge is we have a missing lang record
                {
                    Validate();
                    DataLangRecord = objCtrl.GetDataLang(categoryId, _lang);
                }
            }
            else
            {
                // new product being created, so set the portal id to the correct one 
                if (PortalSettings.Current != null) //(should not be in sceduler, but just check)
                {
                    if (StoreSettings.Current.Get("shareproducts") != "True") // option in storesetting to share products created here across all portals.
                    {
                        _portalId = PortalSettings.Current.PortalId;
                    }
                }
            }
        }

        private int AddNew()
        {
            var nbi = new NBrightInfo(true);
            nbi.PortalId = _portalId;
            nbi.TypeCode = "CATEGORY";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            nbi.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", "cat");
            nbi.SetXmlProperty("genxml/checkbox/chkishidden", "True");
            nbi.SetXmlPropertyDouble("genxml/hidden/recordsortorder", 99999);
            var objCtrl = new NBrightBuyController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = _portalId;
                nbi.TypeCode = "CATEGORYLANG";
                nbi.ModuleId = -1;
                nbi.ItemID = -1;
                nbi.Lang = lang;
                nbi.ParentItemId = itemId;
                objCtrl.Update(nbi);
            }

            LoadData(itemId);

            return itemId;
        }


        #endregion
    }
}

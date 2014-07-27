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
    public class GroupData
    {
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;

        private String _lang = ""; // needed for webservice

        /// <summary>
        /// Populate the DATA in this class
        /// </summary>
        /// <param name="groupId"> </param>
        /// <param name="lang"> </param>
        public GroupData(int groupId, String lang)
        {
            _lang = lang;
            if (_lang == "") _lang = StoreSettings.Current.EditLanguage;
            LoadData(groupId);
        }

        #region "public functions/interface"

        /// <summary>
        /// Set to true if product exists
        /// </summary>
        public bool Exists { get; private set; }

        public String Name
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/groupname");
                return "";
            }
            set { if (Exists) DataLangRecord.SetXmlProperty("genxml/textbox/groupname", value); }
        }

        public String Ref
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/textbox/groupref");
                return "";
            }
            set { if (Exists) DataRecord.SetXmlProperty("genxml/textbox/groupref", value); }
        }


        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);
        }

        #endregion



        #region " private functions"

        private void LoadData(int groupId)
        {
            Exists = false;
            if (groupId == -1) AddNew(); // add new record if -1 is used as id.
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(groupId, "GROUPLANG", _lang);
            if (Info != null)
            {
                Exists = true;
                DataRecord = objCtrl.GetData(groupId);
                DataLangRecord = objCtrl.GetDataLang(groupId, _lang);
            }
        }

        private void AddNew()
        {
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId; ;
            nbi.TypeCode = "GROUP";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            var objCtrl = new NBrightBuyController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.TypeCode = "GROUPLANG";
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

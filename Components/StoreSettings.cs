using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class StoreSettings
    {
        private readonly Dictionary<string, string> _settingDic;

        public const String ManagerRole = "Manager";
        public const String EditorRole = "Editor";
        public const String DealerRole = "Dealer";

        #region Constructors
        public StoreSettings(int portalId)
        {
            DebugMode = false;

            _settingDic = new Dictionary<string, string>();

            //Get NBrightBuy Portal Settings.
            var modCtrl = new NBrightBuyController();
            SettingsInfo = modCtrl.GetByGuidKey(portalId, -1, "SETTINGS", "NBrightBuySettings");
            if (SettingsInfo != null)
            {
                AddToSettingDic(SettingsInfo, "genxml/hidden/*");
                AddToSettingDic(SettingsInfo, "genxml/textbox/*");
                AddToSettingDic(SettingsInfo, "genxml/checkbox/*");
                AddToSettingDic(SettingsInfo, "genxml/dropdownlist/*");
                AddToSettingDic(SettingsInfo, "genxml/radiobuttonlist/*");
                AddToSettingDicSelectedTextAttr(SettingsInfo, "genxml/dropdownlist/*");
                AddToSettingDicSelectedTextAttr(SettingsInfo, "genxml/radiobuttonlist/*");
            }

            //add DNN Portalsettings
            var pCtrl = new PortalController();
            var portalInfo = pCtrl.GetPortal(portalId);
            if (!_settingDic.ContainsKey("portalid")) _settingDic.Add("portalid", portalInfo.PortalID.ToString(""));
            if (!_settingDic.ContainsKey("portalname")) _settingDic.Add("portalname", portalInfo.PortalName);
            if (!_settingDic.ContainsKey("homedirectory")) _settingDic.Add("homedirectory", portalInfo.HomeDirectory);
            if (!_settingDic.ContainsKey("homedirectorymappath")) _settingDic.Add("homedirectorymappath", portalInfo.HomeDirectoryMapPath);
            if (!_settingDic.ContainsKey("culturecode")) _settingDic.Add("culturecode", Utils.GetCurrentCulture());


            ThemeFolder = Get("themefolder");

            if (_settingDic.ContainsKey("debug.mode") && _settingDic["debug.mode"] == "True") DebugMode = true;  // set debug mmode
            StorageTypeClient = DataStorageType.Cookie;
            if (Get("storagetypeclient") == "SessionMemory") StorageTypeClient = DataStorageType.SessionMemory;
            
            AdminEmail = Get("adminemail");
            ManagerEmail = Get("manageremail");
            FolderDocumentsMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderdocs");
            FolderImagesMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderimages");
            FolderUploadsMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderuploads");

            FolderDocuments = "/" + Get("homedirectory").TrimEnd('/') + "/" + Get("folderdocs").Replace("\\", "/");
            FolderImages = "/" + Get("homedirectory").TrimEnd('/') + "/" + Get("folderimages").Replace("\\", "/");
            FolderUploads = "/" + Get("homedirectory").TrimEnd('/') + "/" + Get("folderuploads").Replace("\\", "/");

            if (!_settingDic.ContainsKey("FolderDocumentsMapPath")) _settingDic.Add("FolderDocumentsMapPath",FolderDocumentsMapPath );
            if (!_settingDic.ContainsKey("FolderImagesMapPath")) _settingDic.Add("FolderImagesMapPath",FolderImagesMapPath );
            if (!_settingDic.ContainsKey("FolderUploadsMapPath")) _settingDic.Add("FolderUploadsMapPath",FolderUploadsMapPath );
            if (!_settingDic.ContainsKey("FolderDocuments")) _settingDic.Add("FolderDocuments", FolderDocuments);
            if (!_settingDic.ContainsKey("FolderImages")) _settingDic.Add("FolderImages",FolderImages );
            if (!_settingDic.ContainsKey("FolderUploads")) _settingDic.Add("FolderUploads", FolderUploads);

        }

        #endregion

        public static StoreSettings Current
        {
            get { return NBrightBuyController.GetCurrentPortalData(); }
        }
        
        public Dictionary<string, string> Settings()
        {
            // redo the edit langauge for backoffice.
            if (_settingDic != null)
            {
                if (_settingDic.ContainsKey("editlanguage"))
                    _settingDic["editlanguage"] = EditLanguage;
                else
                    _settingDic.Add("editlanguage", EditLanguage);
            }
            return _settingDic;
        }

        public String EditLanguage
        {
            get
            {
                var editlang = "";
                // need to test if HttpContext.Current.Session is null, because webservice calling storesettings will raise exception. 
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["NBrightBuy_EditLanguage"] != null) editlang = (String)HttpContext.Current.Session["NBrightBuy_EditLanguage"];
                if (editlang == "") return Utils.GetCurrentCulture();
                return editlang;
            }
            set { HttpContext.Current.Session["NBrightBuy_EditLanguage"] = value; }
        }


        #region "properties"

        /// <summary>
        /// Return setting using key value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return _settingDic.ContainsKey(key) ? _settingDic[key] : "";
        }

        public int GetInt(string key)
        {
            if (_settingDic.ContainsKey(key))
            {
                if (Utils.IsNumeric(_settingDic[key]))
                {
                    return Convert.ToInt32(_settingDic[key]);
                }
            }
            return 0;
        }

        //get properties
        public int CartTabId
        {
            get
            {
                var i = Get("carttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }

        public int PaymentTabId
        {
            get
            {
                var i = Get("paymenttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }

        // this section contain a set of properties that are assign commanly used setting.

        public bool DebugMode { get; private set; }
        /// <summary>
        /// Get Client StorageType type Cookie,SessionMemory
        /// </summary>
        public DataStorageType StorageTypeClient { get; private set; }
        public String AdminEmail { get; private set; }
        public String ManagerEmail { get; private set; }
        public NBrightInfo SettingsInfo { get; private set; }
        public String ThemeFolder { get; private set; }
        public int ActiveCatId { get; set; }

        public String FolderImagesMapPath { get; private set; }
        public String FolderDocumentsMapPath { get; private set; }
        public String FolderUploadsMapPath { get; private set; }
        public String FolderImages { get; private set; }
        public String FolderDocuments { get; private set; }
        public String FolderUploads { get; private set; }

        #endregion

        private void AddToSettingDic(NBrightInfo settings, string xpath)
        {
            if (settings.XMLDoc != null)
            {
                var nods = settings.XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (_settingDic.ContainsKey(nod.Name))
                        {
                            _settingDic[nod.Name] = nod.InnerText; // overwrite same name node
                        }
                        else
                        {
                            _settingDic.Add(nod.Name, nod.InnerText);
                        }
                    }
                }
            }
        }

        private void AddToSettingDicSelectedTextAttr(NBrightInfo settings, string xpath)
        {
            if (settings.XMLDoc != null)
            {
                var nods = settings.XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (_settingDic.ContainsKey(nod.Name + "text"))
                        {
                            if (nod.Attributes != null) _settingDic[nod.Name + "text"] = nod.Attributes["selectedtext"].InnerText;
                        }
                        else
                        {
                            if (nod.Attributes != null) _settingDic.Add(nod.Name + "text", nod.Attributes["selectedtext"].InnerText);
                        }
                    }
                }
            }
        }

    }
}

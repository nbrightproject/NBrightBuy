using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class StoreSettings
    {
        private readonly Dictionary<string, string> _settingDic;

        #region Constructors
        public StoreSettings()
        {
            DebugMode = false;

            //[TODO: NB_Store v2.4 code for settings, needs to be changed/removed for v3]
            SettingsInfo = NBrightBuyUtils.GetGlobalSettings(PortalSettings.Current.PortalId);
            if (SettingsInfo != null)
            {
                _settingDic = SettingsInfo.ToDictionary();
            }

            //Get NBrightBuy Portal Settings.
            var modCtrl = new NBrightBuyController();
            var indexSettings = modCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightBuySettings");
            if (indexSettings != null)
            {
                AddToSettingDic(indexSettings, "genxml/hidden/*");
                AddToSettingDic(indexSettings, "genxml/textbox/*");
                AddToSettingDic(indexSettings, "genxml/checkbox/*");
                AddToSettingDic(indexSettings, "genxml/dropdownlist/*");
                AddToSettingDic(indexSettings, "genxml/radiobuttonlist/*");
            }

            //add DNN Portalsettings
            if (!_settingDic.ContainsKey("portalid")) _settingDic.Add("portalid", PortalSettings.Current.PortalId.ToString(""));
            if (!_settingDic.ContainsKey("portalname")) _settingDic.Add("portalname", PortalSettings.Current.PortalName);
            if (!_settingDic.ContainsKey("homedirectory")) _settingDic.Add("homedirectory", PortalSettings.Current.HomeDirectory);
            if (!_settingDic.ContainsKey("defaultportalalias")) _settingDic.Add("defaultportalalias", PortalSettings.Current.DefaultPortalAlias);
            if (!_settingDic.ContainsKey("culturecode")) _settingDic.Add("culturecode", Utils.GetCurrentCulture());


            ThemeFolder = Get("themefolder");

            if (_settingDic.ContainsKey("debug.mode")) DebugMode = Convert.ToBoolean(_settingDic.ContainsKey("debug.mode"));  // set debug mmode

        }

        #endregion

        public static StoreSettings Current
        {
            get { return NBrightBuyController.GetCurrentPortalData(); }
        }

        public Dictionary<string, string> Settings()
        {
            return _settingDic;
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

        // this section contain a set of properties that are assign commanly used setting.

        public bool DebugMode { get; private set; }
        public NBrightInfo SettingsInfo { get; private set; }
        public String ThemeFolder { get; private set; }
        public int ActiveCatId { get; set; }

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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CartController
    {
        public List<NBrightInfo> CartList;
        public NBrightInfo CurrentCart;

        public CartController(string lang)
        {
            var uInfo = UserController.GetCurrentUserInfo();
            if (uInfo != null)
            {
                // build group category list
                var strCacheKey = "NBS_CartList_" + lang + "_" + uInfo.UserID + "_" + PortalSettings.Current.PortalId;
                CartList = (List<NBrightInfo>)NBrightBuyUtils.GetModCache(strCacheKey);
                if (CartList == null)
                {
                    CartList = GetCartListFromDatabase(uInfo.UserID, lang);
                    NBrightBuyUtils.SetModCache(-1, strCacheKey, CartList);
                }                
            }

            
        }



        #region "base methods"

        public NBrightInfo GetCart()
        {
            var objCart = 
            
        }

        #endregion

        #region "private methods"

        private List<NBrightInfo> GetCartListFromDatabase(int usrId, String lang)
        {
            var rtnList = new List<NBrightInfo>();



            return rtnList;
        }


        #endregion

    }
}

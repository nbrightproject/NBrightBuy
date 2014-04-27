using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{

    /// <summary>
    /// Class to hold Category data, so we can use linq and help speed up access from the memory var CategoryList
    /// </summary>
    public class GroupCategoryData
    {
        public int categoryid { get; set; }
        public string categoryref { get; set; }
        public string grouptyperef { get; set; }
        public bool archived { get; set; }
        public bool ishidden { get; set; }
        public int parentcatid { get; set; }
        public int recordsortorder { get; set; }
        public string imageurl { get; set; }
        public string categoryname { get; set; }
        public string categorydesc { get; set; }
        public string seoname { get; set; }
        public string metadescription { get; set; }
        public string metakeywords { get; set; }
        public string seopagetitle { get; set; }
        public string breadcrumb { get; set; }
        public int depth { get; set; }
        public bool disabled { get; set; }
        public int entrycount { get; set; }
        public string url { get; set; }

    }
    
    public enum ModuleEventCodes { none, displaycategoryheader, displaycategorybody, displaycategoryfooter, displayentryheader, displayentrybody, displayentryfooter, displayheader, displaybody, displayfooter, selectsearch, selectheader, selectbody, selectfooter, selectedheader, selectedbody, selectedfooter, editheader, editbody, editlang, editfooter, editlistsearch, editlistheader, editlistbody, editlistfooter, email, emailsubject, emailclient, emailreturnmsg, jsinsert, exportxsl };

    public enum DataStorageType { Cookie,SessionMemory,Database };

}
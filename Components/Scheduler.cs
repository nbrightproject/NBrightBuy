using System;
using System.IO;
using System.Runtime.Remoting;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class NBrightBuyScheduler : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public NBrightBuyScheduler(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }



        public override void DoWork()
        {
            try
            {

                var portallist = DnnUtils.GetAllPortals();

                foreach (var portal in portallist)
                {

                    // clear down NBStore temp folder
                    var storeSettings = new StoreSettings(portal.PortalID);
                    if (Directory.Exists(storeSettings.FolderTempMapPath))
                    {
                        string[] files = Directory.GetFiles(storeSettings.FolderTempMapPath);

                        foreach (string file in files)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.LastAccessTime < DateTime.Now.AddHours(-1)) fi.Delete();
                        }

                        // DO Scheduler Jobs
                        var pluginData = new PluginData(portal.PortalID);
                        var l = pluginData.GetSchedulerProviders();

                        foreach (var p in l)
                        {
                            var prov = p.Value;
                            ObjectHandle handle = null;
                            handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                            if (handle != null)
                            {
                                var objProvider = (SchedulerInterface)handle.Unwrap();
                                var strMsg = objProvider.DoWork(portal.PortalID);
                                if (strMsg != "")
                                {
                                    this.ScheduleHistoryItem.AddLogNote(strMsg);
                                }
                            }

                        }
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true;

            }
            catch (Exception Ex)
            {
                //--intimate the schedule mechanism to write log note in schedule history
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("NBS Service Start. Failed. " + Ex.ToString());
                this.Errored(ref Ex);
            }
        }


    }


}

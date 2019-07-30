using DecisionsFramework;
using DecisionsFramework.Design;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.ServiceLayer.Services.Folder;
using DecisionsFramework.ServiceLayer.Services.Portal;
using DecisionsFramework.ServiceLayer.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This is a module initializer.  This is an optional aspect to a module.
/// This code is run whenever ServiceHostManager is started so this code can be
/// used to check and enforce branding, ensure certain things are present, make registrations,
/// enforce licensing and more.  Since this code is run on EVERY start of SHM it should not 
/// always recreate the same things, but should check and create as needed.
/// </summary>
namespace Rollbar
{
    public class ModuleInit : ILogWriter, IInitializable
    {
        public static object instance;
        public void Initialize()
        {

            instance = this;
            Log.AddLogWriter(this);
            // This code on run on every start.
            //ChangedBranding();

            //SetupFolders();

            EnsureCustomSettingsObject();

        }


        
        public void Write(LogData LogMessage)
        {
            //System.IO.File.AppendAllText("jsonLogs.json", JsonConvert.SerializeObject(LogMessage));
            var ip = GetIPAddress();
            var idic = new Dictionary<string, object>()
{
                {"Decisions.Version", DecisionsVersion.CODE_VERSION },
                {"Decisions.Host",  "http://" + ip +"/" + Settings.InstanceSettings.VirtualDirectory },
                { "user_ip",ip}
            };

            // ADD IN Level so we dont blow the budget 
            // ADD IN Level so we dont blow the budget 

         
            var SettingsInDecisions = ModuleSettingsAccessor<RollbarSettings>.GetSettings();
            
            // this rollbar ID is for DecisionsInternal.
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(SettingsInDecisions.PostServerItemID));
            //new RollbarConfig
            //{
            //    Transform = payload =>
            //    {

            //        payload.Data.CodeVersion = "2";
            //        payload.Data.Request = new Request()
            //        {
            //            Url = "http://rollbar.com",
            //            user = "192.121.222.92"
            //        };
            //    }
            //};
            if (LogMessage.Level == LogSeverity.Debug && SettingsInDecisions.LogDebug == true)
            {
                RollbarLocator.RollbarInstance.Debug(LogMessage, idic);
            }
            else if (LogMessage.Level == LogSeverity.Error && SettingsInDecisions.LogError == true)
            {
                RollbarLocator.RollbarInstance.Error(LogMessage, idic);
            }
            else if (LogMessage.Level == LogSeverity.Fatal && SettingsInDecisions.LogFatal == true)
            {
                RollbarLocator.RollbarInstance.Critical(LogMessage, idic);
            }
            else if (LogMessage.Level == LogSeverity.Info && SettingsInDecisions.LogInfo == true)
            {
                RollbarLocator.RollbarInstance.Info(LogMessage, idic);
            }
            
            else if (LogMessage.Level == LogSeverity.Warn && SettingsInDecisions.LogWarn == true)
            {
                RollbarLocator.RollbarInstance.Warning(LogMessage, idic);
            }

           

        }

        public string GetIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            return ipAddress.ToString();
        }

        private void EnsureCustomSettingsObject()
        {
            ModuleSettingsAccessor<RollbarSettings>.GetSettings();
            ModuleSettingsAccessor<RollbarSettings>.SaveSettings();
        }


        //private void SetupFolders()
        //{
        //    SystemUserContext system = new SystemUserContext();
        //    if (FolderService.Instance.Exists(system, "MY CO ROOT FOLDER") == false) {
        ///        // The null here is for a Folder Bheavior which allows a lot of customization
        //        // of a folder including custom actions and a lot of filtering capability.
        //        FolderService.Instance.CreateRootFolder(system, "MY CO ROOT FOLDER", "My Company Designs", null);
        //    }
        //}

        //private void ChangedBranding()
        //{
        //    ModuleSettingsAccessor<PortalSettings>.Instance.SloganText = "My Custom Rule Portal";

        //    ModuleSettingsAccessor<DesignerSettings>.Instance.StudioSlogan = "My Custom Rule Studio";

        //}
    }
}

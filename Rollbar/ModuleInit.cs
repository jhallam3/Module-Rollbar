using DecisionsFramework;
using DecisionsFramework.Design;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.ServiceLayer.Services.Folder;
using DecisionsFramework.ServiceLayer.Services.Portal;
using DecisionsFramework.ServiceLayer.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
        RollbarSettings SettingsInDecisions = ModuleSettingsAccessor<RollbarSettings>.GetSettings();
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
            
            if (SettingsInDecisions.EnableLogging == true)
            {


                if (LogMessage.Level == LogSeverity.Debug && SettingsInDecisions.LogDebug == true)
                {
                    PostToRollbar(CreateDataToSend(SettingsInDecisions, LogMessage));
                }
                else if (LogMessage.Level == LogSeverity.Error && SettingsInDecisions.LogError == true)
                {
                    PostToRollbar(CreateDataToSend(SettingsInDecisions, LogMessage));
                }
                else if (LogMessage.Level == LogSeverity.Fatal && SettingsInDecisions.LogFatal == true)
                {
                    PostToRollbar(CreateDataToSend(SettingsInDecisions, LogMessage));
                }
                else if (LogMessage.Level == LogSeverity.Info && SettingsInDecisions.LogInfo == true)
                {
                    PostToRollbar(CreateDataToSend(SettingsInDecisions, LogMessage));
                }

                else if (LogMessage.Level == LogSeverity.Warn && SettingsInDecisions.LogWarn == true)
                {
                    PostToRollbar(CreateDataToSend(SettingsInDecisions, LogMessage));
                }


            }

        }

        private Rollbar.Classes.JsonRollbarRootObject CreateDataToSend (RollbarSettings settings, LogData LogMessage)
        {


            var jdatabody = new Rollbar.Classes.Body()
            {
                message = new Classes.Message { body = LogMessage.Exception.ToString() }
            };



            

            var jdataperson = new Rollbar.Classes.Person()
            {  email = SettingsInDecisions.ContactEmailAddress, id = SettingsInDecisions.ContactEmailAddress, username = SettingsInDecisions.ContactEmailAddress};

            var jdataserver = new Rollbar.Classes.Server()
            {
                code_version = DecisionsVersion.CODE_VERSION.ToString(), cpu = GetCPUManufacturer(), host = GetComputerName(), ram = GetPhysicalMemory()
            };

            var jdatarequest = new Rollbar.Classes.Request() { user_ip = GetIPAddress() };

            var jdatatosend = new Rollbar.Classes.Data()
            {
                body = jdatabody,
                code_version = DecisionsVersion.CODE_VERSION.ToString(),
                environment = SettingsInDecisions.environment,
                level = LogMessage.Level.ToString(),
                person = jdataperson,
                request = jdatarequest,
                server = jdataserver
            };


            return new Classes.JsonRollbarRootObject() { access_token = settings.PostServerItemID, data = jdatatosend };

            

            
        }

        public string GetIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            return ipAddress.ToString();
        }

        private void PostToRollbar(Rollbar.Classes.JsonRollbarRootObject jsonRollbar)
        {
            using (var client = new WebClient())
            {
                var dataString = JsonConvert.SerializeObject(jsonRollbar);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.UploadString(new Uri("https://api.rollbar.com/api/1/item/"), "POST", dataString);
            }
        }
        private void EnsureCustomSettingsObject()
        {
            ModuleSettingsAccessor<RollbarSettings>.GetSettings();
            ModuleSettingsAccessor<RollbarSettings>.SaveSettings();
        }

        public static string GetCPUManufacturer()
        {
            string cpuMan = String.Empty;
            //create an instance of the Managemnet class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuMan == String.Empty)
                {
                    // only return manufacturer from first CPU
                    cpuMan = obj.Properties["Manufacturer"].Value.ToString();
                }
            }
            return cpuMan;
        }

        public static String GetComputerName()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            String info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                info = (string)mo["Name"];
                //mo.Properties["Name"].Value.ToString();
                //break;
            }
            return info;
        }

        public static string GetPhysicalMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
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

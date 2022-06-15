using System;

namespace ClickOnceHelper
{
    public class ApplicationDeployment
    {
        private static ApplicationDeployment currentDeployment = null;
        private static bool currentDeploymentInitialized = false;

        private static bool isNetworkDeployed = false;
        private static bool isNetworkDeployedInitialized = false;

        public static bool IsNetworkDeployed
        {
            get
            {
                if (!isNetworkDeployedInitialized)
                {
                    ApplicationDeployment.isNetworkDeployed = false;

                    string value = Environment.GetEnvironmentVariable("ClickOnce_IsNetworkDeployed");
                    if (value?.ToLower() == "true")
                    {
                        ApplicationDeployment.isNetworkDeployed = true;
                    }

                    ApplicationDeployment.isNetworkDeployedInitialized = true;
                }

                return ApplicationDeployment.isNetworkDeployed;
            }
        }

        public static ApplicationDeployment CurrentDeployment
        {
            get
            {
                if (!currentDeploymentInitialized)
                {
                    ApplicationDeployment.currentDeployment = IsNetworkDeployed ? new ApplicationDeployment() : null;
                    ApplicationDeployment.currentDeploymentInitialized = true;
                }

                return ApplicationDeployment.currentDeployment;
            }
        }

        public Uri ActivationUri
        {
            get
            {
                string value = Environment.GetEnvironmentVariable("ClickOnce_ActivationUri");
                return string.IsNullOrEmpty(value) ? null : new Uri(value);
            }
        }

        public Version CurrentVersion
        {
            get
            {
                string value = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion");
                return string.IsNullOrEmpty(value) ? null : new Version(value);
            }
        }
        public string DataDirectory
        {
            get { return Environment.GetEnvironmentVariable("ClickOnce_DataDirectory");  }
        }

        public bool IsFirstRun
        {
            get
            {
                bool val;
                bool.TryParse(Environment.GetEnvironmentVariable("ClickOnce_IsFirstRun"), out val);
                return val;
            }
        }

        public DateTime TimeOfLastUpdateCheck
        {
            get
            {
                DateTime value;
                DateTime.TryParse(Environment.GetEnvironmentVariable("ClickOnce_TimeOfLastUpdateCheck"), out value);
                return value;
            }
        }
        public string UpdatedApplicationFullName
        {
            get
            {
                return Environment.GetEnvironmentVariable("ClickOnce_UpdatedApplicationFullName");
            }
        }

        public Version UpdatedVersion
        {
            get
            {
                string value = Environment.GetEnvironmentVariable("ClickOnce_UpdatedVersion");
                return string.IsNullOrEmpty(value) ? null : new Version(value);
            }
        }

        public Uri UpdateLocation
        {
            get
            {
                string value = Environment.GetEnvironmentVariable("ClickOnce_UpdateLocation");
                return string.IsNullOrEmpty(value) ? null : new Uri(value);
            }
        }

        public Version LauncherVersion
        {
            get
            {
                string value = Environment.GetEnvironmentVariable("ClickOnce_LauncherVersion");
                return string.IsNullOrEmpty(value) ? null : new Version(value);
            }
        }

        private ApplicationDeployment()
        {
            // As an alternative solution, we could initialize all properties here
        }
    }
}

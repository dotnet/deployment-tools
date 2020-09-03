// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MageCLI
{
    /// <summary>
    /// Contains the entry point and a few error-reporting methods useful
    /// throughout the application.
    /// </summary>
    class Application
    {
        /// <summary>
        /// Supported process exit values.
        /// </summary>
        private enum ProcessExitCodes
        {
            Success = 0,
            ErrorInvalidArgument,
            ErrorUnknown
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static int Main(string[] args)
        {            
            ProcessExitCodes result = ProcessExitCodes.Success;
                    
            try
            {
                // Parse command line arguments
                Command command = new Command(args);
                
                // Validate command-line arguments, print an error message 
                // if any invalid arguments are present
                if (command.CanExecute())
                {
                    // Execute the command
                    command.Execute();
                }
                else
                {
                    // Command.CanExecute will print an error message, so 
                    // the only action necessary at this point is to return
                    // an 'invalid argument' exit code.
                    result = ProcessExitCodes.ErrorInvalidArgument;
                }
            }
            catch (System.Security.SecurityException)
            {
                // "Mage does not have the security permissions necessary to run.  Ensure that mage is not running from an untrusted source such as a network share or web site."
                string ex = Resources.GetString("ErrorMessage") + ": " + Resources.GetString("SecurityMessage");
                Console.WriteLine(ex);
            }
            catch (System.Exception e)
            {
                InternalError(e.Message, e.StackTrace);
                result = ProcessExitCodes.ErrorUnknown;
            }
            
            return (int) result;
        }


        /// <summary>
        /// Demand-loaded resource manager.  This member should be accessed ONLY
        /// by the 'resources' property.
        /// </summary>
        private static ResourceManager resources = null;

        /// <summary>
        /// This wrapper around '_resources' loads the resource manager on demand.
        /// </summary>
        /// <value></value>
        public static ResourceManager Resources
        {
            get
            {
                if (resources == null)
                {
                    resources = new ResourceManager(typeof (Application));
                }

                return resources;
            }
        }


        /// <summary>
        /// Print an error message if an exception is thrown.  In debug 
        /// builds, the error message will include a stack trace.
        /// </summary>
        /// <param name="message">Error message - printed to console in release and debug builds</param>
        /// <param name="stackTrace">Stack trace - only used for debug builds</param>
        static void InternalError(string message, string stackTrace)
        {
#if (DEBUG)
            message = message + Environment.NewLine + stackTrace;            
#endif            
            string error = Resources.GetString("InternalError");

            if (error == null)
            {
                error = "Internal Error:";
            }

            Console.WriteLine(error + " " + message);
        }

        public static void ReportException (Exception ex)
        {
            Console.WriteLine (ex.Message);
#if (DEBUG)
            Console.WriteLine (ex.StackTrace);
#endif
        }

        /// <summary>
        /// Prints a message.
        /// </summary>
        public static void PrintOutputMessage(string manifestname, String message)
        {
            if (String.IsNullOrEmpty(manifestname))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(manifestname + " " + message);
            }
        }

        /// <summary>
        /// Prints the short version of the help message.
        /// </summary>
        public static void PrintHelpMessage()
        {
            Console.WriteLine(Resources.GetString("HelpTerse"));
        }

        /// <summary>
        /// Prints the long version of the help message.
        /// </summary>
        public static void PrintVerboseHelpMessage()
        {
            Console.WriteLine(Resources.GetString("HelpVerbose"));
        }

        /// <summary>
        /// Returns a Processors enum constant for the named processor type, without
        /// throwing an exception if the processor is unknown.
        /// </summary>
        /// <param name="proc">String containing a processor name</param>
        /// <returns>Processor enum constant</returns>
        internal static Processors GetProcessor(string processor)
        {
            Processors result = Processors.Undefined;

            try
            {
                result = (Processors)Enum.Parse(typeof (Processors), processor, true);
            }
            catch (System.Exception)
            {
            }

            return result;
        }

        /// <summary>
        /// Prints the message about invalid options.
        /// 'The option "$option" can only be used with the following file types: "$type1"[, "$type2"]'
        /// </summary>
        /// <param name="option">The option the user has given (as a literal string, not a resource)</param>
        /// <param name="allowed1">The file type to which that option applies (literal, not resource)</param>
        /// <param name="allowed2">Second file type (may be null)</param>
        internal static void PrintInvalidOptionErrorMessage(string option, string allowed1, string allowed2)
        {
            string message;

            message = string.Format(Resources.GetString("InvalidOption") + " ", option);
            message += allowed1;
            if (allowed2 != null)
            {
                message += Resources.GetString("InvalidOptionConjuction");
                message += allowed2;
            }

            Console.WriteLine(message);
        }

        /// <summary>
        /// Prints an error message that has no replaceable parameter.
        /// </summary>
        /// <param name="error">error message ID</param>
        internal static void PrintErrorMessage(ErrorMessages error)
        {
            PrintErrorMessage(error, null);
        }

        /// <summary>
        /// Prints an error message with one replaceable parameter.
        /// </summary>
        /// <param name="error">error message ID</param>
        /// <param name="parameter">string to insert into the error message (may be 
        /// null)</param>
        internal static void PrintErrorMessage(ErrorMessages error, string parameter)
        {
            string message;

            if (null == parameter)
            {
                message = Resources.GetString(error.ToString());
            }
            else
            {
                message = Resources.GetString(error.ToString());
                message = string.Format(message, parameter);
            }

            Console.WriteLine(message);

            // Follow the 'invalid processor' error message with a list of known processors
            if (error == ErrorMessages.InvalidProcessor)
            {
                for (int i = 0; i < (int)Processors.Undefined; i++)
                {
                    Processors p = (Processors)i;

                    Console.Write(p.ToString() + " ");
                }
            }
        }
    }

    /// <summary>
    /// Error messages are specified by enum constants so that all error 
    /// string processing and printing can be encapsulated in this class.
    /// </summary>
    internal enum ErrorMessages
    {
        /// <summary>
        /// Parameter not recognized - "foo"
        /// </summary>
        UnrecognizedParameter,

        /// <summary>
        /// File not found - "foo.manifest"
        /// </summary>
        InvalidPath,

        /// <summary>
        /// Unrecognized file type - "foo.doc"
        /// </summary>
        InvalidInputFile,

        /// <summary>
        /// Key file is not of proper format - "foo.doc"
        /// </summary>
        InvalidKeyFile,

        /// <summary>
        /// Certificate file is not of proper format - "foo.doc"
        /// </summary>
        InvalidCertFile,

        /// <summary>
        /// No certificate found matching specified hash - "foo"
        /// </summary>
        InvalidHash,

        /// <summary>
        /// Version must be of format X.X.X.X (ex 1.0.0.0) - "foo"
        /// </summary>
        InvalidVersion,

        /// <summary>
        /// Invalid trust level, must be either "LocalIntranet", "Internet", or "FullTrust" - "FooTrust"
        /// </summary>
        InvalidTrustLevel,

        /// <summary>
        /// Invalid file type, must be either "AppManifest", "DeployManifest", or "TrustLicense" - "Foo"
        /// </summary>
        InvalidFileType,

        /// <summary>
        /// Certificate password is not correct
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// Missing value following the "Update" option.
        /// </summary>
        MissingArgument,

        /// <summary>
        /// Missing Password option, this is required when using the CertFile option.
        /// </summary>
        MissingPassword,

        /// <summary>
        /// Only one type of signing method may be specified.
        /// </summary>
        MultipleKeys,

        /// <summary>
        /// Cannot write to file "outfile.foo".
        /// </summary>
        FileNotWriteable,

        /// <summary>
        /// Unknown processor type "foo".
        /// </summary>
        InvalidProcessor,

        /// <summary>
        /// The -RequiredUpdate option must be "true", "false", "t", or "f" - "foo"
        /// </summary>
        InvalidRequiredUpdate,

        /// <summary>
        /// The "foo" option can only be used with the -New or -Update commands.
        /// </summary>
        InvalidSignOption,

        /// <summary>
        /// The -Sign command requires one of -KeyFile, -CertFile, or -CertHash.
        /// </summary>
        NoKeySpecified,

        /// <summary>
        /// Only one of -KeyFile, -CertFile, or -CertHash can be used.
        /// </summary>
        TooManyKeysSpecified,

        /// <summary>
        /// No output file was specified.  Please use -ToFile <filename>.
        /// </summary>
        NoOutputFileSpecified,

        /// <summary>
        /// The URL is not of the proper format - "foo:///"
        /// </summary>
        InvalidUrl,

        /// <summary>
        /// The first argument must be one of the following: -New, -Update, -Sign
        /// </summary>
        NoVerb,

        /// <summary>
        /// Directory not found - "foo/blah"
        /// </summary>
        InvalidDirectory,

        /// <summary>
        /// Did not include a locked file {0}
        /// (Appears when searching directories for files to include in an
        /// application manifest - we cannot compute the hash of a locked file, so
        /// it gets skipped.)
        /// </summary>
        LockedFile,

        /// <summary>
        /// Deployment manifest is not signed - "{0}"
        /// </summary>
        DeployManifestNotSigned,

        /// <summary>
        /// Deployment manifest contains invalid signature - "{0}"
        /// </summary>
        DeployManifestSignatureInvalid,

        /// <summary>
        /// Unable to locate Mage UI - "{0}"
        /// </summary>
        UnableToStartGUI,

        /// <summary>
        /// The -Algorithm option value must be "sha256RSA" or "sha1RSA" - "{0}"
        /// </summary>
        InvalidAlgorithmValue,

        /// <summary>
        /// This certificate cannot be used for signing - "{0}"
        /// </summary>
        InvalidCertUsage,

        /// <summary>
        /// This certificate does not contain a private key - "{0}"
        /// </summary>
        InvalidCertNoPrivateKey,

        /// <summary>
        /// The codebase is invalid - "{0}"
        /// </summary>
        InvalidCodebase,

        /// <summary>
        /// The -Install option must be "true", "false", "t", or "f" - "{0}"
        /// </summary>
        InvalidInstall,

        /// <summary>
        /// Unable to open certificate "{0}"
        /// </summary>
        UnableToOpenCertificate,

        /// <summary>
        /// Internal error.
        /// </summary>
        InternalError,

        /// <summary>
        /// Invalid minimum version.
        /// </summary>
        InvalidMinVersion,

        /// <summary>
        /// Invalid timestamp.
        /// </summary>
        InvalidTimestamp,

        /// <summary>
        /// ClearApplicationCache cannot be used with any other switch.
        /// </summary>
        NotAllowedCleanCache,

        /// <summary>
        /// The -IncludeProviderURL option must be "true", "false", "t", or "f" - "{0}"
        /// </summary>
        InvalidIncludeProviderURL,

        /// <summary>
        /// The -UseManifestForTrust option must be "true", "false", "t", or "f" - "{0}"
        /// </summary>
        InvalidUseManifestForTrust,

        /// <summary>
        /// The IncludeProviderURL option is set to true, but no deployment provider Url is provided.
        /// </summary>
        MissingDeploymentProviderUrl,

        /// <summary>
        /// The -WPFBrowserApp option must be "true", "false", "t", or "f" - "{0}"
        /// </summary>
        InvalidWPFBrowserApp,

        /// <summary>
        /// The WPFBrowserApp and Install options cannot be set to true at the same time
        /// </summary>
        InvalidWPFBrowserAppInstallCombination,

        /// <summary>
        /// The application manifest specified does not include the HostInBrowser tag required for a browser-hosted application
        /// </summary>
        ApplicationManifestMissingHostInBrowserTag,

        /// <summary>
        /// This certificate does not contain a private key - "{0}", if this is a public 
        /// key certificate, please provide cryptographic service provider and key container names.
        /// </summary>
        MissingCspOrContainer,

        /// <summary>
        /// 'verify' command can't be combined with any other command.
        /// </summary>
        VerifyIsExclusive,

        /// <summary>
        /// The UseManifestForTrust argument needs to be set to true when generating/updating an application manifest
        /// while a Publisher or SupportURL argument is provided.
        /// </summary>
        MissingUseApplicationManifestForTrustInfo
    }
}

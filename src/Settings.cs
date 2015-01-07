using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;

namespace TDriver {
    public struct AP_Subsection {
        //Constants
        private const string DEFAULT_RIGHT_FAX_COMMENT = "Sent via T-Driver";
        //Optional Fields
        public readonly string FaxComment;
        //Required fields
        public readonly string MoveFolder;
        public readonly string Name;
        public readonly string Password;
        public readonly string SendEmailFrom;
        public readonly string Server;
        public readonly string UserId;
        public readonly string WatchFolder;
        //Internal options.
        public bool DisableEmail;
        public bool DisableFax;
        public bool DisableMail;
        public bool IsValid;

        public AP_Subsection(SectionData section) {
            //Default Settings
            DisableEmail = false;
            DisableFax = false;
            DisableMail = false;
            IsValid = true;

            //Parse the following fields
            //Todo Check if required fields are null/invalid
            Name = section.SectionName;
            Server = section.Keys["Server"];
            UserId = section.Keys["User"];
            Password = section.Keys["Password"];
            WatchFolder = section.Keys["WatchFolder"];
            MoveFolder = section.Keys["MoveFolder"];
            SendEmailFrom = section.Keys["SendEmailFrom"];
            //Default to the default Rightfax comment if one is not defined.
            FaxComment = section.Keys["RightFaxComment"] ??
                         String.Format("{0} {1}", DEFAULT_RIGHT_FAX_COMMENT, Settings.AppVersion.ToString());

            if (!Directory.Exists(WatchFolder)) {
                IsValid = false;
            }
            if (!Directory.Exists(MoveFolder)) {
                IsValid = false;
            }
            /* User defined document type
                string tempDoctype = section.Keys["DocumentType"];
                if (Enum.TryParse(tempDoctype, true, out DocType)) {
                    DocType = (DocumentType) Enum.Parse(typeof(DocumentType), section.Keys["DocumentType"], true);
                }
                */
        }
    }


    internal static class Settings {
        public const short MAX_WATCHLIST_SIZE = 10; //Maximum number of folders to add to WatchList
        private const short MIN_FILE_DELAY_TIME = 1;
        public static String DatabaseFile;
        public static String ErrorLogfile;
        public static string EmailMsg;
        public static string PdfsPath;
        public static String SmtpServer;
        public static short EmailPort;
        public static Int16 FileDelayTime;
        public static List<AP_Subsection> WatchList;
        private static IniData _iniData;
        public static Version AppVersion;

        public static void Setup(String settingsIni) {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version;

            var iniFileParser = new FileIniDataParser();
            iniFileParser.Parser.Configuration.CommentString = "#";
            //Todo handle two or more sections with same name.
            _iniData = iniFileParser.ReadFile(settingsIni);

            //Load the general section variables
            if (!_iniData.Sections.ContainsSection("General"))
                ShowSettingsFileError("The 'General' section is missing from the settings file.");
            try {
                DatabaseFile = _iniData["General"]["Database"];
                ErrorLogfile = _iniData["General"]["ErrorFile"];
                SmtpServer = _iniData["General"]["SMTPServer"];
                PdfsPath = _iniData["General"]["PathToPDFs"];
                EmailMsg = _iniData["General"]["EmailMesssge"];
                EmailPort = Convert.ToInt16(_iniData["General"]["Port"]);
                FileDelayTime = Convert.ToInt16(_iniData["General"]["FileDelayTime"]);
                if (FileDelayTime < MIN_FILE_DELAY_TIME)
                    FileDelayTime = MIN_FILE_DELAY_TIME;
            }
            catch (Exception ex) {
                ShowSettingsFileError(ex.Message);
            }

            WatchList = new List<AP_Subsection>(MAX_WATCHLIST_SIZE);
            SetupWatchLists();
        }

        /// <summary>
        ///     Notifies the user of a settings file error then closes.
        /// </summary>
        /// <param name="infoString"></param>
        private static void ShowSettingsFileError(string infoString) {
            const string msgboxTitle = "Error with the settings file!";
            const string instructions =
                "Please verify the settings.ini file is correct and properly formated. Otherwise delete the file and run again to generate a template.";

            MessageBox.Show(infoString + Environment.NewLine + Environment.NewLine + instructions, msgboxTitle);
            Environment.Exit(-1);
        }

        /// <summary>
        ///     Create The WatchList using linq.
        /// </summary>
        private static void SetupWatchLists() {
            foreach (
                AP_Subsection newSubsection in
                    from section in _iniData.Sections
                    where section.SectionName != "General"
                    select new AP_Subsection(section)
                ) {
                WatchList.Add(newSubsection);
            }
        }

        /// <summary>
        ///     Create the settings template from the template stored within the executable
        /// </summary>
        public static void CreateSettingsTemplate(string settingsFile) {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TDriver.Settings.ini");
            // Null value check
            if (stream == null) {
                ShowSettingsFileError("Error generating template Settings.ini.");
                return;
            }

            //Copy the internal template to the application's folder.
            FileStream fileStream = new FileStream(settingsFile, FileMode.CreateNew);
            for (var i = 0; i < stream.Length; i++)
                fileStream.WriteByte((byte) stream.ReadByte());
            fileStream.Close();

            ShowSettingsFileError("New Settings.ini file generated.");
        }
    }
}
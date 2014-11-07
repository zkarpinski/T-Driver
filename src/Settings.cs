using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

namespace TDriver {
    public struct DPAType {
        public readonly string MoveFolder;
        public readonly string Name;
        public readonly string Password;
        public readonly string SendEmailFrom;
        public readonly string Server;
        public readonly string UserId;
        public readonly string WatchFolder;
        public readonly string FaxComment;

        public DPAType(SectionData section) {
            Name = section.SectionName;
            Server = section.Keys["Server"];
            UserId = section.Keys["User"];
            Password = section.Keys["Password"];
            WatchFolder = section.Keys["WatchFolder"];
            MoveFolder = section.Keys["MoveFolder"];
            SendEmailFrom = section.Keys["SendEmailFrom"];
            FaxComment = section.Keys["RightFaxComment"];
        }
    }


    internal class Settings {
        public readonly String ErrorFile;
        public readonly String LogFile;
        public readonly int FileDelayTime;
        private readonly IniData _iniData;
        public const short MaxWatchlistSize = 10; //Maximum number of folders to add to WatchList
        public readonly List<DPAType> WatchList;


        public Settings(String settingsIni) {
            if (File.Exists(settingsIni)) {
                var iniFileParser = new FileIniDataParser();
                iniFileParser.Parser.Configuration.CommentString = "#";
                _iniData = iniFileParser.ReadFile(settingsIni);
                LogFile = _iniData["General"]["LogFile"];
                ErrorFile = _iniData["General"]["ErrorFile"];
                //File delay setting added to compensate for duplicate file bug.
                try {
                    FileDelayTime = Convert.ToInt32(_iniData["General"]["FileDelayTime"]);
                }
                    //String in field
                catch (FormatException) {
                    FileDelayTime = 10000; //Set delay time to default 10seconds.
                }
                catch (OverflowException ) {
                    FileDelayTime = 10000; //Set delay time to default 10seconds.
                }
                
                WatchList = new List<DPAType>(MaxWatchlistSize);
                SetupWatchLists();
            }
            else {
                CreateSettingsTemplate(settingsIni);
                //TODO ADD Alert
            }
        }

        /// <summary>
        ///     Create The WatchList
        /// </summary>
        private void SetupWatchLists() {
            foreach (SectionData section in _iniData.Sections) {
                if (section.SectionName != "General") {
                    var newDPAType = new DPAType(section);
                    WatchList.Add(newDPAType);
                }
            }
        }

        /// <summary>
        ///     Create the settings template
        /// </summary>
        /// <param name="settingsIni">Path to settings file to create.</param>
        private void CreateSettingsTemplate(string settingsIni) {
            File.Create(settingsIni);
            //TODO Create settings template.
        }
    }
}
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

        public DPAType(SectionData section) {
            Name = section.SectionName;
            Server = section.Keys["Server"];
            UserId = section.Keys["User"];
            Password = section.Keys["Password"];
            WatchFolder = section.Keys["WatchFolder"];
            MoveFolder = section.Keys["MoveFolder"];
            SendEmailFrom = section.Keys["SendEmailFrom"];
        }
    }


    internal class Settings {
        public readonly String ErrorFile;
        public readonly String LogFile;
        private readonly IniData _iniData;
        public const short MaxWatchlistSize = 10; //Max Size Of WatchList
        public readonly List<DPAType> WatchList;


        public Settings(String settingsIni) {
            if (File.Exists(settingsIni)) {
                var iniFileParser = new FileIniDataParser();
                iniFileParser.Parser.Configuration.CommentString = "#";
                _iniData = iniFileParser.ReadFile(settingsIni);
                LogFile = _iniData["General"]["LogFile"];
                ErrorFile = _iniData["General"]["ErrorFile"];
                WatchList = new List<DPAType>(MaxWatchlistSize);
                SetupWatchLists();
            }
            else {
                CreateSettingsTemplate(settingsIni);
                //ADD Alert
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
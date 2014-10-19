using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using IniParser;
using IniParser.Model;

namespace TDriver
{

    public struct DPAType
    {

        public DPAType(IniParser.Model.SectionData section)
        {
            Name = section.SectionName;
            Server = section.Keys["Server"];
            UserId = section.Keys["User"];
            Password = section.Keys["Password"];
            WatchFolder = section.Keys["WatchFolder"];
            MoveFolder = section.Keys["MoveFolder"];
            SendEmailFrom = section.Keys["SendEmailFrom"];

        }
        public readonly string Name;
        public readonly string Server;
        public readonly string UserId;
        public readonly string Password;
        public readonly string WatchFolder;
        public readonly string MoveFolder;
        public readonly string SendEmailFrom;

    }


    class Settings
    {
        private const short MaxWatchlistSize = 10; //Max Size Of WatchList
        private IniData _iniData;

        public readonly String LogFile;
        public readonly String ErrorFile;
        public List<DPAType> WatchList;


        public Settings(String settingsIni)
        {
         if (File.Exists(settingsIni))
         {
             var iniFileParser = new FileIniDataParser();
             iniFileParser.Parser.Configuration.CommentString = "#";
             _iniData = iniFileParser.ReadFile(settingsIni);
             this.LogFile = _iniData["General"]["LogFile"];
             this.ErrorFile = _iniData["General"]["ErrorFile"];
             WatchList = new List<DPAType>(MaxWatchlistSize);
             SetupWatchLists();
         }
         else
         {
             this.CreateSettingsTemplate(settingsIni);
             //ADD Alert
         }
        }

        /// <summary>
        /// Create The WatchList
        /// </summary>
        private void SetupWatchLists()
        {
            foreach (var section in _iniData.Sections)
            {
                if (section.SectionName != "General")
                {
                    var newDPAType = new DPAType(section);
                    this.WatchList.Add(newDPAType);
                }
            }
        }

        /// <summary>
        /// Create the settings template
        /// </summary>
        /// <param name="settingsIni">Path to settings file to create.</param>
        private void CreateSettingsTemplate(string settingsIni)
        {
            File.Create(settingsIni);
            //TODO Create settings template.
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IniParser;
using IniParser.Model;

namespace TDriver
{

    class Settings
    {
        private IniData _iniData;

        public readonly String LogFile;
        public readonly String ErrorFile;

        public Settings(String settingsIni)
        {
         if (File.Exists(settingsIni))
         {
             var iniFileParser = new FileIniDataParser();
             iniFileParser.Parser.Configuration.CommentString = "#";
             _iniData = iniFileParser.ReadFile(settingsIni);

         }
         else
         {

             this.CreateSettingsTemplate(settingsIni);
         }
        }

        private void CreateSettingsTemplate(string settingsIni)
        {
            File.Create(settingsIni);
        }
    }
}

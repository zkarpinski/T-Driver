﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TDriver
{
    public class DPA {
        public string CustomerName { get; set; }
        public string Document { get; set; }
        public string Account { get; set; }
        public string FileName { get; set; }
        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public Boolean Rejected { get; set; }

        protected DPA(String document) {
            IsValid = true;
            Sent = false;
            Rejected = false;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            Account = RegexFileName(@"\d{5}-\d{5}");
        }

        protected DPA() {
            IsValid = true;
            Sent = false;
            Rejected = false;
        }

        protected string RegexFileName(string pattern)
        {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(FileName);
            if (matches.Count > 0)
            {
                return matches[0].Value;
            }
            IsValid = false;
            return "NOT_FOUND";
        }
    }

}
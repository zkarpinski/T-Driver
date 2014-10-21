using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TDriver
{
    /// <summary>
    /// Determines the DPA type and creates the corresponding object file.
    ///  </summary>
    /// <remarks>
    /// Valid File Names:
    ///     DPA-99999-99999-FaxTo-1-999-999-9999-To-NAME_HERE.doc
    ///     DPA-99999-99999.doc
    ///     
    /// 
    ///  </remarks>
    public class DPAFactory
    {

        public static DPA Create(string file) {
            //Verify it follows general naming convention
            String fileName = Path.GetFileNameWithoutExtension(file);
            String accountNumber = RegexFileName(@"\d{5}-\d{5}", fileName);
            String faxNumber = RegexFileName(@"\d{3}-\d{3}-\d{4}", fileName); //Without US-code

            //Check if Fax name
            if ((faxNumber != "NOT_FOUND") && (accountNumber != "NOT_FOUND"))
            {
                return new Fax(file);
            }
            
        


        //Create Word Object
                //Check if Email @
            //return new Email(file);
                //Check if Address City NY 11111

                //Check again for fax number
            return new Fax(file);
        }

        private static string RegexFileName(string pattern, string fileName)
        {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(fileName);
            if (matches.Count == 1) //SHOULD only be one match.
            {
                return matches[0].Value;
            }
            return "NOT_FOUND";
        }
    }
}

using System;
using System.Linq;
using DCSoft.RTF;

namespace TDriver {
    internal class Parser {
        /* Commented out: Idea for later (break Parser apart)
        //TODO Decide to load in a separate function or not.
        private RTFDomDocument _doc;
        public string LoadedFile { get; private set; }
        public DocumentType LoadedFiletype { get; private set; }

        //Loads the desired file into the parser.
        public bool Load(string file, DocumentType docType) {
            LoadedFiletype = docType;
            LoadedFile = file;
            _doc = new RTFDomDocument();
            _doc.Load(file);
            return true;
        }
        */

        /// <summary>
        ///     Loads the file and parses it depending on the DocumentType passed.
        /// </summary>
        /// <param name="file">File to be parsed.</param>
        /// <param name="docType">How the document should be parsed.</param>
        /// <returns>AP_Document derived class or NULL</returns>
        /// <remarks>
        ///     This is a dumb parser, as in
        ///     You MUST explicitly tell it which document type you are trying to parse.
        /// </remarks>
        public AP_Document FindData(string file, DocumentType docType) {
            //TODO Check if file in use
            RTFDomDocument domDoc = new RTFDomDocument();
            domDoc.Load(file);

            switch (docType) {
                case DocumentType.DPA:
                    return DPA_Parser(ref domDoc, file);
                case DocumentType.CME:
                    return CME_Parser(ref domDoc, file);
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Parser for CME Medical Documents
        /// </summary>
        /// <param name="cmeDoc"></param>
        /// <param name="file"></param>
        /// <returns>New MedicalCME class or NULL</returns>
        /// <remarks>
        ///     Table 1: Dr information: 5 Rows x 2 Columns (except 1st row, 3 columns)
        ///     Table 2: Customer Info:  5 Rows x 2 Columns
        /// </remarks>
        private AP_Document CME_Parser(ref RTFDomDocument cmeDoc, string file) {
            if (cmeDoc.Elements.OfType<RTFDomTable>().Count() == 2) {
                RTFDomTable[] docTables = cmeDoc.Elements.OfType<RTFDomTable>().ToArray();

                //Doctor Specific Information
                //1st table.
                RTFDomElementList drTableRows = docTables[0].Elements;
                if (drTableRows.Count != 5) return null; //Verify the correct number of rows exists.
                String drName = DataScrubber(drTableRows[1].Elements[0].InnerText.Replace("To: ", String.Empty));
                String drCompany =
                    DataScrubber(drTableRows[2].Elements[0].InnerText.Replace("Company/Department:", String.Empty));
                String drPhoneNumber =
                    DataScrubber(drTableRows[3].Elements[0].InnerText.Replace("Phone Number:", String.Empty));
                String drFaxNumber =
                    DataScrubber(drTableRows[4].Elements[0].InnerText.Replace("Fax Number: ", String.Empty));

                //Customer Specific Information
                //2nd table.
                RTFDomElementList custTableRows = docTables[1].Elements;
                if (custTableRows.Count != 5) return null; //Verify the correct number of rows exists.
                String accountHolder = DataScrubber(custTableRows[0].Elements[1].InnerText);
                String serviceAddress = DataScrubber(custTableRows[1].Elements[1].InnerText);
                String accountNumber = DataScrubber(custTableRows[2].Elements[1].InnerText);
                String patientName = DataScrubber(custTableRows[3].Elements[1].InnerText);
                //String lastVisit = DataScrubber(custTableRows[4].Elements[1].InnerText); //This field is blank. (Filled by Dr.)

                return
                    (new MedicalCME(drFaxNumber, drName, drPhoneNumber, drCompany, accountNumber, accountHolder,
                        patientName, serviceAddress, file));
            }
            return null;
        }

        /// <summary>
        ///     Parser for DPA Documents
        /// </summary>
        /// <param name="dpaDoc"></param>
        /// <param name="file"></param>
        /// <returns>New DPA class or NULL</returns>
        /// <remarks>
        ///     Table 1: 3 Rows x 2 Columns
        /// </remarks>
        private AP_Document DPA_Parser(ref RTFDomDocument dpaDoc, string file) {
            //Code is similar to CME_Parser but utilizes Linq. (Cause why not try out different things) =)
            if (dpaDoc.Elements.OfType<RTFDomTable>().Any()) {
                return (from table in dpaDoc.Elements.OfType<RTFDomTable>()
                    select table.Elements
                    into rows
                    let sendTo = DataScrubber(rows[0].Elements[0].InnerText)
                    let customer = DataScrubber(rows[1].Elements[0].InnerText)
                    let accountNumber = DataScrubber(rows[1].Elements[1].InnerText)
                    let serviceAddress = DataScrubber(rows[2].Elements[0].InnerText)
                    let dateOffered = DataScrubber(rows[2].Elements[1].InnerText)
                    select new DPA(accountNumber, sendTo, customer, serviceAddress, dateOffered, file)).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        ///     Cleans the string of unwanted characters. (carriage return and line feed)
        /// </summary>
        /// <param name="strData"></param>
        /// <returns>Cleansed source string.</returns>
        private string DataScrubber(string strData) {
            return strData.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }
    }
}
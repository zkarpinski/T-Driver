using System.Linq;
using DCSoft.RTF;

namespace TDriver {
    internal class Parser {
        

        public Parser() {
        }

        public DPA FindData(string file) {
            RTFDomDocument _domDoc = new RTFDomDocument();
            _domDoc.Load(file);
            if (_domDoc.Elements.OfType<RTFDomTable>().Any()) {
                return (from table in _domDoc.Elements.OfType<RTFDomTable>() select table.Elements into rows let sendTo = DataScrubber(rows[0].Elements[0].InnerText) let customer = DataScrubber(rows[1].Elements[0].InnerText) let accountNumber = DataScrubber(rows[1].Elements[1].InnerText) let serviceAddress = DataScrubber(rows[2].Elements[0].InnerText) let dateOffered = DataScrubber(rows[2].Elements[1].InnerText) select new DPA(accountNumber, sendTo, customer, serviceAddress, dateOffered, file)).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Cleans the string from unwanted characters. (carriage return and line feed)
        /// </summary>
        /// <param name="strData"></param>
        /// <returns>Cleansed source string.</returns>
        private string DataScrubber(string strData) {
            return strData.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
    }
}
using System;

namespace TDriver {
    //TODO Design email DPA
    public class Email : DPA {
        /// <summary>
        ///     Email constructor where data is parsed from filename.
        /// </summary>
        /// <param name="document"></param>
        public Email(String document) {
        }

        public string EmailAddress { get; set; }
    }
}
namespace TDriver {
    internal static class WorkFactory {
        /// <summary>
        ///     Creates the corresponding work object depending on the DeliveryMethod.
        /// </summary>
        /// <param name="dpa"></param>
        /// <param name="dpaType"></param>
        /// <returns>Work</returns>
        public static Work Create(DPA dpa, DPAType dpaType) {
            if (!dpa.IsValid) return null;
            switch (dpa.DeliveryMethod) {
                case DPA.DeliveryMethodTypes.Fax: //Fax
                    return new FaxWork((Fax) dpa, ref dpaType);
                case DPA.DeliveryMethodTypes.Email: //Email
                    return new EmailWork((Email) dpa, ref dpaType);
                case DPA.DeliveryMethodTypes.Mail: //Mail
                    return new MailWork((Mail) dpa, ref dpaType);
                default:
                    //TODO Add error log, unexpected deliveryMethod
                    return null;
            }
        }
    }
}
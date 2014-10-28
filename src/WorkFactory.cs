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
                    var work = new FaxWork((Fax) dpa, dpaType);
                    return work;
                case DPA.DeliveryMethodTypes.Email: //Email
                    //TODO add Email work
                    return null;
                case DPA.DeliveryMethodTypes.Mail: //Mail
                    //TODO add Mail work
                    return null;
                default:
                    //TODO Add error log, unexpected deliveryMethod
                    return null;
            }
        }
    }
}
namespace TDriver {
    internal static class WorkFactory {
        
        public static Work Create(DPA dpa, DPAType dpaType) {
            //Create a work request
            if (!dpa.IsValid) return null;
            switch (dpa.DeliveryMethod) {
                case DPA.DeliveryMethodTypes.Fax: //Fax
                    var work = new FaxWork((Fax) dpa, dpaType);
                    return work;
                    break;
                case DPA.DeliveryMethodTypes.Email: //Email
                    //TODO add Email work
                    return null;
                    break;
                case DPA.DeliveryMethodTypes.Mail: //Mail
                    //TODO add Mail work
                    return null;
                    break;
                case DPA.DeliveryMethodTypes.Err:
                default:
                    //TODO Add error log, unexpected deliveryMethod
                    return null;
                    break;
            }
        }
    }
}
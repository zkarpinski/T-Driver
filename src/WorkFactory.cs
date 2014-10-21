using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TDriver
{
    class WorkFactory
    {

        public static Work Create(DPA dpa, DPAType dpaType)
        {
            return new FaxWork((Fax)dpa,dpaType);
        }
    }
}

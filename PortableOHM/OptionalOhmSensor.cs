using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHMWrapper
{
    public class OptionalOhmSensor
    {
        private OHMSensor value;

        public OptionalOhmSensor(OHMSensor value)
        {
            this.value = value;
        }

        public bool isPresent()
        {
            return value != null;
        }

        public OHMSensor get()
        {
            return value;
        }
    }
}

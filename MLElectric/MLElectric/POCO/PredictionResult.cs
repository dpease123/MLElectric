using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLElectric.POCO
{
    [Serializable()]
    public class PredictionResult
    { 
        public float kWH_Usage { get; set; }
        public int Hour { get; set; }
    }
}

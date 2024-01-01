using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USD_EURO_Conversion_rate
{
    public class RateData
    {
        public DateTime TransactionDate { get; set; }
        public float HistoricalRate { get; set; }
    }


    public class RatePrediction
    {
        
        public float[] CurrentRate { get; set; }

        
    }




}

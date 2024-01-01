using Microsoft.ML;
using System;
using System.IO;
using USD_EURO_Conversion_rate;

namespace PriceModelsTrainer
{
    class Program
    {


        static readonly string dataPath = "E:\\ECB_Euro_Dollar_Data22.csv";
        static void Main(string[] args)
        {
            
            try
            {

                //Time Series using Single Spectrum Analysis
                var mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment



                Console.WriteLine("Forecast using Time Series SSA estimation");

                TimeSeriesModelHelper.PerformTimeSeriesRateForecasting(mlContext, dataPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Press Enter to continue...");

            Console.ReadLine();

        
        
        
        }

    }
}




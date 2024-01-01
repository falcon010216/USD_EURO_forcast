using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace USD_EURO_Conversion_rate
{
    public  class TimeSeriesModelHelper
    {
        public static void PerformTimeSeriesRateForecasting(MLContext mlContext, string dataPath)
        {
            Console.WriteLine("=============== Forecasting Rate ===============");

            
            DateTime Mydate = DateTime.Now;
            ForecastRate(mlContext, Mydate, dataPath);
        }

        private static void ForecastRate(MLContext mlContext, DateTime dateId, string dataPath)
        {
            var rateModelPath = "Rate{dateId}_month_timeSeriesSSA.zip";

            if (File.Exists(rateModelPath))
            {
                File.Delete(rateModelPath);
            }

            IDataView RateDataView =  LoadData(mlContext, dataPath);

            




            FitAndSaveModel(mlContext, RateDataView, rateModelPath);
            TestPrediction(mlContext, rateModelPath);
        }


        private static IDataView LoadData(MLContext mlContext, string dataPath)
        {

            List<RateData> infoList = new List<RateData>();
            // populate list
            infoList = FileParser(infoList);
            IDataView data = mlContext.Data.LoadFromEnumerable<RateData>(infoList);
            
            
            return data;
        }



        private static void FitAndSaveModel(MLContext mlContext, IDataView RateDataSeries, string outputModelPath)
        {
            Console.WriteLine("Fitting rate forecasting Time Series model");

            const int numRateDataPoints = 6461; //The underlying data has a total of 20 years worth of data

            // Create and add the forecast estimator to the pipeline. 
            var forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(RatePrediction.CurrentRate),
                inputColumnName: nameof(RateData.HistoricalRate), // This is the column being forecasted.
                windowSize: 100, 
                seriesLength: numRateDataPoints, // This parameter specifies the number of data points that are used when performing a forecast.
                trainSize: numRateDataPoints, // This parameter specifies the total number of data points in the input time series, starting from the beginning.
                horizon: 1, // Indicates the number of values to forecast; 
                confidenceLevel: 0.95f );// Indicates the likelihood the real observed value will fall within the specified interval bounds.

            SsaForecastingTransformer forecaster = forecastEstimator.Fit(RateDataSeries);
            

            var forecastEngine = forecaster.CreateTimeSeriesEngine<RateData, RatePrediction>(mlContext);
            
            forecastEngine.CheckPoint(mlContext, outputModelPath);
        }
        
        private static void TestPrediction(MLContext mlContext, string outputModelPath)
        {
            Console.WriteLine("Testing exchange rate forecast Time Series model");

            // Load the forecast engine that has been previously saved.
            ITransformer forecaster;
            using (var file = File.OpenRead(outputModelPath))
            {
                forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
            }

            // We must create a new prediction engine from the persisted model.
            TimeSeriesPredictionEngine<RateData, RatePrediction> forecastEngine = forecaster.CreateTimeSeriesEngine<RateData, RatePrediction>(mlContext);

            // Get the prediction; this will include the forecast estimator was originally created.
            Console.WriteLine("\n** Original prediction **");
            RatePrediction originalRatePrediction = forecastEngine.Predict();

            foreach (var myvalue in originalRatePrediction.CurrentRate)
            {
                Console.WriteLine(myvalue);
                
            }





        }


        public static DataTable BuildDataTable<T>(IList<T> lst)
        {
            //create DataTable Structure
            DataTable tbl = CreateTable<T>();
            Type entType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entType);
            //get the list item and add into the list
            foreach (T item in lst)
            {
                DataRow row = tbl.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item);
                }
                tbl.Rows.Add(row);
            }
            return tbl;
        }

        private static DataTable CreateTable<T>()
        {
            //T –> ClassName
            Type entType = typeof(T);
            //set the datatable name as class name
            DataTable tbl = new DataTable(entType.Name);
            //get the property list
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entType);
            foreach (PropertyDescriptor prop in properties)
            {
                //add property as column
                tbl.Columns.Add(prop.Name, prop.PropertyType);
            }
            return tbl;
        }

        static List<RateData> FileParser(List<RateData> infoList)
        {

            using (TextFieldParser textFieldParser = new TextFieldParser("C:\\Users\\FaisalAmin\\source\\repos\\USD_EURO_forcast\\data\\ECB_Euro_Dollar_Data22.csv"))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters(",");
                string myline = "NoData";
                while (!textFieldParser.EndOfData)
                {
                    string[] rows = { "zero", "one", "two" };
                    myline = textFieldParser.ReadLine();
                    rows = myline.Split(',');

                    if (rows.Length == 3)
                    {
                        RateData mydata = new RateData();
                        DateTime timestamp;
                        bool success = DateTime.TryParseExact(rows[0], "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out timestamp);
                        mydata.TransactionDate = timestamp;
                        float myfloat = float.Parse(rows[2], CultureInfo.InvariantCulture);
                        mydata.HistoricalRate = myfloat;
                        infoList.Add(mydata);


                    }


                }
            }
            return infoList;
        }


    }
}

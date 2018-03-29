using System;
using System.Configuration;
using System.IO;
using System.CodeDom
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SpeechToText.AzureFunction.BingSpeech;
using SpeechToText.AzureFunction.Models;

namespace SpeechToText.Function
{
    public static class SpeechToText
    {
        [FunctionName("SpeechToText")]
        public static void Run([BlobTrigger("audiofiles/{name}", Connection = "AudioStorage")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info($"Speech2Text Function Start: Name:{name} \n Size: {myBlob.Length} Bytes");
            
            string bingSpeechSubscriptionKey = ConfigurationManager.AppSettings["BingSpeechSubscriptionKey"];
            string resultAzureTableName = ConfigurationManager.AppSettings["ResultAzureTableName"];
            string resultAzureTableConnectionString = ConfigurationManager.AppSettings["audioStorage"];

            try
            {
                // Bing Speech API convertion Speech to Text
                var converter = new SpeechConverterService(log, bingSpeechSubscriptionKey);
                var convertedLines = converter.DoRequest(myBlob).Result;

                // Store the result in an Azure Table
                if(convertedLines.Any())
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(resultAzureTableConnectionString);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference(resultAzureTableName);
                    table.CreateIfNotExistsAsync().Wait();

                    var result = new SpeechToTextResultEntity("Speech2TextFunction", name)
                    {
                        ConvertedText = string.Join("\n", convertedLines)
                    };

                    TableOperation insertOperation = TableOperation.Insert(result);
                    table.ExecuteAsync(insertOperation).Wait();
                }
            }
            catch (Exception e)
            {
                log.Info(e.Message);
            }
            
            log.Info($"Speech2Text Function End: Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}

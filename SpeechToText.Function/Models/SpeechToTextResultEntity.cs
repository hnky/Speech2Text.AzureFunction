using Microsoft.WindowsAzure.Storage.Table;

namespace SpeechToText.AzureFunction.Models
{
    public class SpeechToTextResultEntity : TableEntity
    {
        public SpeechToTextResultEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public SpeechToTextResultEntity() { }

        public string ConvertedText { get; set; }

    }
}

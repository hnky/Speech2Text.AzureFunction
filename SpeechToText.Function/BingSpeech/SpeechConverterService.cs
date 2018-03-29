using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Bing.Speech;

namespace SpeechToText.AzureFunction.BingSpeech
{
    public class SpeechConverterService
    {

        private readonly string _locale = "en-US";
        private readonly Uri _serviceUrl = new Uri(@"wss://speech.platform.bing.com/api/service/recognition/continuous");
        private readonly TraceWriter _log;
        private readonly string _subscriptionKey;
        private List<string> _lines;
        private static readonly Task CompletedTask = Task.FromResult(true);

        public SpeechConverterService(TraceWriter log, string subscriptionKey)
        {
            _log = log;
            _subscriptionKey = subscriptionKey;
            _lines = new List<string>();
        }

        public async Task<List<string>> DoRequest(Stream audioFile)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var preferences = new Preferences(_locale, _serviceUrl, new CognitiveServicesAuthorizationProvider(_subscriptionKey));

            using (var speechClient = new SpeechClient(preferences))
            {
                speechClient.SubscribeToRecognitionResult(this.OnRecognitionResult);

                var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Ethernet, OsName.Windows, "1607", "Dell", "T3600");
                var applicationMetadata = new ApplicationMetadata("SampleApp", "1.0.0");
                var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "SampleAppService");

                await speechClient.RecognizeAsync(new SpeechInput(audioFile, requestMetadata), cts.Token).ConfigureAwait(false);
            }

            return _lines;
        }

        public Task OnRecognitionResult(RecognitionResult args)
        {
            var response = args;

            if (response.Phrases != null && response.Phrases.Any())
            {
                _log.Info($"OnRecognitionResult: {response.Phrases.First().DisplayText}");
                _lines.Add(response.Phrases.First().DisplayText);
            }

            return CompletedTask;
        }

    }
}

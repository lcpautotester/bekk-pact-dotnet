using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bekk.Pact.Common;
using Bekk.Pact.Common.Contracts;
using Bekk.Pact.Common.Exceptions;
using Bekk.Pact.Common.Extensions;
using Bekk.Pact.Consumer.Contracts;
using Bekk.Pact.Consumer.Rendering;

namespace Bekk.Pact.Consumer.Repo
{
    class PactRepo : Common.Utils.PactRepoBase
    {
        public PactRepo(IConfiguration configuration): base(configuration)
        {
        }

        public async Task Put(IPactDefinition pact)
        {
            await PutPacts(pact ,new PactJsonRenderer(pact).ToString());
        }

        private async Task PutPacts(IPactPathMetadata metadata, string payload)
        {
            var broker = PublishToBroker(metadata, payload);
            var filesystem = PublishToFilesystem(metadata, payload);
            await Task.WhenAll(new[]{broker, filesystem});
        }

        private async Task PublishToFilesystem(IPactPathMetadata metadata, string payload)
        {
            if(Configuration.PublishPath == null) return;
            var folder = Path.Combine(Configuration.PublishPath, "pacts", metadata.Consumer, metadata.Provider);
            var filename = $"{metadata.Consumer}_{metadata.Provider}_{metadata.Version}.json";
            var filePath = Path.Combine(folder, filename);
            Directory.CreateDirectory(folder);
            using(var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                var encoded = Encoding.UTF8.GetBytes(payload);
                await file.WriteAsync(encoded, 0, encoded.Length);
            }
            Configuration.LogSafe(LogLevel.Info, $"Saved pact to {filePath}.");
        }
        private async Task PublishToBroker(IPactPathMetadata metadata, string payload)
        {
            if(Configuration.BrokerUri == null) return;
            var uri = new Uri(Configuration.BrokerUri, $"/pacts/provider/{metadata.Provider}/consumer/{metadata.Consumer}/version/{metadata.Version}");
            var client = Client;
            HttpResponseMessage result;
            try
            {
                result = await client.PutAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, "application/json"));
            }
            catch(Exception e)
            {
                var exception = e.InnerException??e;
                throw new PactException($"Error when connecting to broker <{uri}>: {exception.Message}", exception);
            }
            if(!result.IsSuccessStatusCode)
            {
                Configuration.LogSafe(LogLevel.Error, $"Broker replied with {(int)result.StatusCode}: {result.ReasonPhrase}");
                Configuration.LogSafe(LogLevel.Verbose, await result.Content.ReadAsStringAsync());
                throw new PactRequestException("Couldn't put pact to broker.", result);
            }
            Configuration.LogSafe(LogLevel.Info, $"Uploaded pact to {uri}.");
        }
    }
}
using System;
using Bekk.Pact.Common.Contracts;

namespace Bekk.Pact.Common.Utils
{
    public abstract class Configuration<T> : IConfiguration where T: class
    {
        private Uri brokerUri;
        private Action<string> log;
        
        public T BrokerUri(Uri uri)
        {
            brokerUri = uri;
            return this as T;
        }
        
        public T BrokerUri(Uri serverUri, string providerName) => BrokerUri(new Uri(serverUri,
            $"/pacts/provider/{providerName}/latest"));
            
        public T Log(Action<string> log){
            this.log = log;
            return this as T;
        }
        Action<string> IConfiguration.Log => log;
        Uri IConfiguration.BrokerUri => brokerUri;
    }
}
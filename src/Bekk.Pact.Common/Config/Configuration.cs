using System;
using System.IO;
using Bekk.Pact.Common.Contracts;

namespace Bekk.Pact.Common.Config
{
    public abstract class Configuration<T> : IConfiguration where T: class
    {
        protected Configuration(IConfiguration inner)
        {
            if(inner != null)
            {
                Inner = inner;
                brokerUri = Inner.BrokerUri;
                brokerUserName = Inner.BrokerUserName;
                brokerPassword = Inner.BrokerPassword;
                publishPath = Inner.PublishPath;
                log = Inner.Log;
                logLevel = Inner.LogLevel.GetValueOrDefault(logLevel);
                logFile = Inner.LogFile;
            }
            Log(Console.WriteLine);
        }
        private Uri brokerUri;
        private string brokerUserName;
        private string brokerPassword;
        private string publishPath;
        private Action<string> log;
        private LogLevel logLevel = Bekk.Pact.Common.Contracts.LogLevel.Scarce;
        private string logFile;
        protected IConfiguration Inner { get; }

        /// <summary>
        /// Sets the values of <see cref="IConfiguration.BrokerUserName" /> and <see cref="IConfiguration.BrokerPassword" />.
        /// </summary>
        public T BrokerCredentials(string userName, string password)
        {
            brokerUserName = Inner?.BrokerUserName ?? userName;
            brokerPassword = Inner?.BrokerPassword ?? password;
            return this as T;
        }
        string IConfiguration.BrokerUserName => brokerUserName;
        string IConfiguration.BrokerPassword => brokerPassword;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.BrokerUri"/>
        /// </summary>
        public T BrokerUri(Uri uri)
        {
            brokerUri = Inner?.BrokerUri ?? uri;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.BrokerUri"/>.
        /// Must be parsable to an absolute uri.
        /// </summary>
        public T BrokerUrl(string url) => BrokerUri(new Uri(url));
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.Log"/>.
        /// </summary>
        public T Log(Action<string> log){
            this.log = Inner?.Log ?? log;
            return this as T;
        }
        Action<string> IConfiguration.Log => log;
        Uri IConfiguration.BrokerUri => brokerUri;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.PublishPath"/>.
        /// </summary>
        public T PublishPath(string path)
        {
            publishPath = Inner?.PublishPath ?? path;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.PublishPath"/>.
        /// </summary>
        /// <param name="path">A path appended to the temp path.</param>
        public T PublishPathInTemp(string path = null) => PublishPath(path == null ? Path.GetTempPath() : Path.Combine(Path.GetTempPath(), path));
        string IConfiguration.PublishPath => publishPath;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogLevel"/>.
        /// </summary>
        public T LogLevel(LogLevel level)
        {
            logLevel = Inner?.LogLevel ?? level;
            return this as T;
        }
        LogLevel? IConfiguration.LogLevel => logLevel;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogFile"/>.
        /// </summary>
        public T LogFile(string path)
        {
            logFile = Inner?.LogFile ?? path;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogFile"/>.
        /// </summary>
        /// <param name="filename">The filename of the logfile in the temp folder.</param>
        public T LogFileInTemp(string filename) => LogFile(Path.Combine(Path.GetTempPath(), filename));
        string IConfiguration.LogFile => logFile;
    }
}
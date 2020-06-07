
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;

namespace GeneralConsole
{
    public class GeneralConsoleStarter
    {
        [JsonProperty("IsServer")]
        public int IsServer { get; set; }
        [JsonProperty("Host")]
        public string Host { get; set; }
        [JsonProperty("Port")]
        public int Port { get; set; }
    }
    class Configure
    {
        static GeneralConsoleStarter starter = null;
        public static void Init(string fileName)
        {
            if (starter == null)
            {
                starter = JsonConvert.DeserializeObject<GeneralConsoleStarter>(File.OpenText(fileName).ReadToEnd());
            }
        }
        public static GeneralConsoleStarter GetInstance()
        {
            if (starter == null)
            {
                starter = JsonConvert.DeserializeObject<GeneralConsoleStarter>(File.OpenText("config.json").ReadToEnd());
            }
            return starter;
        }
    }
}

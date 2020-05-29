using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
namespace suflor
{
    class Suflor : IDisposable
    {
        public void Dispose()
        {

        }

        [JsonConstructor]
        public Suflor(bool a = true)
        {

        }
        public Suflor()
        {
 
        }
        public Suflor(string path)
        {
            using (StreamReader streamReader = new StreamReader(path))
            using (Suflor options = JsonConvert.DeserializeObject<Suflor>(streamReader.ReadToEnd()))
            {
                foreach (var property in typeof(Suflor).GetProperties())
                {
                    try
                    {
                        property.SetValue(this, property.GetValue(options));
                    }
                    catch { }
                }
            }
            SendText = "";
        }
        public void Save(string path)
        {
            SendText = "";
            lock (this)
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.ContractResolver = new WritablePropertiesOnlyResolver();
                    serializer.Converters.Add(new StringEnumConverter());
                    serializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(jsonWriter, this);
                    jsonWriter.Flush();
                }
            }
        }
        internal class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
        public  Dictionary<string, string> items { get; set; }= new Dictionary<string, string>();
        public int time { get; set; } = 110;
        public int timeSpace { get; set; } = 10;
        public int timeDot { get; set; } = 10;
        public int timeNew { get; set; } = 10;
        public string SendText="";
        public string processName { get; set; }
        public System.Drawing.Font Font { get; set; }
    }
}

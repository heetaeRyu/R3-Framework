using Newtonsoft.Json;

namespace Netmarble.Core
{
	public class JsonDotNet : JsonWrapper
	{
		public override T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeAnonymousType<T>(json, new T());
		}

		public override string Serialize(object json, bool pretty = false)
		{
			return JsonConvert.SerializeObject(json, pretty ? Formatting.Indented : Formatting.None);
		}
	}
}
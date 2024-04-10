using System.Collections.Concurrent;
using System.ComponentModel;

namespace Netmarble.Core
{
	public class StateFreeBackgroundWorker : BackgroundWorker
	{
		private ConcurrentDictionary<string, object> _customDataDic;

		public StateFreeBackgroundWorker()
		{
			_customDataDic = new ConcurrentDictionary<string, object>();
		}

		public void SetCustomData(string key, object data)
		{
			_customDataDic[key] = data;
		}

		public bool ContainsCutomData(string key)
		{
			return _customDataDic.ContainsKey(key);
		}

		public bool GetCustomData(string key, out object data)
		{
			return _customDataDic.TryGetValue(key, out data);
		}

		public bool PopCustomData(string key, out object data)
		{
			return _customDataDic.TryRemove(key, out data);
		}

		public bool RemoveCustomData(string key)
		{
			return _customDataDic.TryRemove(key, out object data);
		}
	}
}
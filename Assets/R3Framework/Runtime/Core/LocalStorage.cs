using System;
using UnityEngine;

namespace Netmarble.Core
{
    public static class LocalStorage
    {
        public static void Reset()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Set(object value)
        {
            PlayerPrefs.SetString(value.GetType().FullName, JsonProxy.Serialize(value));
        }

        public static T Get<T>() where T : LocalData, new()
        {
            T inst;
            var json = PlayerPrefs.GetString(typeof(T).FullName, null);
            if (string.IsNullOrEmpty(json))
            {
                inst = new T();
                Set(inst);
            }
            else
            {
                inst = JsonProxy.Deserialize<T>(json);
            }

            return inst;
        }

        public static void Delete<T>() where T : LocalData, new()
        {
            PlayerPrefs.DeleteKey(typeof(T).FullName);
        }
    }

    [Serializable]
    public class LocalData
    {
        public void Update()
        {
            LocalStorage.Set(this);
        }
    }
}
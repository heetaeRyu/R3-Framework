using System;
using UnityEngine;

namespace Netmarble.Core
{
    public static class JsonProxy
    {
        private static JsonWrapper _module = new JsonDotNet();

        public static JsonWrapper Module
        {
            set => _module = value;
        }

        public static T DeserializePath<T>(string path) where T : new()
        {
            string json = FileReference.Read(path);
            return Deserialize<T>(json);
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            if (_module == null)
                throw new ArgumentNullException(nameof(JsonProxy) + ".Module is NULL, You must be set module first");
            else return _module.Deserialize<T>(json);
        }

        public static void SerializePath(string path, object json, bool pretty = false)
        {
            string jsonData = Serialize(json, pretty);
            FileReference.Write(path, jsonData, true);
        }

        public static string Serialize(object json, bool pretty = false)
        {
            if (_module == null)
                throw new ArgumentNullException(nameof(JsonProxy) + ".Module is NULL, You must be set module first");
            else return _module.Serialize(json, pretty);
        }
    }

    public abstract class JsonWrapper
    {
        public abstract T Deserialize<T>(string json) where T : new();
        public abstract string Serialize(object json, bool pretty = false);
    }

    public class JsonUnity : JsonWrapper
    {
        public override T Deserialize<T>(string json)
        {
            //Debug.Log("You use JsonUtility Parser. We do not ReCommand use this. Set the other module in JsonProxy.Module");
            return JsonUtility.FromJson<T>(json);
        }

        public override string Serialize(object json, bool pretty = false)
        {
            //Debug.Log("You use JsonUtility Parser. We do not ReCommand use this. Set the other module in JsonProxy.Module");
            return JsonUtility.ToJson(json);
        }
    }
}
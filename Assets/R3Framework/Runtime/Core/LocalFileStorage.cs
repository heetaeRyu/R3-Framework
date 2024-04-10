using System.IO;
using System.Collections.Generic;

namespace Netmarble.Core
{
    public class LocalFileStorage<T> where T : new()
    {
        private static readonly Dictionary<string, LocalFileStorage<T>> Dic =
            new Dictionary<string, LocalFileStorage<T>>();

        public static LocalFileStorage<T> GetStorage(string path)
        {
            LocalFileStorage<T> storage;
            if (Dic.TryGetValue(path, out LocalFileStorage<T> localFileStorage))
            {
                storage = localFileStorage;
            }
            else
            {
                storage = new LocalFileStorage<T>(path);
            }

            return storage;
        }

        public static T Get(string path)
        {
            return GetStorage(path).Data;
        }

        public static void Set(string path, T value)
        {
            LocalFileStorage<T> storage;
            if (Dic.TryGetValue(path, out LocalFileStorage<T> localFileStorage))
            {
                storage = localFileStorage;
            }
            else
            {
                storage = new LocalFileStorage<T>(path);
            }

            storage.Data = value;
        }

        private string _path;

        private T _instance;

        public T Data
        {
            get
            {
                if (this._instance == null)
                {
                    if (File.Exists(this._path))
                    {
                        this._instance = JsonProxy.DeserializePath<T>(this._path);
                    }
                    else
                    {
                        this._instance = default(T);
                    }
                }

                return this._instance;
            }
            set
            {
                if (value == null) return;
                this._instance = value;
                this.Save();
            }
        }

        public LocalFileStorage(string jsonPath)
        {
            this._path = jsonPath;
            if (!Dic.ContainsKey(this._path))
            {
                Dic[this._path] = this;
            }
        }

        public void Save()
        {
            JsonProxy.SerializePath(this._path, this._instance, true);
        }

        public void SaveAs(string path)
        {
            JsonProxy.SerializePath(path, this._instance);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Netmarble.Core
{
    public interface IObjectPoolable : IDisposable
    {
        void Initialize();
    }

    public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : IObjectPoolable, new()
    {
        private readonly Queue<T> _queue = new Queue<T>();

        public int Capacity => this._queue.Count;

        public ObjectPool<T> SetDefaultObjectCount(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                this._queue.Enqueue(this.GetInstance());
            }

            return this;
        }

        private T GetInstance()
        {
            return new T();
        }

        public T Pull()
        {
            T instance = this._queue.Count > 0 ? this._queue.Dequeue() : this.GetInstance();
            instance.Initialize();
            return instance;
        }

        public void Push(T instance)
        {
            instance.Dispose();
            this._queue.Enqueue(instance);
        }
    }
}
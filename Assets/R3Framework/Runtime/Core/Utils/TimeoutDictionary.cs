using System.Collections.Generic;
using System.Timers;
using System;

namespace Netmarble.Core
{
    public class TimeoutDictionary<TKey, TValue>
    {
        private class Expirable<T>
        {
            private readonly T _value;
            private readonly Timer _timer;
            private readonly Action<Expirable<T>> _expirationCallback;

            private void OnElapsed(object sender, ElapsedEventArgs e)
            {
                _timer.Stop();
                CoreUtil.MainThreadCallAsync(() => { _expirationCallback?.Invoke(this); });
            }

            public Expirable(T value, int expiration, Action<Expirable<T>> expirationCallback)
            {
                _value = value;
                _expirationCallback = expirationCallback;
                _timer = new Timer(expiration);
                _timer.Elapsed += OnElapsed;
                _timer.AutoReset = false;
                _timer.Start();
            }

            public T GetValue()
            {
                _timer.Stop();
                return _value;
            }

        }

        private readonly Dictionary<TKey, Expirable<TValue>> _dicKeyValue;
        private readonly Dictionary<Expirable<TValue>, TKey> _dicValueKey;
        private int _timeout;

        public delegate void TimeoutDelegate(TKey key, TValue value);

        public TimeoutDelegate onTimeout;

        public int Timeout
        {
            get => _timeout;
            set => _timeout = value;
        }

        public TimeoutDictionary(int timeout)
        {
            _timeout = timeout;
            _dicKeyValue = new Dictionary<TKey, Expirable<TValue>>();
            _dicValueKey = new Dictionary<Expirable<TValue>, TKey>();
        }

        private void OnExpiration(Expirable<TValue> expirable)
        {
            if (_dicValueKey.TryGetValue(expirable, out TKey key))
            {
                _dicKeyValue.Remove(key);
                _dicValueKey.Remove(expirable);
                //Debug.Log("on expire :: kvdic count : " + _dicKeyValue.Count +"  vkdic count : " + _dicValueKey.Count);
                onTimeout?.Invoke(key, expirable.GetValue());
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!_dicKeyValue.ContainsKey(key))
            {
                var expirable = new Expirable<TValue>(value, _timeout, OnExpiration);
                _dicKeyValue.Add(key, expirable);
                _dicValueKey.Add(expirable, key);
            }
        }

        public TValue Get(TKey key)
        {
            if (_dicKeyValue.TryGetValue(key, out Expirable<TValue> expirable))
            {
                return expirable.GetValue();
            }

            return default(TValue);
        }

        public bool Remove(TKey key)
        {
            if (_dicKeyValue.TryGetValue(key, out Expirable<TValue> expirable))
            {
                bool isRemoved = _dicKeyValue.Remove(key) && _dicValueKey.Remove(expirable);
                //Debug.Log("after remove :: kvdic count : " + _dicKeyValue.Count +"  vkdic count : " + _dicValueKey.Count);
                return isRemoved;
            }
            else return false;
        }
    }
}
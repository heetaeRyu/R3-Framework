using System;
using System.Collections.Generic;
using MessagePipe;

namespace Netmarble.Core
{
    public class EventData : IObjectPoolable
    {
        public static EventData New => ObjectPool<EventData>.Instance.Pull();

        public Enum type;
        public object data;

        public T Get<T>()
        {
            T inst = default(T);
            try
            {
                inst = (T)data;
            }
            catch (Exception)
            {
                // ignored

            }

            if (inst == null)
            {
                throw new ArgumentException(type + " is need type of '" + typeof(T) + "' data");
            }

            return inst;
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {
            this.type = null;
            this.data = null;
        }

        public void Release()
        {
            ObjectPool<EventData>.Instance.Push(this);
        }
    }

    public enum AppFacadeEvent
    {
        Startup
    }

    public class DisposableListeners : IObjectPoolable
    {
        public Enum type;
        public IDisposable disposable;

        public DisposableListener listener;
        public DisposableDataListener listenerData;

        public void Initialize()
        {

        }

        public void Execute(EventData data)
        {
            listener?.Execute(data);
            listenerData?.Execute(data);
        }

        public bool Disposable()
        {
            if (listener != null)
                if (!listener.Disposable())
                    return false;

            if (listenerData != null)
                if (!listenerData.Disposable())
                    return false;

            this.disposable.Dispose();
            return true;
        }

        public void Dispose()
        {
            this.type = null;
            this.disposable = null;

            if (listener != null) AppFacade.POOL_LISTENER.Push(this.listener);
            if (listenerData != null) AppFacade.POOL_LISTENER_DATA.Push(this.listenerData);
        }
    }

    public abstract class DisposableListenerBase : IObjectPoolable
    {
        public abstract void Execute(EventData data);
        public abstract bool Disposable();

        public virtual void Initialize()
        {

        }

        public virtual void Dispose()
        {

        }
    }

    public class DisposableListener : DisposableListenerBase
    {
        public Action listeners;
        public Action listenersOnce;

        public void AddListener(Action listener, bool executeOnce)
        {
            if (executeOnce)
            {
                if (listenersOnce == null)
                {
                    this.listenersOnce = listener;
                }
                else
                {
                    this.listenersOnce -= listener;
                    this.listenersOnce += listener;
                }
            }
            else
            {
                if (listeners == null)
                {
                    this.listeners = listener;
                }
                else
                {
                    this.listeners -= listener;
                    this.listeners += listener;
                }
            }
        }

        public override void Execute(EventData data)
        {
            listeners?.Invoke();
            if (listenersOnce != null)
            {
                listenersOnce();
                listenersOnce = null;
            }
        }

        public void RemoveListener(Action listener)
        {
            this.listeners -= listener;
            this.listenersOnce -= listener;
        }

        public override bool Disposable()
        {
            return this.listeners == null && this.listenersOnce == null;
        }

        public override void Dispose()
        {
            this.listeners = null;
            this.listenersOnce = null;
        }
    }

    public class DisposableDataListener : DisposableListenerBase
    {
        public Action<EventData> listeners;
        public Action<EventData> listenersOnce;

        public void AddListener(Action<EventData> listener, bool executeOnce)
        {
            if (executeOnce)
            {
                if (listenersOnce == null)
                {
                    this.listenersOnce = listener;
                }
                else
                {
                    this.listenersOnce -= listener;
                    this.listenersOnce += listener;
                }
            }
            else
            {
                if (listeners == null)
                {
                    this.listeners = listener;
                }
                else
                {
                    this.listeners -= listener;
                    this.listeners += listener;
                }
            }
        }

        public override void Execute(EventData data)
        {
            listeners?.Invoke(data);
            if (listenersOnce != null)
            {
                listenersOnce(data);
                listenersOnce = null;
            }
        }

        public void RemoveListener(Action<EventData> listener)
        {
            this.listeners -= listener;
            this.listenersOnce -= listener;
        }

        public override bool Disposable()
        {
            return this.listeners == null && this.listenersOnce == null;
        }

        public override void Dispose()
        {
            this.listeners = null;
            this.listenersOnce = null;
        }
    }

    public class AppFacade : Singleton<AppFacade>
    {
        public static readonly ObjectPool<DisposableListeners> POOL_LISTENERS =
            ObjectPool<DisposableListeners>.Instance.SetDefaultObjectCount(10);

        public static readonly ObjectPool<DisposableListener> POOL_LISTENER =
            ObjectPool<DisposableListener>.Instance.SetDefaultObjectCount(10);

        public static readonly ObjectPool<DisposableDataListener> POOL_LISTENER_DATA =
            ObjectPool<DisposableDataListener>.Instance.SetDefaultObjectCount(10);

        public static void Notification(Enum eventType)
        {
            Instance.SendNotification(eventType);
        }

        public static void Notification(Enum eventType, object data)
        {
            Instance.SendNotification(eventType, data);
        }

        public static void Register(Enum type, Action listener)
        {
            Register(type, listener, true);
        }

        public static void Register(Enum type, Action listener, bool executeOnce)
        {
            Dictionary<Enum, DisposableListeners> eventDic = Instance._eventDic;
            DisposableListeners listeners;
            if (eventDic.TryGetValue(type, out DisposableListeners value))
            {
                listeners = value;
                if (listeners.listener == null)
                {
                    listeners.listener = POOL_LISTENER.Pull();
                }
            }
            else
            {
                listeners = POOL_LISTENERS.Pull();
                listeners.listener = POOL_LISTENER.Pull();
                listeners.disposable = Instance._subscriber
                    .Subscribe(x =>
                    {
                        if (x.type.Equals(type))
                        {
                            eventDic[type].Execute(x);
                            Unregister(type);
                        }
                    });
                eventDic.Add(type, listeners);
            }

            listeners.listener.AddListener(listener, executeOnce);
        }

        public static void Unregister(Enum type, Action listener)
        {
            Dictionary<Enum, DisposableListeners> eventDic = Instance._eventDic;
            if (eventDic.TryGetValue(type, out DisposableListeners disposableListeners))
            {
                DisposableListener listenerData;
                if ((listenerData = disposableListeners.listener) != null)
                {
                    listenerData.RemoveListener(listener);
                    Unregister(type);
                }
            }
        }

        public static void Register(Enum type, Action<EventData> listener)
        {
            Register(type, listener, true);
        }

        public static void Register(Enum type, Action<EventData> listener, bool executeOnce)
        {
            Dictionary<Enum, DisposableListeners> eventDic = Instance._eventDic;
            DisposableListeners listeners;
            if (eventDic.TryGetValue(type, out var disposableListeners))
            {
                listeners = disposableListeners;
                if (listeners.listenerData == null)
                {
                    listeners.listenerData = POOL_LISTENER_DATA.Pull();
                }
            }
            else
            {
                listeners = POOL_LISTENERS.Pull();
                listeners.listenerData = POOL_LISTENER_DATA.Pull();
                listeners.disposable = Instance._subscriber
                    .Subscribe(x =>
                    {
                        if (x.type.Equals(type))
                        {
                            eventDic[type].Execute(x);
                            Unregister(type);
                        }
                    });
                eventDic.Add(type, listeners);
            }

            listeners.listenerData.AddListener(listener, executeOnce);
        }

        public static void Unregister(Enum type, Action<EventData> listener)
        {
            Dictionary<Enum, DisposableListeners> eventDic = Instance._eventDic;
            if (eventDic.TryGetValue(type, out var disposableListeners))
            {
                DisposableDataListener listenerData;
                if ((listenerData = disposableListeners.listenerData) != null)
                {
                    listenerData.RemoveListener(listener);
                    Unregister(type);
                }
            }
        }

        private static void Unregister(Enum type)
        {
            Dictionary<Enum, DisposableListeners> eventDic = Instance._eventDic;
            if (eventDic.ContainsKey(type))
            {
                DisposableListeners disposableListener = eventDic[type];
                if (disposableListener.Disposable())
                {
                    eventDic.Remove(type);
                    POOL_LISTENERS.Push(disposableListener);
                }
            }
        }

        private readonly ISubscriber<EventData> _subscriber;
        private readonly IPublisher<EventData> _publisher;

        private readonly Dictionary<System.Type, List<IMediator>> _mediatorDic = new Dictionary<System.Type, List<IMediator>>();
        private readonly Dictionary<System.Type, ICommand> _commandDic = new Dictionary<System.Type, ICommand>();
        private readonly Dictionary<System.Type, IProxy> _proxyDic = new Dictionary<System.Type, IProxy>();
        private readonly Dictionary<Enum, DisposableListeners> _eventDic = new Dictionary<Enum, DisposableListeners>();

        public AppFacade()
        {
            ObjectPool<EventData>.Instance.SetDefaultObjectCount(100);
            //this._observer = MessageBroker.Default.Receive<EventData>();

            // MessagePipe 초기화
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe().AddMessageBroker<EventData>();

            IServiceProvider provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            // Subscriber 생성
            _subscriber = GlobalMessagePipe.GetSubscriber<EventData>();

            // Publisher 생성
            _publisher = GlobalMessagePipe.GetPublisher<EventData>();
        }

        public void Initialize(StartupCommandBase startupCommand)
        {
            this.RegistCommand(startupCommand);
        }

        public void RegisterMediator(IMediator mediator)
        {
            System.Type type = mediator.GetType();
            if (this._mediatorDic.ContainsKey(type))
            {
                this._mediatorDic[type].Add(mediator);
            }
            else
            {
                if (mediator.InterestEvent == null)
                {
                    throw new Exception("InterestHash could not be null in [<color=red>" + type +
                                        "</color>] Mediator.");
                }

                this._mediatorDic.Add(type, new List<IMediator> { mediator });
            }

            IDisposable dispose = _subscriber.Subscribe(data =>
            {
                if (mediator.InterestEvent.TryGetValue(data.type, out Action<EventData> action))
                    action(data);
            });
            mediator.Self = dispose;
        }

        public void DisposeMediator(IMediator mediator)
        {
            System.Type type = mediator.GetType();
            if (this._mediatorDic.ContainsKey(type))
            {
                List<IMediator> mediatorList = this._mediatorDic[type];
                if (mediatorList.Contains(mediator))
                {
                    mediatorList.Remove(mediator);
                    mediator.Self.Dispose();
                }

                if (mediatorList.Count == 0)
                {
                    this._mediatorDic.Remove(type);
                }
            }
        }

        public T RetrieveMediator<T>() where T : IMediator
        {
            List<IMediator> list = this.RetrieveMediators<T>();
            if (list != null) return (T)list[0];
            return default(T);
        }

        public List<IMediator> RetrieveMediators<T>() where T : IMediator
        {
            List<IMediator> mediatorList = null;
            System.Type type = typeof(T);
            if (this._mediatorDic.TryGetValue(type, out List<IMediator> value))
            {
                mediatorList = value;
            }

            return mediatorList;
        }

        /******************** Command ******************/
        public void RegistCommand(ICommand command)
        {
            System.Type type = command.GetType();
            if (this._commandDic.ContainsKey(type))
            {
                throw new Exception("Already exists same name of Command");
            }

            command.InitDic();
            if (command.InterestEvent == null)
            {
                throw new Exception("InterestHash could not be null in [<color=red>" + type + "</color>] Command.");
            }

            this._commandDic.Add(type, command);
            IDisposable disposable = this._subscriber
                .Subscribe(eventData =>
                {
                    if (command.InterestEvent.TryGetValue(eventData.type, out Action<EventData> action))
                        action(eventData);
                });
            command.Self = disposable;
        }

        public T RetrieveCommand<T>() where T : ICommand
        {
            T command = default(T);
            System.Type type = typeof(T);
            if (this._commandDic.TryGetValue(type, out ICommand value))
            {
                command = (T)value;
            }

            return command;
        }

        /******************** Proxy ******************/
        public void RegisterProxy(IProxy proxy)
        {
            System.Type type = proxy.GetType();
            if (!this._proxyDic.TryAdd(type, proxy))
            {
                throw new Exception("Already exists same name of proxy.");
            }
        }

        public T RetrieveProxy<T>() where T : IProxy
        {
            T proxy = default(T);
            System.Type type = typeof(T);
            if (this._proxyDic.TryGetValue(type, out IProxy value))
            {
                proxy = (T)value;
            }

            return proxy;
        }

        /******************** Notify ******************/
        public void SendNotification(EventData data)
        {
            //Debug.Log("<color=grey>[Notify] Send Notification: " + data.type + ", Data: " + data.data + "</color>" );
            // MessageBroker.Default.Publish<EventData>( data );
            _publisher.Publish(data);
        }

        public void SendNotification(Enum eventType)
        {
            EventData eventData = EventData.New;
            eventData.type = eventType;
            this.SendNotification(eventData);
            eventData.Release();
        }

        public void SendNotification(Enum eventType, object data)
        {
            EventData eventData = EventData.New;
            eventData.type = eventType;
            eventData.data = data;
            this.SendNotification(eventData);
            eventData.Release();
        }

        public void Dispose()
        {
            // _inst = null;
            this._commandDic.Clear();
            this._eventDic.Clear();
            this._mediatorDic.Clear();
            this._proxyDic.Clear();
        }
    }
}
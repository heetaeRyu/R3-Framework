using System;
using System.Collections.Generic;

namespace Netmarble.Core
{
	public abstract class MediatorBase : UIComponent, IMediator
	{
		protected Dictionary<Enum, Action<EventData>> interestEvent;
		public AppFacade Facade => AppFacade.Instance;
		public IDisposable Self { get; set; }
		public virtual Dictionary<Enum, Action<EventData>> InterestEvent => interestEvent;

		protected virtual void Awake()
		{
			this.InitDic();
			this.Facade.RegisterMediator(this);
			this.Init();
		}

		protected virtual void OnDestroy()
		{
			this.Facade.DisposeMediator(this);
			this.Destroy();
		}

		//Should init hashset
		protected abstract void InitDic();

		protected void SendNotification(Enum type)
		{
			this.SendNotification(type, null);
		}

		protected void SendNotification(Enum type, object data)
		{
			this.Facade.SendNotification(type, data);
		}

		protected virtual void RemoveEvent(EventData data)
		{
			this.RemoveEvent(data.type);
			data.Release();
		}

		protected virtual void RemoveEvent(Enum eventType)
		{
			this.interestEvent.Remove(eventType);
		}

		protected virtual void RemoveEvent(Enum eventType, Action<EventData> handler)
		{
			if (this.interestEvent.ContainsKey(eventType))
			{
				this.interestEvent[eventType] -= handler;
				if (this.interestEvent[eventType] == null)
					this.RemoveEvent(eventType);
			}
		}

		protected virtual void AddEvent(Enum eventType, Action<EventData> handler)
		{
			if (this.interestEvent.ContainsKey(eventType))
			{
				this.interestEvent[eventType] += handler;
			}
			else
			{
				this.interestEvent.Add(eventType, handler);
			}
		}
	}
}
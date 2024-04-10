using System;
using System.Collections.Generic;

namespace Netmarble.Core
{
	public abstract class CommandBase : ICommand
	{
		protected Dictionary<Enum, Action<EventData>> interestEvent;

		public AppFacade Facade => AppFacade.Instance;

		//Registed in Facade that disposable my self object.
		public IDisposable Self { get; set; }

		//Must be implements at Subclass
		public virtual Dictionary<Enum, Action<EventData>> InterestEvent => this.interestEvent;

		public abstract void InitDic();

		protected virtual void RemoveEvent(EventData data)
		{
			this.RemoveEvent(data.type);
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

		public void SendNotification(Enum eventType, object data)
		{
			this.Facade.SendNotification(eventType, data);
		}

		public void SendNotification(Enum eventType)
		{
			this.Facade.SendNotification(eventType);
		}
	}
}
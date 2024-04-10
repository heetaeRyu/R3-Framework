using System;
using System.Collections.Generic;

namespace Netmarble.Core
{
	public abstract class StartupCommandBase : CommandBase
	{
		public override void InitDic()
		{
			this.interestEvent = new Dictionary<Enum, Action<EventData>>
			{
				{ AppFacadeEvent.Startup, this.Initialize }
			};
		}

		private void Initialize(EventData data)
		{
			this.RemoveEvent(AppFacadeEvent.Startup);
			this.InitializeModules();
		}

		protected abstract void InitializeModules();
	}
}
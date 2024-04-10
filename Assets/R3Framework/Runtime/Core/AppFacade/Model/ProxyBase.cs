using System;

namespace Netmarble.Core
{
    public class ProxyBase : IProxy
    {
        public AppFacade Facade => AppFacade.Instance;

        protected void SendNotification(Enum type, object data = null)
        {
            this.Facade.SendNotification(type, data);
        }
    }
}
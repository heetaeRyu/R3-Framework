using System;
using System.Collections.Generic;

namespace Netmarble.Core
{
    public interface IEventHandleable
    {
        // Core
        AppFacade Facade { get; }

        // Interest event type
        Dictionary<Enum, Action<EventData>> InterestEvent { get; }

        // Returned instance from MessagePipe
        IDisposable Self { get; set; }

    }
}
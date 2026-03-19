using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Events
{
    public static class EventBusRegister
    {
        public static void RegisterEvents()
        {
            // Emitter/Publishers has GUID
            /*
            EventBus.RegisterEvent(
                OMCM.EVENTS_LISTENER_GUID,
                EventBusPublisher.,
                "Listens for when to serialize sent GameObject.",
                EventRegistryParamsHelper.Get(EventRegistryParams.GameObject),
                EventRegistryParamsHelper.Get(EventRegistryParams.SerializePath)
            );
            */
        }

    }
}

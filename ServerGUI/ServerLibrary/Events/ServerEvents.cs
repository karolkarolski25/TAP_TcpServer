using Prism.Events;

namespace ServerLibrary.Events
{
    public class ServerStartedEvent : PubSubEvent
    {

    }

    public class WrongServerConfigurationEvent : PubSubEvent<string>
    {

    }
}

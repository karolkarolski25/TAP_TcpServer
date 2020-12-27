using Prism.Events;

namespace Server.Events
{
    public class ServerStartedEvent : PubSubEvent
    {

    }

    public class WrongServerConfigurationEvent : PubSubEvent<string>
    {

    }

    public class ServerLogsChanged : PubSubEvent<string>
    {

    }
}

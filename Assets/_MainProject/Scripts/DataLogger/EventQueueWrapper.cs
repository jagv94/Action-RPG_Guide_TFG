using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class EventQueueWrapper
{
    [JsonProperty("events")]
    public List<UserEventData> events;
}

[Serializable]
public class BasicEventQueueWrapper
{
    [JsonProperty("events")]
    public List<UserEventBasicData> events;
}
using System;

[Serializable]
public class HassEntity
{
    public string entity_id;
    public string state;
    public HassEntityAttributes attributes;
    public EDeviceType DeviceType;
}

[Serializable]
public class HassEntityAttributes
{
    public string friendly_name;
}
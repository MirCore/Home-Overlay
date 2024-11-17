using Utils;

/// <summary>
/// Device types in Home Assistant
/// </summary>
public enum EDeviceType
{
        [DisplayName("All types")]
        DEFAULT,
        [DisplayName("Light")]
        LIGHT,
        [DisplayName("Switch")]
        SWITCH,
}
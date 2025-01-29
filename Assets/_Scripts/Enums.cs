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
        [DisplayName("Sensor")]
        SENSOR,
        [DisplayName("Binary Sensor")]
        BINARY_SENSOR
}

public enum ESensorDeviceClass
{
        DEFAULT,
        APPARENT_POWER, // VA - Apparent power
        AQI, // None - Air Quality Index
        AREA, // m², cm², km², mm², in², ft², yd², mi², ac, ha - Area
        ATMOSPHERIC_PRESSURE, // cbar, bar, hPa, mmHg, inHg, kPa, mbar, Pa, psi - Atmospheric pressure
        BATTERY, // % - Percentage of battery that is left
        BLOOD_GLUCOSE_CONCENTRATION, // mg/dL, mmol/L - Blood glucose concentration
        CO2, // ppm - Concentration of carbon dioxide
        CO, // ppm - Concentration of carbon monoxide
        CONDUCTIVITY, // S/cm, mS/cm, µS/cm - Conductivity
        CURRENT, // A, mA - Current
        DATA_RATE, // bit/s, kbit/s, Mbit/s, Gbit/s, B/s, kB/s, MB/s, GB/s, KiB/s, MiB/s, GiB/s - Data rate
        DATA_SIZE, // bit, kbit, Mbit, Gbit, B, kB, MB, GB, TB, PB, EB, ZB, YB, KiB, MiB, GiB, TiB, PiB, EiB, ZiB, YiB - Data size
        DATE, // Date - Requires a date object or None
        DISTANCE, // km, m, cm, mm, mi, nmi, yd, in - Generic distance
        DURATION, // d, h, min, s, ms - Time period
        ENERGY, // J, kJ, MJ, GJ, mWh, Wh, kWh, MWh, GWh, TWh, cal, kcal, Mcal, Gcal - Energy consumption
        ENERGY_STORAGE, // J, kJ, MJ, GJ, mWh, Wh, kWh, MWh, GWh, TWh, cal, kcal, Mcal, Gcal - Stored energy
        ENUM, // Non-numeric states
        FREQUENCY, // Hz, kHz, MHz, GHz - Frequency
        GAS, // m³, ft³, CCF - Volume of gas
        HUMIDITY, // % - Relative humidity
        ILLUMINANCE, // lx - Light level
        IRRADIANCE, // W/m², BTU/(h⋅ft²) - Irradiance
        MOISTURE, // % - Moisture
        MONETARY, // ISO 4217 - Monetary value with currency
        NITROGEN_DIOXIDE, // µg/m³ - Concentration of nitrogen dioxide
        NITROGEN_MONOXIDE, // µg/m³ - Concentration of nitrogen monoxide
        NITROUS_OXIDE, // µg/m³ - Concentration of nitrous oxide
        OZONE, // µg/m³ - Concentration of ozone
        PH, // None - Potential hydrogen (pH)
        PM1, // µg/m³ - Particulate matter < 1 micrometer
        PM25, // µg/m³ - Particulate matter < 2.5 micrometers
        PM10, // µg/m³ - Particulate matter < 10 micrometers
        POWER, // mW, W, kW, MW, GW, TW - Power
        POWER_FACTOR, // %, None - Power Factor
        PRECIPITATION, // cm, in, mm - Accumulated precipitation
        PRECIPITATION_INTENSITY, // in/d, in/h, mm/d, mm/h - Precipitation intensity
        PRESSURE, // cbar, bar, hPa, mmHg, inHg, kPa, mbar, Pa, psi - Pressure
        REACTIVE_POWER, // var - Reactive power
        SIGNAL_STRENGTH, // dB, dBm - Signal strength
        SOUND_PRESSURE, // dB, dBA - Sound pressure
        SPEED, // ft/s, in/d, in/h, in/s, km/h, kn, m/s, mph, mm/d, mm/s - Generic speed
        SULPHUR_DIOXIDE, // µg/m³ - Concentration of sulphur dioxide
        TEMPERATURE, // °C, °F, K - Temperature
        TIMESTAMP, // Requires datetime object with timezone
        VOLATILE_ORGANIC_COMPOUNDS, // µg/m³ - VOC concentration
        VOLATILE_ORGANIC_COMPOUNDS_PARTS, // ppm, ppb - VOC ratio
        VOLTAGE, // V, mV, µV, kV, MV - Voltage
        VOLUME, // L, mL, gal, fl. oz., m³, ft³, CCF - Generic volume
        VOLUME_FLOW_RATE, // m³/h, ft³/min, L/min, gal/min, mL/s - Volume flow rate
        VOLUME_STORAGE, // L, mL, gal, fl. oz., m³, ft³, CCF - Stored volume
        WATER, // L, gal, m³, ft³, CCF - Water consumption
        WEIGHT, // kg, g, mg, µg, oz, lb, st - Generic mass (weight)
        WIND_SPEED, // ft/s, km/h, kn, m/s, mph - Wind speed
        
        //BATTERY, // On: Low, Off: Normal
        BATTERY_CHARGING, // On: Charging, Off: Not charging
        //CO, // On: CO detected, Off: No CO
        COLD, // On: Cold, Off: Normal
        CONNECTIVITY, // On: Connected, Off: Disconnected
        DOOR, // On: Open, Off: Closed
        GARAGE_DOOR, // On: Open, Off: Closed
        //GAS, // On: Gas detected, Off: No gas
        HEAT, // On: Hot, Off: Normal
        LIGHT, // On: Light detected, Off: No light
        LOCK, // On: Unlocked, Off: Locked
        //MOISTURE, // On: Wet, Off: Dry
        MOTION, // On: Motion detected, Off: No motion
        MOVING, // On: Moving, Off: Not moving
        OCCUPANCY, // On: Occupied, Off: Not occupied
        OPENING, // On: Open, Off: Closed
        PLUG, // On: Plugged in, Off: Unplugged
        //POWER, // On: Power detected, Off: No power
        PRESENCE, // On: Home, Off: Away
        PROBLEM, // On: Problem detected, Off: OK
        RUNNING, // On: Running, Off: Not running
        SAFETY, // On: Unsafe, Off: Safe
        SMOKE, // On: Smoke detected, Off: No smoke
        SOUND, // On: Sound detected, Off: No sound
        TAMPER, // On: Tampering detected, Off: No tampering
        UPDATE, // On: Update available, Off: Up-to-date
        VIBRATION, // On: Vibration detected, Off: No vibration
        WINDOW // On: Open, Off: Closed
}
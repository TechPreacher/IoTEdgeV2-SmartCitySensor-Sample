namespace FilterModule
{
    using System;
    class Sensor
    {
        public string SensorName { get; set; }
        public int NoiseLevel { get; set; }
        public string AirQuality { get; set; }
        public int AirPressure { get; set; }
        public DateTime Timecreated { get; set; }
    }
}

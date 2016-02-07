using System;
using System.ComponentModel;
using OpenHardwareMonitor.Hardware;

namespace PortableOHM
{
    public class OHMSensor
    {
        public OHMSensor(ISensor sensor, DataType dataType, string label, bool round = false, double alertValue = 0)
        {
            _sensor = sensor;

            DataType = dataType;
            Append = dataType.GetAppend();
            Label = label;
            Round = round;
        }

        public void Update()
        {
            if (_sensor.Value.HasValue)
            {
                double _value = _sensor.Value.Value;

                if (_converter != null)
                {
                    _converter.Convert(ref _value);
                }

                if (Round)
                {
                    _value = Math.Round(_value);
                }

                Value = _value;
            }
            else
            {
                Value = 0;
            }
        }

        private string _text { get; set; }

        public string Text()
        {
            if (Value == 0)
            {
                return string.Format("{0}: No Value", Label);
            }
            else
            {
                return string.Format(
        "{0}: {1:#,##0.##}{2}",
        Label,
        Value,
        Append
        );
            }

        }

        public double Value { get; private set; }

        public DataType DataType { get; private set; }

        public string Label { get; set; }

        public string Append { get; private set; }

        public bool Round { get; set; }

        private ISensor _sensor { get; set; }

        private CelciusToFahrenheit _converter { get; set; }
    }
}
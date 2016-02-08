using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class MainboardMonitor : OHMMonitor
    {
        public OHMSensor[] BoardTemperatures { get; private set; }
        public OHMSensor[] BoardFanRPM { get; private set; }
        public OHMSensor[] BoardFanPercent { get; private set; }
        public OHMSensor[] HddTemperatures { get; private set; }

        public MainboardMonitor(IHardware board) : base(board)
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();
            BoardTemperatures = new OHMSensor[0];
            BoardFanRPM = new OHMSensor[0];
            BoardFanPercent = new OHMSensor[0];
            HddTemperatures = new OHMSensor[0];
            if (board != null)
            {
                initBoardTemperatures(board, _sensorList);
                initBoardFanRpm(board, _sensorList);
                initBoardFanPercent(board, _sensorList);
            }
            Sensors = _sensorList.ToArray();

        }

        private void initBoardTemperatures(IHardware board, List<OHMSensor> _sensorList)
        {
            List<ISensor> boardTemperatureSensors = board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Temperature).ToList();
            if (boardTemperatureSensors.Count > 0)
            {
                BoardTemperatures = boardTemperatureSensors.Select(s => new OHMSensor(s, DataType.Celcius, s.Name)).ToArray();
                _sensorList.AddRange(BoardTemperatures);
            }
        }

        private void initBoardFanRpm(IHardware board, List<OHMSensor> _sensorList)
        {

            List<ISensor> boardRpmSensors = board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Fan).ToList();
            if (boardRpmSensors.Count > 0)
            {
                BoardFanRPM = boardRpmSensors.Select(s => new OHMSensor(s, DataType.RPM, s.Name)).ToArray();
                _sensorList.AddRange(BoardFanRPM);
            }
        }
        private void initBoardFanPercent(IHardware board, List<OHMSensor> _sensorList)
        {
            List<ISensor> boardFanPercentSensors = board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Control && s.Name.Contains("Fan")).ToList();
            if (boardFanPercentSensors.Count > 0)
            {
                BoardFanPercent = boardFanPercentSensors.Select(s => new OHMSensor(s, DataType.Percent, s.Name)).ToArray();
                _sensorList.AddRange(BoardFanPercent);
            }
        }
    }
}

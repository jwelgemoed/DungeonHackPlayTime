using System.Diagnostics;

namespace FunAndGamesWithSharpDX.Engine
{
    public class GameTimer
    {
        private long _baseTime;
        private long _pausedTime;
        private long _stopTime;
        private long _prevTime;
        private long _currTime;
        private bool _stopped;
        private double _secondsPerCount;
        private long _countsPerSecond;
        private readonly Stopwatch _stopwatch;

        public double DeltaTime { get; private set; }

        public GameTimer()
        {
            _stopwatch = new Stopwatch();
            _countsPerSecond = Stopwatch.Frequency;
            _secondsPerCount = 1.0f / _countsPerSecond;
            DeltaTime = -1.0;
            _baseTime = 0;
            _pausedTime = 0;
            _prevTime = 0;
            _currTime = 0;
            _stopped = false;
            _stopwatch.Start();
        }

        public void Tick()
        {
            if (_stopped)
            {
                DeltaTime = 0.0;
                return;
            }

            _currTime = _stopwatch.ElapsedTicks;
            DeltaTime = (_currTime - _prevTime) * _secondsPerCount;
            _prevTime = _currTime;

            if (DeltaTime < 0.0)
                DeltaTime = 0.0;
        }

        public void Reset()
        {
            long _currTime = _stopwatch.ElapsedTicks;
            _baseTime = _currTime;
            _prevTime = _currTime;
            _stopTime = 0;
            _stopped = false;
            _stopwatch.Reset();
        }

        public void Stop()
        {
            if (_stopped)
                return;

            long _currTime = _stopwatch.ElapsedTicks;
            _stopTime = _currTime;
            _stopped = true;
        }

        public void Start()
        {
            long _startTime = _stopwatch.ElapsedTicks;

            if (_stopped)
            {
                _pausedTime += (_startTime - _stopTime);
                _prevTime = _startTime;
                _stopTime = 0;
                _stopped = false;
            }
        }

        public double TotalTime()
        {
            if (_stopped)
            {
                return ((_stopTime - _pausedTime) - _baseTime) * _secondsPerCount;
            }

            return ((_currTime - _pausedTime) - _baseTime) * _secondsPerCount;
        }
    }
}
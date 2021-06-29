// ==============================================
// Author：qiuyukun
// Date:2020-09-21 11:16:44
// ==============================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace Lockstep
{
    public class LockstepEngine
    {
        //确认帧
        public int confirmedFrameIndex { get; private set; }
        //预测帧
        public int predictiveFrameIndex { get; private set; }
        //是否正在运行
        public bool isRunning { get; private set; }
        //逻辑帧间隔毫秒
        public uint frameDeltaTime => 33;
        //时间校准值(外部预估ping值等)
        public volatile int timeAdjust = 0;
        //是否可以预测
        public bool predictable { get { return predictiveFrameIndex < confirmedFrameIndex + _maxPredictionCount; } }
        //最大预测数量
        private int _maxPredictionCount = 8;
        //在一帧内最多可以执行逻辑数
        private int _maxLogicCountPerFrame = 20;
        //暂停
        private bool _isPause = false;

        private Queue<IFrameInput> _predictiveInputQueue;
        private Queue<IFrameInput> _confirmedInputQueue;
        private Action<IFrameInput> _excuteCallback;
        private Func<IFrameInput> _getInputAction;
        private Action<int> _rollback;
        private Thread _logicThread;
        private Stopwatch _startStopwatch;
        private long _timeOffset;
        private object _inputLock = new object();

#if UNITY_EDITOR
        //private LockstepDebug _debug;
#endif

        public LockstepEngine(Action<IFrameInput> excuteCallback, Action<int> rollback = null, Func<IFrameInput> getInputAction = null)
        {
            _predictiveInputQueue = new Queue<IFrameInput>(_maxPredictionCount);
            _confirmedInputQueue = new Queue<IFrameInput>(_maxPredictionCount);
            _excuteCallback = excuteCallback;
            _getInputAction = getInputAction;
            _rollback = rollback;
#if UNITY_EDITOR
            //_debug = new UnityEngine.GameObject("LockstepDebug").AddComponent<LockstepDebug>();
            //_debug.Init(this);
#endif 
        }

        public void Start(bool newThread = true)
        {
            if (isRunning) return;
            confirmedFrameIndex = -1;
            predictiveFrameIndex = -1;
            _isPause = false;
            isRunning = true;
            if (newThread)
            {
                _logicThread = new Thread(LogicThread);
                _logicThread.Start();
                _startStopwatch = new Stopwatch();
            }
        }

        public void Pause()
        {
            _isPause = isRunning;
        }

        public void Resume()
        {
            _isPause = false;
        }

        public void Close()
        {
            isRunning = false;
            _logicThread?.Join();
            _logicThread = null;
            _confirmedInputQueue.Clear();
            _predictiveInputQueue.Clear();
            _startStopwatch = null;
#if UNITY_EDITOR
            //if (_debug != null)
            //    UnityEngine.GameObject.Destroy(_debug.gameObject);
#endif
        }

        public void OnInput(IFrameInput input)
        {
            if (_startStopwatch != null)
            {
                if (input.frameIndex == 0)
                    _startStopwatch.Start();
                var realTime = input.frameIndex * frameDeltaTime;
                var offset = _startStopwatch.ElapsedMilliseconds - realTime;
                if (offset < _timeOffset)
                    _timeOffset = offset;
            }
            lock (_inputLock)
            {
                _confirmedInputQueue.Enqueue(input);
            }
        }

        public void NextStep()
        {
            var time = (predictiveFrameIndex + 1) * frameDeltaTime;
            LogicUpdate(time, 1);
        }

        private long GetPredictTime()
        {
            if (_startStopwatch != null)
                return _startStopwatch.ElapsedMilliseconds - _timeOffset + timeAdjust;
            return 0;
        }

        private void LogicThread()
        {
            while (isRunning)
            {
                if (!_isPause)
                {
                    LogicUpdate(GetPredictTime(), _maxLogicCountPerFrame);
                }
                Thread.Sleep(1);
            }
        }

        private void LogicUpdate(long time, int maxLogicCount)
        {
            bool isRollBack = false;
            int logicCount = 0;

            lock (_inputLock)
            {
                while (_confirmedInputQueue.Count > 0 && logicCount < maxLogicCount)
                {
                    confirmedFrameIndex++;
                    logicCount++;
                    var confirmedInput = _confirmedInputQueue.Dequeue();
                    if (_predictiveInputQueue.Count > 0)
                    {
                        var predictiveInput = _predictiveInputQueue.Dequeue();
                        //回滚
                        if (!isRollBack && !predictiveInput.Equals(confirmedInput))
                        {
#if UNITY_EDITOR
                            //_debug?.BeginRollback();
#endif
                            _rollback?.Invoke(confirmedFrameIndex);

#if UNITY_EDITOR
                            //_debug?.EndRollback();
#endif
                            isRollBack = true;
                        }
                    }
                    else
                        Excute(confirmedInput);

                    if (isRollBack)
                        Excute(confirmedInput);
                }
            }

            if (confirmedFrameIndex > predictiveFrameIndex)
                predictiveFrameIndex = confirmedFrameIndex;

            if (isRollBack)
            {
                var count = _predictiveInputQueue.Count;
                //追上预测帧
                while (count > 0)
                {
                    var input = _predictiveInputQueue.Dequeue();
                    Excute(input);
                    _predictiveInputQueue.Enqueue(input);
                    count--;
                }
            }

            //预测
            if (predictiveFrameIndex < confirmedFrameIndex + _maxPredictionCount &&
                time >= (predictiveFrameIndex + 1) * frameDeltaTime)
            {
                Predict();
            }
        }

        //开始预测
        private void Predict()
        {
            predictiveFrameIndex += 1;
            var input = _getInputAction?.Invoke();
            input.frameIndex = predictiveFrameIndex;
            _predictiveInputQueue.Enqueue(input);
            Excute(input);
        }

        private void Excute(IFrameInput input)
        {
            if (input == null) return;
#if UNITY_EDITOR
            //_debug?.BeginExcute();
#endif
            _excuteCallback?.Invoke(input);
#if UNITY_EDITOR
            //_debug?.EndExcute();
#endif
        }
    }
}


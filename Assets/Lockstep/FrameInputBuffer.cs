// ==============================================
// Author：qiuyukun
// Date:2020-09-23 09:43:44
// ==============================================

using UnityEngine;

namespace Lockstep
{
    //每帧输入
    public interface IFrameInput
    {
        bool Equals(IFrameInput other);
        int frameIndex { get; set; }
    }

    //输入缓存
    public class FrameInputBuffer
    {
        public int lastIndex { get; private set; } = -1;
        private IFrameInput[] _inputs;
        private object _lock = new object();

        public FrameInputBuffer(int count = 36000)
        {
            _inputs = new IFrameInput[count];
        }

        public void AddInput(IFrameInput input)
        {
            var index = input.frameIndex;
            if (index < 0)
            {
                Debug.LogException(new System.Exception("index must be non-negative"));
                return;
            }
            lock (_lock)
            {
                if (index >= _inputs.Length)
                {
                    var length = _inputs.Length * 2;
                    var old = _inputs;
                    _inputs = new IFrameInput[length];
                    for (int i = 0; i < old.Length; i++)
                        _inputs[i] = old[i];

                    Debug.Log("expand input buffer " + length);
                }

                if (_inputs[index] == default(IFrameInput))
                {
                    _inputs[index] = input;
                    if (index == lastIndex + 1)
                    {
                        while (index + 1 < _inputs.Length && _inputs[index + 1] != null)
                        {
                            index++;
                        }
                        lastIndex = index;
                    }
                }
            }
        }

        public bool TryGetIndex(int index, out IFrameInput input)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _inputs.Length)
                {
                    Debug.LogErrorFormat("index out of the length index:{0} lastIndex:{1}", index, lastIndex);
                    input = null;
                    return false;
                }
                input = _inputs[index];
            }
            return input != null;
        }

        public bool Equals(int index, IFrameInput input)
        {
            IFrameInput self;
            if (TryGetIndex(index, out self))
            {
                return self.Equals(input);
            }
            return true;
        }
    }
}


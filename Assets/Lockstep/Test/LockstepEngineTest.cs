// ==============================================
// Author：qiuyukun
// Date:2020-09-24 11:36:15
// ==============================================

using System.Collections.Generic;
using NUnit.Framework;

namespace Lockstep.Test
{
    [TestFixture]
    internal class LockstepEngineTest
    {
        //不做预测,直接确认执行
        [Test]
        public void Confirmed()
        {
            var env = new EngineEnv();
            var input = new EngineEnv.TestInput();
            input.frameIndex = 0;
            input.data = 2;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(2, 0, 0, 1, 0);

            input.frameIndex = 1;
            input.data = 3;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(5, 1, 1, 2, 0);

            input.frameIndex = 2;
            input.data = 4;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(9, 2, 2, 3, 0);
        }

        //预测
        [Test]
        public void Predict()
        {
            var env = new EngineEnv();
            env.inputData = 2;
            env.engine.NextStep();
            env.Assert(2, -1, 0, 1, 0);

            env.inputData = 3;
            env.engine.NextStep();
            env.Assert(5, -1, 1, 2, 0);

            env.inputData = 10;
            env.engine.NextStep();
            env.Assert(15, -1, 2, 3, 0);
        }

        //回滚
        [Test]
        public void Rollback()
        {
            var env = new EngineEnv();

            //确认输入第一帧
            var input = new EngineEnv.TestInput();
            input.frameIndex = 0;
            input.data = 2;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(2, 0, 0, 1, 0);

            //预测第二帧
            env.inputData = 5;
            env.engine.NextStep();
            env.Assert(7, 0, 1, 2, 0);

            //预测第三帧
            env.inputData = 3;
            env.engine.NextStep();
            env.Assert(10, 0, 2, 3, 0);

            //第四帧,与第二帧相同输入的确认帧
            input.frameIndex = 1;
            input.data = 5;
            env.inputData = 1;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(11, 1, 3, 4, 0);

            //第五帧,与第三帧不同输入的确认帧,开始回滚
            input.frameIndex = 2;
            input.data = 6;
            env.inputData = 2;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(16, 2, 4, 7, 1);

            //预测第六帧
            env.inputData = 2;
            env.engine.NextStep();
            env.Assert(18, 2, 5, 8, 1);

            //预测第七帧
            env.inputData = 3;
            env.engine.NextStep();
            env.Assert(21, 2, 6, 9, 1);

            //第八帧,与第四帧不同输入的确认帧,开始回滚
            input.frameIndex = 3;
            input.data = 7;
            env.inputData = 6;
            env.engine.OnInput(input);
            env.engine.NextStep();
            env.Assert(30, 3, 7, 14, 2);
        }
    }

    internal class EngineEnv
    {
        internal struct TestInput : IFrameInput
        {
            public int frameIndex { get; set; }
            public int data;

            public bool Equals(IFrameInput other)
            {
                var o = (TestInput)other;
                return frameIndex == other.frameIndex && data == o.data;
            }

            public void Release()
            {
                
            }
        }

        public LockstepEngine engine;
        public int inputData;

        private int _excuteCount = 0;
        private int _curFrameIndex = 0;
        private int _rollbackCount = 0;
        private int _data = 0;
        private Dictionary<int, int> _dataDict = new Dictionary<int, int>();

        internal EngineEnv()
        {
            engine = new LockstepEngine(Excute, Rollback, Predict);
            engine.Start(false);
        }

        public void Assert(int data, int confirmedFrameIndex, int predictiveFrameIndex, int excuteCount, int rollbackCount)
        {
            NUnit.Framework.Assert.AreEqual(excuteCount, this._excuteCount);
            NUnit.Framework.Assert.AreEqual(data, this._data);
            NUnit.Framework.Assert.AreEqual(confirmedFrameIndex, engine.confirmedFrameIndex);
            NUnit.Framework.Assert.AreEqual(predictiveFrameIndex, engine.predictiveFrameIndex);
            NUnit.Framework.Assert.AreEqual(rollbackCount, this._rollbackCount);
        }

        private void Excute(IFrameInput input)
        {
            var testInput = (TestInput)input;

            _excuteCount += 1;
            _data += testInput.data;
        }

        private void Rollback(int frame)
        {
            _rollbackCount += 1;
            NUnit.Framework.Assert.True(_dataDict.TryGetValue(frame, out _data));
        }

        private IFrameInput Predict()
        {
            var input = new TestInput();
            input.frameIndex = engine.predictiveFrameIndex;
            input.data = this.inputData;
            _dataDict.Add(input.frameIndex, _data);
            return input;
        }
    }
}


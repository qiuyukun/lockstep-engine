// ==============================================
// Author：qiuyukun
// Date:2020-12-22 17:35:41
// ==============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Lockstep;

namespace Lockstep.Editor
{
    internal class LockstepDebug : MonoBehaviour
    {
        internal LockstepEngine engine;
        private IList _excuteTimeList;
        private IList _rollbackTimeList;
        private Stopwatch _excuteStopwatch = new Stopwatch();
        private Stopwatch _rollbackStopwatch = new Stopwatch();

        public void Init(LockstepEngine engine)
        {
            this.engine = engine;
            _excuteTimeList = ArrayList.Synchronized(new List<int>(10000));
            _rollbackTimeList = ArrayList.Synchronized(new List<int>(100));
        }

        public void BeginExcute()
        {
            _excuteStopwatch.Reset();
            _excuteStopwatch.Start();
        }

        public void EndExcute()
        {
            _excuteTimeList.Add((int)_excuteStopwatch.ElapsedMilliseconds);
            _excuteStopwatch.Stop();
        }

        public void BeginRollback()
        {
            _rollbackStopwatch.Reset();
            _rollbackStopwatch.Start();
        }

        public void EndRollback()
        {
            _rollbackTimeList.Add((int)_rollbackStopwatch.ElapsedMilliseconds);
            _rollbackStopwatch.Stop();
        }
    }
}

// ==============================================
// Author：qiuyukun
// Date:2020-09-23 09:43:44
// ==============================================

using System;

namespace Lockstep
{
    //每帧输入
    public interface IFrameInput
    {
        int frameIndex { get; set; }
        bool Equals(IFrameInput other);
        void Release();
    }
}


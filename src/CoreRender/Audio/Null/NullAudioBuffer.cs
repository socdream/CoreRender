using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.Null
{
    public class NullAudioBuffer : AudioBuffer
    {
        public override void BufferData<T>(T[] buffer, BufferAudioFormat format, int sizeInBytes, int frequency)
        {
        }

        public override void Dispose() { }
    }
}

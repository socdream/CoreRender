using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio
{
    public abstract class AudioEngine
    {
        public abstract void SetListenerPosition(float[] position);
        public abstract void SetListenerOrientation(float[] forward, float[] up);
        public abstract AudioResourceFactory ResourceFactory { get; }
    }
}

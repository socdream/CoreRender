using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.Null
{
    public class NullAudioEngine : AudioEngine
    {
        public override AudioResourceFactory ResourceFactory { get; }

        public NullAudioEngine()
        {
            ResourceFactory = new NullAudioResourceFactory();
        }

        public override void SetListenerOrientation(float[] forward, float[] up)
        {
        }

        public override void SetListenerPosition(float[] position)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio
{
    public abstract class AudioResourceFactory
    {
        public abstract AudioSource CreateAudioSource();
        public abstract AudioBuffer CreateAudioBuffer();
    }
}

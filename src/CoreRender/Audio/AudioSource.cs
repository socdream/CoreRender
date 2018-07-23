using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio
{
    public abstract class AudioSource : IDisposable
    {
        public abstract float Gain { get; set; }
        public abstract float Pitch { get; set; }
        public abstract bool Looping { get; set; }
        public abstract float[] Position { get; set; }
        public abstract float[] Direction { get; set; }
        public abstract AudioPositionKind PositionKind { get; set; }
        public abstract void Dispose();
        public abstract void Play(AudioBuffer buffer);
        public abstract void Stop();
        public abstract float PlaybackPosition { get; set; }
        public abstract bool IsPlaying { get; }
    }
}

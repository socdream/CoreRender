using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.Null
{
    public class NullAudioSource : AudioSource
    {
        public override float[] Direction { get; set; }

        public override float Gain { get; set; }
        public override float Pitch { get; set; }

        public override bool Looping { get; set; }

        public override float[] Position { get; set; }

        public override AudioPositionKind PositionKind { get; set; }

        public override float PlaybackPosition { get { return 1f; } set { } }

        public override bool IsPlaying => false;

        public override void Dispose()
        {
        }

        public override void Play(AudioBuffer buffer)
        {
        }

        public override void Stop()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.Null
{
    public class NullAudioResourceFactory : AudioResourceFactory
    {
        private readonly AudioBuffer _nullAudioBuffer = new NullAudioBuffer();
        private readonly AudioSource _nullAudioSource = new NullAudioSource();

        public override AudioBuffer CreateAudioBuffer()
        {
            return _nullAudioBuffer;
        }

        public override AudioSource CreateAudioSource()
        {
            return _nullAudioSource;
        }
    }
}

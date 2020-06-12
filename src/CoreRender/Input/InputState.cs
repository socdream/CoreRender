using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Input
{
    public class InputState
    {
        public bool Up { get; set; } = false;
        public bool Down { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;
        public bool Action1 { get; set; } = false;
        public bool Action2 { get; set; } = false;
        public bool Action3 { get; set; } = false;
    }
}

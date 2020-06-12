using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Input
{
    public class InputSetup
    {
        public Key Up { get; set; } = Key.W;
        public Key Down { get; set; } = Key.S;
        public Key Left { get; set; } = Key.A;
        public Key Right { get; set; } = Key.D;
        public Key Shoot { get; set; } = Key.K;
        public Key Special { get; set; } = Key.L;
        public Key Jump { get; set; } = Key.Space;
    }
}

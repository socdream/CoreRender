using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Input
{
    public class InputManager
    {
        public static List<InputSetup> InputSetups { get; set; } = new List<InputSetup>()
        {
            new InputSetup()
        };

        public static List<InputState> Inputs { get; set; } = new List<InputState>()
        {
            new InputState()
        };

        public static void Update(KeyboardKeyEventArgs e, bool pressed)
        {
            for (int i = 0; i < InputSetups.Count; i++)
            {
                if (InputSetups[i].Up == e.Key)
                    Inputs[i].Up = pressed;
                else if (InputSetups[i].Down == e.Key)
                    Inputs[i].Down = pressed;
                else if (InputSetups[i].Left == e.Key)
                    Inputs[i].Left = pressed;
                else if (InputSetups[i].Right == e.Key)
                    Inputs[i].Right = pressed;
                else if (InputSetups[i].Shoot == e.Key)
                    Inputs[i].Action1 = pressed;
                else if (InputSetups[i].Special == e.Key)
                    Inputs[i].Action2 = pressed;
                else if (InputSetups[i].Jump == e.Key)
                    Inputs[i].Action3 = pressed;
            }
        }

        public static void KeyDown(KeyboardKeyEventArgs e)
        {
            Update(e, true);
        }

        public static void KeyUp(KeyboardKeyEventArgs e)
        {
            Update(e, false);
        }
    }
}

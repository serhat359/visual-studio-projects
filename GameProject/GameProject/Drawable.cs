using System.Collections.Generic;
using System.Drawing;

namespace GameProject
{
    public abstract class Drawable
    {
        public GameFrame frameRef;
        private bool willBeDrawn = true;
        public bool WillBeDrawn { get { return willBeDrawn; } }

        public abstract void Draw(Graphics graphics, long microseconds);

        public void Destroy()
        {
            willBeDrawn = false;
        }

        public bool IsKeyDown(KeyBindings.GameInput key)
        {
            return frameRef.IsKeyDown(key);
        }
    }
}

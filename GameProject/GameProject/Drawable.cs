using System.Collections.Generic;
using System.Drawing;

namespace GameProject
{
    public abstract class Drawable
    {
        private int layerNo;
        private List<Layer> layersRef;
        private bool willBeDrawn = true;
        public bool WillBeDrawn { get { return willBeDrawn; } }

        public abstract void Draw(Graphics graphics, long microseconds);

        public void Destroy()
        {
            willBeDrawn = false;
        }
    }
}

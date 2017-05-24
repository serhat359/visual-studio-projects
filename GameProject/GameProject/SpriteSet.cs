using System.Collections.Generic;
using System.Drawing;

namespace GameProject
{
    class SpriteSet
    {
        private List<Bitmap> sprites;
        public long animateDelayMs { get; private set; }

        // TODO we create another list for mirrored sprites, could serve a memory problem
        public SpriteSet(List<Bitmap> standSprites, long wholeAnimationTime)
        {
            this.sprites = standSprites;
            this.animateDelayMs = wholeAnimationTime / standSprites.Count;
        }

        public List<Bitmap> GetSprites()
        {
            return sprites;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GameProject
{
    class SpriteSet
    {
        private List<Bitmap> sprites;
        private List<Bitmap> spritesMirrored;
        public long animateDelayMs { get; private set; }

        // TODO we create another list for mirrored sprites, could serve a memory problem
        public SpriteSet(List<Bitmap> standSprites, long wholeAnimationTime)
        {
            this.sprites = standSprites;
            this.spritesMirrored = standSprites.Select(x => new Bitmap(x)).ToList();
            this.spritesMirrored.ForEach(x => x.RotateFlip(RotateFlipType.RotateNoneFlipX));
            this.animateDelayMs = wholeAnimationTime / standSprites.Count;
        }

        public List<Bitmap> GetSprites(bool flip) {
            return flip ? spritesMirrored : sprites;
        }
    }
}

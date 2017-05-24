using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GameProject.Objects
{
    class Character : Drawable, KeyListener
    {
        private enum State
        {
            Standing,
            Running
        }

        private enum LookingDirection
        {
            Left,
            Right
        }

        static SpriteSet standing;
        static SpriteSet running;

        static Character()
        {
            List<Bitmap> standSprites = Enumerable.Range(1, 6)
                .Select(i => string.Format(@"Sprites\stand{0}.png", i))
                .Select(filename => new Bitmap(Extensions.GetPath(filename)))
                .ToList();
            standing = new SpriteSet(standSprites, 1000);

            List<Bitmap> runningSprites = Enumerable.Range(1, 20)
                .Select(i => string.Format(@"Sprites\run{0}.png", i))
                .Select(filename => new Bitmap(Extensions.GetPath(filename)))
                .ToList();
            running = new SpriteSet(runningSprites, 1000);
        }

        private Point location;
        private long creationTime;
        private State state;
        private LookingDirection lookingDirection;

        public Character(Point location)
        {
            this.location = location;
            this.creationTime = Extensions.GetMicroSeconds();
            this.state = State.Standing;
            this.lookingDirection = LookingDirection.Right;
        }

        public override void Draw(Graphics g, long microseconds)
        {
            long timediff = microseconds - creationTime;
            long timediffms = timediff / 1000;

            SpriteSet sprites;

            switch (state)
            {
                case State.Standing:
                    sprites = standing;
                    break;
                case State.Running:
                    sprites = running;
                    break;
                default:
                    sprites = null;
                    break;
            }

            AnimateSprites(g, sprites, timediffms);
        }

        private void AnimateSprites(Graphics g, SpriteSet spriteSet, long timediffms)
        {
            var sprites = spriteSet.GetSprites(this.lookingDirection == LookingDirection.Left);
            long index = (timediffms / spriteSet.animateDelayMs) % sprites.Count;
            Image image = sprites[(int)index];

            g.DrawImageUnscaled(image, location);
        }

        public void OnKeyDown(KeyBindings.GameInput key)
        {
            switch (key)
            {
                case KeyBindings.GameInput.Left:
                    {
                        state = State.Running;
                        lookingDirection = LookingDirection.Left;
                        break;
                    };
                case KeyBindings.GameInput.Right:
                    {
                        state = State.Running;
                        lookingDirection = LookingDirection.Right;
                        break;
                    };
                default:
                    break;
            }
        }

        public void OnKeyUp(KeyBindings.GameInput key)
        {
            switch (key)
            {
                case KeyBindings.GameInput.Left:
                case KeyBindings.GameInput.Right: state = State.Standing; break;
                default:
                    break;
            }
        }
    }
}

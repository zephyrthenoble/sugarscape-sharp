using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sugarscape;

public class Sugar
    {
        public int yield;
        private int max;
        private int growth_rate;
        private int growth_cycle = 0;
        private bool harvasted = false;
        public Sugar(int max = 4, int start = 0, int growth_rate = 3)
        {
            yield = start;
            this.max = max;
            this.growth_rate = growth_rate;
        }
        public int Harvest()
        {
            int x = yield;
            yield = 0;
            harvasted = true;
            return x;
        }
        public void Grow()
        {
            growth_cycle += 1;
            if (growth_cycle >= growth_rate)
            {
                yield += 1;
                yield = MathHelper.Clamp(yield, 0, max);
                growth_cycle = 0;
            }

        }
        internal void Draw(SpriteBatch g, GameTime gameTime, Point loc, Point size)
        {
            Color color = World.sugarcolors[yield];
            Point offset = new Point(1, 1);
            g.Draw(World.whiteRectangle, new Rectangle(loc * World.squaresize + offset, World.squaresize - offset - offset), color);

        }

        internal void Update(GameTime gameTime)
        {

        }
    }
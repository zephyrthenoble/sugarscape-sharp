using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;
using sugarscape;
using System;

namespace sugarscape

{
    public class ColorWrapper
    {
        public Color color
        {
            get { return new Color(R, G, B); }
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }
        public readonly Tweener _tweener = new Tweener();
        public float R;
        public float G;
        public float B;
        public ColorWrapper(Color c)
        {
            Vector3 color_vector = c.ToVector3();

            R = color_vector.X;
            G = color_vector.Y;
            B = color_vector.Z;
        }
        public ColorWrapper()
        {
            Whiteout();
        }
        public void TweenTo(Color c)
        {
            Vector3 color_vector = c.ToVector3();
            _tweener.TweenTo(target: this,
                expression: sugar => this.R,
                toValue: color_vector.X,
                duration: .5f,
                delay: 0f);

            _tweener.TweenTo(target: this,
                expression: sugar => this.G,
                toValue: color_vector.Y,
                duration: .5f,
                delay: 0f);

            _tweener.TweenTo(target: this,
                expression: sugar => this.B,
                toValue: color_vector.Z,
                duration: .5f,
                delay: 0f);
        }
        public void Whiteout()
        {
            R = 1.0f;
            G = 1.0f;
            B = 1.0f;
        }

        public void Update(GameTime g)
        {
            _tweener.Update(g.GetElapsedSeconds());
        }

        public static implicit operator Color(ColorWrapper d) => d.color;

    }
    public class Sugar
    {
        public int yield;
        private int max;
        private int growth_rate;
        private int growth_cycle = 0;
        private bool harvasted = false;
        public bool tweening = false;
        public ColorWrapper color = new ColorWrapper(Color.White);

        public Sugar(int max = 4, int start = 0, int growth_rate = 3)
        {
            yield = start;
            color = new ColorWrapper(World.sugarcolors[yield]);
            var target_yield = MathHelper.Clamp(yield + 1, 0, max);
            Tween(target_yield);
            this.max = max;
            this.growth_rate = growth_rate;
        }
        public int Harvest()
        {
            int x = yield;
            yield = 0;
            growth_cycle = 0;
            harvasted = true;
            color.Whiteout(); 

            return x;
        }
        public void Tween(int target_yield)
        {
            color.TweenTo(World.sugarcolors[target_yield]);
        }
        public void Grow()
        { 
            if (harvasted == true)
            {
                harvasted = false;
                return;
            }

            growth_cycle += 1;
            if (growth_cycle >= growth_rate)
            {
                yield += 1;
                yield = MathHelper.Clamp(yield, 0, max);
                var target_yield = MathHelper.Clamp(yield + 1, 0, max);
                if (target_yield > yield)
                {
                    Tween(target_yield);
                }

                growth_cycle = 0;
            }
        }
        public void Draw(SpriteBatch g, GameTime gameTime, Point loc)
        {
            Point offset = new Point(1, 1);
            var worldpos = loc * World.squaresize;
            var destinationrec = new Rectangle(worldpos + offset, World.squaresize - offset - offset);
            g.Draw(World.whiteRectangle, destinationrec, color);
            g.DrawString(GameDriver.font, yield.ToString() + "/" + max.ToString(), worldpos.ToVector2(), Color.Black);
            g.DrawString(GameDriver.font, growth_cycle.ToString(), worldpos.ToVector2() + new Vector2(0, 16), Color.Black);

        }

        public void Update(GameTime gameTime)
        {
            color.Update(gameTime);
        }
    }
}

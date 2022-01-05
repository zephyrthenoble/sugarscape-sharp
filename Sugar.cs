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
        public int max;
        private int growth_rate;
        private int growth_cycle = 0;
        public int times_harvested = 0;
        private bool harvasted = false;
        public bool checked_ = false;
        public bool tweening = false;
        public bool spawn = false;
        public ColorWrapper color = new ColorWrapper(Color.White);

        public Sugar(int max = 4, int start = 0, int growth_rate = 3)
        {
            yield = start;
            color = new ColorWrapper(World.sugarcolors[yield]);
            // var target_yield = MathHelper.Clamp(yield + 1, 0, max);
            Tween(yield);
            this.max = max;
            this.growth_rate = growth_rate;
        }
        public void validateSugar()
        {

        }
        public int Harvest()
        {
            int harvestvalue = yield;
            yield = 0;
            growth_cycle = 0;
            harvasted = true;
            checked_ = false;
            times_harvested += 1;
            color.Whiteout();

            return harvestvalue;
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
                checked_ = false;
                return;
            }

            growth_cycle += 1;
            if (growth_cycle >= growth_rate)
            {
                yield += 1;
                yield = MathHelper.Clamp(yield, 0, max);
                //var target_yield = MathHelper.Clamp(yield + 1, 0, max);
                Tween(yield);

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
            g.DrawString(GameDriver.font, "harvest: " + times_harvested.ToString(), worldpos.ToVector2() + new Vector2(0, 32), Color.Black);
            g.DrawString(GameDriver.font, "checked: " + checked_.ToString(), worldpos.ToVector2() + new Vector2(0, 48), Color.Black);
            g.DrawString(GameDriver.font, "spawn: " + spawn.ToString(), worldpos.ToVector2() + new Vector2(0, 64), Color.Black);


        }

        public void Update(GameTime gameTime)
        {
            color.Update(gameTime);
                    }
    }
}

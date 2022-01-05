using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Tweening;


using System;
using System.Collections.Generic;

namespace sugarscape
{
    public class Agent
    {
        private int vision;
        private int stores;
        private Point rsize;
        private int metabolism;
        public Point gridposition;
        public Vector2 worldposition;
        public Point last_position;
        private Color color;
        private readonly Tweener _tweener = new Tweener();
        public bool moving = false;
        public Point offset = new Point(4, 4);
        public Point best = new Point(0, 0);
        public int maxval = 0;
        public Agent(Point pos, int vision = 2, int metabolism = 1, int stores = 5)
        {
            Random r = new Random();
            color = new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
            this.gridposition = pos;
            worldposition = GridToPixel(pos);
            this.vision = vision;
            this.metabolism = metabolism;
            this.stores = stores;
            var wsize = World.squaresize.X - offset.X * 2;
            rsize = new Point(wsize, wsize);

        }
        public void Update(GameTime g)
        {
            _tweener.Update(g.GetElapsedSeconds());
        }
        public void Activate()
        {
            var random = new Random();
            List<Point> possible = Look();
            best = possible[random.Next(possible.Count)];
            Move(best);
            Eat();
        }
        public void Move(Point p)
        {
            last_position = gridposition;
            gridposition = p;

            Vector2 newpoint = GridToPixel(p) + offset.ToVector2();
            _tweener.TweenTo(target: this,
                expression: player => this.worldposition,
                toValue: newpoint,
                duration: .5f,
                delay: 0)
                .Easing(EasingFunctions.CubicInOut);
            moving = true;
        }
        public void Eat()
        {
            stores += World.Instance.HarvestSugar(gridposition);
        }
        public List<Point> Look()
        {
            List<Point> possible = new List<Point>();
            maxval = 0;

            int worldlength = World.Instance.fields.GetLength(0);
            int worldwidth = World.Instance.fields.GetLength(1);
            int agentx = gridposition.X;
            int agenty = gridposition.Y;

            List<Point> points = new List<Point>();
            for (int i = vision * -1; i <= vision; i++)
            {
                for (int j = vision * -1; j <= vision; j++)
                {
                    int worldx = agentx + i;
                    int worldy = agenty + j;
                    if (i == 0 && j == 0)
                        continue;
                    if (worldx < 0 || worldx >= worldwidth || worldy < 0 || worldy >= worldlength)
                        continue;
                    points.Add(new Point(worldx, worldy));

                }
            }

            foreach (Point checkpoint in points)
            {
                var worldx = checkpoint.X;
                var worldy = checkpoint.Y;
                if (World.Instance.AvailableSpace(checkpoint))
                {
                    Sugar checked_sugar = World.Instance.fields[worldx, worldy];
                    checked_sugar.checked_ = true;
                    int sugar = checked_sugar.yield;

                    if (sugar > maxval)
                    {
                        maxval = sugar;
                        possible.Clear();
                        possible.Add(checkpoint);
                    }

                    else if (sugar == maxval)
                    {
                        possible.Add(checkpoint);
                    }
                }
            }
        

            return possible;

        }
    public static Vector2 GridToPixel(Point p)
    {
        var tmp = p * World.squaresize;
        return new Vector2(tmp.X, tmp.Y);
    }


    internal void Draw(SpriteBatch g, GameTime gameTime, Point size)
    {
        //g.Draw(World.whiteRectangle, new Rectangle(gridposition * World.squaresize + offset, World.squaresize - offset - offset), color);

        g.Draw(World.whiteRectangle, new Rectangle(worldposition.ToPoint(), rsize), color);
        g.DrawString(GameDriver.font, best.ToString() + " " + maxval.ToString(), worldposition, Color.Black);

        //p.DrawSolidRectangle(worldposition, rsize, rsize, color);
        /*Point current_pos_real = gridposition * World.squaresize + new Point(World.squaresize.X / 2, World.squaresize.Y / 2);
        Point last_pos_real = last_position * World.squaresize + new Point(World.squaresize.X / 2, World.squaresize.Y / 2);
        p.DrawSegment(current_pos_real.ToVector2(), last_pos_real.ToVector2(), color);*/
    }
}
}

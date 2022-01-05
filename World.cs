using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using System;
using System.Collections.Generic;

namespace sugarscape
{
    public sealed class World
    {
        private static World instance = null;
        private static readonly object padlock = new object();
        public Sugar[,] fields;
        public Agent[,] agents;
        private int worldwidth;
        private int worldlength;
        public static Texture2D whiteRectangle;
        public static Dictionary<int, Color> sugarcolors = new Dictionary<int, Color>();
        public static Point squaresize;
        public static Point screensize;
        public static Random r = new Random();


        World()
        {
        }
        public void Init(GraphicsDevice g, Point _screensize)

        {
            screensize = _screensize;

            // Create a 1px square rectangle texture that will be scaled to the
            // desired size and tinted the desired color at draw time
            whiteRectangle = new Texture2D(g, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });
            sugarcolors.Add(0, Color.White);
            sugarcolors.Add(1, Color.LightGoldenrodYellow); //R:250 G:250 B:210
            sugarcolors.Add(2, Color.Goldenrod); // {R:218 G:165 B:32 A:255}
            sugarcolors.Add(3, Color.DarkGoldenrod); //{R:184 G:134 B:11 A:255}
            sugarcolors.Add(4, Color.Chocolate); // { R: 210 G: 105 B: 30 A: 255}
        }
        public void Draw(SpriteBatch g, GameTime gameTime)
        {
            Point size = new Point(20, 20);
            for (int i = 0; i < worldwidth; i++)
                for (int j = 0; j < worldlength; j++)
                    if (fields[i, j] != null)
                    {
                        Sugar s = fields[i, j];
                        s.Draw(g, gameTime, new Point(i, j));
                        //g.Draw(whiteRectangle, new Rectangle(30, 30, 10, 10), Color.White);
                    }


            foreach (Agent a in agents)
                if (a != null)
                    a.Draw(g, gameTime, size);

        }
        public bool AvailableSpace(Point p)
        {
            if (agents[p.X, p.Y] == null)
                return true;

            return false;
        }
        public int HarvestSugar(Point p)
        {
            Sugar s = fields[p.X, p.Y];
            return s.Harvest();
        }
        public void GenerateGrid(int x, int y)
        {
            worldwidth = x;
            worldlength = y;
            int squareside = 0;
            if (worldwidth < worldlength)
                squareside = screensize.X / worldwidth - 1;
            else
                squareside = screensize.Y / worldlength - 1;

            squaresize = new Point(squareside);

            fields = new Sugar[x, y];
            agents = new Agent[x, y];

            for (int i = 0; i < worldwidth; i++)            
                for (int j = 0; j < worldlength; j++)
                {
                    int smax = r.Next(1, 4);
                    int sstart = r.Next(0, smax);
                    fields[i, j] = new Sugar(smax, sstart);
                }

            
            AddAgent(r.Next(5), r.Next(5));
            /*for (int i = 0; i < worldwidth; i++)
                for (int j = 0; j < worldlength; j++)
                {
                    int rando = r.Next(20);
                    Console.WriteLine(rando);
                    if (r.Next(1, 20) == 10)
                    {
                        Console.WriteLine("added");
                        AddAgent(i, j);
                    }
                }*/
        }
        public void AddAgent(int x, int y)
        {
            agents[x, y] = new Agent(new Point(x, y));
            fields[x, y].spawn = true;
            fields[x, y].Harvest();
            fields[x, y].max = 4;
        }
        public static World Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new World();
                    }
                    return instance;
                }
            }
        }
    }
}

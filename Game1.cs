using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Diagnostics;
using System.Collections.Generic;


namespace sugarscape
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private double _timeSinceUpdate;

        public float UpdateRate = 2.0f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.IsBorderless = true;

            if (GraphicsDevice == null)
            {

                _graphics.ApplyChanges();

            }

            Debug.WriteLine(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            _graphics.ApplyChanges();
            Debug.WriteLine(_graphics.PreferredBackBufferWidth);
        }

        protected override void Initialize()
        {

            base.Initialize();

            World.Instance.GenerateGrid(20, 20);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            World.Instance.Init(GraphicsDevice, new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // In Game.Update
            _timeSinceUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            while (_timeSinceUpdate > 1f / UpdateRate)
            {
                _timeSinceUpdate -= 1f / UpdateRate;

                    foreach (Agent a in World.Instance.agents)
                    {
                        if (a != null)
                        {
                            a.Update();
                        }
                    }
                foreach (Sugar s in World.Instance.fields)
                {
                    if (s != null)
                    {
                        s.Grow();
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
            _spriteBatch.Begin();

            World.Instance.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();
        }
    }
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
            sugarcolors.Add(1, Color.LightGoldenrodYellow);
            sugarcolors.Add(2, Color.Goldenrod);
            sugarcolors.Add(3, Color.DarkGoldenrod);
            sugarcolors.Add(4, Color.Chocolate);
        }
        public void Draw(SpriteBatch g, GameTime gameTime)
        {
            Point size = new Point(20, 20);
            for (int i = 0; i < worldwidth; i++)
            {
                for (int j = 0; j < worldlength; j++)
                {
                    if (fields[i, j] != null)
                    {
                        Sugar s = fields[i, j];
                        s.Draw(g, gameTime, new Point(i, j), size);
                    }
                }
            }

            foreach(Agent a in agents)
            {
                if (a != null)
                {
                    a.Draw(g, gameTime, size);
                }
            }
        }
        public bool AvailableSpace(Point p)
        {
            if(agents[p.X, p.Y] == null)
            {
                return true;
            }
            return false;
        }
        public int HarvestSugar(Point p)
        {
            Sugar s = fields[p.X, p.Y];
            return s.Harvest();
        }
        public void GenerateGrid(int x, int y)
        {

            Random r = new Random();
            worldwidth = x;
            worldlength = y;

            if(worldwidth > worldlength)
            {
                int squareside = screensize.X / worldwidth;
                squaresize = new Point(squareside);
            }
            else
            {
                int squareside = screensize.Y / worldlength;
                squaresize = new Point(squareside);
            }
            fields = new Sugar[x, y];
            agents = new Agent[x, y];

            for (int i = 0; i < worldwidth; i++)
            {
                for (int j = 0; j < worldlength; j++)
                {
                    int smax = r.Next(1, 4);
                    int sstart = r.Next(0, smax);
                    fields[i, j] = new Sugar(smax, sstart);
                }
            }

            for (int i = 0; i < worldwidth; i++)
            {
                for (int j = 0; j < worldlength; j++)
                {
                    int rando = r.Next(20);
                    Console.WriteLine(rando);
                    if (r.Next(1,20) == 10)
                    {
                        Console.WriteLine("added");
                        AddAgent(i, j);
                    }
                }
            }
        }
        public void AddAgent(int x, int y)
        {
            agents[x, y] = new Agent(new Point(x, y));
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
    }

    public class Agent
    {
        private int vision;
        private int stores;
        private int metabolism;
        public Point position;
        private Color color;

        public Agent(Point pos, int vision = 3, int metabolism=1, int stores=5)
        {
            Random r = new Random();
            color = new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
            this.position = pos;
            this.vision = vision;
            this.metabolism = metabolism;
            this.stores = stores;
        }
        public void Update()
        {
            var random = new Random();
            List <Point> possible = Look();
            Point best = possible[random.Next(possible.Count)];
            Move(best);
            Eat();
        }
        public void Move(Point p)
        {
            position = p;
        }
        public void Eat()
        {
            stores += World.Instance.HarvestSugar(position);
        }
        public List<Point> Look()
        {
            List<Point> possible = new List<Point>();
            int maxval = 0;
            
            int worldlength = World.Instance.fields.GetLength(0);
            int worldwidth = World.Instance.fields.GetLength(1);
            for (int i = vision * -1; i <= vision; i++)
            {
                for (int j = vision * -1; j <= vision; j++)
                {
                    int worldx = MathHelper.Clamp(position.X + i, 0, worldlength-1);
                    int worldy = MathHelper.Clamp(position.Y + j, 0, worldwidth-1);

                    int sugar = World.Instance.fields[worldx, worldy].yield;
                    if (sugar > maxval)
                    {
                        Point checkpoint = new Point(worldx, worldy);
                        if (World.Instance.AvailableSpace(checkpoint))
                        {
                            maxval = sugar;
                            possible = new List<Point>();
                            possible.Add(checkpoint);
                                
                        }
                    }
                    if (sugar == maxval)
                    {
                        Point checkpoint = new Point(worldx, worldy);
                        if (World.Instance.AvailableSpace(checkpoint))
                        {
                            possible.Add(checkpoint);
                         }

                    }
                }
            }

            return possible;
            
        }

        internal void Draw(SpriteBatch g, GameTime gameTime, Point size)
        {
            Point offset = new Point(4, 4);
            g.Draw(World.whiteRectangle, new Rectangle(position* World.squaresize + offset, World.squaresize - offset - offset),
                    color);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using MLEM.Ui;


using System;
using System.Diagnostics;
using System.Collections.Generic;
using MLEM.Ui.Style;
using MLEM.Misc;
using MLEM.Ui.Elements;
using MLEM.Font;
using MLEM.Textures;

namespace sugarscape
{
    public class GameDriver : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private double _timeSinceUpdate;

        //UPS
        public float UpdateRate = 2.0f;

        private SpriteFont font;

        public UiSystem UiSystem;


        public GameDriver()
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

         
            World.Instance.GenerateGrid(30, 20);
            
            base.Initialize();


        }

        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Menu"); // Use the name of your sprite font file here instead of 'Score'.

            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            // Initialize the Ui system


            var style = new UntexturedStyle(_spriteBatch)
            {
                Font = new GenericSpriteFont(this.Content.Load<SpriteFont>("Menu")),
                //ButtonTexture = new NinePatch(this.Content.Load<Texture2D>("Textures/ExampleTexture"), padding: 1)
            };
            UiSystem = new UiSystem(this, style);

            World.Instance.Init(GraphicsDevice, new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));



            var panel = new Panel(Anchor.Center, size: new Vector2(100, 100), positionOffset: Vector2.Zero);
            this.UiSystem.Add("ExampleUi", panel);
            var box = new Panel(Anchor.Center, new Vector2(100, 1), Vector2.Zero, setHeightBasedOnChildren: true);
            box.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is some example text!"));
            box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay")
            {
                OnPressed = element => this.UiSystem.Remove("InfoBox"),
                PositionOffset = new Vector2(0, 1)
            });
            this.UiSystem.Add("InfoBox", box);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.UiSystem.Update(gameTime);

            // In Game.Update
            _timeSinceUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            while (_timeSinceUpdate > 1f / UpdateRate)
            {
                _timeSinceUpdate -= 1f / UpdateRate;

                foreach (Agent a in World.Instance.agents)
                {
                    if (a != null)
                    {
                        a.Activate();
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
            foreach (Agent a in World.Instance.agents)
            {
                if (a != null)
                {
                    a.Update(gameTime);
                }
            }
            foreach (Sugar s in World.Instance.fields)
            {
                if (s != null)
                {
                    s.Update(gameTime);
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            // DrawEarly needs to be called before clearing your graphics context
            UiSystem.DrawEarly(gameTime,_spriteBatch);
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _spriteBatch.Begin();

            World.Instance.Draw(_spriteBatch,  gameTime);


            // Call Draw at the end to draw the Ui on top of your game
            _spriteBatch.End();

            UiSystem.Draw(gameTime, _spriteBatch);


            base.Draw(gameTime);
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

            foreach (Agent a in agents)
            {
                if (a != null)
                {
                    a.Draw(g, gameTime, size);
                }
            }
        }
        public bool AvailableSpace(Point p)
        {
            if (agents[p.X, p.Y] == null)
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

            if (worldwidth < worldlength)
            {
                int squareside = screensize.X / worldwidth - 1;
                squaresize = new Point(squareside);
            }
            else
            {
                int squareside = screensize.Y / worldlength - 1;
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
                    if (r.Next(1, 20) == 10)
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
    public static class MyExtensions
    {
        public static Vector2 ToVector(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}

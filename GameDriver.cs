using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using MLEM.Ui;
using System.Diagnostics;
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
        public float UpdateRate = 1.0f;

        //private SpriteFont font;



        public GameDriver()
        {
            Debug.WriteLine("Start");
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
            World.Instance.Init(GraphicsDevice, new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));

            World.Instance.GenerateGrid(20, 20);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Debug.WriteLine("Load");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = Content.Load<SpriteFont>("Menu"); // Use the name of your sprite font file here instead of 'Score'.

        }

        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _timeSinceUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            while (_timeSinceUpdate > 1f / UpdateRate)
            {
                _timeSinceUpdate -= 1f / UpdateRate;

                Debug.WriteLine(gameTime.TotalGameTime);
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

        }

        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _spriteBatch.Begin();

            World.Instance.Draw(_spriteBatch,  gameTime);

            _spriteBatch.End();


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

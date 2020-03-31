using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TextureSplitter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region Fields

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        #endregion Fields

        #region Constructors

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #endregion Constructors

        #region Properties

        public Queue<(Action<string> Split, string Path)> FileQ { get; private set; }

        public Queue<(string path, Texture2D tex)> SaveQ { get; private set; }
        public Texture2D Tex { get; private set; }
        public Queue<Texture2D> TexQ { get; private set; }
        public byte Threshold { get; set; }
        private Regex RegMatch { get; set; }

        #endregion Properties

        #region Methods

        public void Clean(string safe)
        {
            if (RegMatch.IsMatch(Path.GetFileName(safe) ?? throw new InvalidOperationException()))
                return;
            var textureIn = LoadPNG(safe);

            var (width, height) = (textureIn.Width, textureIn.Height);
            Console.WriteLine($"Cleaning {width}x{height} texture");
            TexQ.Enqueue(textureIn);
            {
                var data = new Color[width * height];

                textureIn.GetData(data);
                for (var i = 0; i < data.Length; i++)
                {
                    var color = data[i];
                    if (color.A <= Threshold)
                        data[i] = Color.TransparentBlack;
                }

                var textureOut = new Texture2D(_graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                textureOut.SetData(data);

                SaveQ.Enqueue((safe, textureOut));
            }
        }

        public Texture2D LoadPNG(string path, int palette = -1)
        {
            Texture2D tex;
            using (var fs = File.OpenRead(path))
            {
                tex = Texture2D.FromStream(_graphics.GraphicsDevice, fs);
            }
            return tex;
        }

        public void Save(string path, Texture2D tex)
        {
            //string clean = Path.GetFileNameWithoutExtension(Regex.Replace(Filename, @"{[^}]+}", ""));

            Console.WriteLine($"Saving {tex.Width}x{tex.Height} texture => {path}");
            using (var fs = File.Create(path))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (SaveQ.Count >= TexQ.Count && SaveQ.Count > 0)
            {
                var (path, tex) = SaveQ.Dequeue();
                Save(path, tex);
            }

            if (SaveQ.Count >= TexQ.Count || TexQ.Count <= 0) return;
            // TODO: Add your drawing code here
            Tex = TexQ.Dequeue();
            if (Tex != null)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                _spriteBatch.Begin();
                var s = (float)_graphics.PreferredBackBufferWidth / Tex.Width;
                var s2 = (float)_graphics.PreferredBackBufferHeight / Tex.Height;
                _spriteBatch.Draw(Tex, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(s <= s2 ? s : s2), SpriteEffects.None, 0);
                _spriteBatch.End();
                Tex.Dispose();
                Tex = null;
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run. This is
        /// where it can query for any required services and load any non-graphic related content.
        /// Calling base.Initialize will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            if (CleanPNG.Program.Args.Count <= 0)
            {
                Console.WriteLine("No PNG files or folders in arguments.");
                Exit();
                return;
            }

            byte result;
            do
            {
                Console.Write("Enter Threshold (0-255):  ");
            } while (!byte.TryParse(Console.ReadLine(), out result));

            Threshold = result;
            // TODO: Add your initialization logic here
            FileQ = new Queue<(Action<string> Split, string Path)>();
            TexQ = new Queue<Texture2D>();
            SaveQ = new Queue<(string path, Texture2D tex)>();
            RegMatch = new Regex(@"_\d+x\d+", RegexOptions.IgnoreCase);
            foreach (var a in CleanPNG.Program.Args)
            {
                var safe = a.Trim('"');
                if (File.Exists(safe) && safe.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    FileQ.Enqueue((Clean, safe));
                else if (Directory.Exists(safe))
                {
                    var list = Directory.GetFiles(safe, "*.png", SearchOption.AllDirectories);
                    foreach (var f in list)
                        FileQ.Enqueue((Clean, f));
                }
            }
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferHeight = 512;
            _graphics.PreferredBackBufferWidth = 512;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent() =>
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        // TODO: use this.Content to load your game content here
        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions,
        /// gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (TexQ == null || FileQ == null || TexQ.Count == 0 && FileQ.Count == 0 && SaveQ.Count == 0 ||
                (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                 Keyboard.GetState().IsKeyDown(Keys.Escape)))
            {
                Exit();
                return;
            }

            // TODO: Add your update logic here
            if (TexQ.Count == 0 && FileQ.Count > 0)
            {
                var f = FileQ.Dequeue();
                f.Split(f.Path);
                SuppressDraw();
            }

            base.Update(gameTime);
        }

        #endregion Methods
    }
}
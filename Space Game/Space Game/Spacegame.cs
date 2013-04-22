using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Printing;
using System.Printing.Activation;
using System.Printing.IndexedProperties;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Spine_Library;

namespace Space_Game
{
    /// <summary>
    /// The Space Game!
    /// 
    /// V0.0.5+
    /// </summary>
    
    /// Changelog:
    /// 
    /// V0.0.1
    ///-Initial Release
    ///  
    /// Indev 0.0.2
    ///- Added a lot of new Planets (now 15 celestial bodies)
    ///- Added a framerate Counter
    ///- Cleaned up debug
    ///- Added names for planets
    ///- Added a debug feature to show how many Kuma days it takes to orbit
    ///- Added planet's orbital angle to debug
    ///- Changed internally how planets pick sizes, heightmaps, and radius
    ///
    /// Indev 0.0.3
    ///- Added planet tempuratures
    ///- Added atmospheres to planets
    ///- Added a basic GUI, which can be minimized
    ///- Removed test ship, ships will soon go into heavier development
    ///
    /// Indev 0.04
    ///- Added basic vegetation
    ///- Added water to planets
    ///    
    /// Indev 0.0.4-1
    ///- Tweaked drawing
    ///- Changed drawing internally to improve framerate
    ///- Added colors to water
    ///- Performance tweaks
    ///- Bug Fixes
    ///
    /// Indev 0.0.5
    ///- Added more vegetation
    ///- Converted to fullscreen
    ///- Fleshed out bottom GUI
    ///- Added planet information GUI
    ///- Added new planet (Planet X-32)
    ///- Added main menu with temp music
    ///- Added menu (currently has no features)
    ///- Reworked view slightly to make zooming in easier
    ///- Tweaked planet rendering to look better    
    public class Spacegame : Microsoft.Xna.Framework.Game
    {
        #region Game
        #region Variables

        //Game State
        byte gameState = 0;
        bool isPaused = false;
        float gameSpeed = 1;
        string version = "0.0.5_B";
        string gameName = "Space Game";
        Config config;
        //

        #region main variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont drawFont, guiFont, titleFont;
        SoundManager soundManager;
        static Texture2D blank;

        Random rand = new Random();
        Texture2D spaceTex;
        Vector2 winPos = new Vector2(0, 0);
        Vector2 winOffset = new Vector2(400, 240);
        Vector2 trackedVeiwOffset;
        BasicEffect basicEffect;
        BasicEffect quadEffect;
        #endregion

        #region game variables
        Spine_Library.FPSHandler FPS = new Spine_Library.FPSHandler();
        MainMenu menu;

        SolarSystem system;
        Planet targetPlanet;
        int targetPlanetID = 0;
        bool isTrackingPlanet = true;
        bool isTrackingSurface = false;

        bool[] isKeydown = new bool[100];

        Texture2D[] guiElements = new Texture2D[10];
        int guiBottomRelative = 0, guiPlanetRelative = 16;

        bool debug = false;

        double zoom = 1, winY = 0, winRot = 0;
        float tempZoom = 100;
        int winWidth = 800, winHeight = 480;


        Fleet player;

        KeyWatcher planetUp = new KeyWatcher(Keys.PageUp);
        KeyWatcher planetDown = new KeyWatcher(Keys.PageDown);
        KeyWatcher tracking = new KeyWatcher(Keys.T);
        KeyWatcher debugKey = new KeyWatcher(Keys.F1);
        KeyWatcher fullscreen = new KeyWatcher(Keys.F2);
        KeyWatcher speedUp = new KeyWatcher(new List<Keys>() { Keys.OemPlus, Keys.Add }, false);
        KeyWatcher speedDown = new KeyWatcher(new List<Keys>() { Keys.OemMinus, Keys.Subtract }, false);
        KeyWatcher save = new KeyWatcher(new List<Keys>() { Keys.LeftControl, Keys.S }, true);
        KeyWatcher load = new KeyWatcher(new List<Keys>() { Keys.LeftControl, Keys.L }, true);
        KeyWatcher trackSurface = new KeyWatcher(Keys.Q);
        #endregion

        #region menu variables
        Planet menuPlanet, menuMoon;
        int menuTextAlpha = 128;
        sbyte menuTextAlphaChange = 1;
        int menuSoundID = 0;
        #endregion

        //double cameraLength = 1;

        #endregion

        public Spacegame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public Spacegame(string[] args)
        {
            if (args.Length > 0)
                system = SolarSystem.load(args[0], this);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1D/240D);
            graphics.ToggleFullScreen();

            //initialize projection          
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (winPos.X - winOffset.X, graphics.GraphicsDevice.Viewport.Width + (winPos.X - winOffset.X),
            graphics.GraphicsDevice.Viewport.Height + (winPos.Y - winOffset.Y), (winPos.Y - winOffset.Y), 0, 1);
            //Done!

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            guiFont = Content.Load<SpriteFont>(@"guiFont");

            spriteBatch.Begin();
            spriteBatch.DrawString(guiFont, "Loading", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            //set up the blank 1 pixel texture for drawing lines and shapes
            blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blank.SetData(new[] { Color.White });

            #region game initialization
            //create a test planet
            system = new SolarSystem("Sol");
            
            //initialize planets
            //Sun
            system.addPlanet(new Planet(system, "Sun", new Vector2(250, 250), 5, Color.Red, Color.Yellow, 0D));

            //Home planet
            system.addPlanet(new Planet(system, "Kuma", 3, Color.Red, Color.DarkGreen, system.getPlanet(0), 2000, 0.0005F, rand.NextDouble() * MathHelper.TwoPi, 200, 255, 320));
            system.addPlanet(new Planet(system, "Luna", 1, Color.Yellow, Color.Maroon, system.getPlanet("Kuma"), 700, -0.000345F, rand.NextDouble() * MathHelper.TwoPi, 10, 0));

            //Altos - 3
            system.addPlanet(new Planet(system, "Altos", 2, Color.Purple, Color.DarkRed, system.getPlanet(0), 5000, -0.000345F, rand.NextDouble() * MathHelper.TwoPi, 255, 200, 220, Color.Plum));
            system.addPlanet(new Planet(system, "Litha", 1, Color.DarkGray, Color.OliveDrab, system.getPlanet("Altos"), 500, 0.0005F, rand.NextDouble() * MathHelper.TwoPi, 128, 200));
            system.addPlanet(new Planet(system, "Nora", 1, Color.Maroon, Color.DarkGray, system.getPlanet("Altos"), 1000, -0.001F, rand.NextDouble() * MathHelper.TwoPi, 0, 0));

            //Jodon - 6
            system.addPlanet(new Planet(system, "Jodon", 3, Color.Orange, Color.Green, system.getPlanet(0), 15000, -0.00023F, rand.NextDouble() * MathHelper.TwoPi, 255, 150, 320, Color.LightGreen, 1000));
            system.addPlanet(new Planet(system, "Joso", 1, Color.HotPink, Color.Red, system.getPlanet("Jodon"), 1000, -0.00154F, rand.NextDouble() * MathHelper.TwoPi, 255, 100, 130, Color.Purple));

            //Katos - 8
            system.addPlanet(new Planet(system, "Katos", 3, Color.Yellow, Color.Purple, system.getPlanet(0), 25000, 0.0000345F, rand.NextDouble() * MathHelper.TwoPi, 100, 100));
            system.addPlanet(new Planet(system, "Japa", 2, Color.DarkBlue, Color.Aqua, system.getPlanet("Katos"), 1000, -0.00005435F, rand.NextDouble() * MathHelper.TwoPi, 255, 0));

            //Aposos - 10
            system.addPlanet(new Planet(system, "Aposos", 3, Color.Blue, Color.AntiqueWhite, system.getPlanet(0), 50000, 0.000004532F, rand.NextDouble() * MathHelper.TwoPi, 200, 100));

            //Kato - 11
            system.addPlanet(new Planet(system, "Racheal-Topia", 4, Color.Lerp(Color.Turquoise, Color.Pink, 0.5F), Color.Red, system.getPlanet(0), 70000, 0.000004532F, rand.NextDouble() * MathHelper.TwoPi, 255, 200, 300, Color.Tomato, 2500));
            system.addPlanet(new Planet(system, "Orion", 1, Color.Purple, Color.Purple, system.getPlanet(11), 1000, 0.000004532F, rand.NextDouble() * MathHelper.TwoPi, 100, 100));

            //Ceriese - 11
            system.addPlanet(new Planet(system, "Ceriese", 5, Color.Blue, Color.Red, system.getPlanet(0), 75000, 0.000000234F, rand.NextDouble() * MathHelper.TwoPi, 255, 0));
            system.addPlanet(new Planet(system, "Kratos", 2, Color.Maroon, Color.Red, system.getPlanet("Ceriese"), 1000, -0.000234F, rand.NextDouble() * MathHelper.TwoPi, 200, 220));
            system.addPlanet(new Planet(system, "Mars", 2, Color.SaddleBrown, Color.SandyBrown, system.getPlanet("Ceriese"), 1500, 0.0000342F, rand.NextDouble() * MathHelper.TwoPi, 100, 200));
            system.addPlanet(new Planet(system, "Ares", 2, Color.White, Color.LightBlue, system.getPlanet("Ceriese"), 3000, 0.0001234F, rand.NextDouble() * MathHelper.TwoPi, 50, 150));

            targetPlanetID = system.getPlanetId("Kuma");
            player = new Fleet(system, "Player");

            system.getPlanet("Kuma").addBuilding(new testBuilding(system, system.getPlanet("Kuma"), player, 2.134));
            player.addShip(new testShip(player, system, "Kuma", 500, 0F));
            #endregion

            menuPlanet =new Planet(new Vector2(0,0), "Kuma", 3, Color.Red, Color.DarkGreen, 200, 255, 320, Color.Aqua);
            menuMoon = new Planet("Luna",1, Color.Orange, Color.Red, menuPlanet, 100, -0.0002F, 0.123, 0, 0);

            menu = new MainMenu(new Vector2(GraphicsDevice.Viewport.Width / 2 - 32, GraphicsDevice.Viewport.Height / 2 - 32), Color.Red, Color.Blue, false, this);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            soundManager = new SoundManager(Content, 1F);
            menuSoundID = soundManager.playSound(SoundManager.SND_TITLE, true);

            spriteBatch.Begin();
            spriteBatch.DrawString(guiFont, "Loading", new Vector2(10, 10), Color.White);
            spriteBatch.End();

            spaceTex = Content.Load<Texture2D>(@"space");
            drawFont = Content.Load<SpriteFont>(@"spriteFont");
            guiFont = Content.Load<SpriteFont>(@"guiFont");
            titleFont = Content.Load<SpriteFont>(@"FutureFont");
            Texture2D tex_ship = Content.Load<Texture2D>(@"texShip");
            Texture2D tex_unit = Content.Load<Texture2D>(@"texUnit");

            guiElements[0] = Content.Load<Texture2D>(@"GUI\gui");
            guiElements[1] = Content.Load<Texture2D>(@"GUI\guiPlanet");
            guiElements[2] = Content.Load<Texture2D>(@"GUI\smallArrow");

            //motherShip = new ship(system, "Kuma", 500, 0, spriteBatch, tex_ship);

            quadEffect = new BasicEffect(graphics.GraphicsDevice);
            quadEffect.EnableDefaultLighting();

            quadEffect.World = Matrix.Identity;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = spaceTex;

            if (Config.exists("Temp"))
            {
                config = Config.load("Temp");
                config.applyConfig(this);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
         protected override void Update(GameTime gameTime)
        {
            if (gameState == 0)
                updateMenu(gameTime);
            if (gameState == 1)
                updateInGame(gameTime);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (gameState == 0)
                drawMenu(gameTime);
            if (gameState == 1)
                drawInGame(gameTime);


            base.Draw(gameTime);
        }

        private void updateMenu(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            menuPlanet.position = new Vector2(GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4);
            menuPlanet.Update(0.5F, false, this, basicEffect, GraphicsDevice, true, false, 1, 0);
            menuMoon.Update(0.5F, false, this, basicEffect, GraphicsDevice, true, false, 1, 0);

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                gameState = 1;
                soundManager.stopSound(SoundManager.SND_TITLE);
            }            

            menuTextAlpha += menuTextAlphaChange;
            if (menuTextAlpha == 256)
            {
                menuTextAlphaChange = -1;
                menuTextAlpha = 255;
            }
            if (menuTextAlpha == 127)
            {
                menuTextAlphaChange = 1;
                menuTextAlpha = 128;
            }
        }

        private void drawMenu(GameTime gameTime)
        {
            spriteBatch.Begin();
            winPos = Vector2.Zero;

            //render planets
            menuPlanet.Render(0.5F, false, this, basicEffect, GraphicsDevice, true, false, 1);
            menuMoon.Render(0.5F, false, this, basicEffect, GraphicsDevice, true, false, 1);
            //draw fade overlay
            spriteBatch.Draw(blank, new Rectangle(0, 0, 1600, 960), new Color(32, 32, 32, 160));
            //draw blinking text
            spriteBatch.DrawString(drawFont, "<Press Enter>", new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height - 32), 
                Color.FromNonPremultiplied(255,255,255,(int)menuTextAlpha), 0.001F, 
                new Vector2(drawFont.MeasureString("<Press Enter>").X/2,drawFont.MeasureString("<Press Enter>").Y/2), 1F, SpriteEffects.None, 0);
            //draw version in corner
            spriteBatch.DrawString(guiFont, version, new Vector2(2, GraphicsDevice.Viewport.Height - 14), Color.White);

            //draw title
            spriteBatch.DrawString(titleFont, "-" + gameName + "-", new Vector2(GraphicsDevice.Viewport.Width / 2 - titleFont.MeasureString(gameName).X / 2, 10), Color.White);


            spriteBatch.End();
        }

        private void updateInGame(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
        }

        private void drawInGame(GameTime gameTime)
        {
            RasterizerState r = new RasterizerState();
            //r.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = r;

            double tempWinRot = winRot;
            if (targetPlanet != null)
            {
                if (isTrackingSurface)
                    tempWinRot = -targetPlanet.rotation + winRot;
            }

            winHeight = GraphicsDevice.Viewport.Height;
            winWidth = GraphicsDevice.Viewport.Width;

            //update fps
            FPS.onDraw(gameTime);

            spriteBatch.Begin();

            system.update(this, (float)zoom, debug, basicEffect, GraphicsDevice, isPaused, gameSpeed, winRot);

            //update view
            changeVeiw();

            //track planet
            if (isTrackingPlanet)
            {
                pushViewToPlanet(targetPlanet, trackedVeiwOffset);
                winOffset = extraMath.calculateVectorOffset(targetPlanet.position, MathHelper.PiOver2, (float)winY); 
            }
            //reset view
            Matrix rot = Matrix.CreateRotationZ(0);
            Matrix trans = Matrix.CreateTranslation(new Vector3((-winPos + winOffset + new Vector2(GraphicsDevice.Viewport.Width / 6, GraphicsDevice.Viewport.Height / 6)), 0));
            basicEffect.View = trans * rot;

            //spriteBatch.Draw(spaceTex, this.graphics.GraphicsDevice.Viewport.TitleSafeArea, null, Color.White, 0F, new Vector2(0,0), SpriteEffects.None, 0);
            system.render(this, (float)zoom, debug, basicEffect, GraphicsDevice, isPaused, gameSpeed, tempWinRot);

            //motherShip.tick(Vector2.Subtract(winPos, winOffset), (float)zoom, this);


            //update player's fleet
            player.update(this, gameTime, isPaused, gameSpeed, basicEffect, GraphicsDevice, zoom, tempWinRot);
            
            #region Debug
            //draw debug data
            if (debug)
            {
                spriteBatch.DrawString(drawFont, "Zoom: " + zoom.ToString(), new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(drawFont, "FPS: " + FPS.getFrameRate().ToString(), new Vector2(10, 25), Color.White);
                spriteBatch.DrawString(drawFont, "Planet: " + targetPlanet.Name, new Vector2(10, 40), Color.White);
                spriteBatch.DrawString(drawFont, "Planet Count: " + system.getPlanetCount().ToString(), new Vector2(10, 55), Color.White);
                spriteBatch.DrawString(drawFont, "Orbital Period: " + Math.Round(targetPlanet.getDaysInYear(), 3).ToString() + " Kuma Days", new Vector2(10, 70), Color.White);
                spriteBatch.DrawString(drawFont, "Orbital Angle: " + targetPlanet.orbitAngle.ToString(), new Vector2(10, 85), Color.White);
                spriteBatch.DrawString(drawFont, "Surface Temp: " + Math.Round(targetPlanet.getPlanetTemp(), 3).ToString() + "­° Celcius", new Vector2(10, 100), Color.White);
                spriteBatch.DrawString(drawFont, "Game Speed: " + Math.Round(gameSpeed, 2).ToString(), new Vector2(10, 115), Color.White);
                spriteBatch.DrawString(drawFont, "Mouse X: " + Mouse.GetState().X, new Vector2(10, 130), Color.White);
                spriteBatch.DrawString(drawFont, "Mouse Y: " + Mouse.GetState().Y, new Vector2(10, 145), Color.White);
                spriteBatch.DrawString(drawFont, "Planet X: " + targetPlanet.position.X, new Vector2(10, 160), Color.White);
                spriteBatch.DrawString(drawFont, "Planet Y: " + targetPlanet.position.Y, new Vector2(10, 175), Color.White);
                spriteBatch.DrawString(drawFont, "WinPos X: " + winPos.X, new Vector2(10, 190), Color.White);
                spriteBatch.DrawString(drawFont, "WinPos Y: " + winPos.Y, new Vector2(10, 205), Color.White);
            }
            #endregion

            #region Draw GUI

            # region planetGUI

            //planet info
            spriteBatch.Draw(guiElements[1], new Rectangle(0, winHeight - 32 + guiBottomRelative - guiPlanetRelative, 168, 200), Color.White);
            //planet
            spriteBatch.DrawString(guiFont, targetPlanet.Name, new Vector2(26, winHeight - 30 + guiBottomRelative - guiPlanetRelative), Color.Black);
            //tempurature
            if (targetPlanet.getPlanetTemp() != 0)
                spriteBatch.DrawString(guiFont, "Temp: " + Math.Round(targetPlanet.getPlanetTemp(), 2).ToString() + "° C",
                    new Vector2(4, winHeight - 16 + guiBottomRelative - guiPlanetRelative), Color.Black);
            else
                spriteBatch.DrawString(guiFont, "Temp: Unknown° C",
                    new Vector2(4, winHeight - 16 + guiBottomRelative - guiPlanetRelative), Color.Black);
            //orbit period
            if (targetPlanet.getDaysInYear() != Double.PositiveInfinity)
                spriteBatch.DrawString(guiFont, "Orbit Period: " + Math.Round(targetPlanet.getDaysInYear(), 1).ToString() + "KD",
                    new Vector2(4, winHeight + 0 + guiBottomRelative - guiPlanetRelative), Color.Black);
            else
                spriteBatch.DrawString(guiFont, "Orbit Period: None",
                    new Vector2(4, winHeight + 0 + guiBottomRelative - guiPlanetRelative), Color.Black);
            //atmosphere density
            spriteBatch.DrawString(guiFont, "Atm. Density: " + Math.Round((targetPlanet.getPlanetAtmosDensity() / 200) * 100) + " % KP",
                new Vector2(4, winHeight + 16 + guiBottomRelative - guiPlanetRelative), Color.Black);
            //atmoshpere breathability
            spriteBatch.DrawString(guiFont, "Breathability: " + Math.Round((targetPlanet.getPlanetBreathability() / 255) * 100) + "%",
                new Vector2(4, winHeight + 32 + guiBottomRelative - guiPlanetRelative), Color.Black);
            //atmoshpere toxicity
            spriteBatch.DrawString(guiFont, "Toxicity: " + Math.Round((targetPlanet.getPlanetToxicity() / 255) * 100) + "%",
                new Vector2(4, winHeight + 48 + guiBottomRelative - guiPlanetRelative), Color.Black);
            #endregion

            #region bottom
            //backdrop
            spriteBatch.Draw(guiElements[0], new Rectangle(0, winHeight - 48 + guiBottomRelative, 800, 48), Color.White);
            //coin
            if (drawFont.MeasureString(player.getMoney().ToString()).X < 100)
                spriteBatch.DrawString(drawFont, player.getMoney().ToString(), new Vector2(34, winHeight - 30 + guiBottomRelative), Color.Black);
            //material
            if (drawFont.MeasureString(player.getMaterial().ToString()).X < 100)
                spriteBatch.DrawString(drawFont, player.getMaterial().ToString(), new Vector2(168, winHeight - 30 + guiBottomRelative), Color.Black);
            //food
            if (drawFont.MeasureString(player.getFood().ToString()).X < 100)
                spriteBatch.DrawString(drawFont, player.getFood().ToString(), new Vector2(303, winHeight - 30 + guiBottomRelative), Color.Black);
            //fuel
            if (drawFont.MeasureString(player.getFuel().ToString()).X < 100)
                spriteBatch.DrawString(drawFont, player.getFuel().ToString(), new Vector2(438, winHeight - 30 + guiBottomRelative), Color.Black);
            if (gameSpeed >= 1)
            {
                for (int i = 1; i <= gameSpeed; i++)
                    spriteBatch.Draw(guiElements[2], new Rectangle(700 + i * 14, winHeight - 48 + guiBottomRelative, 16, 16), Color.White);
            }
            else
            {
                spriteBatch.Draw(guiElements[2], new Rectangle(714, winHeight - 48 + guiBottomRelative, 7, 16), new Rectangle(0, 0, 7, 16), Color.White);
            }

            #endregion

            if (isPaused)
                spriteBatch.Draw(blank, new Rectangle(0, 0, 1600, 960), new Color(32, 32, 32, 160));

            menu.render(spriteBatch, GraphicsDevice, this);
            #endregion

            spriteBatch.End();
        }

        private void changeVeiw()
        {
            if (!isPaused)
            {
                # region keyboard
                #region Zoom
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
                {
                    if (zoom > 0.01F)
                    {
                        zoom /= (float)(1.05F * FPS.getCommonDiff());
                        trackedVeiwOffset /= new Vector2(1.05F);
                        winY /= 1.05F;
                    }
                }

                if (Keyboard.GetState().IsKeyDown(Keys.NumPad8))
                {
                    if (zoom < 6)
                    {
                        zoom *= (float)(1.05F * FPS.getCommonDiff());
                        trackedVeiwOffset *= new Vector2(1.05F);
                        winY *= 1.05F;
                    }
                }
                #endregion

                #region Move
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    winRot -= 0.01;
                    //winPos.X -= (float)(3 * FPS.getCommonDiff());
                    //trackedVeiwOffset.X -= (float)(3 * FPS.getCommonDiff());
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    winRot += 0.01;
                    //winPos.X += (float)(3 * FPS.getCommonDiff());
                    //trackedVeiwOffset.X += (float)(3 * FPS.getCommonDiff());
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    if (winY >= 2)
                        winY -= 2;
                    //winPos.Y += (float)(3 * FPS.getCommonDiff());
                    //trackedVeiwOffset.Y += (float)(3 * FPS.getCommonDiff());
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    winY += 2;
                    //winPos.Y -= (float)(3 * FPS.getCommonDiff());
                    //trackedVeiwOffset.Y -= (float)(3 * FPS.getCommonDiff());
                }
                #endregion

                #region Planet Options
                //planet up
                planetUp.update();
                if (planetUp.wasPressed)
                {
                    targetPlanetID += 1;
                    if (targetPlanetID >= system.getPlanetCount())
                    {
                        targetPlanetID = 0;
                    }
                    trackedVeiwOffset = Vector2.Zero;
                }
                //planet down
                planetDown.update();
                if (planetDown.wasPressed)
                {
                    targetPlanetID -= 1;
                    if (targetPlanetID < 0)
                    {
                        targetPlanetID = system.getPlanetCount() - 1;
                    }
                    trackedVeiwOffset = Vector2.Zero;
                }
                //toggle tracking
                tracking.update();
                if (tracking.wasPressed)
                {
                    isTrackingPlanet = !isTrackingPlanet;
                    trackedVeiwOffset = Vector2.Zero;
                }
                //toggle surface tracking
                trackSurface.update();
                if (trackSurface.wasPressed)
                {
                    isTrackingSurface = !isTrackingSurface;
                }
                #endregion

                # region Debug
                debugKey.update();
                if (debugKey.wasPressed)
                {
                    debug = !debug;
                }

                speedUp.update();
                if (speedUp.wasPressed)
                {
                        if (gameSpeed < 16)
                            gameSpeed *= 2;
                }

                speedDown.update();
                if (speedDown.wasPressed)
                {
                        if (gameSpeed > 0.5)
                            gameSpeed /= 2;
                }

                #endregion

                #region Under Development
                //Save
                save.update();
                if (save.wasPressed)
                {
                    system.save(Directory.GetCurrentDirectory() + "\\test.sol");
                    trackedVeiwOffset = Vector2.Zero;
                }

                //Load
                load.update();
                if (load.wasPressed)
                {
                    system = SolarSystem.load(Directory.GetCurrentDirectory() + "\\test.sol", this);
                    trackedVeiwOffset = Vector2.Zero;
                    load.wasPressed = false;
                }

                //fullscreen
                fullscreen.update();
                if (fullscreen.wasPressed)
                {
                    graphics.ToggleFullScreen();
                    fullscreen.wasPressed = false;
                }
                #endregion
                # endregion

                #region mouse

                #region bottom gui
                // maximize/minimize bottom gui
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & Mouse.GetState().X > 776 & Mouse.GetState().X < 799 &
                    Mouse.GetState().Y > winHeight - 48 + guiBottomRelative & Mouse.GetState().Y < winHeight - 48 + guiBottomRelative + 16)
                {
                    if (isKeydown[6] == false)
                    {
                        isKeydown[6] = true;

                        if (guiBottomRelative == 0)
                        {
                            guiBottomRelative = 32;
                            return;
                        }

                        if (guiBottomRelative == 32)
                        {
                            guiBottomRelative = 0;
                            return;
                        }
                    }
                }

                else
                {
                    if (isKeydown[6])
                        isKeydown[6] = false;
                }
                #endregion

                #region panet gui
                //maximize/minimize planet gui
                if (Mouse.GetState().LeftButton == ButtonState.Pressed &
                    Mouse.GetState().X > 0 & Mouse.GetState().X < 164 &
                    Mouse.GetState().Y > winHeight - 32 + guiBottomRelative - guiPlanetRelative &
                    Mouse.GetState().Y < winHeight - 16 + guiBottomRelative - guiPlanetRelative)
                {
                    if (isKeydown[8] == false)
                    {
                        isKeydown[8] = true;

                        if (guiPlanetRelative == 16)
                        {
                            guiPlanetRelative = 200;
                            return;
                        }

                        if (guiPlanetRelative == 200)
                        {
                            guiPlanetRelative = 16;
                            return;
                        }
                    }
                }

                else
                {
                    if (isKeydown[8])
                        isKeydown[8] = false;
                }
                #endregion

                #region menu
                //opens menu
                if (Mouse.GetState().LeftButton == ButtonState.Pressed &
                    Mouse.GetState().X > 750 & 
                    Mouse.GetState().X < 775 &
                    Mouse.GetState().Y > winHeight - 16 + guiBottomRelative - guiPlanetRelative &
                    Mouse.GetState().Y < winHeight + 16 + guiBottomRelative - guiPlanetRelative)
                {
                    if (isKeydown[8] == false)
                    {
                        isKeydown[8] = true;
                        menu.changeState(true, this);
                    }
                }

                else
                {
                    if (isKeydown[8])
                        isKeydown[8] = false;
                }
                #endregion
                #endregion
            }

            targetPlanet = system.getPlanet(targetPlanetID); ;
        }

        public void pushViewToPlanet(Planet planet)
        {
            winPos.X = planet.position.X;
            winPos.Y = planet.position.Y;
        }

        public void pushViewToPlanet(Planet planet, Vector2 offset)
        {
            if (planet != null)
            {
                winPos.X = planet.position.X + offset.X;
                winPos.Y = planet.position.Y + offset.Y;
            }
        }

        public Planet getNearestPlanet(Vector2 position)
        {
            Planet returnPlanet = system.getPlanet("Kuma");
            double distance = Double.PositiveInfinity;
            foreach (Planet p in system.getPlanets())
            {
                if (p != null)
                {
                    double tempDist = Vector2.Distance(position - winPos, p.position) / zoom;
                    if (tempDist < distance)
                    {
                        distance = tempDist;
                        returnPlanet = p;
                    }
                }
            }
            return returnPlanet;
        }

        public static Texture2D getBlankTexture()
        {
            return blank;
        }
        #endregion

        #region Classes
        /// <summary>
        /// Handles the main menu tab in game
        /// </summary>
        public class MainMenu
        {
            Vector2 position;
            Color main, secondary;
            Slider masterVol, SFXVol, musicVol;
            bool isShowing;

            public MainMenu(Vector2 position, Color mainColor, Color secondaryColor, bool isShowing, Spacegame game)
            {
                this.position = position;
                this.main = mainColor;
                this.secondary = secondaryColor;
                this.isShowing = isShowing;
                this.masterVol = new Slider(1F, game.spriteBatch, new Rectangle(0, 0, 100, 10), blank, blank);
                this.SFXVol = new Slider(1F, game.spriteBatch, new Rectangle(0, 0, 100, 10), blank, blank);
                this.musicVol = new Slider(1F, game.spriteBatch, new Rectangle(0, 0, 100, 10), blank, blank);
            }

            public void changeState(bool isShowing, Spacegame game)
            {
                this.isShowing = game.isPaused = isShowing;
            }

            public void render(SpriteBatch spriteBatch, GraphicsDevice graphics, Spacegame game)
            {
                position = new Vector2(graphics.Viewport.Width/2 - 64, graphics.Viewport.Height/2 - 128);
                update(graphics, game);

                if (isShowing)
                {
                    DrawFunctions.drawRectangle(spriteBatch, 1F, main, secondary, new Rectangle((int)position.X, (int)position.Y, 128, 256));
                    DrawFunctions.drawRectangle(spriteBatch, 1F, main, secondary, new Rectangle(graphics.Viewport.Width / 2 - 48, graphics.Viewport.Height / 2 - 120, 96, 16));
                    float stringLength = game.drawFont.MeasureString("Close").X;
                    float stringHeight = game.drawFont.MeasureString("Close").Y;
                    spriteBatch.DrawString(game.drawFont, "Close", new Vector2((position.X + 64) - (stringLength/2), (position.Y + 17) - (stringHeight/2)), secondary);
                }
                if (isShowing)
                {
                    masterVol.update(true);
                    SFXVol.update(true);
                    musicVol.update(true);
                }

                //spriteBatch.DrawString(game.guiFont, slider.getValue().ToString(), new Vector2(105, 0), Color.White);
            }

            public void update(GraphicsDevice graphics, Spacegame game)
            {
                masterVol.setRect(new Rectangle((int)position.X + 16, (int)position.Y + 32, 96, 16));
                SFXVol.setRect(new Rectangle((int)position.X + 16, (int)position.Y + 50, 96, 16));
                musicVol.setRect(new Rectangle((int)position.X + 16, (int)position.Y + 68, 96, 16));
                MouseState m = Mouse.GetState();

                if (m.LeftButton == ButtonState.Pressed)
                {
                    if (new Rectangle(graphics.Viewport.Width / 2 - 48, graphics.Viewport.Height / 2 - 120, 96, 16).Contains(new Point((int)m.X, (int)m.Y)))
                    {
                        Config c = new Config("Temp", masterVol.getValue(), SFXVol.getValue(), musicVol.getValue());
                        c.save();
                        c.applyConfig(game);
                        this.changeState(false, game);
                    }
                }

            }

            public void setSliders(float masterVolume, float SFXVolume, float musicVolume)
            {
                masterVol.setValue(masterVolume);
                SFXVol.setValue(SFXVolume);
                musicVol.setValue(musicVolume);
            }
        }

        /// <summary>
        /// Handles sounds
        /// </summary>
        public class SoundManager
        {
            public const byte SND_TITLE = 0, SND_EXPLOSION = 1, SND_LASER = 2;
            private byte[] types = new byte[] { 2, 1, 1};
            List<SoundEffectInstance> soundFX = new List<SoundEffectInstance>();
            SoundEffect[] baseSounds = new SoundEffect[3];
            private byte VI_master = 0, VI_SFX = 1, VI_music = 2;
            float[] volumes = new float[]{ 1F, 1F, 1F};

            public SoundManager(ContentManager Content, float masterVolume)
            {
                for (int i = 0; i < baseSounds.Length; i++)
                {
                    baseSounds[i] = Content.Load<SoundEffect>(@"Sounds\snd_" + i.ToString());
                }

                this.volumes[VI_master] = masterVolume;
            }

            public SoundManager(ContentManager Content, float masterVolume, float SFXVolume, float musicVolume)
            {
                for (int i = 0; i < baseSounds.Length; i++)
                {
                    baseSounds[i] = Content.Load<SoundEffect>(@"Sounds\snd_" + i.ToString());
                }

                this.volumes[VI_master] = masterVolume;
                this.volumes[VI_SFX] = SFXVolume;
                this.volumes[VI_music] = musicVolume;
            }

            public SoundManager(ContentManager Content)
            {
                for (int i = 0; i < baseSounds.Length; i++)
                {
                    baseSounds[i] = Content.Load<SoundEffect>(@"Sounds\snd_" + i.ToString());
                }
            }

            public int playSound(int id)
            {
                SoundEffectInstance temp = baseSounds[id].CreateInstance();
                temp.Volume = volumes[VI_master] * volumes[types[id]];
                soundFX.Add(temp);
                soundFX.Last().Play();
                return soundFX.Count - 1;
            }

            public int playSound(int id, bool isLooping)
            {
                SoundEffectInstance temp = baseSounds[id].CreateInstance();
                temp.Volume = volumes[VI_master] * volumes[types[id]];
                temp.IsLooped = isLooping;
                soundFX.Add(temp);
                soundFX.Last().Play();
                return soundFX.Count - 1;
            }

            public void stopSound(int id)
            {
                soundFX.RemoveAt(id);
            }

            public bool isPlaying(int id)
            {
                if (soundFX.ElementAt(id).State == SoundState.Playing)
                    return true;
                return false;
            }

            public float getVolume(){ return volumes[VI_master]; }

            public void setVolume(float volume)
            {
                float v = MathHelper.Clamp(volume, 0, 1F);
                this.volumes[VI_master] = v;
            }

            public float getSFXVolume() { return volumes[VI_SFX]; }

            public void setSFXVolume(float volume)
            {
                float v = MathHelper.Clamp(volume, 0, 1F);
                this.volumes[VI_SFX] = v;
            }

            public float getMusicVolume() { return volumes[VI_music]; }

            public void setMusicVolume(float volume)
            {
                float v = MathHelper.Clamp(volume, 0, 1F);
                this.volumes[VI_music] = v;
            }
        }

        /// <summary>
        /// Acess to some basic drawing function using spritebatches
        /// </summary>
        public abstract class DrawFunctions
        {
            static Texture2D blank;

            public static void drawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
            {
                blank = Spacegame.getBlankTexture();

                float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                float length = Vector2.Distance(point1, point2);

                batch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
            }

            public static void drawLine(SpriteBatch batch, float width, Color color, Vector2 point1, int length, float angle)
            {
                blank = Spacegame.getBlankTexture();

                batch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
            }

            public static void drawArrow(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
            {
                blank = Spacegame.getBlankTexture();

                float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                float length = Vector2.Distance(point1, point2);
                drawLine(batch, width, color, point1, point2);
                batch.Draw(blank, point2, null, color, angle - (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, width), SpriteEffects.None, 0);
                batch.Draw(blank, point2, null, color, angle + (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, width), SpriteEffects.None, 0);
            }

            public static void drawRectangle(SpriteBatch batch, float width, Color color, Rectangle rect)
            {
                blank = Spacegame.getBlankTexture();

                Vector2 point1 = new Vector2(rect.X, rect.Y);
                Vector2 point2 = new Vector2(rect.X + rect.Width, rect.Y);
                Vector2 point3 = new Vector2(rect.X, rect.Y + rect.Height);
                Vector2 point4 = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

                batch.Draw(blank, point1, null, color, 0, Vector2.Zero, new Vector2(rect.Width, rect.Height), SpriteEffects.None, 0);
            }

            public static void drawRectangle(SpriteBatch batch, float width, Color innerColor, Color outerColor, Rectangle rect)
            {
                blank = Spacegame.getBlankTexture();

                Vector2 point1 = new Vector2(rect.X, rect.Y);
                Vector2 point2 = new Vector2(rect.X + rect.Width, rect.Y);
                Vector2 point3 = new Vector2(rect.X, rect.Y + rect.Height);
                Vector2 point4 = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

                batch.Draw(blank, point1, null, innerColor, 0, Vector2.Zero, new Vector2(rect.Width, rect.Height), SpriteEffects.None, 0);

                drawLine(batch, width, outerColor, point1, point2);
                drawLine(batch, width, outerColor, point1, point3);
                drawLine(batch, width, outerColor, point2, point4);
                drawLine(batch, width, outerColor, new Vector2(point3.X - width, point3.Y), point4);
            }

            public static void drawCircle(SpriteBatch batch, Vector2 center, int stepping, int radius, Color color)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    drawLine(batch, 2F, color, extraMath.calculateVector(center, angle, radius),
                        extraMath.calculateVector(center, angle + increment, radius));
                    angle += increment;
                }
            }

            public static void drawCircle(SpriteBatch batch, Vector2 center, int stepping, int radius, Color color, double scaling, Vector2 offSet)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;
                Vector2 pos = Vector2.Subtract(center, offSet);

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    drawLine(batch, 2F, color, extraMath.calculateVector(pos, angle, radius * scaling),
                        extraMath.calculateVector(pos, angle + increment, radius * scaling));
                    angle += increment;
                }
            }

            public static void drawOrbit(SpriteBatch batch, Vector2 center, int stepping, double heighVal, double lowVal, Color color, double scaling, Vector2 offSet)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;
                Vector2 pos = Vector2.Subtract(center, offSet);

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    drawLine(batch, 2F, color, extraMath.calculateVector(pos, angle, extraMath.getAltitudeFromCenteredOrbit(heighVal, lowVal, angle) * scaling),
                        extraMath.calculateVector(pos, angle + increment, extraMath.getAltitudeFromCenteredOrbit(heighVal, lowVal, angle + increment) * scaling));
                    angle += increment;
                }
            }

            public static void drawOffsetOrbit(SpriteBatch batch, Vector2 center, int stepping, double heighVal, double lowVal, double offset, double rotate, Color color, double scaling, Vector2 offSet)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;
                Vector2 pos = Vector2.Subtract(center, offSet);

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    drawLine(batch, 2F, color, extraMath.calculateVector(pos, angle, extraMath.getAltitudeFromOffsetOrbit(heighVal, lowVal, offset, angle,rotate) * scaling),
                        extraMath.calculateVector(pos, angle + increment, extraMath.getAltitudeFromOffsetOrbit(heighVal, lowVal, offset, angle + increment, rotate) * scaling));
                    angle += increment;
                }
            }

            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color)
            {
                blank = Spacegame.getBlankTexture();

                int xx = 0, yy = 0;
                for (int x = rect.X; x < rect.X + rect.Width; x += 0)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y += 0)
                    {
                        Rectangle tempRect = new Rectangle(x, y, tex.Width, tex.Height);
                        if (x + tex.Width < rect.X + rect.Width & y + tex.Height < rect.Y + rect.Height)
                            batch.Draw(tex, tempRect, color);
                        else
                        {
                            Rectangle tempSourceRect = new Rectangle(0, 0, rect.Width - (tex.Width * xx), rect.Height - (tex.Height * yy));
                            Rectangle drawRect = new Rectangle(x, y, tempSourceRect.Width, tempSourceRect.Height);
                            batch.Draw(tex, drawRect, tempSourceRect, color);
                        }
                        y += tex.Height;
                    }
                    x += tex.Width;
                    xx += 1;
                }
            }

            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Vector2 offSet, Color color)
            {
                blank = Spacegame.getBlankTexture();

                rect.X -= (int)offSet.X;
                rect.Y -= (int)offSet.Y;
                int xx = 0, yy = 0;
                for (int x = rect.X; x < rect.X + rect.Width; x += tex.Width)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y += tex.Height)
                    {
                        Rectangle tempRect = new Rectangle(x, y, tex.Width, tex.Height);
                        if (x + tex.Width < rect.X + rect.Width & y + tex.Height < rect.Y + rect.Height)
                            batch.Draw(tex, tempRect, color);
                        else
                        {
                            Rectangle tempSourceRect = new Rectangle(0, 0, rect.Width - (tex.Width * xx), rect.Height - (tex.Height * yy));
                            Rectangle drawRect = new Rectangle(x, y, tempSourceRect.Width, tempSourceRect.Height);
                            batch.Draw(tex, drawRect, tempSourceRect, color);
                        }
                    }
                    xx++;
                }

                //rect.X -= (int)offSet.X;
                //rect.Y -= (int)offSet.Y;
                //int xx = (int)Math.Floor((decimal)(rect.Width / tex.Width));
                //int sx = (rect.Width / tex.Width) - xx;
                //int yy = (int)Math.Floor((decimal)(rect.Height / tex.Height));
                //int sy = (rect.Height / tex.Height) - yy;

                //for (int x = 0; x < rect.Width; x += tex.Width)
                //{
                //    for (int y = 0; y < rect.Height; y += tex.Height)
                //    {
                //        int rx = rect.X + x;
                //        int ry = rect.Y + y;
                //        if (x + tex.Width < rect.Width & y + tex.Height < rect.Height)
                //        {
                //            batch.Draw(tex, new Rectangle(rx, ry, tex.Width, tex.Height), color);
                //        }
                //        else
                //            if (x + tex.Width >= rect.Width & y + tex.Height <= rect.Height)
                //            {
                //                batch.Draw(tex, new Rectangle(rx, ry, (x + tex.Width) - rect.Width, tex.Height),
                //                    new Rectangle(rx, ry, (x + tex.Width) - rect.Width, tex.Height), color);
                //            }
                //            else
                //                if (x + tex.Width <= rect.Width & y + tex.Height >= rect.Height)
                //                {
                //                    batch.Draw(tex, new Rectangle(rx, ry, tex.Width, (y + tex.Height) - rect.Height),
                //                        new Rectangle(rx, ry, tex.Width, (y + tex.Height) - rect.Height), color);
                //                }
                //        //        else
                //        //            if (x + tex.Width >= rect.Width & y + tex.Height >= rect.Height)
                //        //            {
                //        //                batch.Draw(tex, new Rectangle(rx, ry, (x + tex.Width) - rect.Width, (y + tex.Height) - rect.Height),
                //        //                    new Rectangle(rx, ry, (x + tex.Width) - rect.Width, (y + tex.Height) - rect.Height), color);
                //        //            }
                //    }
                //}
            }

            public static void renderPlanet(int[] heightmap, SpriteBatch spriteBatch, Vector2 vector)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / heightmap.Length;

                //render
                double angle = 0;
                for (int i = 0; i < heightmap.Length - 1; i++)
                {
                    //draw ground
                    drawLine(spriteBatch, 3F, Color.Gray, extraMath.calculateVector(vector, angle, heightmap[i]), vector);
                    //draw outline
                    drawLine(spriteBatch, 2F, Color.Brown, extraMath.calculateVector(vector, angle, heightmap[i] - 1),
                        extraMath.calculateVector(vector, angle + increment, heightmap[i + 1] - 1));
                    angle += increment;
                }
                //draw ground
                drawLine(spriteBatch, 3F, Color.Gray, extraMath.calculateVector(vector, angle, heightmap[heightmap.Length - 1]), vector);
                //draw outline
                drawLine(spriteBatch, 2F, Color.Brown, extraMath.calculateVector(vector, angle, heightmap[heightmap.Length - 1] - 1),
                    extraMath.calculateVector(vector, angle + increment, heightmap[0] - 1));
            }

            public static void renderPlanet(int[] heightmap, SpriteBatch spriteBatch, Vector2 vector, float scaling)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / heightmap.Length;

                //render
                double angle = 0;
                for (int i = 0; i < heightmap.Length - 1; i++)
                {
                    //draw ground
                    drawLine(spriteBatch, 2F, Color.Gray, extraMath.calculateVector(vector, angle, heightmap[i] * scaling), vector);
                    //draw outline
                    drawLine(spriteBatch, 2F * (scaling / 6F), Color.Brown, extraMath.calculateVector(vector, angle, heightmap[i] * scaling),
                        extraMath.calculateVector(vector, angle + increment, heightmap[i + 1] * scaling));
                    angle += increment;
                }
                //draw ground
                drawLine(spriteBatch, 2F, Color.Gray, extraMath.calculateVector(vector, angle, heightmap[heightmap.Length - 1] * scaling), vector);
                //draw outline
                drawLine(spriteBatch, 2F * (scaling / 6F), Color.Brown, extraMath.calculateVector(vector, angle, heightmap[heightmap.Length - 1] * scaling),
                    extraMath.calculateVector(vector, angle + increment, heightmap[0] * scaling));
            }

            public static void renderPlanet(int[] heightmap, SpriteBatch spriteBatch, Color color, Vector2 vector, float scaling, double rotation)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / heightmap.Length;

                //render
                double angle = 0;
                for (int i = 0; i < heightmap.Length - 1; i++)
                {
                    //draw ground
                    drawLine(spriteBatch, 3F * (scaling / 6F), color, extraMath.calculateVector(vector, (((increment * i) + increment) + rotation), heightmap[i] * scaling),
                        extraMath.calculateVector(vector, (-(((increment * i) + increment)) + rotation), heightmap[heightmap.Length - i - 1] * scaling));
                    //draw outline
                    drawLine(spriteBatch, 3F * (scaling / 6F), color, extraMath.calculateVector(vector, angle + rotation, heightmap[i] * scaling),
                        extraMath.calculateVector(vector, angle + rotation + increment, heightmap[i + 1] * scaling));
                    angle += increment;
                }
            }
        }

        ///<summary>
        ///A region to hold advanced drawing functions in 3D
        ///</summary>
        #region AdvancedDrawFunctions

            // draws a line using 2 vecors
            public void DrawLine(Vector2 vect, Vector2 vect2, Color col1, Color col2)
            {
                float x1 = vect.X;
                float y1 = vect.Y;
                float x2 = vect2.X;
                float y2 = vect2.Y;
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[2];
                vertices[0].Position = new Vector3(x1, y1, 0);
                vertices[0].Color = col1;
                vertices[1].Position = new Vector3(x2, y2, 0);
                vertices[1].Color = col2;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
            }

            //draws a line using 2 sets of co-ordinates
            public void DrawLine(float x1, float y1, float x2, float y2, Color col1, Color col2)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[2];
                vertices[0].Position = new Vector3(x1, y1, 0);
                vertices[0].Color = col1;
                vertices[1].Position = new Vector3(x2, y2, 0);
                vertices[1].Color = col2;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
            }

            //draw a rectangle
            public void DrawRect(float x1, float y1, float x2, float y2, Color col1, Color col2, bool filled)
            {
                if (filled == true)
                {
                    drawRectFill(x1, y1, x2, y2, col2);
                }
                DrawLine(x1, y1, x2, y1, col1, col1);
                DrawLine(x1, y2, x2, y2, col1, col1);
                DrawLine(x1, y1, x1, y2, col1, col1);
                DrawLine(x2, y1, x2, y2, col1, col1);
            }

            public void drawRectFill(float x1, float y1, float x2, float y2, Color col)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[6];
                vertices[0].Position = new Vector3(x1, y1, 0.5F);
                vertices[0].Color = col;
                vertices[1].Position = new Vector3(x1, y2, 0);
                vertices[1].Color = col;
                vertices[2].Position = new Vector3(x2, y2, 0);
                vertices[2].Color = col;
                vertices[3].Position = new Vector3(x2, y1, 0);
                vertices[3].Color = col;
                vertices[4].Position = new Vector3(x1, y1, 0);
                vertices[4].Color = col;
                vertices[5].Position = new Vector3(x1, y2, 0);
                vertices[5].Color = col;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                short[] triangleStripIndices = new short[6] { 0, 1, 2, 3, 4, 5};
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, vertices, 0, 6, triangleStripIndices, 0, 4);
            }

            public void drawTexture(float x1, float y1, float x2, float y2, Texture2D tex, Color col)
            {
                VertexPositionNormalTexture[] vertices;
                vertices = new VertexPositionNormalTexture[6];
                vertices[0].Position = new Vector3(x1, y1, 0.5F);
                vertices[0].Normal = new Vector3(x1, y1, 0.5F);
                vertices[0].TextureCoordinate = new Vector2(0, 0);

                vertices[1].Position = new Vector3(x1, y2, 0);
                vertices[1].Normal = new Vector3(x1, y2, 0);
                vertices[1].TextureCoordinate = new Vector2(0, 1);

                vertices[2].Position = new Vector3(x2, y2, 0);
                vertices[2].Normal = new Vector3(x2, y2, 0);
                vertices[2].TextureCoordinate = new Vector2(1, 1);

                vertices[3].Position = new Vector3(x2, y1, 0);
                vertices[3].Normal = new Vector3(x2, y1, 0);
                vertices[3].TextureCoordinate = new Vector2(1, 0);

                vertices[4].Position = new Vector3(x1, y1, 0);
                vertices[4].Normal = new Vector3(x1, y1, 0);
                vertices[4].TextureCoordinate = new Vector2(0, 0);

                vertices[5].Position = new Vector3(x1, y2, 0);
                vertices[5].Normal = new Vector3(x1, y2, 0);
                vertices[5].TextureCoordinate = new Vector2(0, 1);

                quadEffect.CurrentTechnique.Passes[0].Apply();
                short[] triangleStripIndices = new short[6] { 0, 1, 2, 3, 4, 5 };
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1, VertexPositionColor.VertexDeclaration);
            }

            //draw a rectangle using 2 vectors
            public void DrawRect(Vector2 vect, Vector2 vect2, Color col1, Color col2, bool filled)
            {
                float x1 = vect.X;
                float y1 = vect.Y;
                float x2 = vect2.X;
                float y2 = vect2.Y;

                if (filled == true)
                {
                    drawRectFill(vect.X, vect.Y, vect2.X, vect2.Y, col2);
                }
                DrawLine(x1, y1, x2, y1, col1, col1);
                DrawLine(x1, y2, x2, y2, col1, col1);
                DrawLine(x1, y1, x1, y2, col1, col1);
                DrawLine(x2, y1, x2, y2, col1, col1);
            }

            //draw a rectangle from a rectangle
            public void DrawRect(Rectangle rect, Color col1, Color col2, bool filled)
            {
                float x1 = rect.X;
                float y1 = rect.Y;
                float x2 = rect.X + rect.Width;
                float y2 = rect.Y + rect.Height;

                if (filled == true)
                {
                    drawRectFill(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, col2);
                }
                DrawLine(x1, y1, x2, y1, col1, col1);
                DrawLine(x1, y2, x2, y2, col1, col1);
                DrawLine(x1, y1, x1, y2, col1, col1);
                DrawLine(x2, y1, x2, y2, col1, col1);
            }

            //draws a triangle
            public void drawTriangle(Vector2 pos1, Vector2 pos2, Vector2 pos3, Color col)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[3];
                vertices[0].Position = new Vector3(pos1.X, pos1.Y, 0);
                vertices[0].Color = col;
                vertices[1].Position = new Vector3(pos2.X, pos2.Y, 0);
                vertices[1].Color = col;
                vertices[2].Position = new Vector3(pos3.X, pos3.Y, 0);
                vertices[2].Color = col;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1, VertexPositionColor.VertexDeclaration);
            }
            
            //WTF do you think it does?
            public void drawCircle(Vector2 center, double radius, int stepping, Color innerColor, Color outerColor)
            {
                List<VertexPositionColor> vertices = new List<VertexPositionColor>();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0), outerColor));
                    vertices.Add(new VertexPositionColor(new Vector3(center, 0), innerColor));
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), outerColor));
                }
                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);
            }

            public void drawUnfilledCircle(Vector2 center, double radius, int stepping, Color col)
            {
                List<VertexPositionColor> vertices = new List<VertexPositionColor>();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i <= stepping; i++, angle += increment)
                {
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0), col));
                }
                basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices.ToArray(), 0, vertices.Count-1, VertexPositionColor.VertexDeclaration);
            }

            //draw a simple flag :P
            public void drawFlag(Vector2 vect, double size, Color col, int win_x, int win_y)
            {
                //bottom of shaft
                float x1 = vect.X - win_x;
                float y1 = vect.Y - win_y;

                //top of shaft
                float x2 = x1;
                float y2 = (float)(y1 - size);

                //tip of flag
                float x3 = (float)(x1 + (size / 4));
                float y3 = (float)(y2 + (size / 8));

                //where bottom of flag meets the mast
                float x4 = x1;
                float y4 = (float)(y2 + (size / 4));
                //draw flag
                DrawLine(x1, y1, x2, y2, Color.Red, Color.Red);
                drawTriangle(new Vector2(x2,y2), new Vector2(x3,y3), new Vector2(x4,y4), col);
            }

            //get a color from a gradient
            public Color getColorFromGradient(Color color1, Color color2, int stretch, float getPos, int alpha)
            {
                float R;
                float G;
                float B;

                int R1 = color1.R;
                int G1 = color1.G;
                int B1 = color1.B;

                int R2 = color2.R;
                int G2 = color2.G;
                int B2 = color2.B;

                float Rchange;
                float Gchange;
                float Bchange;

                Rchange = (R2 - R1) / stretch;
                Bchange = (B2 - B1) / stretch;
                Gchange = (G2 - G1) / stretch;

                R = R2 - (Rchange * getPos);
                G = G2 - (Gchange * getPos);
                B = B2 - (Bchange * getPos);

                return Color.FromNonPremultiplied((int)R, (int)G, (int)B, 255);
            }

            public void drawOrbit(SpriteBatch batch, Vector2 center, int stepping, double heighVal, double lowVal, Color color, double scaling, Vector2 offSet)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    DrawLine(extraMath.calculateVector(center, angle, extraMath.getAltitudeFromCenteredOrbit(heighVal, lowVal, angle) * scaling),
                        extraMath.calculateVector(center, angle + increment, extraMath.getAltitudeFromCenteredOrbit(heighVal, lowVal, angle + increment) * scaling), color, color);
                    angle += increment;
                }
            }

            public void drawOrbit(Vector2 center, int stepping, double heighVal, Color color)
            {
                drawUnfilledCircle(center, heighVal, stepping, color); 
            }

            public void drawOffsetOrbit(SpriteBatch batch, Vector2 center, int stepping, double heighVal, double lowVal, double offset, double rotate, Color color, double scaling, Vector2 offSet)
            {
                blank = Spacegame.getBlankTexture();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;
                Vector2 pos = Vector2.Subtract(center, offSet);

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    DrawLine(extraMath.calculateVector(pos, angle, extraMath.getAltitudeFromOffsetOrbit(heighVal, lowVal, offset, angle, rotate) * scaling),
                        extraMath.calculateVector(pos, angle + increment, extraMath.getAltitudeFromOffsetOrbit(heighVal, lowVal, offset, angle + increment, rotate) * scaling), color, color);
                    angle += increment;
                }
            }


        #endregion

        /// <summary>
        /// Handles advanced math functions
        /// </summary>
        public abstract class extraMath
        {
            /// <summary>
            /// Caclulates a new vector from a given vector and a relative angle/length
            /// </summary>
            /// <param name="vector"></param>
            /// <param name="angle"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public static Vector2 calculateVector(Vector2 vector, double angle, double length)
            {
                Vector2 returnVect = new Vector2(vector.X + (float)((Math.Cos(angle - Math.PI) * length) - Math.PI), vector.Y + (float)(-(Math.Sin(angle - Math.PI) * length)));
                return returnVect;
            }

            public static Vector2 calculateVectorOffset(Vector2 vector, double angle, double length)
            {
                Vector2 returnVect = new Vector2(vector.X + (float)((Math.Cos(angle - Math.PI) * length) - Math.PI), vector.Y + (float)(-(Math.Sin(angle - Math.PI) * length)));
                return returnVect - vector;
            }

            public static double findAngle(Vector2 point1, Vector2 point2)
            {
                double angle = Math.Atan2(point1.Y - point2.Y, point2.X - point1.X);
                return angle;
            }

            public static double[] MidpointDisplacement(int h, int baseVal, int length)
            {
                //arguments:
                //
                //argument0 = room width
                //
                //argument1 = height change variable
                //
                //argument2 = land id
                //
                double[] output = new double[length + 1];
                Random RandNum = new Random();

                for (int xx = 0; xx <= length; xx++)
                {
                    output[xx] = baseVal;
                }
                output[0] = baseVal;

                //generate values
                for (int rep = 2; rep < length; rep *= 2)
                {
                    for (int i = 1; i <= rep; i += 1)
                    {

                        int x1 = (length / rep) * (i - 1);
                        int x2 = (length / rep) * i;
                        double avg = (output[x1] + output[x2]) / 2;
                        double Rand = RandNum.Next(-h, h);
                        output[(x1 + x2) / 2] = avg + (Rand);
                    }
                    h /= 2;
                }

                //returns array
                return output;
            }

            public static double findCircumfenceAngleChange(double radius, double speed)
            {
                double n = Math.Acos((Math.Pow(speed, 2) - 2 * Math.Pow(radius, 2)) / (-2 * Math.Pow(radius, 2))) * speed;
                if (n == Double.NaN)
                {
                    n = -1;
                    throw (new ArithmeticException("Number is NAN!"));
                }
                return n;
            }

            public static double getAltitudeFromCenteredOrbit(double length, double width, double theta)
            {
                return length * width / (Math.Sqrt(Math.Pow((length * Math.Cos(theta)), 2) + Math.Pow((width * Math.Sin(theta)), 2)));
            }

            public static double getAltitudeFromOffsetOrbit(double length, double width, double offset, double theta, double angleOffset)
            {
                theta -= angleOffset;
                return ((length * width) / Math.Sqrt(Math.Pow(width * Math.Cos(theta), 2) + Math.Pow(length * Math.Sin(theta), 2)));
            }

            public static double map(double lowVal, double highVal, double newLowVal, double newHighVal, double value)
            {
                double range = newHighVal - newLowVal;
                double oldRange = highVal - lowVal;
                double multiplier = range / oldRange;
                return newLowVal + ((value-lowVal) * multiplier);

            }

            public static double getDrawAngle(Vector2 planetCenter, Vector2 shipCenter)
            {
                return -findAngle(planetCenter, shipCenter) + Math.PI / 2;
            }

            public static Vector2 getOffsetOrbitCenter(double length, double width, double offset, double rotate, Vector2 pin)
            {
                return pin + calculateVector(pin, rotate, offset);
            }

            public static double getDistance(Vector2 vect1, Vector2 vect2)
            {
                return Math.Sqrt(Math.Pow(vect2.X - vect1.X, 2) + Math.Pow(vect2.Y - vect1.Y, 2));
            }

            public static bool isInRectangle(Rectangle rect, Vector2 point, double angle, Vector2 orgin)
            {
                return (Math.Pow(point.X - orgin.X, 2) + Math.Pow(point.Y - orgin.Y, 2) <= Math.Pow(rect.Width, 2));
            }
        }

        /// <summary>
        /// Handles an entrire solar system, as well as saving/loading it
        /// </summary>
        public class SolarSystem
        {
            List<Planet> planets = new List<Planet>();
            string name;

            public SolarSystem(string name)
            {
                this.name = name;
            }

            public void addPlanet(Planet planet)
            {
                planets.Add(planet);
            }

            public Planet getPlanet(string name)
            {
                foreach (Planet p in planets)
                {
                    if (p.Name == name)
                        return p;
                }

                return null;
            }

            public int getPlanetId(string name)
            {
                int i = 0;
                foreach (Planet p in planets)
                {
                    if (p.Name == name)
                        return i;
                    i++;
                }
                return 0;
            }

            public Planet getPlanet(int id)
            {
                if (id >= 0 & id < planets.Count)
                {
                    return planets.ElementAt(id);
                }

                return null;
            }

            public Planet[] getPlanets()
            {
                return planets.ToArray();
            }

            public bool deletePlanet(string name)
            {
                foreach (Planet p in planets)
                {
                    if (p.Name == name)
                    {
                        planets.Remove(p);
                        return true;
                    }
                }
                return false;
            }

            public bool deletePlanet(int id)
            {
                if (id >= 0 & id < planets.Count)
                {
                    planets.RemoveAt(id);
                    return true;
                }
                return false;
            }

            public void render(Spacegame game, float zoom, bool debug, BasicEffect effect, GraphicsDevice graphics, bool paused, float gameSpeed)
            {
                foreach (Planet p in planets)
                {
                    if (p != null)
                    {
                        if (p.waterMap != null)
                            p.Render(zoom, debug, game, effect, graphics, true, paused, gameSpeed);
                        else
                            p.Render(zoom, debug, game, effect, graphics, false, paused, gameSpeed);
                    }
                }
            }

            public void render(Spacegame game, float zoom, bool debug, BasicEffect effect, GraphicsDevice graphics, bool paused, float gameSpeed, double winRot)
            {
                foreach (Planet p in planets)
                {
                    if (p != null)
                    {
                        if (p.waterMap != null)
                            
                            p.Render(zoom, debug, game, effect, graphics, true, paused, gameSpeed, winRot);
                        else
                            p.Render(zoom, debug, game, effect, graphics, false, paused, gameSpeed, winRot);
                    }
                }
            }

            public void update(Spacegame game, float zoom, bool debug, BasicEffect effect, GraphicsDevice graphics, bool paused, float gameSpeed, double winRot)
            {
                foreach (Planet p in planets)
                {
                    if (p != null)
                    {
                        p.Update(zoom, debug, game, effect, graphics, true, paused, gameSpeed, winRot);
                    }
                }
            }

            public int getPlanetCount()
            {
                return planets.Count;
            }

            public void save(string filePath)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(name);
                    writer.Write(planets.Count);
                    foreach (Planet p in planets)
                    {
                        p.writeToStream(writer);
                    }
                    writer.Close();
                }
            }

            public static SolarSystem load(string filePath, Spacegame game)
            {
                if (File.Exists(filePath))
                {
                    FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryReader read = new BinaryReader(stream);
                    SolarSystem sol = new SolarSystem(read.ReadString());
                    
                    int rep = read.ReadInt32();
                    for (int i = 0; i < rep; i++)
                    {
                        sol.addPlanet(Planet.readFromStream(sol, read));
                    }
                    return sol;
                }
                SolarSystem ret = new SolarSystem("Null System");
                ret.addPlanet(new Planet(Vector2.Zero, 2, Color.White, Color.White));
                game.targetPlanet = ret.getPlanet(0);
                game.targetPlanetID = 0;
                return ret;
            }

            public bool isInPlanet(Vector2 pos)
            {
                foreach (Planet p in planets)
                {
                    if (p.isInPlanet(pos))
                        return true;
                }
                return false;
            }

            public bool isInPlanet(Vector2 pos, double angle)
            {
                foreach (Planet p in planets)
                {
                    if (p.isInPlanet(pos, angle))
                        return true;
                }
                return false;
            }

            public bool isInPlanet(Vector2 pos, double angle, double zoom)
            {
                foreach (Planet p in planets)
                {
                    if (p.isInPlanet(pos, angle, zoom))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Handles a player
        /// </summary>
        public class Fleet
        {
            List<IShip> ships = new List<IShip>();
            public List<Laser> lasers = new List<Laser>();

            int money = 1000, material = 100, food = 100, fuel = 0;
            int moneyIncome = 50, materialIncome = 50, foodIncome = 50, fuelIncome = 10;

            string name;
            SolarSystem system;

            double timer = 0;

            public Fleet(SolarSystem system, string name)
            {
                this.system = system;
                this.name = name;
            }

            public void update(Spacegame game, GameTime gameTime, bool paused, float gameSpeed, BasicEffect effect, GraphicsDevice device, double zoom, double winRot)
            {
                foreach (IShip s in ships)
                {
                    s.tick(Vector2.Zero, (float)zoom, (float)winRot, game);
                }

                for (int i = 0; i < lasers.Count; i++)
                {
                    Laser laser = lasers.ElementAt(i);
                    if (laser != null)
                    {
                        laser.update(effect, device, zoom, winRot, gameSpeed);
                        if (system.isInPlanet(laser.position, winRot, zoom))
                            disposeLaser(laser);
                    }
                }
                if (!paused)
                    timer += gameTime.ElapsedGameTime.Milliseconds * gameSpeed;

                if (timer >= 1000)
                {
                    this.money += moneyIncome;
                    if (money > 999999999)
                        money = 999999999;
                    this.material += materialIncome;
                    if (material > 999999999)
                        material = 999999999;
                    this.food += foodIncome;
                    if (food > 999999999)
                        food = 999999999;
                    this.fuel += fuelIncome;
                    if (fuel > 999999999)
                        fuel = 999999999;
                    timer = 0;
                }
            }

            public int getMoney()
            {
                return money;
            }

            public int getMaterial()
            {
                return material;
            }

            public int getFood()
            {
                return food;
            }

            public int getFuel()
            {
                return fuel;
            }

            public bool disposeLaser(Laser laser)
            {
                return lasers.Remove(laser);
            }

            public void addShip(IShip ship)
            {
                ships.Add(ship);
            }
        }

        /// <summary>
        /// Handles planets
        /// </summary>
        public class Planet
        {
            #region Variables
            public Color surfaceColor, coreColor, waterColor = Color.Aqua;
            Planet orbitPlanet;
            public String Name = "Unnamed Planet";
            SolarSystem system;
            List<Vegetation> vegtables = new List<Vegetation>();
            public List<IBuilding> buildings = new List<IBuilding>();

            public Vector2 position;
            double[] heightmap;
            public int[] waterMap;
            int waterHeight;
            public double rotation;
            double rotationSpeed = 0.0001D;
            int orbitHeight;
            public double orbitAngle;
            float orbitSpeed;
            double surfaceTemp = 0D;
            byte atmosphereDensity = 0, atmosphereBreathability = 0, toxicity = 0;
            int coreTemp = 0;
            
            //declares a list of verticies to use
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<VertexPositionColor> lines = new List<VertexPositionColor>();

            //declares a list of verticies to use
            List<VertexPositionColor> waterVertices = new List<VertexPositionColor>();
            List<VertexPositionColor> waterLines = new List<VertexPositionColor>();
            #endregion

            #region Initializations
            public Planet(Vector2 position, int size, Color color, Color surfaceColor)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 8 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.position = position;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;

                buildModel();
            }

            public Planet(string name, Vector2 position, int size, Color color, Color surfaceColor)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 8 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.position = position;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.Name = name;

                buildModel();
            }

            public Planet(string name, Vector2 position, int size, Color color, Color surfaceColor, double startAngle)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 8 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.position = position;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.Name = name;
                this.orbitAngle = startAngle;

                buildModel();
            }

            public Planet(SolarSystem system, string name, Vector2 position, int size, Color color, Color surfaceColor, double startAngle)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 8 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.system = system;
                this.position = position;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.Name = name;
                this.orbitAngle = startAngle;

                buildModel();
            }

            public Planet(Vector2 position, string name, int size, Color color, Color surfaceColor, byte atmosphereThickness, byte breathability, int waterHeight, Color waterColor)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(size * 32, radius + 32, scale);
                
                this.waterHeight = waterHeight;
                this.waterMap = new int[heightmap.Length];
                for (int i = 0; i < heightmap.Length; i++)
                    waterMap[i] = waterHeight;

                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.Name = name;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;
                this.waterColor = waterColor;

                buildModel();

                setPlanetTemp();
                populateVegetation();
            }

            public Planet(int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;

                this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;

                setPlanetTemp(); buildModel(); populateVegetation();
            }

            public Planet(string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;

                this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;

                setPlanetTemp(); buildModel(); populateVegetation();
            }

            public Planet(string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;

                setPlanetTemp(); buildModel(); populateVegetation();
            }

            public Planet(string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;

                setPlanetTemp(); buildModel(); populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;

                setPlanetTemp(); buildModel(); populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(size * 32, radius, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;

                setPlanetTemp(); 
                buildModel();
                populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability, double[] heightmap)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                this.heightmap = heightmap;

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;

                setPlanetTemp();
                buildModel();
                populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability, int waterHeight)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(size * 32, radius + 32, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                this.waterHeight = waterHeight;
                this.waterMap = new int[heightmap.Length];
                for (int i = 0; i < heightmap.Length; i++)
                    waterMap[i] = waterHeight;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;

                buildModel();

                setPlanetTemp();
                populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability, int waterHeight, Color waterColor)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(size * 32, radius + 32, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                this.waterHeight = waterHeight;
                this.waterMap = new int[heightmap.Length];
                for (int i = 0; i < heightmap.Length; i++)
                    waterMap[i] = waterHeight;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;
                this.waterColor = waterColor;

                buildModel();

                setPlanetTemp();
                populateVegetation();
            }

            public Planet(SolarSystem system, string name, int size, Color color, Color surfaceColor, Planet orbitPlanet, int orbitHeight, float orbitSpeed, double startAngle, byte atmosphereThickness, byte breathability, int waterHeight, Color waterColor, int extraCoreTemp)
            {
                int scale = 0;
                int radius = 0;
                scale = (int)Math.Pow(2, 6 + size);
                radius = 100 * size;

                //initialize array
                heightmap = extraMath.MidpointDisplacement(size * 32, radius + 32, scale);

                this.orbitHeight = orbitHeight;
                this.orbitPlanet = orbitPlanet;
                this.system = system;

                this.waterHeight = waterHeight;
                this.waterMap = new int[heightmap.Length];
                for (int i = 0; i < heightmap.Length; i++)
                    waterMap[i] = waterHeight;

                if (orbitPlanet != null)
                    this.position = extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight);
                else
                    this.position = Vector2.Zero;
                this.coreColor = color;
                this.surfaceColor = surfaceColor;
                this.orbitSpeed = orbitSpeed;
                this.Name = name;
                this.orbitAngle = startAngle;
                this.atmosphereDensity = atmosphereThickness;
                this.atmosphereBreathability = breathability;
                this.waterColor = waterColor;
                this.coreTemp = extraCoreTemp;

                buildModel();

                setPlanetTemp();
                populateVegetation();
            }
            #endregion

            public void Render(float zoom, bool debug, Spacegame game, BasicEffect effect, GraphicsDevice device, bool drawWater, bool paused, float gameSpeed)
            {
                foreach (IBuilding b in buildings)
                {
                    b.update(game, new GameTime(), 0, zoom, gameSpeed);
                }

                if (orbitPlanet != null & debug)
                {
                    game.drawOrbit(orbitPlanet.position, 100, (int)(orbitHeight) * zoom, Color.Red);
                }
                game.drawCircle(position, (heightmap.Average() + (atmosphereDensity * heightmap.Average()) / 200) * zoom, 50,
                    Color.FromNonPremultiplied(255 - atmosphereBreathability, 0, atmosphereBreathability, 255), Color.FromNonPremultiplied(0, 0, 0, 0));
                //if (drawWater)
                    //game.renderWater(this);   

                renderLists(effect, device, zoom, 0);
            }

            public void Render(float zoom, bool debug, Spacegame game, BasicEffect effect, GraphicsDevice device, bool drawWater, bool paused, float gameSpeed, double winRot)
            {
                foreach (IBuilding b in buildings)
                {
                    b.update(game, new GameTime(), winRot, zoom, gameSpeed);
                }

                if (orbitPlanet != null & debug)
                {
                    game.drawOrbit(orbitPlanet.position, 100, (int)(orbitHeight) * zoom, Color.Red);
                }
                game.drawCircle(position, (heightmap.Average() + (atmosphereDensity * heightmap.Average()) / 200) * zoom, 50,
                    Color.FromNonPremultiplied(255 - atmosphereBreathability, 0, atmosphereBreathability, 255), Color.FromNonPremultiplied(0, 0, 0, 0));
                //if (drawWater)
                //game.renderWater(this);   

                renderLists(effect, device, zoom, winRot);
            }

            public void Update(float zoom, bool debug, Spacegame game, BasicEffect effect, GraphicsDevice device, bool drawWater, bool paused, float gameSpeed, double winRot)
            {
                if (!paused)
                {
                    if (orbitPlanet != null)
                    {
                        orbitAngle += orbitSpeed * gameSpeed;
                        position = extraMath.calculateVector(orbitPlanet.position, orbitAngle + winRot, orbitHeight * zoom);
                    }
                    rotation += rotationSpeed * gameSpeed;
                }

                if (orbitPlanet != null & system != null)
                {
                    if (orbitPlanet != system.getPlanet(0))
                    {
                        setPlanetTemp(zoom);
                    }
                }
            }

            public void renderLists(BasicEffect effect, GraphicsDevice device, double zoom, double winRot)
            {
                //renders the planet
                Matrix temp = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(position,0));
                Matrix rot = Matrix.CreateRotationZ((float)(-rotation - winRot));
                Matrix scale = Matrix.CreateScale((float)zoom);
                outMatrix = scale * rot * outMatrix;
                effect.World = outMatrix;
                effect.CurrentTechnique.Passes[0].Apply();

                //render water
                if (waterVertices.Count > 0)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, waterVertices.ToArray(), 0, waterVertices.Count / 3, VertexPositionColor.VertexDeclaration);
                if (waterLines.Count >= 2)
                    device.DrawUserPrimitives(PrimitiveType.LineList, waterLines.ToArray(), 0, waterLines.Count / 2, VertexPositionColor.VertexDeclaration);

                //render vegetation
                foreach (Vegetation v in vegtables)
                {
                    v.render(zoom, effect, device, winRot);
                }

                effect.CurrentTechnique.Passes[0].Apply();

                //render buildings
                foreach (Building b in buildings)
                {
                    b.render(zoom, effect, device, winRot);
                }

                //re-apply effect
                effect.CurrentTechnique.Passes[0].Apply();

                //render planet
                if (vertices.Count >=3)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);
                if (lines.Count >=2)
                    device.DrawUserPrimitives(PrimitiveType.LineList, lines.ToArray(), 0, lines.Count / 2, VertexPositionColor.VertexDeclaration);

                effect.World = temp;
            }

            public void buildModel()
            {
                //figure out the angle to add in the loop
                double increment = (Math.PI * 2) / heightmap.Length;

                ///render
                double angle = 0;
                for (int i = 0; i < heightmap.Length - 1; i++, angle += increment)
                {
                    //adds the outer left point to the rendering list
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0,0), angle, heightmap[i]), 0), Color.Lerp(coreColor, surfaceColor, (float)heightmap[i]/(float)heightmap.Max())));
                    //adds the center point to the list
                    vertices.Add(new VertexPositionColor(new Vector3(new Vector2(0, 0), 0), coreColor));
                    //adds the outer right point to the list
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle + increment, heightmap[i + 1]), 0), Color.Lerp(coreColor, surfaceColor, (float)heightmap[i]/(float)heightmap.Max())));

                    //lines
                    lines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle, heightmap[i]), 0), Color.Black));
                    lines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle + increment, heightmap[i + 1]), 0), Color.Black));
                }
                //adds the final peice
                vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle, heightmap[0]), 0), Color.Lerp(coreColor, surfaceColor, (float)heightmap[0] / (float)heightmap.Max())));
                vertices.Add(new VertexPositionColor(new Vector3(new Vector2(0, 0), 0), coreColor));
                vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle + increment, heightmap[heightmap.Length - 1]), 0), Color.Lerp(coreColor, surfaceColor, (float)heightmap[heightmap.Length - 1] / (float)heightmap.Max())));

                //adds final line
                lines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle, heightmap[0]), 0), Color.Black));
                lines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle + increment, heightmap[heightmap.Length - 1]), 0), Color.Black));

                //builds the water model
                if (waterMap != null)
                {
                    //figure out the angle to add in the loop
                    double increment2 = (Math.PI * 2) / waterMap.Length;

                    double angle2 = 0;
                    for (int i = 0; i < waterMap.Length - 1; i++, angle2 += increment2)
                    {
                        //adds the outer left point to the rendering list
                        waterVertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2, waterHeight), 0), waterColor));
                        //adds the center point to the list
                        waterVertices.Add(new VertexPositionColor(new Vector3(new Vector2(0, 0), 0), Color.Black));
                        //adds the outer right point to the list
                        waterVertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2 + increment2, waterHeight), 0), waterColor));

                        //lines
                        waterLines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2, waterHeight), 0), Color.Black));
                        waterLines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2 + increment2, waterHeight), 0), Color.Black));
                    }
                    //adds the final peice
                    waterVertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2, waterHeight), 0), waterColor));
                    waterVertices.Add(new VertexPositionColor(new Vector3(new Vector2(0, 0), 0), Color.Black));
                    waterVertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2 + increment2, waterHeight), 0), waterColor));

                    //lines
                    waterLines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2, waterHeight), 0), Color.Black));
                    waterLines.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(new Vector2(0, 0), angle2 + increment2, waterHeight), 0), Color.Black));
                }
            }

            public void populateVegetation()
            {
                Random rand = new Random();
                if (this.atmosphereDensity >= 150 & this.atmosphereBreathability >= 150 & this.surfaceTemp >= -10)
                {
                    int count = rand.Next(100, 300);
                    for (int i = 0; i < count; i++)
                    {
                        double angle = rand.NextDouble() * (Math.PI * 2);
                        if (getheight(angle) > waterHeight)
                            vegtables.Add(new Vegetation(system, this, angle));
                    }
                }
                else
                {
                    if (this.atmosphereDensity >= 100 & this.atmosphereBreathability >= 10 & this.surfaceTemp >= -20)
                    {
                        int count = rand.Next(50, 100);
                        for (int i = 0; i < count; i++)
                        {
                            double angle = rand.NextDouble() * (Math.PI * 2);
                            if (getheight(angle) > waterHeight)
                            vegtables.Add(new Vegetation(system, this, angle, true));
                        }
                    }
                }
            }

            #region Hooks
            public void setHeightMap(double[] map)
            {
                this.heightmap = map;
                lines.Clear();
                vegtables.Clear();
                buildings.Clear();
                waterLines.Clear();
                buildModel();
            }

            public double getDaysInYear()
            {
                double returnVal = (((Math.PI * 2) / orbitSpeed) / 12566.3700174886);
                if (returnVal >= 0)
                    return returnVal;
                else
                    return -returnVal;
            }

            public double[] getHeightMap()
            {
                return heightmap;
            }

            public void writeToStream(BinaryWriter binWriter)
            {
                binWriter.Write(Name);
                if (orbitPlanet != null)
                    binWriter.Write(orbitPlanet.Name);
                else
                    binWriter.Write("");
                binWriter.Write(surfaceTemp);
                binWriter.Write(atmosphereBreathability);
                binWriter.Write(atmosphereDensity);
                binWriter.Write(orbitHeight);
                binWriter.Write(orbitAngle);
                binWriter.Write((double)orbitSpeed);
                binWriter.Write(rotation);
                binWriter.Write(rotationSpeed);
                binWriter.Write(coreColor.R);
                binWriter.Write(coreColor.G);
                binWriter.Write(coreColor.B);
                binWriter.Write(coreColor.A);
                binWriter.Write(surfaceColor.R);
                binWriter.Write(surfaceColor.G);
                binWriter.Write(surfaceColor.B);
                binWriter.Write(surfaceColor.A);
                binWriter.Write(heightmap.Length);
                for (int i = 0; i < heightmap.Length; i++)
                {
                    binWriter.Write(heightmap[i]);
                }
            }

            public static Planet readFromStream(SolarSystem system, BinaryReader binReader)
            {
                string name = binReader.ReadString();
                string hostPlanet = binReader.ReadString();
                double surfaceTemp = binReader.ReadDouble();
                byte breath = binReader.ReadByte();
                byte density = binReader.ReadByte();
                int orbitHeight = binReader.ReadInt32();
                double orbitAngle  = binReader.ReadDouble();
                float orbitSpeed = (float)binReader.ReadDouble();
                double rotation = binReader.ReadDouble();
                double rotationSpeed = binReader.ReadDouble();
                Color coreCol = new Color(binReader.ReadByte(), binReader.ReadByte(), binReader.ReadByte(), binReader.ReadByte());
                Color surfaceCol = new Color(binReader.ReadByte(), binReader.ReadByte(), binReader.ReadByte(), binReader.ReadByte());

                int rep = binReader.ReadInt32();
                double[] height = new double[rep];
                for (int i = 0; i < rep; i++)
                {
                    height[i] = binReader.ReadDouble();
                }

                Planet retPlan = new Planet(system, name, 0, coreCol, surfaceCol, system.getPlanet(hostPlanet), orbitHeight, orbitSpeed, orbitAngle, density, breath, height);
                retPlan.rotation = rotation;
                retPlan.rotationSpeed = rotationSpeed;

                return retPlan;
            }

            private double setPlanetTemp()
            {
                try
                {
                    if (system != null)
                        if (system.getPlanet(0) != null)
                            surfaceTemp = ((((1D / extraMath.getDistance(position, system.getPlanet(0).position)) * 1000D) * 150 - 50) + ((atmosphereDensity/255) * 30) + coreTemp/100);
                }
                catch (DivideByZeroException)
                {
                    surfaceTemp =  1000000000000D;
                }
                return surfaceTemp;
            }

            private double setPlanetTemp(double zoom)
            {
                try
                {
                    if (system != null)
                        surfaceTemp = (((1D / extraMath.getDistance(position, system.getPlanet(0).position)) * 1000D) * zoom) * 150 - 50;
                }
                catch (DivideByZeroException)
                {
                    surfaceTemp = 1000000000000D;
                }
                return surfaceTemp;
            }

            public double getPlanetTemp()
            {
                return surfaceTemp;
            }

            public double getPlanetAtmosDensity()
            {
                return atmosphereDensity;
            }

            public double getPlanetBreathability()
            {
                return atmosphereBreathability;
            }

            public double getPlanetToxicity()
            {
                return toxicity;
            }
            
            public double getheight(double angle)
            {
                //angle = MathHelper.WrapAngle((float)angle) + Math.PI;
                double offset = MathHelper.TwoPi / (heightmap.Length - 1);
                double lowerAngle = ((int)Math.Floor(angle / offset)) * offset;
                double upperAngle = ((int)Math.Floor((angle + offset) / offset)) * offset;
                double height1 = heightmap[(int)Math.Floor(lowerAngle / offset)];
                double height2 = heightmap[(int)Math.Floor(upperAngle / offset)];
                double u = (angle - lowerAngle) / (upperAngle - lowerAngle);
                return height1 - (height1 - height2) * u;
            }

            public bool isInPlanet(double angle, double height)
            {
                return height < getheight(angle);
            }

            public bool isInPlanet(Vector2 pos)
            {
                return Vector2.Distance(position, pos) < getheight(extraMath.findAngle(position, pos) + Math.PI);
            }

            public bool isInPlanet(Vector2 pos, double winRot)
            {
                return Vector2.Distance(position, pos) < getheight(MathHelper.WrapAngle((float)(extraMath.findAngle(position, pos) + winRot)) + Math.PI);
            }

            public bool isInPlanet(Vector2 pos, double winRot, double zoom)
            {
                return (Vector2.Distance(position, pos) * zoom) + 1 + (1/zoom) < getheight(MathHelper.WrapAngle((float)(extraMath.findAngle(position, pos) + winRot)) + Math.PI) * zoom;
            }

            public void addBuilding(Building building)
            {
                 buildings.Add(building);
            }

            public bool removeBuilding(Building building)
            {
                return buildings.Remove(building);
            }

            public void removeBuilding(int id)
            {
                buildings.RemoveAt(id);
            }

            public override string ToString()
            {
                return Name;
            }

            public Vector2 getVect(double zoom)
            {
                return extraMath.calculateVector(orbitPlanet.position, orbitAngle, orbitHeight * zoom);
            }
            #endregion
        }

        /// <summary>
        /// Vegetation
        /// </summary>
        public class Vegetation : Instance
        {
            SolarSystem system;
            Planet planet;
            double height = 0;
            double angle = 0;
            byte type = 0;

            public Vegetation(SolarSystem system, Planet planet, double angle)
            {
                Random rand = new Random((int)(planet.position.X + planet.orbitAngle + angle));
                this.system = system;
                this.planet = planet;
                this.angle = angle;
                double[] hMap = planet.getHeightMap();
                double i = hMap.Length / (Math.PI * 2);
                this.height = hMap[(int)(angle * i)];
                if (new Random((int)angle).NextDouble() <= 0.5)
                    this.type = 3;
                else
                    switch (rand.Next(2))
                    {
                        case 0:
                            this.type = 0;
                            break;

                        case 1:
                            this.type = 1;
                            break;
                    }
            }

            public Vegetation(SolarSystem system, Planet planet, double angle, bool isBush)
            {
                Random rand = new Random((int)(planet.position.X + planet.orbitAngle + angle));
                this.system = system;
                this.planet = planet;
                this.angle = angle;
                double[] hMap = planet.getHeightMap();
                double i = hMap.Length / (Math.PI * 2);
                this.height = hMap[(int)(angle * i)];
                if (isBush)
                    this.type = 2;
                else
                    switch (rand.Next(2))
                    {
                        case 0:
                            this.type = 0;
                            break;

                        case 1:
                            this.type = 1;
                            break;

                        case 2:
                            this.type = 2;
                            break;
                    }
            }
            
            public void render(double zoom, BasicEffect effect, GraphicsDevice device)
            {
                Matrix tempMatrix = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(extraMath.calculateVector(planet.position, (float)(planet.rotation + angle), (float)(height * zoom)),0));
                Matrix rot = Matrix.CreateRotationZ((float)(-planet.rotation + (Math.PI/2) - angle));
                Matrix scale = Matrix.CreateScale((float)zoom);
                outMatrix = scale * rot * outMatrix;
                effect.World = outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                switch (type)
                {
                    case 0:
                        if (Models.OakTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.OakTris, 0, (int)(Models.OakTris.Length + 1) / 3);
                        if (Models.OakLines.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.OakLines, 0, (int)((Models.OakLines.Length + 1) / 2));
                        break;

                    case 1:
                        if (Models.SpruceTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.SpruceTris, 0, (int)(Models.SpruceTris.Length + 1) / 3);
                        if (Models.SpruceLines.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.SpruceLines, 0, (int)((Models.SpruceLines.Length + 1) / 2));
                        break;

                    case 2:
                        if (Models.BushTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.BushTris, 0, (int)(Models.BushTris.Length + 1) / 3);
                        if (Models.getBushLines().Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.getBushLines(), 0, (int)((Models.getBushLines().Length + 1) / 2));
                        break;

                }

                effect.World = tempMatrix;
            }

            public void render(double zoom, BasicEffect effect, GraphicsDevice device, double winRot)
            {
                Matrix tempMatrix = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(extraMath.calculateVector(planet.position, (float)(planet.rotation + angle + winRot), (float)(height * zoom)), 0));
                Matrix rot = Matrix.CreateRotationZ((float)(-(planet.rotation + winRot) + (Math.PI / 2) - angle ));
                Matrix scale = Matrix.CreateScale((float)zoom);
                outMatrix = scale * rot * outMatrix;
                effect.World = outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                switch (type)
                {
                    case 0:
                        if (Models.OakTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.OakTris, 0, (int)(Models.OakTris.Length + 1) / 3);
                        if (Models.OakLines.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.OakLines, 0, (int)((Models.OakLines.Length + 1) / 2));
                        break;

                    case 1:
                        if (Models.SpruceTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.SpruceTris, 0, (int)(Models.SpruceTris.Length + 1) / 3);
                        if (Models.SpruceLines.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.SpruceLines, 0, (int)((Models.SpruceLines.Length + 1) / 2));
                        break;

                    case 2:
                        if (Models.BushTris.Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.BushTris, 0, (int)(Models.BushTris.Length + 1) / 3);
                        if (Models.getBushLines().Length + 1 > 0)
                            device.DrawUserPrimitives(PrimitiveType.LineList, Models.getBushLines(), 0, (int)((Models.getBushLines().Length + 1) / 2));
                        break;
                }

                effect.World = tempMatrix;
            }
        }

        public interface IShip
        {
            void init(Fleet owner, SolarSystem system, string planetName, int orbitHeight, double orbitAngle);

            void tick(Vector2 winPos, float zoom, double winRot, Spacegame game);

            void customTick(Spacegame game);

            void render(double zoom, double winRot, BasicEffect effect, GraphicsDevice device);
        }

        /// <summary>
        /// Handles space ships
        /// </summary>
        public class testShip : Instance, IShip
        {
            public Planet orbitPlanet;
            public float orbitHeight, targetOrbitHeight;
            public double orbitAngle;
            public SolarSystem system;
            public Fleet owner;
            public string planetName;
            public bool selected = true;

            int timer = 0;

            public testShip(Fleet owner, SolarSystem system, string planetName, int orbitHeight, double orbitAngle)
            {
                init(owner, system, planetName, orbitHeight, orbitAngle);
            }

            public void init(Fleet owner, SolarSystem system, string planetName, int orbitHeight, double orbitAngle)
            {
                this.owner = owner;
                this.system = system;
                this.planetName = planetName;
                this.orbitHeight = orbitHeight;
                this.targetOrbitHeight = orbitHeight;
                this.orbitAngle = orbitAngle;
                this.orbitPlanet = system.getPlanet(planetName);
            }

            public void tick(Vector2 winPos, float zoom, double winRot, Spacegame game)
            {
                orbitPlanet = system.getPlanet(planetName);

                position = extraMath.calculateVectorOffset(orbitPlanet.position, orbitAngle + winRot, orbitHeight * zoom);

                this.orbitAngle -= 0.001D * game.gameSpeed;
                if (game.debug)
                {
                    game.drawUnfilledCircle(orbitPlanet.position, orbitHeight * zoom, 120, Color.Red);
                }
                render(zoom, winRot, game.basicEffect, game.GraphicsDevice);
                customTick(game);
            }

            public void customTick(Spacegame game)
            {
                if (owner == game.player)
                {
                    if (Mouse.GetState().RightButton == ButtonState.Pressed)
                        this.setOrbitHeightFromMouse(game.winPos, (float)game.zoom);
                }

                timer++;
                if (timer >= 100)
                {
                    owner.lasers.Add(new Laser((Instance)this, owner, 100, 2F, 5F, direction, Color.Red, 150));
                    game.soundManager.playSound(SoundManager.SND_LASER);
                }

                if(game.debug)
                    game.drawOrbit(orbitPlanet.position, 200, orbitHeight, Color.Red);

                Debug.WriteLine(orbitHeight + "/" + targetOrbitHeight);

                if (orbitHeight > targetOrbitHeight)
                    orbitHeight -= 0.5F * game.gameSpeed;
                if (orbitHeight < targetOrbitHeight)
                    orbitHeight += 0.5F * game.gameSpeed;
            }

            public void render(double zoom, double winRot, BasicEffect effect, GraphicsDevice device)
            {
                Matrix tempMatrix = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(position + orbitPlanet.position, 0));
                Matrix rot = Matrix.CreateRotationZ((float)(-winRot - MathHelper.PiOver2 - orbitAngle));
                Matrix scale = Matrix.CreateScale((float)(zoom));
                outMatrix =scale * rot * outMatrix;
                effect.World = outMatrix;                

                effect.CurrentTechnique.Passes[0].Apply();
                if (Models.getLanderModel(1).Length+1 > 0)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.getLanderModel(1), 0, (int)(Models.getLanderModel(1).Length + 1) / 3);

                double angle = extraMath.findAngle(new Vector2(0, 0), new Vector2(-(float)(90 * zoom), -(float)(5 * zoom)));
                double length = Vector2.Distance(Vector2.Zero, new Vector2(-(float)(90*zoom), -(float)(5*zoom)));
                Vector2 pos = extraMath.calculateVectorOffset(Vector2.Zero, winRot - Math.PI / 2.0D + orbitAngle + angle, length);
                
                outMatrix = Matrix.CreateTranslation(new Vector3(position + pos + orbitPlanet.position, 0));
                effect.World = scale* rot * outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                if (Models.getFlareModel(10, Color.Red, Color.Orange).Length + 1 > 0)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.getFlareModel(10, Color.Red, Color.Orange), 0, (int)(Models.getFlareModel(10, Color.Red, Color.Orange).Length + 1) / 3);
                
                effect.World = tempMatrix;
            }
            
            public void setOrbitHeightFromMouse(Vector2 winPos, float zoom)
            {
                targetOrbitHeight = Vector2.Distance(orbitPlanet.position - winPos + new Vector2(400 / zoom, 240 / zoom), new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) / zoom;
            }

            private void mapAngle()
            {
                if (orbitAngle > Math.PI * 2)
                    orbitAngle -= Math.PI * 2;
                if (orbitAngle < 0)
                    orbitAngle += Math.PI * 2;
            }
        }

        public interface IBuilding
        {
            void Init(SolarSystem system, Planet planet, Fleet owner, double angle, int HP);

            void buildModel();

            void update(Spacegame game, GameTime gameTime, double winRot, double zoom, float gameSpeed);

            void render(double zoom, BasicEffect effect, GraphicsDevice device, double winRot);
        }

        /// <summary>
        /// Handles basic buildings
        /// </summary>
        public class Building : Instance, IBuilding
        {
            public SolarSystem system;
            public Planet planet;
            public Fleet owner;
            public double angle, leftFooting, rightFooting, height, fireAngle;
            public int moneyRate = 0, materialRate = 0, foodRate = 0, fuelRate = 0;
            public List<VertexPositionColor> triangles = new List<VertexPositionColor>();
            public List<VertexPositionColor> lines = new List<VertexPositionColor>();
            //bool metaBool = false;

            public Building(SolarSystem system, Planet planet, Fleet owner, double angle, int HP)
            {
                this.Init(system, planet, owner, angle, HP);
            }

            public void Init(SolarSystem system, Planet planet, Fleet owner, double angle, int HP)
            {
                this.system = system;
                this.planet = planet;
                this.owner = owner;
                this.angle = angle;
                this.HP = HP;
                this.height = planet.getheight(angle);
                this.buildModel();
            }

            public void buildModel() { }

            public void update(Spacegame game, GameTime gameTime, double winRot, double zoom, float gameSpeed) 
            {
                position = extraMath.calculateVector(planet.position, angle + planet.rotation + winRot, height * game.zoom);
            }

            public void render(double zoom, BasicEffect effect, GraphicsDevice device, double winRot)
            {
                Matrix tempMatrix = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(extraMath.calculateVector(planet.position, (float)(planet.rotation + angle + winRot), (float)(height * zoom)), 0));
                Matrix rot = Matrix.CreateRotationZ((float)((-planet.rotation + (Math.PI / 2) - angle) - winRot));
                Matrix scale = Matrix.CreateScale((float)zoom);
                outMatrix = scale * rot * outMatrix;
                effect.World = outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                if (triangles.Count > 0)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, triangles.ToArray(), 0, (int)triangles.Count / 3);
                if (lines.Count > 0)
                    device.DrawUserPrimitives(PrimitiveType.LineList, lines.ToArray(), 0, (int)(lines.Count / 2));

                effect.World = tempMatrix;
            }
        }

        /// <summary>
        /// Creates a test building
        /// </summary>
        public class testBuilding : Building, IBuilding
        {
            public float timer = 0;

            public testBuilding(SolarSystem system, Planet planet, Fleet owner, double angle) : base(system, planet, owner, angle, 100) 
            {
                this.moneyRate = 5;
                this.materialRate = 0;
                this.foodRate = 10;
                this.fuelRate = 0;
                this.leftFooting = planet.getheight(angle + 0.001D);
                this.rightFooting = planet.getheight(angle - 0.001D);
                this.buildModel();
            }

            new public void buildModel()
            {
                this.triangles.Add(new VertexPositionColor(new Vector3(20, -8, 0), Color.Gray));
                this.triangles.Add(new VertexPositionColor(new Vector3(-20, 12, 0), Color.Gray));
                this.triangles.Add(new VertexPositionColor(new Vector3(-20, -8, 0), Color.Gray));

                this.triangles.Add(new VertexPositionColor(new Vector3(20, -8, 0), Color.Gray));
                this.triangles.Add(new VertexPositionColor(new Vector3(20, 12, 0), Color.Gray));
                this.triangles.Add(new VertexPositionColor(new Vector3(-20, 12, 0), Color.Gray));
            }

            new public void update(Spacegame game, GameTime gameTime, double winRot, double zoom, float gameSpeed)
            {
                position = extraMath.calculateVector(planet.position, angle + planet.rotation + winRot, height * game.zoom);
                timer += (1F * gameSpeed);
                if (timer >= 50)
                {
                    game.soundManager.playSound(SoundManager.SND_LASER, false);
                    this.owner.lasers.Add(new Laser((Instance)this, owner, 20, 6F, 2F, angle + planet.rotation + winRot, Color.Red, 150));
                    timer = 0;
                }
            }

            public void render(double zoom, BasicEffect effect, GraphicsDevice device)
            {
                Matrix tempMatrix = effect.World;
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(extraMath.calculateVector(planet.position, (float)(planet.rotation + angle), (float)(height * (float)zoom)), 0));
                Matrix rot = Matrix.CreateRotationZ((float)(-planet.rotation + (Math.PI / 2) - angle));
                Matrix scale = Matrix.CreateScale((float)zoom);
                outMatrix = scale * rot * outMatrix;
                effect.World = outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                if (triangles.Count > 0)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, triangles.ToArray(), 0, (int)triangles.Count / 3);
                if (lines.Count > 0)
                    device.DrawUserPrimitives(PrimitiveType.LineList, lines.ToArray(), 0, (int)(lines.Count / 2));

                effect.World = tempMatrix;
            }
        }

        /// <summary>
        /// Handles the laser beams
        /// </summary>
        public class Laser : Instance
        {
            Instance shooter;
            Fleet parent;
            double prevWinrot = 0, distance = 0;
            float size = 1;
            public float life, startLife;
            Color color;

            public Laser(Instance shooter, Fleet parent, double startDistance, float size, float speed, double direction, Color color, int life)
            {
                this.shooter = shooter;
                this.parent = parent;
                this.size = size;
                this.speed = speed;
                this.direction = MathHelper.WrapAngle((float)direction);
                this.distance = startDistance;
                this.color = color;
                this.startLife = life;
                this.life = life;
                position = extraMath.calculateVector(shooter.position, MathHelper.WrapAngle((float)(direction)), distance);
            }

            public void update(BasicEffect effect, GraphicsDevice device, double zoom, double winRot, float gameSpeed)
            {
                distance += speed * gameSpeed;
                position = extraMath.calculateVector(shooter.position, MathHelper.WrapAngle((float)(direction)), distance * zoom);
                life -= gameSpeed;
                if (life <= 0)
                {
                    parent.disposeLaser(this);
                }
                this.render(effect, device, zoom, winRot);
                //Debug.WriteLine("Tracked laser found at " + position);
                prevWinrot = winRot;
            }

            public void render(BasicEffect effect, GraphicsDevice device, double zoom, double winRot)
            {
                Matrix tempMatrix = effect.World;
                Matrix winRotMatrix = Matrix.CreateRotationZ((float)0);
                Matrix outMatrix = Matrix.CreateTranslation(new Vector3(position, 0));
                Matrix rot = Matrix.CreateRotationZ((float)(-direction));
                Matrix scale = Matrix.CreateScale((float)(zoom * size));
                outMatrix = scale * rot * winRotMatrix * outMatrix;
                effect.World = outMatrix;

                effect.CurrentTechnique.Passes[0].Apply();
                if (color == Color.Red)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.RedLaser(), 0, (int)(Models.RedLaser().Length + 1) / 3);
                if (color == Color.Blue)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, Models.BlueLaser(), 0, (int)(Models.BlueLaser().Length + 1) / 3);

                effect.World = tempMatrix;
            }
        }

        /// <summary>
        /// Handles game configuration
        /// </summary>
        public class Config
        {
            //Version keeps from loading or corrupting unwanted files
            const string version = "1.0.1";
            //Values to configure with
            public float masterVolume, SFXVolume, musicVolume;
            //the name of the configuration
            public string configName;

            /// <summary>
            /// Prepares a new config
            /// </summary>
            /// <param name="Name">The name of the config, used to save</param>
            /// <param name="masterVolume">The initial master volume of the config</param>
            /// <param name="SFXVolume">The initial SFX volume of the config</param>
            /// <param name="musicVolume">The initial music Volume of the config</param>
            public Config(string Name, float masterVolume, float SFXVolume, float musicVolume)
            {
                this.configName = Name;
                this.masterVolume = masterVolume;
                this.SFXVolume = SFXVolume;
                this.musicVolume = musicVolume;
            }

            /// <summary>
            /// Saves the config to the directory of the config's name
            /// </summary>
            public void save()
            {
                string filepath = Directory.GetCurrentDirectory() + "\\Configs\\" + configName + ".CFG";
                if(!Directory.Exists(Directory.GetCurrentDirectory() + "\\Configs"))
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Configs");

                FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);
                BinaryWriter write = new BinaryWriter(fs);

                write.Write(version);
                write.Write(configName);
                write.Write(masterVolume);
                write.Write(SFXVolume);
                write.Write(musicVolume);

                write.Close();
                write.Dispose();
            }

            /// <summary>
            /// Returns a config that is loaded from the specified name
            /// </summary>
            /// <param name="configName">The name of the config to load</param>
            /// <returns>A loaded config</returns>
            public static Config load(string configName)
            {
                string filepath = Directory.GetCurrentDirectory() + "\\Configs\\" + configName + ".CFG";
                try
                {
                    FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);
                    BinaryReader read = new BinaryReader(fs);

                    string tempVersion = read.ReadString();
                    if (tempVersion.Equals(version))
                    {
                        string name = read.ReadString();
                        float masterVol = read.ReadSingle();
                        float SFXVol = read.ReadSingle();
                        float musicVol = read.ReadSingle();

                        return new Config(name, masterVol, SFXVol, musicVol);
                    }
                    read.Close();
                    read.Dispose(); 
                }
                catch (IOException) { }
                return null;
            }

            /// <summary>
            /// Applies this configuration to the game
            /// </summary>
            /// <param name="game">The game to apply to</param>
            public void applyConfig(Spacegame game)
            {
                game.soundManager.setVolume(masterVolume);
                game.soundManager.setSFXVolume(SFXVolume);
                game.soundManager.setMusicVolume(musicVolume);
                game.menu.setSliders(masterVolume, SFXVolume, musicVolume);
            }

            /// <summary>
            /// Checks if the config exists
            /// </summary>
            /// <param name="configName">The config to check for</param>
            /// <returns>True if file exists</returns>
            public static bool exists(string configName)
            {
                return File.Exists(Directory.GetCurrentDirectory() + "\\Configs\\" + configName + ".CFG");
            }
        }

        /// <summary>
        /// Holds models to save resources
        /// </summary>
        public static class Models
        {
            #region lasers
            public static VertexPositionColor[] RedLaser()
            {
                VertexPositionColor[] r = new VertexPositionColor[]
                {
                new VertexPositionColor(new Vector3(0, 0.5F, 0), Color.Red),
                new VertexPositionColor(new Vector3(-(3), 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, -0.5F, 0), Color.Red),

                new VertexPositionColor(new Vector3(0, -0.5F, 0), Color.Red),
                new VertexPositionColor(new Vector3(2, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, 0.5F, 0), Color.Red)
                };
                return r;
            }

            public static VertexPositionColor[] BlueLaser()
            {
                VertexPositionColor[] r = new VertexPositionColor[]
                {
                new VertexPositionColor(new Vector3(0, 0.5F, 0), Color.Blue),
                new VertexPositionColor(new Vector3(-(3), 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, -0.5F, 0), Color.Blue),

                new VertexPositionColor(new Vector3(0, -0.5F, 0), Color.Blue),
                new VertexPositionColor(new Vector3(2, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0.5F, 0), Color.Blue)
                };
                return r;
            }
            
            #endregion

            #region spruce
            public static VertexPositionColor[] SpruceTris = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(0, 10, 0), Color.Brown),
                new VertexPositionColor(new Vector3(0, -6, 0), Color.Brown),
                new VertexPositionColor(new Vector3(6, -6, 0), Color.DarkKhaki),

                new VertexPositionColor(new Vector3(12, 4, 0), Color.DarkGreen),
                new VertexPositionColor(new Vector3(0, 18, 0), Color.Green),
                new VertexPositionColor(new Vector3(-12, 4, 0), Color.DarkGreen),

                new VertexPositionColor(new Vector3(8, 12, 0), Color.Green),
                new VertexPositionColor(new Vector3(0, 24, 0), Color.DarkGreen),
                new VertexPositionColor(new Vector3(-8, 12, 0), Color.Green)
            };

            public static VertexPositionColor[] SpruceLines = new VertexPositionColor[]
            {       
                    new VertexPositionColor(new Vector3(-12, 4, 0), Color.Black),
                    new VertexPositionColor(new Vector3(12, 4, 0), Color.Black),

                    new VertexPositionColor(new Vector3(-8, 12, 0), Color.Black),
                    new VertexPositionColor(new Vector3(8, 12, 0), Color.Black),

                    new VertexPositionColor(new Vector3(-8, 12, 0), Color.Black),
                    new VertexPositionColor(new Vector3(0, 24, 0), Color.Black),

                    new VertexPositionColor(new Vector3(0, 24, 0), Color.Black),
                    new VertexPositionColor(new Vector3(8, 12, 0), Color.Black),

                    new VertexPositionColor(new Vector3(-12, 4, 0), Color.Black),
                    new VertexPositionColor(new Vector3(-5, 12, 0), Color.Black),

                    new VertexPositionColor(new Vector3(12, 4, 0), Color.Black),
                    new VertexPositionColor(new Vector3(5, 12, 0), Color.Black),

                    new VertexPositionColor(new Vector3(0, -6, 0), Color.Black),
                    new VertexPositionColor(new Vector3(0, 4, 0), Color.Black),

                    new VertexPositionColor(new Vector3(6, -6, 0), Color.Black),
                    new VertexPositionColor(new Vector3(2.2F, 4, 0), Color.Black)
            };
            #endregion

            #region oak
            public static VertexPositionColor[] OakTris = new VertexPositionColor[]
            {
                //branch
                new VertexPositionColor(new Vector3(0, 4, 0), Color.Brown),
                new VertexPositionColor(new Vector3(10, 12, 0), Color.Brown),
                new VertexPositionColor(new Vector3(6, 12, 0), Color.Brown),
                //trunk
                new VertexPositionColor(new Vector3(0, 14, 0), Color.Brown),
                new VertexPositionColor(new Vector3(0,-8,0), Color.Brown),
                new VertexPositionColor(new Vector3(6,-8,0), Color.OliveDrab),
                //leaves
                new VertexPositionColor(new Vector3(-4, 10, 0), Color.DarkGreen),
                new VertexPositionColor(new Vector3(13, 10, 0), Color.DarkGreen),
                new VertexPositionColor(new Vector3(3, 24, 0), Color.Green)
            };

            public static VertexPositionColor[] OakLines = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(-4, 10, 0), Color.Black),
                new VertexPositionColor(new Vector3(13, 10, 0), Color.Black),

                new VertexPositionColor(new Vector3(13, 10, 0), Color.Black),
                new VertexPositionColor(new Vector3(3, 24, 0), Color.Black),

                new VertexPositionColor(new Vector3(3, 24, 0), Color.Black),
                new VertexPositionColor(new Vector3(-4, 10, 0), Color.Black),

                new VertexPositionColor(new Vector3(0, -8, 0), Color.Black),
                new VertexPositionColor(new Vector3(-0, 10, 0), Color.Black),

                new VertexPositionColor(new Vector3(6, -8, 0), Color.Black),
                new VertexPositionColor(new Vector3(1.0F, 10, 0), Color.Black),

                new VertexPositionColor(new Vector3(2.2F, 5.9F, 0), Color.Black),
                new VertexPositionColor(new Vector3(7.5F, 10, 0), Color.Black),

                new VertexPositionColor(new Vector3(4.6F, 10, 0), Color.Black),
                new VertexPositionColor(new Vector3(2.0F, 6.6F, 0), Color.Black)
            };
            #endregion

            #region bush
            public static VertexPositionColor[] BushTris = new VertexPositionColor[]
            {
                //trunk
                new VertexPositionColor(new Vector3(0, 4, 0), Color.Brown),
                new VertexPositionColor(new Vector3(0, -10, 0), Color.Brown),
                new VertexPositionColor(new Vector3(8, -10, 0), Color.Brown),
                //trunk2
                new VertexPositionColor(new Vector3(-4, -10, 0), Color.Brown),
                new VertexPositionColor(new Vector3(8, -10, 0), Color.Brown),
                new VertexPositionColor(new Vector3(8, 6, 0), Color.Brown),
                //leaves
                new VertexPositionColor(new Vector3(-4, 1, 0), Color.DarkGreen),
                new VertexPositionColor(new Vector3(13, 1, 0), Color.Green),
                new VertexPositionColor(new Vector3(3, 12, 0), Color.Olive)
            };

            public static VertexPositionColor[] getBushLines()
            {
                return new VertexPositionColor[]
                {
                    new VertexPositionColor(new Vector3(-4, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(13, 1, 0), Color.Black),

                    new VertexPositionColor(new Vector3(13, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(3, 12, 0), Color.Black),

                    new VertexPositionColor(new Vector3(2, 12, 0), Color.Black),
                    new VertexPositionColor(new Vector3(-4, 1, 0), Color.Black),

                    new VertexPositionColor(new Vector3(0, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(0, -10, 0), Color.Black),

                    new VertexPositionColor(new Vector3(8, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(8, -10, 0), Color.Black),

                    new VertexPositionColor(new Vector3(3, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(3, 25, 0), Color.Black),

                    new VertexPositionColor(new Vector3(1, 1, 0), Color.Black),
                    new VertexPositionColor(new Vector3(3, 1, 0), Color.Black)
                };
            }
            #endregion

            public static VertexPositionColor[] getFlareModel(float scale, Color innerColor, Color outerColor)
            {
                float halfScale = scale/2;
                List<VertexPositionColor> list = new List<VertexPositionColor>();

                #region Outer Flame
                list.Add(new VertexPositionColor(new Vector3(-scale, -scale, 0), outerColor));
                list.Add(new VertexPositionColor(new Vector3(0, -halfScale, 0), outerColor));
                list.Add(new VertexPositionColor(new Vector3(0, halfScale, 0), outerColor));
                #endregion

                #region Inner Flame

                #endregion

                return list.ToArray();
            }

            public static VertexPositionColor[] getLanderModel(float scale)
            {
                Color darkPurple = new Color(96, 0, 96);
                Color mediumPurple = new Color(182, 0, 182);
                Color lightPurple = new Color(200, 0, 200);
                Color customGrey = new Color(128, 128, 128);

                List<VertexPositionColor> list = new List<VertexPositionColor>();

                #region Outer Flame
                //1
                list.Add(new VertexPositionColor(new Vector3(-90 * scale, -10 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, -10 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(-90 * scale, 0, 0), darkPurple));

                //2
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, -10 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, 0, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(-90 * scale, 0, 0), darkPurple));

                //3
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, 10 * scale, 0), lightPurple));
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, -20 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-50 * scale, -30 * scale, 0), darkPurple));

                //4
                list.Add(new VertexPositionColor(new Vector3(-50 * scale, -30 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(-40 * scale, -15 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, 10 * scale, 0), lightPurple));

                //5
                list.Add(new VertexPositionColor(new Vector3(-40 * scale, -15 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-50 * scale, 10, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-80 * scale, 10, 0), lightPurple));

                //6
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -30 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-40 * scale, -15 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(-50 * scale, -30 * scale, 0), darkPurple));

                //7
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -30 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -15 * scale, 0), lightPurple));
                list.Add(new VertexPositionColor(new Vector3(-40 * scale, -15 * scale, 0), mediumPurple));

                //8
                list.Add(new VertexPositionColor(new Vector3(80 * scale, -20 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -15 * scale, 0), lightPurple));
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -30 * scale, 0), mediumPurple));

                //9
                list.Add(new VertexPositionColor(new Vector3(80 * scale, -20 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(60 * scale, 10 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(50 * scale, -15 * scale, 0), lightPurple));

                //10
                list.Add(new VertexPositionColor(new Vector3(90 * scale, 10 * scale, 0), darkPurple));
                list.Add(new VertexPositionColor(new Vector3(60 * scale, 10 * scale, 0), mediumPurple));
                list.Add(new VertexPositionColor(new Vector3(80 * scale, -20 * scale, 0), darkPurple));
                #endregion

                return list.ToArray();
            }

            public static VertexPositionColor[] getHumanCruiserModel(float scale)
            {
                Color darkGrey = new Color(64, 64, 64);
                Color mediumGrey = new Color(130, 130, 130);
                Color lightGrey = new Color(200, 200, 200);

                List<VertexPositionColor> list = new List<VertexPositionColor>();

                #region Outer Flame
                //1
                list.Add(new VertexPositionColor(new Vector3(-100, 30, 0), mediumGrey));
                list.Add(new VertexPositionColor(new Vector3(-100, -60, 0), mediumGrey));
                list.Add(new VertexPositionColor(new Vector3(-50, -60, 0), darkGrey));

                //2
                list.Add(new VertexPositionColor(new Vector3(-100, 30, 0), mediumGrey));
                list.Add(new VertexPositionColor(new Vector3(-50, 30, 0), darkGrey));
                list.Add(new VertexPositionColor(new Vector3(-50, -60, 0), darkGrey));

                //3
                list.Add(new VertexPositionColor(new Vector3(-50, -60, 0), darkGrey));
                list.Add(new VertexPositionColor(new Vector3(100, -10, 0), lightGrey));
                list.Add(new VertexPositionColor(new Vector3(100, -60, 0), mediumGrey));

                //4
                list.Add(new VertexPositionColor(new Vector3(-50, 10, 0), mediumGrey));
                list.Add(new VertexPositionColor(new Vector3(100, -10, 0), darkGrey));
                list.Add(new VertexPositionColor(new Vector3(-50, -60, 0), darkGrey));

                #endregion

                return list.ToArray();
            }
        }
        #endregion
    }
}


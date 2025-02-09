// Configura a janela, cria uma textura para desenhar os retângulos 
// instancia e preenche o mapa com tiles de cores sólidas
// tambpem implementa a seleção pelo cursor do mouse

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;


namespace TinyEditor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;


        // Instâncias dos componentes do jogo
        private Camera2D camera;
        private Map currentMap;
        private InputManager inputManager;
        private GUIManager guiManager;
        private MapEditor mapEditor;
        private MapManager mapManager;


        // Textura de 1x1 pixel para desenhar retângulos
        private Texture2D pixel;
        // Fonte para desenhar textos na GUI
        private SpriteFont font;

        // Para gerenciar cliques na área da GUI
        private MouseState previousGuiMouseState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            camera = new Camera2D(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Utilize o MapGenerator para criar um mapa
            currentMap = MapGenerator.GenerateWaterBorderMap(10, 10, 64);

            // Instancia o MapManager e adiciona o mapa gerado
            mapManager = new MapManager();
            mapManager.AddMap("Mapa Default",currentMap);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cria uma textura de 1x1 pixel branco (usada para desenhar retângulos preenchidos)
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            // Carrega uma fonte (assegure-se de ter uma fonte chamada "DefaultFont" no Content)
            font = Content.Load<SpriteFont>("DefaultFont");

            // Instancia os gerenciadores de entrada e edição
            inputManager = new InputManager(camera, currentMap);
            guiManager = new GUIManager(pixel, font, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            mapEditor = new MapEditor(currentMap, camera, guiManager);

            previousGuiMouseState = Mouse.GetState();

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();

            // Se o clique ocorrer na área da GUI (barra lateral) e for um clique único, delega para o GUIManager.
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousGuiMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.X < guiManager.SidebarWidth)
            {
                guiManager.HandleMouseClick(currentMouseState.Position);
            }

            // Atualiza os demais gerenciadores:
            // - InputManager: zoom e movimento da câmera (ainda é permitido usar Shift para mover a câmera)
            inputManager.Update();
            // - MapEditor: se estivermos no modo de edição, permite alterar a cor do tile clicado na área do mapa.
            mapEditor.Update();

            previousGuiMouseState = currentMouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Desenha o mapa com a transformação da câmera
            spriteBatch.Begin(transformMatrix: camera.GetTransformation());
            currentMap.Draw(spriteBatch,pixel);
            spriteBatch.End();

            // Desenha a GUI (sem transformação)
            spriteBatch.Begin();
            guiManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

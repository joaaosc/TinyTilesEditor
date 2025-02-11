using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MessageBox = System.Windows.Forms.MessageBox;

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
        private TextureListUI textureListUI;
        private bool editModeActive = false;

        // Textura de 1x1 pixel para desenhar retângulos
        private Texture2D pixel;
        // Fonte para desenhar textos na GUI
        private SpriteFont font;

        // Para gerenciar cliques na área da GUI e no painel de texturas
        private MouseState previousGuiMouseState;
        // Para evitar disparar múltiplas vezes a importação com a mesma pressão de tecla
        private KeyboardState previousKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            camera = new Camera2D(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Cria um mapa usando o MapGenerator (ex.: mapa com borda de água)
            currentMap = MapGenerator.GenerateWaterBorderMap(25, 30, 32);

            // Instancia o MapManager e adiciona o mapa gerado
            mapManager = new MapManager();
            mapManager.AddMap("Mapa Default", currentMap);

            // Cria a área do painel de texturas (sidebar direita)
            // Neste exemplo, a sidebar ocupa 200 pixels na parte direita da tela
            Rectangle panelArea = new Rectangle(GraphicsDevice.Viewport.Width - 200, 10, 190, GraphicsDevice.Viewport.Height - 20);
            textureListUI = new TextureListUI(panelArea);

            // Inicializa o estado do teclado
            previousKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cria uma textura de 1x1 pixel branco para desenhar retângulos
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            // Carrega a fonte (certifique-se de ter uma fonte chamada "DefaultFont" no Content)
            font = Content.Load<SpriteFont>("DefaultFont");

            // Instancia os gerenciadores de entrada e edição
            inputManager = new InputManager(camera, currentMap);
            guiManager = new GUIManager(pixel, font, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            mapEditor = new MapEditor(currentMap, camera, guiManager);
            // OBSERVAÇÃO: Certifique-se de que o MapEditor utiliza a propriedade SelectedTexture ao pintar

            // Subscreve os eventos da GUI para salvar e carregar mapas
            guiManager.OnSaveClicked += HandleSaveMap;
            guiManager.OnLoadClicked += HandleLoadMap;

            previousGuiMouseState = Mouse.GetState();
        }

        protected override void Update(GameTime gameTime)
        {
            // Encerra o jogo se o botão Back do GamePad ou a tecla Esc forem pressionados
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();

            // Processa cliques na área da GUI (sidebar à esquerda)
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousGuiMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.X < guiManager.SidebarWidth)
            {
                guiManager.HandleMouseClick(currentMouseState.Position);
            }

            // Processa cliques na área do painel de texturas (sidebar direita)
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousGuiMouseState.LeftButton == ButtonState.Released &&
                textureListUI.PanelArea.Contains(currentMouseState.Position))
            {
                TextureEntry selectedEntry = textureListUI.GetTextureEntryAtPoint(currentMouseState.Position);
                if (selectedEntry != null)
                {
                    System.Diagnostics.Debug.WriteLine("Textura selecionada: " + selectedEntry.Name);
                    // Atribui a textura selecionada ao MapEditor
                    mapEditor.SelectedTexture = selectedEntry.Texture;
                    // Aqui, usamos o nome da textura como identificador
                    mapEditor.SelectedTextureID = selectedEntry.Name;
                }
            }

            // Atualiza a GUI e outros gerenciadores
            guiManager.Update();
            inputManager.Update();

            // Se o mouse NÃO estiver na área do painel de texturas, atualiza o MapEditor (para pintura)
            if (!textureListUI.PanelArea.Contains(currentMouseState.Position))
            {
                mapEditor.Update();
            }
            // Caso esteja na sidebar, evita que o MapEditor pinte o mapa

            // Verifica se a tecla I foi pressionada para importar uma nova textura
            KeyboardState currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.I) && !previousKeyboardState.IsKeyDown(Keys.I))
            {
                ImportarTextura();
            }

            if (currentKeyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                // Inverte o estado do Edit Mode
                editModeActive = !editModeActive;

                // Atualiza o botão na GUI
                guiManager.SetEditModeActive(editModeActive);
            }

            previousKeyboardState = currentKeyboardState;
            previousGuiMouseState = currentMouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Desenha o mapa aplicando a transformação da câmera
            spriteBatch.Begin(transformMatrix: camera.GetTransformation());
            currentMap.Draw(spriteBatch, pixel);
            spriteBatch.End();

            // Desenha a GUI (sem transformação de câmera)
            spriteBatch.Begin();
            guiManager.Draw(spriteBatch);
            spriteBatch.End();

            // Desenha o painel de texturas (sidebar direita)
            spriteBatch.Begin();
            textureListUI.Draw(spriteBatch, pixel, font);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Abre o diálogo para importar uma textura, cria um TextureEntry e o adiciona à sidebar.
        /// </summary>
        private void ImportarTextura()
        {
            Texture2D texturaImportada = LoadTextureFromFile(GraphicsDevice);
            if (texturaImportada != null)
            {
                TextureEntry novaEntrada = new TextureEntry
                {
                    Name = "Textura " + (textureListUI.TextureEntries.Count + 1),
                    Texture = texturaImportada
                };
                textureListUI.AddTexture(novaEntrada);
            }
        }

        /// <summary>
        /// Abre um OpenFileDialog para carregar uma imagem e retorna a Texture2D correspondente.
        /// </summary>
        private Texture2D LoadTextureFromFile(GraphicsDevice graphicsDevice)
        {
            Texture2D texture = null;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.FileStream stream = System.IO.File.OpenRead(dialog.FileName))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, stream);
                    }
                }
            }
            return texture;
        }

        /// <summary>
        /// Método chamado quando "Save Map" é clicado na GUI.
        /// </summary>
        private void HandleSaveMap()
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JSON Files|*.json";
                saveDialog.Title = "Save Map";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                    mapManager.SaveMap(currentMap, saveDialog.FileName);
            }
        }

        /// <summary>
        /// Método chamado quando "Load Map" é clicado na GUI.
        /// </summary>
        private void HandleLoadMap()
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "JSON Files|*.json";
                openDialog.Title = "Load Map";
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    Map loadedMap = mapManager.LoadMap(openDialog.FileName);
                    if (loadedMap != null)
                    {
                        // Percorre todos os tiles do mapa para reatribuir as texturas
                        for (int row = 0; row < loadedMap.Rows; row++)
                        {
                            for (int col = 0; col < loadedMap.Columns; col++)
                            {
                                // Se o tile possui um TextureID salvo, tenta recuperar a textura correspondente
                                if (!string.IsNullOrEmpty(loadedMap.Tiles[row, col].TextureID))
                                {
                                    loadedMap.Tiles[row, col].Texture =
                                        textureListUI.GetTextureByID(loadedMap.Tiles[row, col].TextureID);
                                }
                            }
                        }

                        // Atualiza o mapa atual e os gerenciadores
                        currentMap = loadedMap;
                        inputManager = new InputManager(camera, currentMap);
                        mapEditor = new MapEditor(currentMap, camera, guiManager);
                    }
                    else
                    {
                        MessageBox.Show("Falha ao carregar o mapa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DrawFogOfWar(SpriteBatch spriteBatch)
        {
            // Define os limites do mapa em coordenadas do mundo.
            Rectangle mapBounds = new Rectangle(0, 0, currentMap.Columns * currentMap.TileSize, currentMap.Rows * currentMap.TileSize);

            // Calcula o retângulo visível (view rectangle) em coordenadas do mundo.
            Vector2 topLeft = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.GetTransformation()));
            Vector2 bottomRight = Vector2.Transform(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                                    Matrix.Invert(camera.GetTransformation()));
            Rectangle viewRect = new Rectangle((int)topLeft.X, (int)topLeft.Y,
                                               (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

            // Define a cor do fog (preto com transparência, ajuste o valor alfa conforme desejado).
            Color fogColor = new Color(0, 0, 0, 180);

            // Se houver área à esquerda do mapa (quando a visão vai além de x=0).
            if (viewRect.X < mapBounds.X)
            {
                int fogWidth = mapBounds.X - viewRect.X;
                Rectangle fogRect = new Rectangle(viewRect.X, viewRect.Y, fogWidth, viewRect.Height);
                spriteBatch.Draw(pixel, fogRect, fogColor);
            }

            // Se houver área acima do mapa (quando a visão vai além de y=0).
            if (viewRect.Y < mapBounds.Y)
            {
                int fogHeight = mapBounds.Y - viewRect.Y;
                Rectangle fogRect = new Rectangle(viewRect.X, viewRect.Y, viewRect.Width, fogHeight);
                spriteBatch.Draw(pixel, fogRect, fogColor);
            }

            // Se houver área à direita do mapa.
            if (viewRect.Right > mapBounds.Right)
            {
                int fogWidth = viewRect.Right - mapBounds.Right;
                Rectangle fogRect = new Rectangle(mapBounds.Right, viewRect.Y, fogWidth, viewRect.Height);
                spriteBatch.Draw(pixel, fogRect, fogColor);
            }

            // Se houver área abaixo do mapa.
            if (viewRect.Bottom > mapBounds.Bottom)
            {
                int fogHeight = viewRect.Bottom - mapBounds.Bottom;
                Rectangle fogRect = new Rectangle(viewRect.X, mapBounds.Bottom, viewRect.Width, fogHeight);
                spriteBatch.Draw(pixel, fogRect, fogColor);
            }
        }

    }
}

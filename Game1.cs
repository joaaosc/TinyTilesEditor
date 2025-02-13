using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MessageBox = System.Windows.Forms.MessageBox;
using TinyAnimation;
using SharpDX.Direct3D9;

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
        // Armazena a textura do sprite animado que foi escolhida pelo usuário
        private Texture2D pendingAnimatedSpriteTexture = null;
        // Lista de sprites animados adicionados no mapa
        private List<AnimatedSprite> animatedSprites;

        // Variável para controle do sprite que está sendo arrastado (drag & drop)
        private AnimatedSprite draggingSprite = null;

        // Armazena o offset entre a posição do sprite e o mouse no momento do clique
        private Vector2 dragOffset;

        // Flag para indicar que o usuário está no modo de adição de sprite
        private bool addingAnimatedSprite = false;
        // Textura padrão para o sprite
        private Texture2D fireTexture;

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
            animatedSprites = new List<AnimatedSprite>();
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
            TextureLoader.GraphicsDevice = GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            animatedSprites = new List<AnimatedSprite>();

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
            guiManager.OnAddAnimatedSpriteClicked += HandleAddAnimatedSprite;

            animatedSprites = new List<AnimatedSprite>();

            previousGuiMouseState = Mouse.GetState();
        }

        protected override void Update(GameTime gameTime)
        {
            // Encerra o jogo se o botão Back do GamePad ou a tecla Esc forem pressionados
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();


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



            // Se o mouse NÃO estiver na área do painel de texturas, atualiza o MapEditor (para pintura)
            if (!textureListUI.PanelArea.Contains(currentMouseState.Position))
            {
                mapEditor.Update();
            }
            // Caso esteja na sidebar, evita que o MapEditor pinte o mapa

            // Verifica se a tecla I foi pressionada para importar uma nova textura
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

            // Converte a posição do mouse de tela para coordenadas do mundo
            Vector2 mouseScreenPos = new Vector2(currentMouseState.X, currentMouseState.Y);
            Matrix inverseTransform = Matrix.Invert(camera.GetTransformation());
            Vector2 mouseWorldPos = Vector2.Transform(mouseScreenPos, inverseTransform);

            // Se o botão esquerdo foi pressionado e não estamos arrastando, verifica se clicamos em algum sprite
            if (draggingSprite == null &&
                currentMouseState.LeftButton == ButtonState.Pressed &&
                previousGuiMouseState.LeftButton == ButtonState.Released)
            {
                foreach (var sprite in animatedSprites)
                {
                    // Cria um retângulo baseado na posição e tamanho do sprite (usando o primeiro frame)
                    Rectangle spriteRect = new Rectangle(
                        (int)sprite.Position.X,
                        (int)sprite.Position.Y,
                        sprite.FrameWidth,
                        sprite.FrameHeight);
                    if (spriteRect.Contains(new Point((int)mouseWorldPos.X, (int)mouseWorldPos.Y)))
                    {
                        draggingSprite = sprite;
                        dragOffset = sprite.Position - mouseWorldPos;
                        break;
                    }
                }
            }

            // Se estamos arrastando um sprite, atualiza sua posição
            if (draggingSprite != null && currentMouseState.LeftButton == ButtonState.Pressed)
            {
                draggingSprite.Position = mouseWorldPos + dragOffset;
            }

            // Quando o botão esquerdo for liberado, encerra o arraste
            if (draggingSprite != null && currentMouseState.LeftButton == ButtonState.Released)
            {
                draggingSprite = null;
            }

            // Atualiza a animação de cada sprite
            foreach (var sprite in animatedSprites)
            {
                sprite.Update(gameTime);
            }

            // Atualiza outros componentes (GUI, InputManager, MapEditor, etc.)
            guiManager.Update();
            inputManager.Update();
            if (!textureListUI.PanelArea.Contains(currentMouseState.Position))
            {
                mapEditor.Update();
            }

            previousGuiMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);


        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Inicia o spriteBatch com a transformação da câmera para desenhar o mapa e os sprites
            spriteBatch.Begin(transformMatrix: camera.GetTransformation());
            currentMap.Draw(spriteBatch, pixel);
            foreach (var sprite in animatedSprites)
            {
                sprite.Draw(spriteBatch);
            }
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

        private void HandleAddAnimatedSprite()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                dialog.Title = "Selecione o Sprite Sheet";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = System.IO.File.OpenRead(dialog.FileName))
                    {
                        pendingAnimatedSpriteTexture = Texture2D.FromStream(GraphicsDevice, stream);
                    }

                    // Calcula o centro da tela em coordenadas do mundo
                    // Se sua classe Camera2D possuir a propriedade Position, use-a; caso contrário, ajuste conforme necessário.
                    Vector2 worldCenter = camera.Position + new Vector2(GraphicsDevice.Viewport.Width / 2f,
                        GraphicsDevice.Viewport.Height / 2f);

                    // Cria o novo sprite animado no centro do mundo visível
                    AnimatedSprite newSprite = new AnimatedSprite(
                        pendingAnimatedSpriteTexture,
                        worldCenter,
                        frameWidth: 256 / 8,    // 256 / 2 frames
                        frameHeight: 48,
                        frameCount: 8,      // Apenas 2 frames na animação
                        frameTime: 0.2f);
                    ;                        ;
                    animatedSprites.Add(newSprite);

                    // Limpa a textura pendente
                    pendingAnimatedSpriteTexture = null;
                }
            }
        }
        public static class TextureLoader
        {
            // Dicionário para armazenar texturas já carregadas, evitando recarregar a mesma textura.
            private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

            // Propriedade que deve ser definida no Game1, para que o TextureLoader tenha acesso ao GraphicsDevice.
            public static GraphicsDevice GraphicsDevice { get; set; }

            /// <summary>
            /// Carrega uma textura a partir do caminho especificado (textureID). 
            /// Se a textura já tiver sido carregada, retorna a referência em cache.
            /// </summary>
            /// <param name="textureID">Caminho do arquivo ou identificador da textura.</param>
            /// <returns>A instância de Texture2D ou null se não conseguir carregar.</returns>
            public static Texture2D Load(string textureID)
            {
                if (loadedTextures.ContainsKey(textureID))
                {
                    return loadedTextures[textureID];
                }

                if (GraphicsDevice == null)
                {
                    throw new Exception("A propriedade GraphicsDevice do TextureLoader não foi definida.");
                }

                // Verifica se o arquivo existe
                if (!File.Exists(textureID))
                {
                    throw new FileNotFoundException("Arquivo de textura não encontrado.", textureID);
                }

                // Carrega a textura a partir do stream do arquivo
                Texture2D texture;
                using (FileStream stream = new FileStream(textureID, FileMode.Open, FileAccess.Read))
                {
                    texture = Texture2D.FromStream(GraphicsDevice, stream);
                }

                // Armazena a textura no cache e a retorna
                loadedTextures[textureID] = texture;
                return texture;
            }
        }

    }
}

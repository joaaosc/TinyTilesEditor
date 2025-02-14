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
using Microsoft.VisualBasic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TinyEditor
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private MouseState previousMouseState;

        // Instâncias dos componentes do jogo
        private Camera2D camera;
        private Map currentMap;
        private InputManager inputManager;
        private GUIManager guiManager;
        private MapEditor mapEditor;
        private MapManager mapManager;
        private TextureListUI textureListUI;
        private bool editModeActive = false;
        private bool isRemovingSprite = false;

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
            guiManager.OnRemoveAnimatedSpriteClicked += HandleRemoveAnimatedSprite;
            guiManager.OnPlayClicked += HandlePlayButtonClicked;

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

            foreach (var sprite in currentMap.AnimatedSprites)
            {
                sprite.Update(gameTime);
            }

            // Em seguida, trate o drag & drop
            // Se não estamos arrastando nenhum sprite e o botão esquerdo foi clicado, verifica se clicamos em algum sprite
            if (draggingSprite == null &&
                currentMouseState.LeftButton == ButtonState.Pressed &&
                previousGuiMouseState.LeftButton == ButtonState.Released)
            {
                foreach (var sprite in currentMap.AnimatedSprites)
                {
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

            // Se estamos arrastando um sprite, atualize sua posição
            if (draggingSprite != null && currentMouseState.LeftButton == ButtonState.Pressed)
            {
                draggingSprite.Position = mouseWorldPos + dragOffset;
            }

            // Quando o botão esquerdo for liberado, encerre o arraste
            if (draggingSprite != null && currentMouseState.LeftButton == ButtonState.Released)
            {
                draggingSprite = null;
            }

            // Atualiza outros componentes (GUI, InputManager, MapEditor, etc.)
            guiManager.Update();
            inputManager.Update();
            if (!textureListUI.PanelArea.Contains(currentMouseState.Position))
            {
                mapEditor.Update();
            }

            // Verifica se a tecla R está pressionada e se o botão direito foi clicado
            if (currentKeyboardState.IsKeyDown(Keys.R) &&
                currentMouseState.RightButton == ButtonState.Pressed &&
                previousGuiMouseState.RightButton == ButtonState.Released)
            {
                // Percorre os sprites do mapa atual (em ordem reversa para segurança na remoção)
                for (int i = currentMap.AnimatedSprites.Count - 1; i >= 0; i--)
                {
                    AnimatedSprite sprite = currentMap.AnimatedSprites[i];
                    // Define o retângulo do sprite com base na posição e dimensões do primeiro frame
                    Rectangle spriteRect = new Rectangle(
                        (int)sprite.Position.X,
                        (int)sprite.Position.Y,
                        sprite.FrameWidth,
                        sprite.FrameHeight);

                    if (spriteRect.Contains(new Point((int)mouseWorldPos.X, (int)mouseWorldPos.Y)))
                    {
                        // Remove o sprite encontrado e interrompe o loop
                        currentMap.AnimatedSprites.RemoveAt(i);
                        break;
                    }
                }
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
            foreach (var sprite in currentMap.AnimatedSprites)
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
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                dialog.Title = "Selecione uma textura";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Obtenha apenas o nome do arquivo (sem o caminho)
                    string fileName = Path.GetFileName(dialog.FileName);

                    // Carrega a textura a partir do arquivo selecionado
                    Texture2D texturaImportada;
                    using (var stream = File.OpenRead(dialog.FileName))
                    {
                        texturaImportada = Texture2D.FromStream(GraphicsDevice, stream);
                    }

                    if (texturaImportada != null)
                    {
                        // Define o TextureID como o nome real do arquivo
                        TextureEntry novaEntrada = new TextureEntry
                        {
                            Name = fileName,
                            Texture = texturaImportada
                        };
                        textureListUI.AddTexture(novaEntrada);
                    }
                }
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
                    // Define a pasta onde estão as texturas (por exemplo, uma pasta "textures" no diretório de saída)
                    string texturesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "textures");

                    // Importa todas as texturas referenciadas no JSON do mapa
                    ImportarTexturasDoMapa(openDialog.FileName, texturesFolder);

                    // Agora carrega o mapa normalmente
                    Map loadedMap = mapManager.LoadMap(openDialog.FileName);
                    if (loadedMap != null)
                    {
                        // Percorre os tiles para reatribuir as texturas usando o textureListUI, se necessário
                        for (int row = 0; row < loadedMap.Rows; row++)
                        {
                            for (int col = 0; col < loadedMap.Columns; col++)
                            {
                                if (!string.IsNullOrEmpty(loadedMap.Tiles[row, col].TextureID))
                                {
                                    loadedMap.Tiles[row, col].Texture = textureListUI.GetTextureByID(loadedMap.Tiles[row, col].TextureID);
                                }
                            }
                        }

                        // Atualiza o mapa atual e os gerenciadores conforme sua lógica
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
        private void HandlePlayButtonClicked()
        {
            // Salva o mapa atual
            string tempMapPath = @"C:\\Users\\joao\\source\\repos\\TinyGame\\maps\\testmap3.json";
            mapManager.SaveMap(currentMap, tempMapPath);

            // Constrói o caminho para o executável TinyGame.exe.
            // Por exemplo, suponha que o executável esteja em uma pasta "TinyGame" no mesmo nível do diretório atual:
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TinyGame.exe");
            // Se estiver em outro diretório, ajuste o caminho:
            // string exePath = @"C:\Caminho\Para\TinyGame.exe";

            if (File.Exists(exePath))
            {
                Process.Start(exePath);
            }
            else
            {
                MessageBox.Show("TinyGame.exe não foi encontrado em: " + exePath,
                    "Erro ao iniciar o jogo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadNewMap(string filePath)
        {
            Map loadedMap = mapManager.LoadMap(filePath);
            if (loadedMap != null)
            {
                // Substitua o mapa atual para que os sprites antigos sejam descartados.
                currentMap = loadedMap;
            }
            else
            {
                MessageBox.Show("Falha ao carregar o mapa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleRemoveAnimatedSprite()
        {
            // Ativa o modo de remoção
            isRemovingSprite = true;
        }

        private void HandleAddAnimatedSprite()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                dlg.Title = "Selecione o Sprite Sheet para adicionar";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string texturePath = dlg.FileName;
                    Texture2D texture = TextureLoader.Load(texturePath);

                    int frameCount = 1;
                    // Se a largura for múltiplo da altura, podemos detectar automaticamente
                    if (texture.Height > 0 && texture.Width % texture.Height == 0)
                    {
                        frameCount = texture.Width / texture.Height;
                    }
                    else
                    {
                        // Se não for, solicita ao usuário o número de frames
                        string input = Interaction.InputBox(
                            "Não foi possível detectar automaticamente o número de frames.\n" +
                            "Insira o número de frames para essa spritesheet:",
                            "Número de Frames",
                            "1");
                        if (!int.TryParse(input, out frameCount) || frameCount < 1)
                        {
                            MessageBox.Show("Valor inválido. Usando 1 frame (sprite estático).");
                            frameCount = 1;
                        }
                    }

                    int frameWidth = texture.Width / frameCount;
                    int frameHeight = texture.Height;

                    // Cria o sprite no centro da tela (convertendo para coordenadas do mundo, se estiver usando câmera)
                    Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
                    Matrix invTransform = Matrix.Invert(camera.GetTransformation());
                    Vector2 worldCenter = Vector2.Transform(screenCenter, invTransform);

                    AnimatedSprite sprite = new AnimatedSprite(texture, worldCenter, frameWidth, frameHeight, frameCount, 0.2f);
                    sprite.TextureID = texturePath; // Salva o identificador da textura

                    // Adiciona o sprite ao mapa atual
                    currentMap.AnimatedSprites.Add(sprite);
                }
            }
        }

        public static class TextureLoader
        {
            private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

            // Essa propriedade deve ser definida em Game1, por exemplo, em LoadContent.
            public static GraphicsDevice GraphicsDevice { get; set; }

            /// <summary>
            /// Tenta carregar a textura a partir do textureID (geralmente um caminho para o arquivo).
            /// Se o arquivo não for encontrado, exibe um diálogo para o usuário localizá-lo manualmente.
            /// </summary>
            public static Texture2D Load(string textureID)
            {
                if (loadedTextures.ContainsKey(textureID))
                    return loadedTextures[textureID];

                if (GraphicsDevice == null)
                    throw new Exception("GraphicsDevice não foi definido no TextureLoader.");

                if (!File.Exists(textureID))
                {
                    DialogResult result = MessageBox.Show(
                        $"Arquivo de textura não encontrado:\n{textureID}\nDeseja localizar manualmente?",
                        "Textura não encontrada",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (OpenFileDialog dlg = new OpenFileDialog())
                        {
                            dlg.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                            dlg.Title = $"Localize a textura para: {textureID}";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                textureID = dlg.FileName;
                            }
                            else
                            {
                                throw new FileNotFoundException("Usuário cancelou a localização da textura.", textureID);
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("Textura não encontrada e o usuário optou por não localizar.", textureID);
                    }
                }

                Texture2D texture;
                using (FileStream stream = new FileStream(textureID, FileMode.Open, FileAccess.Read))
                {
                    texture = Texture2D.FromStream(GraphicsDevice, stream);
                }
                loadedTextures[textureID] = texture;
                return texture;
            }
        }

        private void ImportarTexturasDoMapa(string mapJsonPath, string texturesFolder)
        {
            // Lê o JSON e desserializa-o para o DTO MapData (usado no MapManager)
            string json = File.ReadAllText(mapJsonPath);
            var mapData = JsonConvert.DeserializeObject<MapManager.MapData>(json);

            // Cria um conjunto para armazenar os nomes únicos das texturas
            HashSet<string> textureNames = new HashSet<string>();

            // Se houver dados de tiles, adicione os TextureID de cada tile
            if (mapData.Tiles != null)
            {
                foreach (var row in mapData.Tiles)
                {
                    foreach (var tileData in row)
                    {
                        if (!string.IsNullOrEmpty(tileData.TextureID))
                            textureNames.Add(tileData.TextureID);
                    }
                }
            }

            // Se houver dados de sprites animados, adicione os TextureID
            if (mapData.AnimatedSprites != null)
            {
                foreach (var spriteData in mapData.AnimatedSprites)
                {
                    if (!string.IsNullOrEmpty(spriteData.TextureID))
                        textureNames.Add(spriteData.TextureID);
                }
            }

            // Para cada textura referenciada, constrói o caminho completo e importa
            foreach (string texName in textureNames)
            {
                string filePath = Path.Combine(texturesFolder, texName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Carrega a textura usando o TextureLoader
                        Texture2D texture = TextureLoader.Load(filePath);
                        // Cria uma entrada de textura usando o nome do arquivo
                        TextureEntry novaEntrada = new TextureEntry
                        {
                            Name = texName,
                            Texture = texture
                        };
                        // Adiciona à lista do TextureListUI se ainda não existir
                        if (!textureListUI.ContainsTexture(texName))
                            textureListUI.AddTexture(novaEntrada);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao carregar a textura '" + texName + "': " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Textura '" + texName + "' não encontrada na pasta: " + texturesFolder);
                }
            }
        }

    }
}

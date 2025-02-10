using System.IO;
using System.Windows.Forms; // Certifique-se de adicionar a referência a System.Windows.Forms
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TinyEditor
{
    public class MapEditor
    {
        private Map map;
        private Camera2D camera;
        private GUIManager guiManager;
        private MouseState previousMouseState;

        // Propriedade para armazenar a textura selecionada para pintura
        public Texture2D SelectedTexture { get; set; }
        // NOVA: Propriedade para armazenar o identificador da textura selecionada (por exemplo, o nome)
        public string SelectedTextureID { get; set; }

        public MapEditor(Map map, Camera2D camera, GUIManager guiManager)
        {
            this.map = map;
            this.camera = camera;
            this.guiManager = guiManager;
            previousMouseState = Mouse.GetState();
            SelectedTexture = null; // Inicialmente, nenhuma textura está selecionada
            SelectedTextureID = null;
        }

        public void Update()
        {
            MouseState currentMouseState = Mouse.GetState();

            // Se o modo de edição estiver ativo, o botão esquerdo estiver pressionado,
            // o mouse estiver fora da área da GUI e não estivermos usando shift (para movimentar a câmera)
            if (guiManager.EditModeActive &&
                currentMouseState.LeftButton == ButtonState.Pressed &&
                currentMouseState.X > guiManager.SidebarWidth &&
                !Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                // Converte a posição do mouse (na tela) para as coordenadas do mundo, usando a matriz inversa da câmera
                Vector2 worldMousePosition = Vector2.Transform(
                    new Vector2(currentMouseState.X, currentMouseState.Y),
                    Matrix.Invert(camera.GetTransformation()));

                // Determina qual tile foi selecionado com base na posição do mouse
                int col = (int)(worldMousePosition.X / map.TileSize);
                int row = (int)(worldMousePosition.Y / map.TileSize);

                if (row >= 0 && row < map.Rows && col >= 0 && col < map.Columns)
                {
                    // Somente pinta se uma textura estiver selecionada
                    if (SelectedTexture != null)
                    {
                        // Aplica a textura selecionada ao tile
                        map.Tiles[row, col].Texture = SelectedTexture;
                        // Salva também o identificador da textura no tile
                        map.Tiles[row, col].TextureID = SelectedTextureID;
                    }
                }
            }

            previousMouseState = currentMouseState;
        }

        /// <summary>
        /// Abre um diálogo para carregar uma textura a partir de um arquivo.
        /// </summary>
        public Texture2D LoadTextureFromFile(GraphicsDevice graphicsDevice)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = File.OpenRead(dialog.FileName))
                    {
                        return Texture2D.FromStream(graphicsDevice, stream);
                    }
                }
            }
            return null;
        }
    }
}

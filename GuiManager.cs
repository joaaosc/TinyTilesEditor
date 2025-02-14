using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TinyEditor;

namespace TinyEditor
{
    public class GUIManager
    {

        public bool TileTypeEditingMode { get; set; }
        // Indica se estamos no modo de edição
        public bool EditModeActive = false;
        public void SetEditModeActive(bool active)
        {
            EditModeActive = active;
        }


        // A cor selecionada vem do ColorPicker
        public Color SelectedColor => colorPicker.SelectedColor;

        // Área da barra lateral (geralmente à esquerda)
        private Rectangle sidebarRect;

        // Botões de controle (por exemplo, Edit Mode, Save, Load)
        private Rectangle editButtonRect;
        private Rectangle saveButtonRect;
        private Rectangle loadButtonRect;
        private Rectangle addAnimatedSpriteButtonRect = new Rectangle(20, 440, 150, 40);
        private Rectangle removeAnimatedSpriteButtonRect = new Rectangle(20, 490, 150, 40);
        private Rectangle playButtonRect = new Rectangle(40, 540, 150, 40);
        private Rectangle tileTypeToggleButtonRect = new Rectangle(20, 570, 150, 40);


        // O nosso ColorPicker para selecionar cores via sliders RGB
        private ColorPicker colorPicker;

        private Texture2D pixel;
        private SpriteFont font;

        // Eventos para Save/Load (se desejar)
        public event Action OnSaveClicked;
        public event Action OnLoadClicked;
        public event Action OnAddAnimatedSpriteClicked;
        public event Action OnRemoveAnimatedSpriteClicked;
        public event Action OnPlayClicked;
        public event Action OnTileTypeToggleClicked;


        public GUIManager(Texture2D pixel, SpriteFont font, int screenWidth, int screenHeight)
        {
            this.pixel = pixel;
            this.font = font;

            // Define a área da barra lateral (por exemplo, 200px de largura)
            sidebarRect = new Rectangle(0, 0, 200, screenHeight);

            // Botão para alternar o modo de edição
            editButtonRect = new Rectangle(10, 10, 180, 40);

            // Botões para salvar e carregar mapa (exemplo)
            int buttonsY = editButtonRect.Bottom + 20;
            saveButtonRect = new Rectangle(10, buttonsY, 180, 40);
            loadButtonRect = new Rectangle(10, buttonsY + 50, 180, 40);

            // Instancia o ColorPicker abaixo dos botões de Save/Load.
            // Os parâmetros (x, y, largura, altura) podem ser ajustados conforme a necessidade.
            int colorPickerY = loadButtonRect.Bottom + 20;
            colorPicker = new ColorPicker(pixel, font, 10, colorPickerY, 180, 220);
        }

        /// <summary>
        /// Trata os cliques na área da GUI.
        /// </summary>
        public void HandleMouseClick(Point mousePosition)
        {
            if (sidebarRect.Contains(mousePosition))
            {
                if (editButtonRect.Contains(mousePosition))
                {
                    EditModeActive = !EditModeActive;
                }
                else if (saveButtonRect.Contains(mousePosition))
                {
                    OnSaveClicked?.Invoke();
                }
                else if (loadButtonRect.Contains(mousePosition))
                {
                    OnLoadClicked?.Invoke();
                }
                else if (addAnimatedSpriteButtonRect.Contains(mousePosition))
                {
                    OnAddAnimatedSpriteClicked?.Invoke();

                }
                else if (removeAnimatedSpriteButtonRect.Contains(mousePosition))
                {
                    OnRemoveAnimatedSpriteClicked?.Invoke();
                    
                }
                else if (playButtonRect.Contains(mousePosition))
                {
                    OnPlayClicked?.Invoke();
                }
                else if (tileTypeToggleButtonRect.Contains(mousePosition))
                {
                    OnTileTypeToggleClicked?.Invoke();
                    return;
                }
                // Como removemos a paleta fixa, não precisamos tratar cliques para selecionar cores fixas.
                // O ColorPicker já tratará os cliques internos nos seus sliders.
            }
        }

        /// <summary>
        /// Atualiza a GUI, inclusive o ColorPicker.
        /// </summary>
        public void Update()
        {
            // Atualiza o ColorPicker para captar mudanças nos sliders
            colorPicker.Update();
        }

        /// <summary>
        /// Desenha a GUI: os botões e o ColorPicker.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Fundo da área da GUI
            spriteBatch.Draw(pixel, sidebarRect, Color.DarkGray);

            // Botão de "Edit Mode"
            Color editButtonColor = EditModeActive ? Color.LightGreen : Color.LightGray;
            spriteBatch.Draw(pixel, editButtonRect, editButtonColor);
            spriteBatch.DrawString(font, "Edit Mode", new Vector2(editButtonRect.X + 10, editButtonRect.Y + 10), Color.Black);

            // Botões para Save e Load
            spriteBatch.Draw(pixel, saveButtonRect, Color.LightBlue);
            spriteBatch.DrawString(font, "Save Map", new Vector2(saveButtonRect.X + 20, saveButtonRect.Y + 10), Color.Black);

            spriteBatch.Draw(pixel, loadButtonRect, Color.LightBlue);
            spriteBatch.DrawString(font, "Load Map", new Vector2(loadButtonRect.X + 20, loadButtonRect.Y + 10), Color.Black);

            // Botão "Adicionar Sprite Animado"
            Color addSpriteButtonColor = Color.Gray;
            spriteBatch.Draw(pixel, addAnimatedSpriteButtonRect, addSpriteButtonColor);
            spriteBatch.DrawString(font, "Add Sprite", new Vector2(addAnimatedSpriteButtonRect.X + 20, addAnimatedSpriteButtonRect.Y + 10), Color.Black);

            // Botão "Remover Sprite"
            Color removeButtonColor = Color.Gray;
            spriteBatch.Draw(pixel, removeAnimatedSpriteButtonRect, removeButtonColor);
            spriteBatch.DrawString(font, "Remove Sprite",
                new Vector2(removeAnimatedSpriteButtonRect.X + 5, removeAnimatedSpriteButtonRect.Y + 10),
                Color.Black);

            // Exemplo: desenha um retângulo para o botão Play
            Color playButtonColor = Color.Yellow;
            spriteBatch.Draw(pixel, playButtonRect, playButtonColor);
            spriteBatch.DrawString(font, "Play", new Vector2(playButtonRect.X + 10, playButtonRect.Y + 10), Color.Black);

            // Desenha o botão de TileType Editing
            Color buttonColor = Color.Gray;
            spriteBatch.Draw(pixel, tileTypeToggleButtonRect, buttonColor);

            // Exibe o estado atual (vamos supor que você receba esse estado externamente)
            string text = "TileType Edit: " + (TileTypeEditingMode ? "ON" : "OFF");
            spriteBatch.DrawString(font, text, new Vector2(tileTypeToggleButtonRect.X + 5, tileTypeToggleButtonRect.Y + 10), Color.White);


            // Desenha o ColorPicker (que agora é a única forma de selecionar uma cor)
            colorPicker.Draw(spriteBatch);
        }

        /// <summary>
        /// Retorna a largura da área da GUI (para que outros componentes possam evitá-la)
        /// </summary>
        public int SidebarWidth => sidebarRect.Width;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyEditor
{
    public class GUIManager
    {
        // Eventos para notificar quando os botões de salvar ou carregar são clicados
        public event Action OnSaveButtonClicked;
        public event Action OnLoadButtonClicked;

        // Indica se estamos no modo de edição
        public bool EditModeActive { get; private set; } = false;


        // A cor selecionada agora pode vir tanto da paleta quanto do ColorPicker
        private Color selectedColor = Color.Red;
        public Color SelectedColor => selectedColor;

        // área da barra lateral
        private Rectangle sidebarRect;
        // Botões
        private Rectangle editButtonRect;
        private List<Tuple<Rectangle, Color>> colorButtons;
        private Rectangle saveButtonRect;
        private Rectangle loadButtonRect;

        // Integração com o ColorPicker
        private ColorPicker colorPicker;

        private Texture2D pixel;
        private SpriteFont font;

        public GUIManager(Texture2D pixel, SpriteFont font, int screenWidth, int screenHeight)
        {
            this.pixel = pixel;
            this.font = font;

            // Define a área da barra lateral (lado esquerdo)
            sidebarRect = new Rectangle(0, 0, 200, screenHeight);

            // Botão de alternância do modo de edição
            editButtonRect = new Rectangle(10, 10, 180, 40);

            // Configura uma paleta de cores para seleção (ex.: 6 cores)
            colorButtons = new List<Tuple<Rectangle, Color>>();
            int buttonSize = 30;
            int spacing = 10;
            int startX = 10;
            int startY = editButtonRect.Bottom + 20;
            Color[] palette = new Color[]
                { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple, Color.Orange };
            
            for (int i = 0; i < palette.Length; i++)
            {
                //Organiza os botões verticalmente
                Rectangle buttonRectangle =
                    new Rectangle(startX, startY + i * (buttonSize + spacing), buttonSize, buttonSize);
                colorButtons.Add(new Tuple<Rectangle, Color>(buttonRectangle, palette[i]));
            }

            // Botões de salvar e carregar
            int buttonsY = startY + palette.Length * (buttonSize + spacing) + 20;
            saveButtonRect = new Rectangle(10, buttonsY, 180, 40);
            loadButtonRect = new Rectangle(10, buttonsY + 50, 180, 40);

            // Instancia o ColorPicker (abaixo dos botões)
            int colorPickerY = loadButtonRect.Bottom + 20;
            colorPicker = new ColorPicker(pixel, font, 10, colorPickerY, 180, 200);
        }

        /// <summary>
        /// Trata os cliques do mouse na GUI
        /// </summary>
        public void HandleMouseClick(Point mousePosition)
        {
            // Se o clique ocorreu dentro da área da barra lateral
            if (sidebarRect.Contains(mousePosition))
                if (editButtonRect.Contains(mousePosition))
                {
                    EditModeActive = !EditModeActive;
                }
                else
                {
                    //Se o clique ocorreu em algum botão da paleta, atualiza a cor selecionada
                    foreach (var button in colorButtons)
                        if (button.Item1.Contains(mousePosition))
                            selectedColor = button.Item2;

                }
                // Verifica o botão de salvar mapa
                if (saveButtonRect.Contains(mousePosition))
                    OnSaveButtonClicked?.Invoke();
                if (loadButtonRect.Contains(mousePosition))
                    OnLoadButtonClicked?.Invoke();
        }

        /// <summary>
        /// Desenha a GUI
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Fundo da barra lateral
            spriteBatch.Draw(pixel,sidebarRect,Color.DarkGray);

            // Botão de edição - muda de cor conforme o estado
            Color buttonColor = EditModeActive ? Color.LightGreen : Color.LightGray;
            spriteBatch.Draw(pixel,editButtonRect,buttonColor);
            if (font != null)
                spriteBatch.DrawString(font,"Edit  Mode ",new Vector2(editButtonRect.X + 10,editButtonRect.Y + 10),Color.Black);

            // Desenha cada botão de cor
            foreach (var button in colorButtons)
            {
                spriteBatch.Draw(pixel,button.Item1,button.Item2);
                // Se este botão representa a cor selecionada, desenha uma borda
                if (button.Item2 == SelectedColor)
                {
                    DrawBorder(spriteBatch, button.Item1, Color.White, 2);
                }
            }
            // Botões de salvar e carregar
            spriteBatch.Draw(pixel, saveButtonRect, Color.LightGray);
            spriteBatch.DrawString(font,"Save Map", new Vector2(saveButtonRect.X + 20,saveButtonRect.Y + 10),Color.Black);

            spriteBatch.Draw(pixel, loadButtonRect, Color.LightGray);
            spriteBatch.DrawString(font, "Load Map", new Vector2(loadButtonRect.X + 20, loadButtonRect.Y + 10),
                Color.Black);

            // Desenha o ColorPicker
            colorPicker.Draw(spriteBatch);

        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            // Desenha uma borda ao redor do retângulo
            // Borda superior
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Borda inferior
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Borda esquerda
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Borda direita
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        ///<summary>
        /// Retorna a largura da barra lateral, para que possamos saber qual área é reservada à GUI.
        /// </summary>
        public int SidebarWidth => sidebarRect.Width;

        /// <summary>
        /// Atualiza a GUI a cada frame. O ColorPicker também precisa ser atualizado,
        /// mesmo se o clique não estiver no exato momento, pois arrastar o slider muda a cor
        /// </summary>
        public void Update()
        {
            colorPicker.Update();
            // Se o usuário arrastar o slider, a cor resultante muda
            selectedColor = colorPicker.SelectedColor;
        }
    }
}

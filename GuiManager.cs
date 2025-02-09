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

        // Cor atualmente selecionado para edição
        public Color SelectedColor { get; private set; } = Color.Red;

        // área da barra lateral
        private Rectangle sidebarRect;
        // Botões
        private Rectangle editButtonRect;
        private List<Tuple<Rectangle, Color>> colorButtons;
        private Rectangle saveButtonRect;
        private Rectangle loadButtonRect;

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
                            SelectedColor = button.Item2;

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
                    int thickness = 2;
                    // Desenha bordas
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X,button.Item1.Y,button.Item1.Width,thickness),Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X, button.Item1.Y + button.Item1.Height - thickness, button.Item1.Width, thickness), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X, button.Item1.Y, thickness, button.Item1.Height), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X + button.Item1.Width - thickness, button.Item1.Y, thickness, button.Item1.Height), Color.White);
                }
            }
            // Botões de salvar e carregar
            spriteBatch.Draw(pixel, saveButtonRect, Color.LightGray);
            spriteBatch.DrawString(font,"Save Map", new Vector2(saveButtonRect.X + 20,saveButtonRect.Y + 10),Color.Black);

            spriteBatch.Draw(pixel, loadButtonRect, Color.LightGray);
            spriteBatch.DrawString(font, "Load Map", new Vector2(loadButtonRect.X + 20, loadButtonRect.Y + 10),
                Color.Black);

        }

        ///<summary>
        /// Retorna a largura da barra lateral, para que possamos saber qual área é reservada à GUI.
        /// </summary>
        public int SidebarWidth => sidebarRect.Width;
    }
}

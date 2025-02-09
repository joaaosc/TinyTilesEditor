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
        // Indica se estamos no modo de edição
        public bool EditModeActive { get; private set; } = false;

        // Cor atualmente selecionado para edição
        public Color SelectedColor { get; private set; } = Color.Red;

        // Define a área da barra lateral (ex.: 200px de largura)
        private Rectangle sidebarRect;
        // Botão para alterar o modo de edição
        private Rectangle editButtonRect;
        // Lista de botões para seleção de cores (cada par: retângulo e a cor)
        private List<Tuple<Rectangle, Color>> colorButtons;

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
        }

        /// <summary>
        /// Deve ser chamado quando ocorrer um clique do mouse (apenas uma vez por clique)
        /// </summary>
        public void HandleMouseClick(Point mousePosition)
        {
            // Se o clique ocorreu dentro da área da barra lateral
            if (sidebarRect.Contains(mousePosition))
                if (editButtonRect.Contains(mousePosition))
                    EditModeActive = !EditModeActive;
            //Se o clique ocorreu em algum botão da paleta, atualiza a cor selecionada
            foreach (var button in colorButtons)
                if (button.Item1.Contains(mousePosition))
                    SelectedColor = button.Item2;
        }

        /// <summary>
        /// Desenha a interface gráfica (barra lateral, botão e paleta
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
                    // borda superior
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X,button.Item1.Y,button.Item1.Width,thickness),Color.White);
                    // Borda inferior
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X, button.Item1.Y + button.Item1.Height - thickness, button.Item1.Width, thickness), Color.White);
                    // Borda esquerda
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X, button.Item1.Y, thickness, button.Item1.Height), Color.White);
                    // Borda direita
                    spriteBatch.Draw(pixel, new Rectangle(button.Item1.X + button.Item1.Width - thickness, button.Item1.Y, thickness, button.Item1.Height), Color.White);

                }

            }

        }

        ///<summary>
        /// Retorna a largura da barra lateral, para que possamos saber qual área é reservada à GUI.
        /// </summary>
        public int SidebarWidth => sidebarRect.Width;
    }
}

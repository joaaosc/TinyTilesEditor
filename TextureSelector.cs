using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyEditor
{
    public class TextureSelector
    {
        public Texture2D[] AvailableTextures { get; private set; }
        public int SelectedIndex { get; private set; } = -1;

        // Área onde as texturas serão exibidas na GUI (por exemplo, no lado direito da janela)
        public Rectangle SelectorArea { get; private set; }
        public int TileDisplaySize { get; private set; }  // Ex: 64 (ou outro valor desejado para a exibição)

        private Texture2D pixel;

        public TextureSelector(Texture2D[] availableTextures, Rectangle selectorArea, int tileDisplaySize, Texture2D pixel)
        {
            AvailableTextures = availableTextures;
            SelectorArea = selectorArea;
            TileDisplaySize = tileDisplaySize;
            this.pixel = pixel;
        }

        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            // Se o mouse estiver dentro da área do seletor e o botão esquerdo for pressionado...
            if (mouseState.LeftButton == ButtonState.Pressed && SelectorArea.Contains(mousePos))
            {
                int spacing = 5;
                int currentX = SelectorArea.X + spacing;
                for (int i = 0; i < AvailableTextures.Length; i++)
                {
                    Rectangle texRect = new Rectangle(currentX, SelectorArea.Y + spacing, TileDisplaySize, TileDisplaySize);
                    if (texRect.Contains(mousePos))
                    {
                        SelectedIndex = i;
                        break;
                    }
                    currentX += TileDisplaySize + spacing;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Fundo da área do seletor
            spriteBatch.Draw(pixel, SelectorArea, Color.DarkSlateGray);

            int spacing = 5;
            int currentX = SelectorArea.X + spacing;
            for (int i = 0; i < AvailableTextures.Length; i++)
            {
                Rectangle texRect = new Rectangle(currentX, SelectorArea.Y + spacing, TileDisplaySize, TileDisplaySize);
                spriteBatch.Draw(AvailableTextures[i], texRect, Color.White);

                // Se este tile estiver selecionado, desenha uma borda amarela
                if (i == SelectedIndex)
                {
                    int borderThickness = 2;
                    spriteBatch.Draw(pixel, new Rectangle(texRect.X, texRect.Y, texRect.Width, borderThickness), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(texRect.X, texRect.Bottom - borderThickness, texRect.Width, borderThickness), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(texRect.X, texRect.Y, borderThickness, texRect.Height), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(texRect.Right - borderThickness, texRect.Y, borderThickness, texRect.Height), Color.Yellow);
                }

                currentX += TileDisplaySize + spacing;
            }
        }

        // Retorna a textura selecionada (ou null se nenhuma for selecionada)
        public Texture2D GetSelectedTexture()
        {
            if (SelectedIndex >= 0 && SelectedIndex < AvailableTextures.Length)
                return AvailableTextures[SelectedIndex];
            return null;
        }
    }
}

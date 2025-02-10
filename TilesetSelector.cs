using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TinyEditor
{
    /// <summary>
    /// Classe responsável por carregar o tileset, dividir o tileset em subretângulos com o tamanho
    /// de cada tile, exibir os tiles numa grade na barra lateral direito, e permitir que o usuário selecione
    /// um tile (armazenando o índice ou retângulo fonte selecionado)
    /// </summary>
    public class TilesetSelector
    {
        private Texture2D tilesetTexture;
        private int tileWidth, tileHeight;
        private List<Rectangle> tileSourceRectangles;
        private Rectangle selectorArea; // área onde o tileset será desenhado na GUI
        private Texture2D pixel;
        private SpriteFont font;

        public int SelectedIndex { get; private set; } = -1;
        public Rectangle? SelectedTileSourceRectangle { get; private set; } = null;
        public Texture2D TilesetTexture => tilesetTexture;

        public TilesetSelector(Texture2D tilesetTexture, int tileWidth, int tileHeight, Rectangle selectorArea,
            Texture2D pixel, SpriteFont font)
        {
            this.tilesetTexture = tilesetTexture;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.selectorArea = selectorArea;
            this.pixel = pixel;
            this.font = font;

            tileSourceRectangles = new List<Rectangle>();
            int columns = tilesetTexture.Width / tileWidth;
            int rows = tilesetTexture.Height / tileHeight;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Rectangle src = new Rectangle(col * tileWidth, row * tileHeight, tileWidth, tileHeight);
                    tileSourceRectangles.Add(src);
                }
            }
        }

        /// <summary>
        ///  Atualiza a seleção dos tiles, verificando se o usuário clicou dentro da área do seletor.
        /// </summary>
        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            Point mousePoint = new Point(mouseState.X, mouseState.Y);
            if (selectorArea.Contains(mousePoint) && mouseState.LeftButton == ButtonState.Pressed)
            {
                // Defina aqui quantas colunas serão exibidas. Por exemplo, 4.
                int columnsDisplay = 4;
                int spacing = 5; // espaçamento entre tiles na exibição
                int displayTileWidth = tileWidth; // tamanho de exibição (pode ser escalado)
                int displayTileHeight = tileHeight;

                int startX = selectorArea.X + spacing;
                int startY = selectorArea.Y + spacing;
                int x = mousePoint.X - startX;
                int y = mousePoint.Y - startY;
                if (x >= 0 && y >= 0)
                {
                    int col = x / (displayTileWidth + spacing);
                    int row = y / (displayTileHeight + spacing);
                    int index = row * columnsDisplay + col;
                    if (index >= 0 && index < tileSourceRectangles.Count)
                    {
                        SelectedIndex = index;
                        SelectedTileSourceRectangle = tileSourceRectangles[index];
                    }
                }
            }

        }

        /// <summary>
        /// Desenha a área do tileset na GUI (barra lateral direita).
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Fundo da área do selector
            spriteBatch.Draw(pixel, selectorArea, Color.DimGray);

            int spacing = 5;
            int displayTileWidth = tileWidth;
            int displayTileHeight = tileHeight;
            int columnsDisplay = 4;
            int startX = selectorArea.X + spacing;
            int startY = selectorArea.Y + spacing;

            for (int i = 0; i < tileSourceRectangles.Count; i++)
            {
                int col = i % columnsDisplay;
                int row = i / columnsDisplay;
                Rectangle destRect = new Rectangle(startX + col * (displayTileWidth + spacing), startY + row * (displayTileHeight + spacing), displayTileWidth, displayTileHeight);
                spriteBatch.Draw(tilesetTexture, destRect, tileSourceRectangles[i], Color.White);

                // Se este tile estiver selecionado, desenha uma borda de destaque.
                if (i == SelectedIndex)
                {
                    int thickness = 2;
                    spriteBatch.Draw(pixel, new Rectangle(destRect.X, destRect.Y, destRect.Width, thickness), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(destRect.X, destRect.Bottom - thickness, destRect.Width, thickness), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(destRect.X, destRect.Y, thickness, destRect.Height), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle(destRect.Right - thickness, destRect.Y, thickness, destRect.Height), Color.Yellow);
                }
            }
        }
    }
}

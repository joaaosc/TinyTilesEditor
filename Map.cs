using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyEditor
{
    public class Map
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TileSize { get; set; }
        public Tile[,] Tiles { get; set; }

        // Índices do tile selecionado (-1 significa que nenhum tile está selecionado)
        public int SelectedRow { get; private set; } = -1;
        public int SelectedColumn { get; private set; } = -1;

        public Map(int rows, int columns, int tileSize)
        {
            Rows = rows;
            Columns = columns;
            TileSize = tileSize;
            Tiles = new Tile[rows, columns];
        }

        /// <summary>
        /// Atualiza o tile selecionado com base na posição do mouse.
        /// </summary>
        public void UpdateSelectedTile(Point mousePosition)
        {
            int col = mousePosition.X / TileSize;
            int row = mousePosition.Y / TileSize;

            if (row >= 0 && row < Rows && col >= 0 && col < Columns)
            {
                SelectedRow = row;
                SelectedColumn = col;
            }
            else
            {
                SelectedRow = -1;
                SelectedColumn = -1;
            }
        }

        /// <summary>
        /// Desenha todos os tiles do mapa. Se um tile estiver selecionado, desenha uma borda para destacar.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int row = 0; row < Rows; row++)
            for (int col = 0; col < Columns; col++)
                if (Tiles[row, col] != null)
                {
                    Tiles[row, col].Draw(spriteBatch, pixel);
                    // Se este tile está selecionado, desenha um efeito de seleção (borda)
                    if (row == SelectedRow && col == SelectedColumn)
                    {
                        int thickness = 3;
                        Color borderColor = Color.Yellow;
                        Rectangle tileRect = Tiles[row, col].DestinationRectangle;

                        // Borda superior
                        spriteBatch.Draw(pixel, new Rectangle(tileRect.X, tileRect.Y, tileRect.Width, thickness), borderColor);
                        // Borda inferior
                        spriteBatch.Draw(pixel, new Rectangle(tileRect.X, tileRect.Y + tileRect.Height - thickness, tileRect.Width, thickness), borderColor);
                        // Borda esquerda
                        spriteBatch.Draw(pixel, new Rectangle(tileRect.X, tileRect.Y, thickness, tileRect.Height), borderColor);
                        // Borda direita
                        spriteBatch.Draw(pixel, new Rectangle(tileRect.X + tileRect.Width - thickness, tileRect.Y, thickness, tileRect.Height), borderColor);
                    }
                }

        }
    }
}

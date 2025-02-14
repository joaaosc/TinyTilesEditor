using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TinyAnimation;
using static TinyEditor.Tile;

namespace TinyEditor
{
    public class Map
    {
        public List<AnimatedSprite> AnimatedSprites { get; set; } = new List<AnimatedSprite>();
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

            // Inicializa a matriz de tiles
            Tiles = new Tile[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Rectangle tileRect = new Rectangle(c * TileSize, r * TileSize, TileSize, TileSize);
                    Tiles[r, c] = new Tile(tileRect, 0, false)
                    {
                        TextureID = "defaultTile.png"
                    };
                }
            }
        }

        /// <summary>
        /// Extrai os dados dos tiles em um array jagged para serialização.
        /// </summary>
        public TileData[][] GetTileData()
        {
            TileData[][] tileData = new TileData[Rows][];
            for (int r = 0; r < Rows; r++)
            {
                tileData[r] = new TileData[Columns];
                for (int c = 0; c < Columns; c++)
                {
                    tileData[r][c] = new TileData
                    {
                        TextureID = Tiles[r, c].TextureID,
                        TileType = Tiles[r, c].TileType
                    };
                }
            }
            return tileData;
        }

        /// <summary>
        /// Restaura os dados dos tiles a partir do array jagged.
        /// </summary>
        public void SetTileData(TileData[][] tileData)
        {
            if (tileData == null)
                return;

            if (tileData.Length != Rows)
                throw new Exception("Número de linhas incompatível com o mapa atual.");

            for (int r = 0; r < Rows; r++)
            {
                if (tileData[r].Length != Columns)
                    throw new Exception("Número de colunas incompatível com o mapa atual.");

                for (int c = 0; c < Columns; c++)
                {
                    Tiles[r, c].TextureID = tileData[r][c].TextureID;
                    Tiles[r, c].TileType = tileData[r][c].TileType;
                }
            }
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
            {
                for (int col = 0; col < Columns; col++)
                {
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
                            spriteBatch.Draw(pixel, new Rectangle(tileRect.X, tileRect.Y, tileRect.Width, thickness),
                                borderColor);
                            // Borda inferior
                            spriteBatch.Draw(pixel,
                                new Rectangle(tileRect.X, tileRect.Y + tileRect.Height - thickness, tileRect.Width,
                                    thickness), borderColor);
                            // Borda esquerda
                            spriteBatch.Draw(pixel, new Rectangle(tileRect.X, tileRect.Y, thickness, tileRect.Height),
                                borderColor);
                            // Borda direita
                            spriteBatch.Draw(pixel,
                                new Rectangle(tileRect.X + tileRect.Width - thickness, tileRect.Y, thickness,
                                    tileRect.Height), borderColor);
                            ///////////
                            ///
                            switch (Tiles[row, col].TileType)
                            {
                                case 0:
                                    borderColor = Color.DarkOrange;
                                    break;
                                case 1:
                                    borderColor = Color.Green;
                                    break;
                                case 2:
                                    borderColor = Color.Yellow;
                                    break;
                                default:
                                    borderColor = Color.White;
                                    break;
                            }

                            // Desenha a borda
                            Rectangle rect = Tiles[row, col].DestinationRectangle;
                            // Borda superior
                            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), borderColor);
                            // Borda inferior
                            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), borderColor);
                            // Borda esquerda
                            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), borderColor);
                            // Borda direita
                            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), borderColor);


                        }
                    }
                }
            }
        }
    }
}

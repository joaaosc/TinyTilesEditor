using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TinyEditor
{
    public static class MapGenerator
    {
        /// <summary>
        /// Gera um mapa padrão com bordas do tipo água e interior de grama
        /// </summary>
        /// <param name="map"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public static Map GenerateWaterBorderMap(int rows, int columns,int tileSize)
        {
            Map map = new Map(rows, columns, tileSize);
            for (int row = 0; row < map.Rows; row++)
            {
                for (int col = 0; col < map.Columns; col++)
                {
                    Rectangle destRect =
                        new Rectangle(col * map.TileSize, row * map.TileSize, map.TileSize, map.TileSize);

                    if (row == 0 || col == 0 || row == map.Rows - 1 || col == map.Columns - 1)
                    {
                        // Tile de água (azul)
                        map.Tiles[row, col] = new Tile(Color.Blue, destRect, TileType.Water, false);
                    }
                    else
                    {
                        // Tile de grama (verde)
                        map.Tiles[row, col] = new Tile(Color.Green, destRect, TileType.Grass, true);
                    }
                }
            }

            return map;
        }
    }
}

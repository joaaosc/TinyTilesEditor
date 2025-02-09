using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyEditor
{

    public enum TileType
    {
        Grass,
        Water,
        Dirt,
    }

    public class Tile
    {
        public TileType Type { get; set; }
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Retângulo que define a posição e tamanho do tile na tela.
        /// </summary>
        public Rectangle DestinationRectangle { get; set; }
        public bool IsWalkable { get; set; }

        /// <summary>
        /// Cor base do tile. Futuramente, essa cor poderá ser substituída por uma textura.
        /// </summary>
        public Color Color { get; set; }


        public Tile(Color color, Rectangle destinationRectangle, TileType type, bool isWalkable)
        {
            Color = color;
            DestinationRectangle = destinationRectangle;
            Type = type;
            IsWalkable = isWalkable;
        }

        /// <summary>
        /// Desenha o tile utilizando a textura de 1x1 pixel.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.Draw(pixel,DestinationRectangle,Color);
        }

    }
}

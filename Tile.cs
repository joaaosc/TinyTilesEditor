using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

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

        public string TileType { get; set; }
        public enum TileTypeEnum
        {
            Passable,
            Impassable,
            Special
        }

        [JsonIgnore] // Não serializa o objeto Texture2D
        public Texture2D Texture { get; set; }

        // Propriedade para salvar o identificador da textura
        public string TextureID { get; set; }

        public Rectangle DestinationRectangle { get; set; }
        public bool IsWalkable { get; set; }

        // DTO para armazenar os dados de um tile
        public class TileData
        {
            public string TextureID { get; set; }
            public string TileType { get; set; }
        }

        public Tile(Rectangle destinationRectangle, TileType type, bool isWalkable)
        {
            DestinationRectangle = destinationRectangle;
            TileType = "Passable";
            IsWalkable = isWalkable;
            Texture = null;
            TextureID = "grass.png";
        }


        /// <summary>
        /// Desenha o tile utilizando a textura aplicada, se houver.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (Texture != null)
                spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
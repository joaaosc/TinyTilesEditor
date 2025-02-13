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

        public TileType Type { get; set; }

        [JsonIgnore] // Não serializa o objeto Texture2D
        public Texture2D Texture { get; set; }

        // Propriedade para salvar o identificador da textura
        public string TextureID { get; set; }

        public Rectangle DestinationRectangle { get; set; }
        public bool IsWalkable { get; set; }
        public int TileType { get; set; }

        // DTO para armazenar os dados de um tile
        public class TileData
        {
            public string TextureID { get; set; }
            public int TileType { get; set; }
        }

        public Tile(Rectangle destinationRectangle, TileType type, bool isWalkable)
        {
            DestinationRectangle = destinationRectangle;
            Type = type;
            IsWalkable = isWalkable;
            Texture = null;
            TextureID = null;
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
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using IronPython.Runtime;
using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using TinyAnimation;
using static TinyEditor.Tile;

namespace TinyEditor
{
    public class MapManager
    {
        private Dictionary<string, Map> maps = new Dictionary<string, Map>();

        // DTO para armazenar os dados de um sprite animado
        public class AnimatedSpriteData
        {
            public string TextureID { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public int FrameCount { get; set; }
            public float FrameTime { get; set; }
        }

        // DTO para armazenar os dados do mapa (incluindo os tiles e os sprites animados)
        public class MapData
        {
            public int Rows { get; set; }
            public int Columns { get; set; }
            public int TileSize { get; set; }
            public TileData[][] Tiles { get; set; }
            public List<AnimatedSpriteData> AnimatedSprites { get; set; } = new List<AnimatedSpriteData>();
        }

        public void AddMap(string mapName, Map map)
        {
            maps[mapName] = map;
        }

        /// <summary>
        /// Salva o mapa especificado em um arquivo JSON, incluindo os tiles e os sprites animados.
        /// </summary>
        public void SaveMap(Map map, string filePath)
        {
            // Cria o DTO e extrai os dados do mapa
            MapData mapData = new MapData
            {
                Rows = map.Rows,
                Columns = map.Columns,
                TileSize = map.TileSize,
                Tiles = map.GetTileData()
            };

            // Extrai os dados dos sprites animados
            if (map.AnimatedSprites != null)
            {
                foreach (var sprite in map.AnimatedSprites)
                {
                    AnimatedSpriteData spriteData = new AnimatedSpriteData()
                    {
                        TextureID = sprite.TextureID,
                        PositionX = sprite.Position.X,
                        PositionY = sprite.Position.Y,
                        FrameWidth = sprite.FrameWidth,
                        FrameHeight = sprite.FrameHeight,
                        FrameCount = sprite.FrameCount,
                        FrameTime = sprite.FrameTime
                    };
                    mapData.AnimatedSprites.Add(spriteData);
                }
            }

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(mapData, Formatting.Indented, settings);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Carrega um mapa a partir de um arquivo JSON.
        /// Retorna o mapa carregado, ou null se falhar.
        /// </summary>
        public Map LoadMap(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                MapData mapData = JsonConvert.DeserializeObject<MapData>(json);

                // Cria um novo objeto Map usando as dimensões salvas
                Map map = new Map(mapData.Rows, mapData.Columns, mapData.TileSize);

                // Restaura os dados dos tiles, se houver
                if (mapData.Tiles != null)
                {
                    map.SetTileData(mapData.Tiles);
                }

                // Recria os sprites animados
                if (mapData.AnimatedSprites != null)
                {
                    map.AnimatedSprites = new List<AnimatedSprite>();

                    foreach (var spriteData in mapData.AnimatedSprites)
                    {
                        // Tenta carregar a textura (se não existir, o TextureLoader solicitará ao usuário)
                        var texture = Game1.TextureLoader.Load(spriteData.TextureID);

                        // Cria o sprite usando os dados carregados
                        AnimatedSprite sprite = new AnimatedSprite(
                            texture,
                            new Vector2(spriteData.PositionX, spriteData.PositionY),
                            spriteData.FrameWidth,
                            spriteData.FrameHeight,
                            spriteData.FrameCount,
                            spriteData.FrameTime);
                        sprite.TextureID = spriteData.TextureID;

                        map.AnimatedSprites.Add(sprite);
                    }
                }

                return map;
            }
            return null;
        }


    }
}

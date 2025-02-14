using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using IronPython.Runtime;
using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using TinyAnimation;
using static TinyEditor.Tile;
using static TinyEditor.Game1;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace TinyEditor
{
    public class MapManager
    {
        // Continuação do dicionário de Mapas pelo nome, se usado em algum lugar
        private Dictionary<string, Map> maps = new Dictionary<string, Map>();

        // Dicionário para associar cada Map ao seu filePath (caminho de arquivo)
        private Dictionary<Map, string> mapPaths = new Dictionary<Map, string>();


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

            // Associa o objeto Map ao filePath no dicionário
            mapPaths[map] = filePath;
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
                // Cria o mapa usando as dimensões salvas
                Map map = new Map(mapData.Rows, mapData.Columns, mapData.TileSize);

                // Restaura os dados dos tiles
                if (mapData.Tiles != null)
                {
                    map.SetTileData(mapData.Tiles);

                    // Define a pasta onde estão as texturas
                    string texturesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "textures");
                    // Para cada tile, procura a textura com o mesmo nome
                    for (int r = 0; r < map.Rows; r++)
                    {
                        for (int c = 0; c < map.Columns; c++)
                        {
                            var tile = map.Tiles[r, c];
                            if (!string.IsNullOrEmpty(tile.TextureID))
                            {
                                // Constrói o caminho completo para a textura
                                string texturePath = Path.Combine(texturesFolder, tile.TextureID);
                                if (File.Exists(texturePath))
                                {
                                    // Carrega a textura e atribui ao tile
                                    tile.Texture = TextureLoader.Load(texturePath);
                                }
                                else
                                {
                                    // Opcional: use uma textura padrão ou avise o usuário
                                    // tile.Texture = TextureLoader.Load(Path.Combine(texturesFolder, "defaultTile.png"));
                                }
                            }
                        }
                    }
                }

                // Recria os sprites animados
                if (mapData.AnimatedSprites != null)
                {
                    map.AnimatedSprites = new List<AnimatedSprite>();
                    string texturesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "textures");
                    foreach (var spriteData in mapData.AnimatedSprites)
                    {
                        // Constrói o caminho completo para a textura do sprite
                        string texturePath = Path.Combine(texturesFolder, spriteData.TextureID);
                        if (File.Exists(texturePath))
                        {
                            Texture2D texture = TextureLoader.Load(texturePath);
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
                        else
                        {
                            // Opcional: avise que a textura do sprite não foi encontrada
                        }
                    }
                }

                // Associa este Map ao filePath
                mapPaths[map] = filePath;

                return map;
            }
            return null;
        }

        /// <summary>
        /// Retorna o filePath associado ao objeto Map, ou null se não estiver registrado.
        /// </summary>
        public string GetMapFilePath(Map map)
        {
            if (mapPaths.TryGetValue(map, out string path))
                return path;
            return null;
        }
    }
}

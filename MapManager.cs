using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace TinyEditor
{
    public class MapManager
    {
        private Dictionary<string,Map> maps = new Dictionary<string,Map>();

        ///<summary>
        /// Adiciona ou substitui um mapa na coleção
        /// </summary>

        public void AddMap(string mapName, Map map)
        {
            maps[mapName] = map;
        }

        /// <summary>
        /// Retorna o mapa associado ao nome, ou null se não existir.
        /// </summary>
        public Map GetMap(string mapName)
        {
            return maps.ContainsKey(mapName) ? maps[mapName] : null;
        }

        /// <summary>
        /// Salva o mapa especificado em um arquivo JSON.
        /// </summary>
        public void SaveMap(string mapName, string filePath)
        {
            if (maps.ContainsKey(mapName))
            {
                string json = JsonConvert.SerializeObject(maps[mapName], Formatting.Indented);
                File.WriteAllText(filePath,json);
            }
        }

        /// <summary>
        /// Carrega um mapa a partir de um arquivo JSON e o adiciona à coleção.
        /// </summary>
        public void LoadMap(string mapName, string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Map map = JsonConvert.DeserializeObject<Map>(json);
                maps[mapName] = map;
            }
        }


    }
}

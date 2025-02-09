using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using IronPython.Runtime;


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
        public void SaveMap(Map map, string filePath)
        {
            string json = JsonConvert.SerializeObject(map, Formatting.Indented);
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
                Map map = JsonConvert.DeserializeObject<Map>(json);
                return map;
            }
            return null;
        }


    }
}

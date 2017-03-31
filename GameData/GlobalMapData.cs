using System.Collections.Generic;

namespace GameData
{
    public class GlobalMapData
    {
        private readonly IList<MapData> _maps = new List<MapData>();

        public void AddMap(MapData map)
        {
            _maps.Add(map);
        }

        public IEnumerable<MapData> GetMaps()
        {
            return _maps;
        }
    }
}

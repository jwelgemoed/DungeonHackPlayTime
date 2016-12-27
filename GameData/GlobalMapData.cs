using System.Collections.Generic;

namespace GameData
{
    public class GlobalMapData
    {
        private readonly List<MapData> _data;
        
        public GlobalMapData()
        {
            _data = new List<GameData.MapData>();
        }

        public void AddMapData(MapData mapData)
        {
            _data.Add(mapData);
        }

        public IEnumerable<MapData> GetMapData()
        {
            return _data;
        }
    }
}

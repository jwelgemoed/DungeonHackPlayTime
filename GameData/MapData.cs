using FunAndGamesWithSlimDX.Entities;
using SlimDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;

namespace GameData
{
    // File Layout:
    // [0] - Starting position of texture entries
    // [1] - Number of texture entries
    // [2] - Starting position of linesegments
    // [3] - Number of line segmentss
    // [4] - Starting position of things
    // [5] - Number of things
    // [6] - Starting position of sectors
    // [7] - Number of sectors
    // [8] - Future use
    // [9] - Future use
    // [10] - Future use
    // [11] - Future use
    // [12] - Future use
    // [13] - Future use
    // [14] - Future use
    // [15] - Future use
    // [16] - Future use
    public class MapData
    {
        public int NumberOfTextureEntries
        {
            get
            {
                return _textureData.Count();
            }
        }

        public int NumberOfLineSegments
        {
            get
            {
                if (_lineSegments == null)
                    return 0;

                return _lineSegments.Count;
            }
        }

        public int NumberOfThings
        {
            get
            {
                if (_things == null)
                    return 0;

                return _things.Count;
            }
        }

        public int NumberOfSectors
        {
            get
            {
                if (_sectors == null)
                    return 0;

                return _sectors.Count;
            }
        }

        public Dictionary<int, LineSegment> LineSegments
        {
            get
            {
                return _lineSegments;
            }            
        }

        public Dictionary<int, string> TextureData
        {
            get
            {
                return _textureData;
            }
        }

        public Dictionary<int, Thing> Things
        {
            get
            {
                return _things;
            }
        }

        public Dictionary<int, Sector> Sectors
        {
            get
            {
                return _sectors;
            }
        }

        private Dictionary<int, LineSegment> _lineSegments;

        private Dictionary<int, string> _textureData;

        private Dictionary<int, Thing> _things;

        private Dictionary<int, Sector> _sectors;

        public MapData()
        {
            _textureData = new Dictionary<int, string>();
            _lineSegments = new Dictionary<int, LineSegment>();
            _things = new Dictionary<int, Thing>();
            _sectors = new Dictionary<int, Sector>();
        }

        public void AddTextureData(string texture)
        {
            if (_textureData.ContainsValue(texture))
                return;

            _textureData.Add(NumberOfTextureEntries, texture);
        }

        public int GetTextureId(string texture)
        {
            if (_textureData.ContainsValue(texture))
            {
                return _textureData.FirstOrDefault(x => x.Value == texture).Key;
            }

            return -1;
        }

        public void AddTextureData(int id, string texture)
        {
            _textureData.Add(id, texture);
        }

        public void AddLineSegment(LineSegment lineSegment)
        {
            _lineSegments.Add(lineSegment.Id, lineSegment);
        }

        public void AddThing(Thing thing)
        {
            _things.Add(thing.Id, thing);
        }

        public void AddSector(Sector sector)
        {
            _sectors.Add(sector.Id, sector);
        }

        public void UpdateLineSegment(LineSegment lineSegment)
        {
            _lineSegments[lineSegment.Id] = lineSegment;
        }

        public void UpdateStartPosition(Vertex startPosition)
        {
            foreach (var line in _lineSegments.Values)
            {
                line.Start = new Vertex()
                {
                    X = line.Start.X + startPosition.X,
                    Y = line.Start.Y + startPosition.Y
                };

                line.End = new Vertex()
                {
                    X = line.End.X + startPosition.X,
                    Y = line.End.Y + startPosition.Y
                };
            }
        }

        public void AddMapData(MapData mapData)
        {
            //Add the textures
            foreach (var textures in mapData.TextureData)
            {
                AddTextureData(textures.Value);
            }

            //Add the line segments
            foreach (var lineSegment in mapData.LineSegments.Values)
            {
                int newTextureId = GetTextureId(mapData.TextureData[lineSegment.TextureId == -1 ? 0 : lineSegment.TextureId]);

                AddLineSegment(new LineSegment()
                {
                    Start = lineSegment.Start,
                    End = lineSegment.End,
                    TextureId = newTextureId,
                    Id = NumberOfLineSegments,
                    IsSolid = lineSegment.IsSolid
                });

                foreach (var sector in mapData.Sectors.Values)
                {
                    for (int i=0; i<sector.SideDefinitions.Count; i++)
                    {
                        if (sector.SideDefinitions[i] == lineSegment.Id)
                        {
                            sector.SideDefinitions[i] = NumberOfLineSegments;
                        }
                    }
                }
            }

            //Add things
            foreach (var thing in mapData.Things.Values)
            {
                AddThing(new Thing()
                {
                    Name = thing.Name,
                    Position = thing.Position,
                    Id = NumberOfThings
                });
            }

            //Add sectors
            foreach (var sector in mapData.Sectors.Values)
            {

                AddSector(new Sector()
                {
                    CeilingTextureId = sector.CeilingTextureId,
                    FloorTextureId = sector.FloorTextureId,
                    Effect = sector.Effect,
                    SideDefinitions = sector.SideDefinitions,
                    Id = NumberOfSectors
                });
            }
        }

        public MapData CreateCopy()
        {
            MapData mapData = new MapData();

            foreach (var texture in _textureData)
            {
                mapData.TextureData.Add(texture.Key, texture.Value);
            }

            foreach (var thing in _things)
            {
                mapData.Things.Add(thing.Key,
                    new Thing
                    {
                        Id = thing.Value.Id,
                        Name = thing.Value.Name,
                        Position = thing.Value.Position,
                        Type = thing.Value.Type
                    });
            }

            foreach (var lineSegment in _lineSegments)
            {
                mapData.LineSegments.Add(lineSegment.Key,
                    new LineSegment
                    {
                        Id = lineSegment.Value.Id,
                        CeilingHeight = lineSegment.Value.CeilingHeight,
                        FloorHeight = lineSegment.Value.FloorHeight,
                        TextureId = lineSegment.Value.TextureId,
                        Start = lineSegment.Value.Start,
                        End = lineSegment.Value.End,
                        IsSolid = lineSegment.Value.IsSolid
                    });
            }

            foreach (var sector in _sectors)
            {
                var newsector = 
                    new Sector
                    {
                        CeilingHeight = sector.Value.CeilingHeight,
                        CeilingTextureId = sector.Value.CeilingTextureId,
                        Effect = sector.Value.Effect,
                        FloorHeight = sector.Value.FloorHeight,
                        FloorTextureId = sector.Value.FloorTextureId,
                        Id = sector.Value.Id,
                        XPlacement = sector.Value.XPlacement,
                        YPlacement = sector.Value.YPlacement,
                        SideDefinitions = new List<int>()
                    };

                foreach (var sideDef in sector.Value.SideDefinitions)
                {
                    newsector.SideDefinitions.Add(sideDef);
                }

                mapData.Sectors.Add(sector.Key, newsector);
            }

            return mapData;
        }
    }
}

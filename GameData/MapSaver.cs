using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameData
{
    public class MapSaver
    {
        private MapData _mapData;

        public MapSaver(MapData mapData)
        {
            _mapData = mapData;
        }

        public void SaveData(string fileName)
        {
            int startPositionTextures = 17;
            int numTextureEntries = _mapData.NumberOfTextureEntries;
            int numLineSegments = _mapData.NumberOfLineSegments;
            int startPositionLinesegs = startPositionTextures + numTextureEntries;
            int startPositionThings = startPositionLinesegs + numLineSegments;
            int numThings = _mapData.NumberOfThings;
            int startPositionSectors = startPositionThings + numThings;
            int numSectors = _mapData.NumberOfSectors;

            using (var file = new StreamWriter(fileName))
            {
                file.WriteLine(startPositionTextures);
                file.WriteLine(numTextureEntries);
                file.WriteLine(startPositionLinesegs);
                file.WriteLine(numLineSegments);
                file.WriteLine(startPositionThings);
                file.WriteLine(numThings);
                file.WriteLine(startPositionSectors);
                file.WriteLine(numSectors);

                foreach (var texture in _mapData.TextureData)
                {
                    file.WriteLine("{0} {1}", texture.Key, texture.Value);
                }

                foreach (var lineSegment in _mapData.LineSegments.Select(x => x.Value))
                {
                    file.WriteLine(lineSegment.ToString());
                }

                foreach (var thing in _mapData.Things.Select(x => x.Value))
                {
                    file.WriteLine(thing.ToString());
                }

                foreach (var sector in _mapData.Sectors.Select(x => x.Value))
                {
                    file.WriteLine(sector.ToString());
                }
            }
        }

        public void LoadData(string fileName)
        {
            int startPositionTexture;
            int numTextureEntries;
            int numLineSegments;
            int startPositionLineSegments;
            int startPositionThings;
            int numThings;
            int startPositionSectors;
            int numSectors;

            using (var file = new StreamReader(fileName))
            {
                startPositionTexture = Convert.ToInt32(file.ReadLine());
                numTextureEntries = Convert.ToInt32(file.ReadLine());
                startPositionLineSegments = Convert.ToInt32(file.ReadLine());
                numLineSegments = Convert.ToInt32(file.ReadLine());
                startPositionThings = Convert.ToInt32(file.ReadLine());
                numThings = Convert.ToInt32(file.ReadLine());
                startPositionSectors = Convert.ToInt32(file.ReadLine());
                numSectors = Convert.ToInt32(file.ReadLine());

                LoadTextures(numTextureEntries, file);

                LoadLineSegments(numLineSegments, file);

                LoadThings(numThings, file);

                LoadSectors(numSectors, file);
            }
        }

        private void LoadSectors(int numSectors, StreamReader file)
        {
            for (int i = 0; i < numSectors; i++)
            {
                string[] line = file.ReadLine().Split(' ');
                int effect = Convert.ToInt32(line[0]);
                int floorTextureId = Convert.ToInt32(line[1]);
                int ceilingTextureId = Convert.ToInt32(line[2]);
                List<int> sideDefs = new List<int>();

                for (int j = 3; j < line.Length; j++)
                {
                    sideDefs.Add(Convert.ToInt32(line[j]));
                }

                _mapData.AddSector(new GameData.Sector()
                {
                    CeilingTextureId = ceilingTextureId,
                    Effect = effect,
                    FloorTextureId = floorTextureId,
                    SideDefinitions = sideDefs
                });
            }
        }

        private void LoadTextures(int numTextureEntries, StreamReader file)
        {
            for (int i = 0; i < numTextureEntries; i++)
            {
                string[] line = file.ReadLine().Split(' ');
                int id = Convert.ToInt32(line[0]);
                string texture = line[1];

                _mapData.AddTextureData(id, texture);
            }
        }

        private void LoadLineSegments(int numLineSegments, StreamReader file)
        {
            for (int i = 0; i < numLineSegments; i++)
            {
                string fileLine = file.ReadLine();
                string[] line = fileLine.Split(' ');
                double x1 = Convert.ToDouble(line[0]);
                double y1 = Convert.ToDouble(line[1]);
                double z1 = Convert.ToDouble(line[2]);
                double x2 = Convert.ToDouble(line[3]);
                double y2 = Convert.ToDouble(line[4]);
                double z2 = Convert.ToDouble(line[5]);
                int textureId = Convert.ToInt32(line[6]);

                Vertex start = new Vertex()
                {
                    X = (float)x1,
                    Y = (float)y1,
                    Z = (float)z1
                };

                Vertex end = new Vertex()
                {
                    X = (float)x2,
                    Y = (float)y2,
                    Z = (float)z2
                };

                _mapData.AddLineSegment(new LineSegment()
                {
                    Start = start,
                    End = end,
                    TextureId = textureId
                });
            }
        }

        private void LoadThings(int numThings, StreamReader file)
        {
            for (int i = 0; i < numThings; i++)
            {
                string[] line = file.ReadLine().Split(' ');
                int thingType = Convert.ToInt32(line[0]);
                string name = line[1];
                double x = Convert.ToDouble(line[2]);
                double y = Convert.ToDouble(line[3]);
                double z = Convert.ToDouble(line[4]);

                _mapData.AddThing(new Thing()
                {
                    Type = thingType,
                    Name = name,
                    Position = new Vertex()
                    {
                        X = (float)x,
                        Y = (float)y,
                        Z = (float)z
                    }
                });
            }
        }
    }
}

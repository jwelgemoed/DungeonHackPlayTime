using DungeonHack.Builders;
using DungeonHack.Entities;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;
using DungeonHack.DirectX;

namespace MazeEditor
{
    public class GridPolygonBuilder
    {
        private GridBoard _gridBoard;
        private int _baseSize = 64;
        private int _floorTextureIndex = 1;
        private int _ceilingTextureIndex = 2;
        private int _wallTextureIndex = 9;
        private readonly PolygonBuilder _meshBuilder;

        public GridPolygonBuilder(GridBoard gridboard, PolygonBuilder meshBuilder)
        {
            _gridBoard = gridboard;
            _meshBuilder = meshBuilder;
        }

        public IEnumerable<Polygon> GeneratePolygons()
        {
            List<Polygon> polygons = new List<Polygon>();

            for (int i=0; i <_gridBoard.SizeX; i++)
            {
                for (int j = 0; j < _gridBoard.SizeY; j++)
                {
                    if (_gridBoard.Grid[i, j] == NodeType.Empty)
                        continue;

                    int startx = i * _baseSize;
                    int endx = (i+1) * _baseSize;
                    int starty = j * _baseSize ;
                    int endy = (j+1) * _baseSize;

                    if (i > 0 && (_gridBoard.Grid[i - 1, j] == NodeType.Empty))
                    {
                        polygons.Add(CreateMesh(startx, endy, startx, starty));
                    }

                    if ((i < _gridBoard.SizeX - 1) && (_gridBoard.Grid[i + 1, j] == NodeType.Empty))
                    {
                        polygons.Add(CreateMesh(endx, starty, endx, endy));
                    }

                    if (j > 0 && (_gridBoard.Grid[i, j - 1] == NodeType.Empty))
                    {
                        polygons.Add(CreateMesh(startx, starty, endx, starty));
                    }

                    if ((j < _gridBoard.SizeY - 1) && (_gridBoard.Grid[i, j+1] == NodeType.Empty))
                    {
                        polygons.Add(CreateMesh(endx, endy, startx, endy));
                    }

                    polygons.Add(CreateFloorMesh(startx, starty, endx, endy));
                    polygons.Add(CreateCeilingMesh(startx, starty, endx, endy));
                }
            }

            return polygons;
        }

        public Polygon CreateFloorMesh(int startx, int starty, int endx, int endy)
        {
            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = startx;
            vectors[0].Y = 0.0f;
            vectors[0].Z = starty;

            vectors[1].X = endx;
            vectors[1].Y = 0.0f;
            vectors[1].Z = starty;

            vectors[2].X = endx;
            vectors[2].Y = 0.0f;
            vectors[2].Z = endy;

            vectors[3].X = startx;
            vectors[3].Y = 0.0f;
            vectors[3].Z = endy;

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                1, 0, 3, 1, 3, 2
            };

            Vector3 normal = Vector3.Cross(new Vector3(vectors[0].X, vectors[0].Y, vectors[0].Z)
                , new Vector3(vectors[1].X, vectors[1].Y, vectors[1].Z));

            normal = Vector3.Normalize(normal);

            model[0].x = vectors[faceIndex[0]].X;
            model[0].y = vectors[faceIndex[0]].Y;
            model[0].z = vectors[faceIndex[0]].Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vectors[faceIndex[1]].X;
            model[1].y = vectors[faceIndex[1]].Y;
            model[1].z = vectors[faceIndex[1]].Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[faceIndex[2]].X;
            model[2].y = vectors[faceIndex[2]].Y;
            model[2].z = vectors[faceIndex[2]].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vectors[faceIndex[3]].X;
            model[3].y = vectors[faceIndex[3]].Y;
            model[3].z = vectors[faceIndex[3]].Z;
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vectors[faceIndex[4]].X;
            model[4].y = vectors[faceIndex[4]].Y;
            model[4].z = vectors[faceIndex[4]].Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vectors[faceIndex[5]].X;
            model[5].y = vectors[faceIndex[5]].Y;
            model[5].z = vectors[faceIndex[5]].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            return _meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_floorTextureIndex)
                            .SetType(PolygonType.Floor)
                            .Build();
        }

        public Polygon CreateCeilingMesh(int startx, int starty, int endx, int endy)
        {
            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = startx;
            vectors[0].Y = _baseSize;
            vectors[0].Z = starty;

            vectors[1].X = endx;
            vectors[1].Y = _baseSize;
            vectors[1].Z = starty;

            vectors[2].X = endx;
            vectors[2].Y = _baseSize;
            vectors[2].Z = endy;

            vectors[3].X = startx;
            vectors[3].Y = _baseSize;
            vectors[3].Z = endy;

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2, 3
            };

            Vector3 normal = Vector3.Cross(new Vector3(vectors[0].X, vectors[0].Y, vectors[0].Z)
                , new Vector3(vectors[1].X, vectors[1].Y, vectors[1].Z));

            normal = Vector3.Normalize(normal);

            model[0].x = vectors[faceIndex[0]].X;
            model[0].y = vectors[faceIndex[0]].Y;
            model[0].z = vectors[faceIndex[0]].Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vectors[faceIndex[1]].X;
            model[1].y = vectors[faceIndex[1]].Y;
            model[1].z = vectors[faceIndex[1]].Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[faceIndex[2]].X;
            model[2].y = vectors[faceIndex[2]].Y;
            model[2].z = vectors[faceIndex[2]].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vectors[faceIndex[3]].X;
            model[3].y = vectors[faceIndex[3]].Y;
            model[3].z = vectors[faceIndex[3]].Z;
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vectors[faceIndex[4]].X;
            model[4].y = vectors[faceIndex[4]].Y;
            model[4].z = vectors[faceIndex[4]].Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vectors[faceIndex[5]].X;
            model[5].y = vectors[faceIndex[5]].Y;
            model[5].z = vectors[faceIndex[5]].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            return _meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_ceilingTextureIndex)
                            .SetType(PolygonType.Ceiling)
                            .Build();
        }

        public Polygon CreateMesh(int startx, int starty, int endx, int endy)
        {
            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = startx;
            vectors[0].Y = _baseSize;
            vectors[0].Z = starty;

            vectors[1].X = endx;
            vectors[1].Y = _baseSize;
            vectors[1].Z = endy;

            vectors[2].X = endx;
            vectors[2].Y = 0.0f;
            vectors[2].Z = endy;

            vectors[3].X = startx;
            vectors[3].Y = 0.0f;
            vectors[3].Z = starty;


            //Indexes for the above square
            short[] faceIndex = new short[6] {
                //0, 1, 2, 0, 2,3
                1, 0, 3, 1, 3, 2
            };

            Vector3 normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
            normal = Vector3.Normalize(normal);

            model[0].x = vectors[faceIndex[0]].X;
            model[0].y = vectors[faceIndex[0]].Y;
            model[0].z = vectors[faceIndex[0]].Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vectors[faceIndex[1]].X;
            model[1].y = vectors[faceIndex[1]].Y;
            model[1].z = vectors[faceIndex[1]].Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[faceIndex[2]].X;
            model[2].y = vectors[faceIndex[2]].Y;
            model[2].z = vectors[faceIndex[2]].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vectors[faceIndex[3]].X;
            model[3].y = vectors[faceIndex[3]].Y;
            model[3].z = vectors[faceIndex[3]].Z;
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vectors[faceIndex[4]].X;
            model[4].y = vectors[faceIndex[4]].Y;
            model[4].z = vectors[faceIndex[4]].Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vectors[faceIndex[5]].X;
            model[5].y = vectors[faceIndex[5]].Y;
            model[5].z = vectors[faceIndex[5]].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            return _meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_wallTextureIndex)
                            .SetType(PolygonType.Wall)
                            .Build();
        }


    }
}

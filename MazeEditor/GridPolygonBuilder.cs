using DungeonHack.Builders;
using DungeonHack.Entities;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;
using DungeonHack.DirectX;
using DungeonHack;
using SharpDX.Direct3D11;

namespace MazeEditor
{
    public class GridPolygonBuilder
    {
        private GridBoard _gridBoard;
        private int _baseSize = 64;
        private int _floorTextureIndex = 1;//1;
        private int _ceilingTextureIndex = 1;//2;//1;//2;
        private int _wallTextureIndex = 1;//1;//9;
        private GlobalVertexList _globalVertexList;
        private Device _device;
        private LightShader _lightShader;
        private BufferFactory _bufferFactory;

        public GridPolygonBuilder(GridBoard gridboard, 
            Device device, LightShader shader, BufferFactory bufferFactory, GlobalVertexList globalVertexList)
        {
            _gridBoard = gridboard;
            _globalVertexList = globalVertexList;
            _device = device;
            _lightShader = shader;
            _bufferFactory = bufferFactory;
        }

        public IEnumerable<Polygon> GeneratePolygons()
        {
            var polygonBuilders = new List<PolygonBuilder>();

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
                        polygonBuilders.Add(CreateMesh(startx, endy, startx, starty, true));
                    }

                    if ((i < _gridBoard.SizeX - 1) && (_gridBoard.Grid[i + 1, j] == NodeType.Empty))
                    {
                        polygonBuilders.Add(CreateMesh(endx, starty, endx, endy, true));
                    }

                    if (j > 0 && (_gridBoard.Grid[i, j - 1] == NodeType.Empty))
                    {
                        polygonBuilders.Add(CreateMesh(startx, starty, endx, starty, true));
                    }

                    if ((j < _gridBoard.SizeY - 1) && (_gridBoard.Grid[i, j+1] == NodeType.Empty))
                    {
                        polygonBuilders.Add(CreateMesh(endx, endy, startx, endy, true));
                    }

                    polygonBuilders.Add(CreateFloorMesh(startx, starty, endx, endy));
                    polygonBuilders.Add(CreateCeilingMesh(startx, starty, endx, endy));
                }
            }

            var polygons = new List<Polygon>();

            polygonBuilders.ForEach(x => polygons.Add(x.Build(_globalVertexList)));

            return polygons;
        }

        public PolygonBuilder CreateFloorMesh(int startx, int starty, int endx, int endy)
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

            foreach (var vector in vectors)
            {
                _globalVertexList.AddVectorToList(vector);
            }

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                1, 0, 3, 1, 3, 2
            };

            Vector3 normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);//Vector3.Cross(new Vector3(vectors[0].X, vectors[0].Y, vectors[0].Z)
                //, new Vector3(vectors[1].X, vectors[1].Y, vectors[1].Z));

            normal = Vector3.Normalize(normal);

            model[0].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[0]]);
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[1]]);
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[2]]);
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[3]]);
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[4]]);
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[5]]);
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            var meshBuilder = new PolygonBuilder(_device, _lightShader, _bufferFactory);
            return meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_floorTextureIndex)
                            .SetType(PolygonType.Floor);
                            
        }

        public PolygonBuilder CreateCeilingMesh(int startx, int starty, int endx, int endy)
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

            foreach (var vector in vectors)
            {
                _globalVertexList.AddVectorToList(vector);
            }

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2, 3
            };

            Vector3 normal = -Vector3.Cross(vectors[2] - vectors[1], vectors[1] - vectors[0]);//Vector3.Cross(new Vector3(vectors[0].X, vectors[0].Y, vectors[0].Z)
                //, new Vector3(vectors[1].X, vectors[1].Y, vectors[1].Z));

            normal = Vector3.Normalize(normal);

            model[0].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[0]]);
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[1]]);
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[2]]);
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[3]]);
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[4]]);
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[5]]);
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            var meshBuilder = new PolygonBuilder(_device, _lightShader, _bufferFactory);
            return meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_ceilingTextureIndex)
                            .SetType(PolygonType.Ceiling);
        }

        public PolygonBuilder CreateMesh(int startx, int starty, int endx, int endy, bool invertNormals)
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

            foreach (var vector in vectors)
            {
                _globalVertexList.AddVectorToList(vector);
            }

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                //0, 1, 2, 0, 2,3
                1, 0, 3, 1, 3, 2
            };

            Vector3 normal;

            if (!invertNormals)
            {
                normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
            }
            else
            {
                normal = Vector3.Cross(vectors[2] - vectors[1], vectors[1] - vectors[0]);
            }

            normal = Vector3.Normalize(normal);

            model[0].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[0]]);
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[1]]);
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = 1.0f;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[2]]);
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = 1.0f;//4.0f;
            model[2].ty = 1.0f;

            model[3].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[3]]);
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[4]]);
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = 1.0f;//4.0f;
            model[4].ty = 1.0f;

            model[5].VertexIndex = _globalVertexList.GetIndexOfVector(vectors[faceIndex[5]]);
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            var meshBuilder = new PolygonBuilder(_device, _lightShader, _bufferFactory);
            return meshBuilder
                            .New()
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .SetTextureIndex(_wallTextureIndex)
                            .SetType(PolygonType.Wall);
        }


    }
}

using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Windows;
using DungeonHack.Entities;
using Point = System.Windows.Point;

namespace MapEditor
{
    public abstract class BaseSegmentEditor
    {
        protected List<Tuple<GameData.LineSegment, Polygon>> _meshList;
        protected GameData.MapData _mapData;
        protected float _midWidth;
        protected float _midHeight;

        public BaseSegmentEditor(float midWidth, float midHeight)
        {
            _meshList = new List<Tuple<GameData.LineSegment, Polygon>>();
            _midWidth = midWidth;
            _midHeight = midHeight;
        }

        public List<Tuple<GameData.LineSegment, Polygon>> GetMeshList()
        {
            return _meshList;
        }

        public void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }
    }
}

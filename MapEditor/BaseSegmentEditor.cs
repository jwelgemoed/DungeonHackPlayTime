using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MapEditor
{
    public abstract class BaseSegmentEditor
    {
        protected List<Tuple<GameData.LineSegment, Mesh>> _meshList;
        protected GameData.MapData _mapData;
        protected float _midWidth;
        protected float _midHeight;

        public BaseSegmentEditor(float midWidth, float midHeight)
        {
            _meshList = new List<Tuple<GameData.LineSegment, Mesh>>();
            _midWidth = midWidth;
            _midHeight = midHeight;
        }

        public List<Tuple<GameData.LineSegment, Mesh>> GetMeshList()
        {
            return _meshList;
        }

        public void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }
    }
}

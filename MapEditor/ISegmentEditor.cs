using FunAndGamesWithSharpDX.Entities;
using GameData;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using Polygon = DungeonHack.Entities.Polygon;

namespace MapEditor
{
    public interface ISegmentEditor
    {
        void CreateMesh(Shape shape, Point startPoint, float currentScale);

        void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale);

        void EditAction(Point startPoint, float currentScale);

        void EditAction(Point startPoint, float currentScale, GlobalMapData mapData);

        List<Tuple<Shape, Polygon>> GetMeshList();

        void MoveAction(Point currentMousePosition, Point startPoint, float currentScale, bool snapToGrid, bool snapToClosestPoint);
    }
}
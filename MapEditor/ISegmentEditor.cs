using FunAndGamesWithSlimDX.Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace MapEditor
{
    public interface ISegmentEditor
    {
        void CreateMesh(Shape shape, Point startPoint, float currentScale);

        void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale);

        void EditAction(Point startPoint, float currentScale);

        void EditAction(Point startPoint, float currentScale, GameData.MapData mapData);

        List<Tuple<Shape, Mesh>> GetMeshList();

        void MoveAction(Point currentMousePosition, Point startPoint, float currentScale, bool snapToGrid, bool snapToClosestPoint);
    }
}
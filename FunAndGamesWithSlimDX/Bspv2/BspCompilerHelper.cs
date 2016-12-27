using System.Collections.Generic;
using System.Linq;
using SlimDX;
using FunAndGamesWithSlimDX.Entities;
using System;

namespace DungeonHack.Bspv2
{
    public enum PointClassification
    {
        Behind,
        Infront
    }

    public enum PolygonClassification
    {
        Behind,
        Infront,
        Spanning,
        Coincident
    }

    public class BspCompilerHelper
    {
        private const float EPSILON = 0.0001f;
        private List<Plane> _alreadyUsedSplitters = new List<Plane>();
        
        public PointClassification ClassifyPoint(Plane plane, Vector3 point)
        {
            var value = Plane.DotCoordinate(plane, point);

            if (value < EPSILON)
            {
                return PointClassification.Behind;
            }
            else if (value > EPSILON)
            {
                return PointClassification.Infront;
            }

            return PointClassification.Behind;
        }

        public PolygonClassification ClassifyPolygon(Plane plane, Mesh polygonMesh)
        {
            int numPositive = 0;
            int numNegative = 0;

            foreach (var vertex in polygonMesh.VertexData)
            {
                var value = ClassifyPoint(plane, vertex.Position.ToVector3());

                if (value == PointClassification.Infront)
                {
                    numPositive++;
                }
                else if (value == PointClassification.Behind)
                {
                    numNegative++;
                }
            }

            if (numPositive > 0 && numNegative == 0)
                return PolygonClassification.Infront;

            if (numPositive == 0 && numNegative > 0)
                return PolygonClassification.Behind;

            if (numPositive > 0 && numNegative > 0)
                return PolygonClassification.Spanning;

            return PolygonClassification.Coincident;
        }

        public bool IsConvexSet(List<Mesh> polygonMeshSet)
        {
            for (int i=0; i< polygonMeshSet.Count; i++)
            {
                for (int j=0; j < polygonMeshSet.Count; j++)
                {
                    if (ClassifyPolygon(polygonMeshSet[i].Plane, polygonMeshSet[j]) == PolygonClassification.Behind)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Plane SelectPartitionPlane(List<Mesh> polygonMeshList)
        {
            long bestScore = 100000000;
            List<Plane> planes = polygonMeshList.Select(x => x.Plane).ToList();
            Plane selectedPlane = planes[0];

            foreach (var splitter in planes)
            {
                long score = 0;
                long splits = 0;
                long backfaces = 0;
                long frontfaces = 0;

                foreach (var polygonMesh in polygonMeshList)
                {
                    if (polygonMesh.Plane == splitter)
                        continue;

                    var polyClassification = ClassifyPolygon(splitter, polygonMesh);

                    switch (polyClassification)
                    {
                        case PolygonClassification.Coincident:
                        case PolygonClassification.Infront:
                            frontfaces++;
                            break;
                        case PolygonClassification.Behind:
                            backfaces++;
                            break;
                        case PolygonClassification.Spanning:
                            splits++;
                            break;
                    }
                }

                score = Math.Abs(frontfaces - backfaces) + (splits * 8);

                if ((score < bestScore) &&
                    (!_alreadyUsedSplitters.Contains(splitter)))
                {
                    bestScore = score;
                    selectedPlane = splitter;
                    _alreadyUsedSplitters.Add(splitter);
                }

            }

            return selectedPlane;
        }
    }
}

using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Plane = DungeonHack.Entities.Plane;

namespace DungeonHack.BSP
{
    public class SplitterSelector
    {
        private readonly PolygonClassifier _polyClassifier;
        private int _splitweight;

        public SplitterSelector(PolygonClassifier polyClassifier, int splitweight)
        {
            _polyClassifier = polyClassifier;
            _splitweight = splitweight;
        }

        public int SelectBestSplitterPlane(IEnumerable<Mesh> meshList, List<Plane> planeArray)
        {
            long bestScore = 100000000;
            Mesh selectedMesh = meshList.First();

            foreach (var splitter in meshList.Where(x => !x.HasBeenUsedAsSplitPlane))
            {
                foreach (var mesh in meshList)
                {
                    if (mesh == splitter)
                        continue;

                    long score = 0;
                    long splits = 0;
                    long backfaces = 0;
                    long frontfaces = 0;

                    var polyClassification = _polyClassifier.ClassifyPolygon(splitter, mesh);

                    switch (polyClassification)
                    {
                        case PolygonClassification.OnPlane:
                        case PolygonClassification.Front:
                            frontfaces++;
                            break;
                        case PolygonClassification.Back:
                            backfaces++;
                            break;
                        case PolygonClassification.Spanning:
                            splits++;
                            break;
                    }

                    score = Math.Abs(frontfaces - backfaces) + (splits * 8);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        selectedMesh = splitter;
                    }
                }
            }

            var plane = new Plane()
            {
                PointOnPlane = new Vector3(
                                        selectedMesh.VertexData[0].Position.X,
                                        selectedMesh.VertexData[0].Position.Y,
                                        selectedMesh.VertexData[0].Position.Z),
                Normal = selectedMesh.Normal
            };

            selectedMesh.HasBeenUsedAsSplitPlane = true;

            planeArray.Add(plane);
            
            return planeArray.IndexOf(plane);
        }

        public Mesh SelectBestSplitter(IEnumerable<Mesh> meshList)
        {
            long bestScore = 100000000;
            Mesh selectedMesh = meshList.First();

            foreach (var splitter in meshList)
            {
                foreach (var mesh in meshList)
                {
                    if (mesh == splitter)
                        continue;

                    long score = 0;
                    long splits = 0;
                    long backfaces = 0;
                    long frontfaces = 0;

                    var polyClassification = _polyClassifier.ClassifyPolygon(splitter, mesh);

                    switch (polyClassification)
                    {
                        case PolygonClassification.OnPlane:
                        case PolygonClassification.Front:
                            frontfaces++;
                            break;
                        case PolygonClassification.Back:
                            backfaces++;
                            break;
                        case PolygonClassification.Spanning:
                            splits++;
                            break;
                    }

                    score = Math.Abs(frontfaces - backfaces) + (splits * _splitweight);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        selectedMesh = splitter;
                    }
                }
            }

            return selectedMesh;
        }
    }
}

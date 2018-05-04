using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class BoundingBoxCalculator
    {
        public BoundingBox CalculateBoundingBox(BoundingBox boundingBox, IList<Polygon> meshes)
        {
            BoundingBox startBox = boundingBox;

            foreach (var mesh in meshes)
            {
                startBox = BoundingBox.Merge(startBox, mesh.BoundingBox.BoundingBox);
            }

            return startBox;
        }
    }
}

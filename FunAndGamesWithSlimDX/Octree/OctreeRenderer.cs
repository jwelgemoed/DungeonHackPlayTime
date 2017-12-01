using FunAndGamesWithSharpDX.Engine;

namespace DungeonHack.Octree
{
    public class OctreeRenderer
    {
        private PolygonRenderer _renderer;
 
        public OctreeRenderer(PolygonRenderer renderer)
        {
            _renderer = renderer;
        }

        public void DrawOctree(OctreeNode node, Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            if (frustrum.CheckBoundingBox(node.BoundingBox) == 0)
                return;

            if (node.Octant1 != null)
                DrawOctree(node.Octant1, frustrum, camera, ref meshRenderedCount);

            if (node.Octant2 != null)
                DrawOctree(node.Octant2, frustrum, camera, ref meshRenderedCount);

            if (node.Octant3 != null)
                DrawOctree(node.Octant3, frustrum, camera, ref meshRenderedCount);

            if (node.Octant4 != null)
                DrawOctree(node.Octant4, frustrum, camera, ref meshRenderedCount);

            if (node.Octant5 != null)
                DrawOctree(node.Octant5, frustrum, camera, ref meshRenderedCount);

            if (node.Octant6 != null)
                DrawOctree(node.Octant6, frustrum, camera, ref meshRenderedCount);

            if (node.Octant7 != null)
                DrawOctree(node.Octant7, frustrum, camera, ref meshRenderedCount);

            if (node.Octant8 != null)
                DrawOctree(node.Octant8, frustrum, camera, ref meshRenderedCount);

            if (node.IsLeaf)
            {
                foreach (var polygon in node.Polygons)
                {
                    _renderer.Render(frustrum, polygon, ref meshRenderedCount);
                }
            }
        }
     }
}

using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class BspBoundingVolumeCalculator
    {
        public void ComputeBoundingVolumes(BspNode root)
        {
            List<BspNode> leafNodes = new List<BspNode>();

            FindAllLeafNodes(root, leafNodes);

            foreach (BspNode leafNode in leafNodes)
            {
                TraverseBottomUpVolumePath(leafNode, null);
            }
        }

        private void TraverseBottomUpVolumePath(BspNode node, BoundingBox? volume)
        {
            if (node.IsLeaf)
            {
                TraverseBottomUpVolumePath(node.Parent, null);
            }

            if (node.Splitter != null)
            {
                var boundingBox = node.Splitter.BoundingBox;

                if (!volume.HasValue)
                {
                    if (node.BoundingVolume.HasValue)
                    {
                        node.BoundingVolume = BoundingBox.Merge(node.BoundingVolume.Value, boundingBox.BoundingBox);
                    }
                    else
                    {
                        node.BoundingVolume = boundingBox.BoundingBox;
                    }
                }
                else
                {
                    if (node.BoundingVolume.HasValue)
                    {
                        node.BoundingVolume = BoundingBox.Merge(node.BoundingVolume.Value, BoundingBox.Merge(boundingBox.BoundingBox, volume.Value));
                    }
                    else
                    {
                        node.BoundingVolume = BoundingBox.Merge(boundingBox.BoundingBox, volume.Value);
                    }
                }
            }

            if (!node.IsRoot)
            {
                TraverseBottomUpVolumePath(node.Parent, node.BoundingVolume);
            }
        }

        private void FindAllLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            if (node.IsLeaf)
            {
                leafNodes.Add(node);
                return;
            }

            FindAllLeafNodes(node.Front, leafNodes);
            FindAllLeafNodes(node.Back, leafNodes);
        }
    }
}

using SlimDX;
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
                if (!volume.HasValue)
                {
                    var boundingBox = new BoundingBox(
                        Vector3.TransformCoordinate(node.Splitter._minimumVector, node.Splitter.WorldMatrix),
                        Vector3.TransformCoordinate(node.Splitter._maximumVector, node.Splitter.WorldMatrix));

                    if (node.BoundingVolume.HasValue)
                    {
                        node.BoundingVolume = BoundingBox.Merge(node.BoundingVolume.Value, boundingBox);
                    }
                    else
                    {
                        node.BoundingVolume = boundingBox;
                    }
                }
                else
                {
                    var boundingBox = new BoundingBox(
                        Vector3.TransformCoordinate(node.Splitter._minimumVector, node.Splitter.WorldMatrix),
                        Vector3.TransformCoordinate(node.Splitter._maximumVector, node.Splitter.WorldMatrix));

                    if (node.BoundingVolume.HasValue)
                    {
                        node.BoundingVolume = BoundingBox.Merge(node.BoundingVolume.Value, BoundingBox.Merge(boundingBox, volume.Value));
                    }
                    else
                    {
                        node.BoundingVolume = BoundingBox.Merge(boundingBox, volume.Value);
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

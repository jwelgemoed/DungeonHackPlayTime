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
                    node.BoundingVolume = new BoundingBox(node.Splitter.BoundingBox.Minimum, node.Splitter.BoundingBox.Maximum);
                }
                else
                {
                    var volumeBoxMin = volume.Value.Minimum;
                    var volumeBoxMax = volume.Value.Maximum;
                    var nodeBoxMin = node.Splitter.BoundingBox.Minimum;
                    var nodeBoxMax = node.Splitter.BoundingBox.Maximum;

                    float smallestX = volumeBoxMin.X;
                    float smallestY = volumeBoxMin.Y;
                    float smallestZ = volumeBoxMin.Z;
                    float largestX = volumeBoxMax.X;
                    float largestY = volumeBoxMax.Y;
                    float largestZ = volumeBoxMax.Z;

                    if (volumeBoxMax.X < smallestX)
                        smallestX = volumeBoxMax.X;

                    if (nodeBoxMin.X < smallestX)
                        smallestX = nodeBoxMin.X;

                    if (nodeBoxMax.X < smallestX)
                        smallestX = nodeBoxMax.X;

                    if (volumeBoxMax.Y < smallestY)
                        smallestY = volumeBoxMax.Y;

                    if (nodeBoxMin.Y < smallestY)
                        smallestY = nodeBoxMin.Y;

                    if (nodeBoxMax.Y < smallestY)
                        smallestY = nodeBoxMax.Y;

                    if (volumeBoxMax.Z < smallestZ)
                        smallestZ = volumeBoxMax.Z;

                    if (nodeBoxMin.Z < smallestZ)
                        smallestZ = nodeBoxMin.Z;

                    if (nodeBoxMax.Z < smallestZ)
                        smallestZ = nodeBoxMax.Z;

                    if (volumeBoxMin.X > largestX)
                        largestX = volumeBoxMin.X;

                    if (nodeBoxMin.X > largestX)
                        largestX = nodeBoxMin.X;

                    if (nodeBoxMax.X > largestX)
                        largestX = nodeBoxMax.X;

                    if (volumeBoxMin.Y > largestY)
                        largestY = volumeBoxMin.Y;

                    if (nodeBoxMin.Y > largestY)
                        largestY = nodeBoxMin.Y;

                    if (nodeBoxMax.Y > largestY)
                        largestY = nodeBoxMax.Y;

                    if (volumeBoxMin.Z > largestZ)
                        largestZ = volumeBoxMin.Z;

                    if (nodeBoxMin.Z > largestZ)
                        largestZ = nodeBoxMin.Z;

                    if (nodeBoxMax.Z > largestZ)
                        largestZ = nodeBoxMax.Z;

                    var minimumVector = new Vector3(smallestX, smallestY, smallestZ);
                    var maximumVector = new Vector3(largestX, largestY, largestZ);

                    node.BoundingVolume = new BoundingBox(minimumVector, maximumVector);
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

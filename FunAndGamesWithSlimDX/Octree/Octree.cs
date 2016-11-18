using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.Octree
{
    public class Octree
    {
        public BoundingBox Region { get; set; }

        public Octree[] ChildNodes = new Octree[8];

        public Octree Parent;

        public List<Mesh> Entities { get; set; }

        public static Queue<Mesh> PendingEntities { get; set; }

        public uint ActiveNodes;

        public const int MinSize = 1;

        public int MaxLifeSpan = 8;

        public int CurLife = -1;

        public static bool TreeReady = false;

        public static bool TreeBuilt = false;

        private Octree(BoundingBox region, List<Mesh> objs)
        {
            Region = region;
            Entities = objs;
            CurLife = -1;
        }

        public Octree()
        {
            Entities = new List<Mesh>();
            Region = new BoundingBox(Vector3.Zero, Vector3.Zero);
            CurLife = -1;
        }

        public Octree(BoundingBox region)
        {
            Region = region;
            Entities = new List<Mesh>();
            CurLife = -1;
        }

        private void UpdateTree()
        {
            if (!TreeBuilt)
            {
                while (PendingEntities.Count > 0)
                {
                    Entities.Add(PendingEntities.Dequeue());
                }

                BuildTree();
            }
            else
            {
                while (PendingEntities.Count > 0)
                {
                  //  Insert(PendingEntities.Dequeue());
                }
            }

            TreeReady = true;
        }

        private void BuildTree()
        {
            if (Entities.Count <= 0)
                return;

            Vector3 dimensions = Region.Maximum - Region.Minimum;

            if (dimensions == Vector3.Zero)
            {
                //FindEnclosingCube();
                dimensions = Region.Maximum - Region.Minimum;
            }

            if (dimensions.X <= MinSize && dimensions.Y <= MinSize && dimensions.Z <= MinSize)
            {
                return;
            }

            Vector3 half = dimensions / 2.0f;
            Vector3 center = Region.Minimum + half;

            BoundingBox[] octant = new BoundingBox[8];

            octant[0] = new BoundingBox(Region.Minimum, center);
            octant[1] = new BoundingBox(new Vector3(center.X, Region.Minimum.Y, Region.Minimum.Z), new Vector3(Region.Maximum.X, center.Y, center.Z));
            octant[2] = new BoundingBox(new Vector3(center.X, Region.Minimum.Y, center.Z), new Vector3(Region.Maximum.X, center.Y, Region.Maximum.Z));
            octant[3] = new BoundingBox(new Vector3(Region.Minimum.X, Region.Minimum.Y, center.Z), new Vector3(center.X, center.Y, Region.Maximum.Z));
            octant[4] = new BoundingBox(new Vector3(Region.Minimum.X, center.Y, Region.Minimum.Z), new Vector3(center.X, Region.Maximum.Y, center.Z));
            octant[5] = new BoundingBox(new Vector3(center.X, center.Y, Region.Minimum.Z), new Vector3(Region.Maximum.X, Region.Maximum.Y, center.Z));
            octant[6] = new BoundingBox(center, Region.Maximum);
            octant[7] = new BoundingBox(new Vector3(Region.Minimum.X, center.Y, center.Z), new Vector3(center.X, Region.Maximum.Y, Region.Maximum.Z));

            List<Mesh>[] octList = new List<Mesh>[8];
            List<Mesh> delist = new List<Mesh>();

            for (int i=0;i<8;i++)
            {
                octList[i] = new List<Mesh>();
            }

            foreach (var obj in Entities)
            {
                if (obj.BoundingBox.Minimum != obj.BoundingBox.Maximum)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (BoundingBox.Contains(octant[a], obj.BoundingBox) == ContainmentType.Contains)
                        {
                            octList[a].Add(obj);
                            delist.Add(obj);
                            break;
                        }
                    }
                }
                else if (obj.BoundingSphere.Radius >= 0.0001f)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (BoundingSphere.Contains(BoundingSphere.FromBox(octant[a]), obj.BoundingSphere) == ContainmentType.Contains)
                        {
                            octList[a].Add(obj);
                            delist.Add(obj);
                            break;
                        }
                    }
                }
            }

            foreach (var obj in delist)
            {
                Entities.Remove(obj);
            }

            for (int i=0; i<8;i++)
            {
                if (octList[i].Count != 0)
                {
                    ChildNodes[i] = CreateNode(octant[i], octList[i]);
                    ActiveNodes |= ActiveNodes << 1;
                    ChildNodes[i].BuildTree();
                }
            }

            TreeBuilt = true;
            TreeReady = true;
        }

        private Octree CreateNode(BoundingBox region, List<Mesh> objList)
        {
            if (objList == null || objList.Count == 0)
                return null;

            Octree tree = new Octree(region, objList);
            tree.Parent = this;

            return tree;
        }

        private Octree CreateNode(BoundingBox region, Mesh item)
        {
            List<Mesh> objList = new List<Mesh>(1);
            objList.Add(item);

            Octree tree = new Octree(region, objList);
            tree.Parent = this;

            return tree;
        }



    }
}

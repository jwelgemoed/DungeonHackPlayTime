﻿using FunAndGamesWithSharpDX.Engine;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.Entities
{
    public class AABoundingBox
    {
        public BoundingBox BoundingBox { get; set; }

        public Vector4[] Vectors { get; set; }

        public Vector3[] Vectors3 { get; set; }

        public int[] Indexes { get; set; }

        public Vector4 Center { get; private set; }

        public float DistanceToCamera(Camera camera)
        {
            return Vector4.Distance(Center, new Vector4(camera.EyeAt, 1.0f));
        }

        public bool ContainsOrIntersectsCamera(Camera camera)
        {
            return BoundingBox.Contains(camera.EyeAt) != ContainmentType.Disjoint;
        }

        public AABoundingBox(BoundingBox box)
        {
            BoundingBox = box;
            var corners = box.GetCorners();
            Vectors = new Vector4[corners.Length];
            Vectors3 = new Vector3[corners.Length];
            
            for (int i = 0; i < corners.Length; i++)
            {
                Vectors[i] = new Vector4(corners[i], 1.0f);
                Vectors3[i] = corners[i];
            }

            var minimum = BoundingBox.Minimum;
            var maximum = BoundingBox.Maximum;

            var halfPoint = maximum - minimum;
            Center = new Vector4(minimum + halfPoint, 1.0f);

            Indexes = new int[] 
            {
                //Front
                4, 5, 6,
                4, 6, 7,

                //Back
                2, 1, 0,
                2, 0, 3,

                //Top
                0, 1, 5,
                0, 5, 4,

                //Bottom
                3, 2, 6,
                3, 6, 7,

                //Left
                0, 4, 7,
                0, 7, 3,

                //Right
                6, 5, 1,
                6, 1, 2

             //   1, 3, 2,
             //   0, 3, 1,
                
	            //// index for bottom
	            //5, 7, 4,
             //   6, 7, 5,
                
	            //// index for left
	            //1, 7, 6,
             //   2, 7, 1,
                
	            //// index for right
	            //3, 5, 4,
             //   0, 5, 3,
                
	            //// index for back
	            //2, 4, 7,
             //   3, 4, 2,
                
	            //// index for front
	            //0, 6, 5,
             //   1, 6, 0
            };

            var front = new SharpDX.Plane(corners[Indexes[4]], corners[Indexes[5]], corners[Indexes[6]]);
            var back = new SharpDX.Plane(corners[Indexes[2]], corners[Indexes[1]], corners[Indexes[0]]);
            var top = new SharpDX.Plane(corners[Indexes[0]], corners[Indexes[1]], corners[Indexes[5]]);
            var bottom = new SharpDX.Plane(corners[Indexes[3]], corners[Indexes[2]], corners[Indexes[6]]);
            var left = new SharpDX.Plane(corners[Indexes[0]], corners[Indexes[4]], corners[Indexes[7]]);
            var right = new SharpDX.Plane(corners[Indexes[6]], corners[Indexes[5]], corners[Indexes[1]]);
        }
    }
}
using FunAndGamesWithSharpDX.Engine;
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

            for (int i = 0; i < corners.Length; i++)
            {
                Vectors[i] = new Vector4(corners[i], 1.0f);
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
        }
    }
}

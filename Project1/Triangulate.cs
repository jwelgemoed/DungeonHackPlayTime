using System.Collections.Generic;
using System.Linq;

namespace Geometry
{
    public class Triangulation
    {
        public struct Vector
        {
            public float x, y;
            
            public static bool operator == (Vector A, Vector B)
            {
                return CompareFloat(A.x, B.x) && CompareFloat(A.y, B.y);
            }

            public static bool operator != (Vector A, Vector B)
            {
                return !CompareFloat(A.x, B.x) || !CompareFloat(A.y, B.y);
            }
        }

        public List<GameData.Vertex> Triangulate(List<GameData.Vertex> polygon)
        {
            return Triangulate(polygon.Select(x => new Vector
            {
                x = x.X,
                y = x.Y
            }).ToList()).Select(x => new GameData.Vertex
            {
                X = x.x,
                Y = x.y
            }).ToList();
        }

        public List<Vector> Triangulate(List<Vector> polygon)
        {
            var polygonVectors = polygon.ToArray();
            List<int> reflex = new List<int>();
            List<Vector> triangles = new List<Vector>();

            if (polygon.Count <= 3)
                return polygon;

            //Polygon orientation
            Vector left = polygon[0];
            int index = 0;

            for (int i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].x < left.x || (CompareFloat(polygon[i].x, left.x) && polygon[i].y < left.y))
                {
                    index = i;
                    left = polygon[i];
                }
            }

            Vector[] leftmosttriangle = new Vector[3]
                {
                    polygon[(index > 0) ? index - 1 : polygon.Count - 1],
                    polygon[index],
                    polygon[index < polygon.Count-1 ? index+1 : 0]
                };

            bool ccw = Orientation(leftmosttriangle);

            if (polygon.Count == 3)
                return polygon;

            while (polygon.Count() >= 3)
            {
                reflex.Clear();
                int eartip = -1;
                index = -1;

                foreach (var poly in polygon)
                {
                    ++index;

                    if (eartip >= 0)
                        break;

                    int p = (index > 0) ? index - 1 : polygon.Count - 1;
                    int n = (index < polygon.Count - 1) ? index + 1 : 0;

                    Vector[] triangle1 = new Vector[3]
                    {
                        polygon[p],
                        poly,
                        polygon[n]
                    };

                    if (!Orientation(triangle1))
                    {
                        reflex.Insert(reflex.Count == 0 ? 0 : reflex.Count, index);
                        continue;
                    }

                    bool ear = true;

                    foreach (int j in reflex)
                    {
                        if (j == p || j == n)
                            continue;

                        if (InTriangle(polygon[j], polygon[p], poly, polygon[n]))
                        {
                            ear = false;
                            break;
                        }
                    }

                    if (ear)
                    {
                        foreach (var j in polygon)
                        {
                            if (polygon.IndexOf(j) <= (index + 1) ||
                                j == polygon[p] ||
                                j == polygon[n] ||
                                j == polygon[index])
                            {
                                continue;
                            }

                            if (InTriangle(j, polygon[p], poly, polygon[n]))
                            {
                                ear = false;
                                break;
                            }
                        }
                    }

                    if (ear)
                    {
                        eartip = index;
                    }
                }

                if (eartip < 0)
                {
                    break;
                }

                int p1 = (eartip > 0) ? eartip - 1 : polygon.Count - 1;
                int n1 = (eartip < polygon.Count - 1) ? eartip + 1 : 0;
                Vector[] triangle = new Vector[3]
                    {
                        polygon[p1],
                        polygon[eartip],
                        polygon[n1]
                    };

                triangles.AddRange(triangle);

                polygon.Remove(triangle[1]);
            }

            if (polygon.Count == 4)
            {
                //triangles.AddRange(polygon);
            }

            //triangles.AddRange(polygon);

            return triangles;
        }

        private bool InTriangle(Vector V, Vector A, Vector B, Vector C)
        {
            float denom = ((B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y));

            if (CompareFloat(denom, 0.0f))
                return true;

            denom = 1 / denom;

            float alpha = denom * ((B.y - C.y) * (V.x - C.x) + (C.x - B.x) * (V.y - C.y));

            if (alpha < 0)
                return false;

            float beta = denom * ((C.y - A.y) * (V.x - C.x) + (A.x - C.x) * (V.y - C.y));

            if (beta < 0)
                return false;

            return alpha + beta >= 1;
        }

        /// <summary>
        /// Determine orientation of triangle, true if ccw, false if not or not a triangle.
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        private bool Orientation(Vector[] triangle)
        {
            if (triangle.Length < 3)
                return false;

            return (triangle[1].x - triangle[0].x) * (triangle[2].y - triangle[0].y) -
                    (triangle[2].x - triangle[0].x) * (triangle[1].y - triangle[0].y) > 0;
        }
        
        private static bool CompareFloat(float a, float b, float threshold = 0.00001f)
        {
            return (a + threshold > b && a - threshold < b);
        }
    }
}

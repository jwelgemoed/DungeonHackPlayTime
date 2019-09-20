using SharpDX;
using System.Collections.Generic;

namespace DungeonHack
{
    public class GlobalVertexList
    {
        private readonly List<Vector3> _vectorList;

        public GlobalVertexList()
        {
            _vectorList = new List<Vector3>();
        }

        public int AddVectorToList(Vector3 vector)
        {
            if (_vectorList.Contains(vector))
            {
                return _vectorList.IndexOf(vector);
            }

            _vectorList.Add(vector);

            return _vectorList.IndexOf(vector);
        }

        public int GetIndexOfVector(Vector3 vector)
        {
            return _vectorList.IndexOf(vector);
        }

        public Vector3 GetVector(int index)
        {
            return _vectorList[index];
        }

        public static string GenerateVertexKey(float x, float y, float z)
        {
            return $"{x} {y} {z}";
        }
    }
}

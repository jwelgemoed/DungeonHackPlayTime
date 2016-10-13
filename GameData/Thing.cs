using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData
{
    public class Thing
    {
        public static int Count;

        public int Id { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }

        public Vertex Position { get; set; }

        public Thing()
        {
            Id = Count + 1;
            Count++;
        }

        public override string ToString()
        {
            return $"{Type} {Name} {Position}";
        }
    }
}

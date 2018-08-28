using DungeonHack.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.Engine
{
    public class RenderedItems
    {
        public List<Polygon>[] RenderedItemLists { get; }

        public RenderedItems(int numberOfThreads)
        {
            RenderedItemLists = new List<Polygon>[numberOfThreads];

            for (int i=0; i<numberOfThreads; i++)
            {
                RenderedItemLists[i] = new List<Polygon>(5000);
            }
        }
    }
}

using System.Collections.Generic;
using System.Windows.Shapes;

namespace MapEditor.ViewModel
{
    public class SectorViewModel
    {
        public GameData.Sector Sector { get; set; }

        public List<Line> Lines { get; set; }

        public List<GameData.LineSegment> Segments { get; set; }
    }
}

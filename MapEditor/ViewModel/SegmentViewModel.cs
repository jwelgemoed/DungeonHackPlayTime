using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MapEditor.ViewModel
{
    public class SegmentViewModel
    {
        public GameData.LineSegment Segment { get; set; }
        public Line Line { get; set; }
    }
}

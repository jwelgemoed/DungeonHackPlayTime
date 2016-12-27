using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FunAndGamesWithSlimDX.Engine;

namespace MeshCreator.Controls
{
    public class D3Panel : Panel
    {
        public Engine Engine { get; set; }
        
        public D3Panel(Engine engine)
        {
            
        }
    }
}

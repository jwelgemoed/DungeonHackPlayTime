using SlimDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunAndGamesWithSlimDX.DirectX
{
    public class IncludeFX : Include
    {
        static string includeDirectory = @"\FX\";

        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            stream = new FileStream(includeDirectory + fileName, FileMode.Open);
        }
    }
}

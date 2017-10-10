using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class IncludeFX : SharpDX.D3DCompiler.Include
    {
        static string includeDirectory = @"\FX\";

        public IDisposable Shadow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

        public void Dispose()
        {
            
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            stream = new FileStream(includeDirectory + fileName, FileMode.Open);
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            return new FileStream(includeDirectory + fileName, FileMode.Open);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FunAndGamesWithSlimDX.DirectX
{
    public static class Util
    {
        private static byte[] _unmanagedStaging = new byte[1024];

        public static byte[] GetArray(object o)
        {
            Array.Clear(_unmanagedStaging, 0, _unmanagedStaging.Length);
            var len = Marshal.SizeOf(o);
            if (len >= _unmanagedStaging.Length)
            {
                _unmanagedStaging = new byte[len];
            }
            var ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, _unmanagedStaging, 0, len);
            Marshal.FreeHGlobal(ptr);
            return _unmanagedStaging;

        }

    }
}

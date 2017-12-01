using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.DirectX
{
    public class OcclusionQuery : IDisposable
    {
        private Query _query;

        private int _pixelCount;

        public int PixelCount { get { return _pixelCount; } }

        public bool IsIssued { get; private set; }

        public OcclusionQuery(Device device)
        {
            _query = new Query(device, new QueryDescription { Type = QueryType.Occlusion, Flags = QueryFlags.None });
        }

        public bool IsComplete()
        {
            return _query.Device.ImmediateContext.GetData(_query, out _pixelCount);
        }

        public void Begin()
        {
            IsIssued = true;

            _query.Device.ImmediateContext.Begin(_query);
        }

        public void End()
        {
            IsIssued = false;
            _query.Device.ImmediateContext.End(_query);
        }

        public void Dispose()
        {
            ((IDisposable)_query).Dispose();
        }

    }
}


































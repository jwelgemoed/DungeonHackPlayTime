﻿using DungeonHack.Lights;
using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferPerFrame
    {
        public DirectionalLight DirectionalLight;
        public Spotlight SpotLight;
        public PointLight PointLight;
        public Vector3 CameraPosition;
        public float FogStart;
        public float FogEnd;
        public Vector3 Pad;

        public static readonly int Stride = Marshal.SizeOf(typeof(ConstantBufferPerFrame));
    }
}

﻿using FunAndGamesWithSlimDX.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunAndGamesWithSlimDX.Lights
{
    public static class LightEngine
    {
        private static DirectionalLight _directionalLights;
        private static PointLight _pointLights;
        private static Spotlight _spotLight;

        public static void AddDirectionalLight(DirectionalLight directional)
        {
            _directionalLights = directional;
        }

        public static void AddPointLight(PointLight point)
        {
            _pointLights = point;
        }

        public static void AddSpotLight(Spotlight spotlight)
        {
            _spotLight = spotlight;
        }

        public static void RenderLights(IShader shader)
        {
            shader.RenderLights(_directionalLights, _pointLights, _spotLight);
        }
    }
}

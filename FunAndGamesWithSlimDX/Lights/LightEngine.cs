using DungeonHack.DirectX;
using System.Collections.Generic;

namespace DungeonHack.Lights
{
    public static class LightEngine
    {
        private static readonly List<AmbientLight> AmbientLights = new List<AmbientLight>();
        private static readonly List<DirectionalLight> DirectionalLights = new List<DirectionalLight>();
        private static readonly List<PointLight> PointLights = new List<PointLight>();
        private static readonly List<Spotlight> SpotLights = new List<Spotlight>();

        private static AmbientLight[] _ambientLightArray = new AmbientLight[1];
        private static DirectionalLight[] _directionalLightArray = new DirectionalLight[1];
        private static Spotlight[] _spotlightArray = new Spotlight[1];
        private static PointLight[] _pointLightArray = new PointLight[1];

        public static void AddAmbientLight(AmbientLight ambient)
        {
            AmbientLights.Add(ambient);
            _ambientLightArray = null;
            _ambientLightArray = AmbientLights.ToArray();
        }

        public static void AddDirectionalLight(DirectionalLight directional)
        {
            DirectionalLights.Add(directional);
            _directionalLightArray = null;
            _directionalLightArray = DirectionalLights.ToArray();
        }

        public static void AddPointLight(PointLight point)
        {
            PointLights.Add(point);
            _pointLightArray = null;
            _pointLightArray = PointLights.ToArray();
        }

        public static void UpdatePointLight(int index, PointLight point)
        {
            PointLights[index] = point;
            _pointLightArray = null;
            _pointLightArray = PointLights.ToArray();
        }

        public static void AddSpotLight(Spotlight spotLight)
        {
            SpotLights.Add(spotLight);
            _spotlightArray = null;
            _spotlightArray = SpotLights.ToArray();
        }

        public static void RenderLights(Shader shader)
        {
            shader.RenderLights(_ambientLightArray, _directionalLightArray, _pointLightArray, _spotlightArray);
        }
    }
}

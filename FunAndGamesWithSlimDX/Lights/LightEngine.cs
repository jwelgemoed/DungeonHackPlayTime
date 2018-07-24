using DungeonHack.DirectX;

namespace DungeonHack.Lights
{
    public static class LightEngine
    {
        private static readonly DirectionalLight[] DirectionalLights = new DirectionalLight[NumberOfDirectionalLights];
        private static readonly PointLight[] PointLights = new PointLight[NumberOfPointLights];
        private static readonly Spotlight[] SpotLight = new Spotlight[NumberOfSpotLights];

        //MUST BE THE SAME VALUE AS DEFINED IN HLSL
        private const int NumberOfDirectionalLights = 1;
        private const int NumberOfPointLights = 2;
        private const int NumberOfSpotLights = 1;

        private static int _currentDirectionalLight;
        private static int _currentPointLight;
        private static int _currentSpotLight;

        public static void AddDirectionalLight(DirectionalLight directional)
        {
            DirectionalLights[_currentDirectionalLight] = directional;
            _currentDirectionalLight = (_currentDirectionalLight + 1) % NumberOfDirectionalLights;
        }

        public static void AddPointLight(PointLight point)
        {
            PointLights[_currentPointLight] = point;
            _currentPointLight = (_currentPointLight + 1) % NumberOfPointLights;
        }

        public static void AddSpotLight(Spotlight spotlight)
        {
            SpotLight[_currentSpotLight] = spotlight;
            _currentSpotLight = (_currentSpotLight + 1) % NumberOfSpotLights;
        }

        public static void RenderLights(Shader shader)
        {
            shader.RenderLights(DirectionalLights, PointLights, SpotLight);
        }
    }
}

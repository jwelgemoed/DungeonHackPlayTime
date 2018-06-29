using DungeonHack.DirectX;

namespace DungeonHack.Lights
{
    public static class LightEngine
    {
        private static readonly DirectionalLight[] DirectionalLights = new DirectionalLight[NumberOfLights];
        private static readonly PointLight[] PointLights = new PointLight[NumberOfLights];
        private static readonly Spotlight[] SpotLight = new Spotlight[NumberOfLights];

        //MUST BE THE SAME VALUE AS DEFINED IN HLSL
        private const int NumberOfLights = 1;
        private static int _currentDirectionalLight;
        private static int _currentPointLight;
        private static int _currentSpotLight;

        public static void AddDirectionalLight(DirectionalLight directional)
        {
            DirectionalLights[_currentDirectionalLight] = directional;
            _currentDirectionalLight = (_currentDirectionalLight + 1) % NumberOfLights;
        }

        public static void AddPointLight(PointLight point)
        {
            PointLights[_currentPointLight] = point;
            _currentPointLight = (_currentPointLight + 1) % NumberOfLights;
        }

        public static void AddSpotLight(Spotlight spotlight)
        {
            SpotLight[_currentSpotLight] = spotlight;
            _currentSpotLight = (_currentSpotLight + 1) % NumberOfLights;
        }

        public static void RenderLights(Shader shader)
        {
            shader.RenderLights(DirectionalLights, PointLights, SpotLight);
        }
    }
}

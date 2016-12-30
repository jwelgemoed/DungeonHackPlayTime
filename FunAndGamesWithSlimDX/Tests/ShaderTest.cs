using FunAndGamesWithSlimDX.DirectX;
using NUnit.Framework;

namespace FunAndGamesWithSlimDX.Tests
{
    [TestFixture]
    public class ShaderTest
    {
        [Test]
        public void TestLightShader()
        {
            LightShader shader = new LightShader();

            shader.Initialize(null);
        }
    }
}
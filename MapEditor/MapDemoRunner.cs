using FunAndGamesWithSlimDX.Engine;
using System;
using System.Collections.Generic;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Lights;
using DungeonHack.BSP;
using System.IO;
using System.Diagnostics;
using DungeonHack;
using DungeonHack.DataDictionaries;
using System.Configuration;

namespace MapEditor
{
    public class MapDemoRunner : CameraEngine, IDisposable
    {
        public List<Mesh> Meshes;
        private Vector3 _startingPosition;
        private Material _wallMaterial;
        private PointLight _pointLight;
        private DirectionalLight _directionalLight;
        private Spotlight _spotlight;
        private BspRenderer _bspRenderer;

        public BspNode BspRootNode { get; set; }

        private int _nodesVisited = 0;

        public MapDemoRunner() : base(8.0f, true)
        {
            _wallMaterial = new Material()
            {
                Ambient = new Color4(0.651f, 0.5f, 0.392f),
                Diffuse = new Color4(0.651f, 0.5f, 0.392f),
                Specular = new Color4(4.0f, 0.2f, 0.2f, 0.2f)
            };
        }

        public Device Device
        {
            get
            {
                return base.Renderer.Device;
            }
        }

        public IShader GetShader
        {
            get
            {
                return base.Shader;
            }
        }

        public override void DrawScene()
        {
            //Construct the frustrum
            if (ConfigManager.FrustrumCullingEnabled)
                _frustrum.ConstructFrustrum(Camera.ViewMatrix * Renderer.ProjectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(Shader);

            _meshRenderedCount = 0;
            base._stopwatch.Restart();

            _nodesVisited = 0;

            _bspRenderer.DrawBspTreeFrontToBack(BspRootNode, Camera.EyeAt, _frustrum, ref _meshRenderedCount);
            //DrawBspTreeBackToFront(BspRootNode, Camera.EyeAt);
        }

        public override void UpdateScene()
        {
            base.UpdateScene();

            var spotlight = new Spotlight(
                new Color4(1.0f, 1.0f, 1.0f),
                new Color4(0.2f, 0.2f, 0.2f),
                Colors.White,
                Camera.EyeAt,
                1000.0f,
                Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                96.0f,
                new Vector3(1.0f, 0.0f, 0.0f)
            );

            LightEngine.AddSpotLight(spotlight);

        }

        public override List<Mesh> GetSceneMeshes()
        {
            return Meshes;
        }

        public void SetStartingPosition(Vector3 startPosition)
        {
            _startingPosition = startPosition;
            Camera.SetPosition(startPosition.X, startPosition.Y, startPosition.Z);
        }

        public override void InitializeScene()
        {
            var textureDictionary = new TextureDictionary(base.Renderer.Device);
            textureDictionary.AddAllTextureFromPath(ConfigurationManager.AppSettings["ResourcePath"]);

            var materialDictionary = new MaterialDictionary();
            materialDictionary.AddMaterial(_wallMaterial);

            var meshRenderer = new MeshRenderer(materialDictionary, textureDictionary, base.Renderer.Context, Camera, Shader);

            _bspRenderer = new BspRenderer(meshRenderer, new PointClassifier());

            if (_startingPosition == null)
                Camera.SetPosition(0, 0, 0);

            Shader.Initialize(base.Renderer.Device);

            _directionalLight = new DirectionalLight(
                new Color4(0.2f, 0.2f, 0.2f),
                new Color4(0.0f, 0.0f, 0.0f),
                new Color4(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f));

            LightEngine.AddDirectionalLight(_directionalLight);

            _pointLight = new PointLight(
                new Color4(0.5f, 0.5f, 0.0f),
                new Color4(0.2f, 0.2f, 0.2f),
                new Color4(0.5f, 0.5f, 0.5f),
                new Vector3(0.0f, 0.0f, 0.0f),
                100.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

            LightEngine.AddPointLight(_pointLight);

            _spotlight = new Spotlight(
                new Color4(0.5f, 1.5f, 0.0f),
                new Color4(0.2f, 0.2f, 0.2f),
                new Color4(1.5f, 0.5f, 0.5f),
                new Vector3(10.0f, 0.0f, 0.0f),
                10.0f,
                new Vector3(0.0f, 1.0f, 0.0f),
                1.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

            LightEngine.AddSpotLight(_spotlight);
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();

            Shader.Dispose();
        }
    }
}

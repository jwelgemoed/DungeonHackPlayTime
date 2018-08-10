using DungeonHack.DataDictionaries;
using DungeonHack.DirectX;
using DungeonHack.Engine;
using DungeonHack.Entities;
using DungeonHack.Lights;
using DungeonHack.Renderers;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MapEditor
{
    public class MapDemoRunner : CameraEngine, IDisposable
    {
        public List<Polygon> Meshes;
        private Vector3 _startingPosition;
        private Material _wallMaterial;
        private PointLight _pointLight;
        private DirectionalLight _directionalLight;
        private Spotlight _spotlight;

        private int _nodesVisited = 0;

        public MapDemoRunner() : base(8.0f, true)
        {
            _wallMaterial = new Material()
            {
                Ambient = new Color4(0.651f, 0.5f, 0.392f, 1.0f),
                Diffuse = new Color4(0.651f, 0.5f, 0.392f, 1.0f),
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

        public Shader GetShader
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
                _frustrum.ConstructFrustrum(Camera.FirstPersonViewMatrix * Camera.ProjectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(Shader);

            _meshRenderedCount = 0;
            base._stopwatch.Restart();

            _nodesVisited = 0;

            //_octreeRenderer.DrawOctree(OctreeRootNode, _frustrum, Camera, ref _meshRenderedCount);
            //DrawBspTreeBackToFront(BspRootNode, Camera.EyeAt);
        }

        public override void UpdateScene()
        {
            base.UpdateScene();

            var spotlight = new Spotlight()
            {
                Ambient = new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                Diffuse = new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                Specular = new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                Position = Camera.EyeAt,
                Range = 1000.0f,
                Direction = Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                Spot = 96.0f,
                Attentuation = new Vector3(1.0f, 1.0f, 1.0f)
             };

            LightEngine.AddSpotLight(spotlight);

        }

        public override List<Polygon> GetSceneMeshes()
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

            var meshRenderer = new PolygonRenderer(materialDictionary, textureDictionary, 
                Renderer.ImmediateContext, Renderer.DeferredContexts, Renderer.CommandLists, Camera, Shader);

            if (_startingPosition == null)
                Camera.SetPosition(0, 0, 0);

            Shader.Initialize();

            _directionalLight = new DirectionalLight()
            {
                Ambient = new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                //Diffuse = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                //Specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                //Direction = new Vector3(0.0f, 1.0f, 0.0f)
            };

            LightEngine.AddDirectionalLight(_directionalLight);

            _pointLight = new PointLight()
            {
                Ambient = new Color4(0.5f, 0.5f, 0.0f, 1.0f),
                Diffuse = new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                Position = new Vector3(0.0f, 0.0f, 0.0f),
                Range = 100.0f,
                Attentuation = new Vector3(1.0f, 1.0f, 1.0f)
            };

            LightEngine.AddPointLight(_pointLight);

            _spotlight = new Spotlight()
            {
                Ambient = new Color4(0.5f, 1.5f, 0.0f, 1.0f),
                Diffuse = new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                Specular = new Color4(1.5f, 0.5f, 0.5f, 1.0f),
                Position = new Vector3(10.0f, 0.0f, 0.0f),
                Range = 10.0f,
                Direction = new Vector3(0.0f, 1.0f, 0.0f),
                Spot = 1.0f,
                Attentuation = new Vector3(1.0f, 1.0f, 1.0f)
            };

            LightEngine.AddSpotLight(_spotlight);
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();

            Shader.Dispose();
        }

        public override void PreRenderScene()
        {
            throw new NotImplementedException();
        }
    }
}

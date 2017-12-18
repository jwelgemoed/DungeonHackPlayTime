using DungeonHack;
using DungeonHack.BSP;
using DungeonHack.DataDictionaries;
using DungeonHack.Octree;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using DungeonHack.QuadTree;
using System.Threading.Tasks;

namespace MazeEditor
{
    public class MazeRunner : CameraEngine, IDisposable
    {
        public List<Polygon> Meshes;
        private Vector3 _startingPosition;
        private Material _wallMaterial;
        private PointLight _pointLight;
        private DirectionalLight _directionalLight;
        private Spotlight _spotlight;
        private BspRendererOptomized _bspRenderer;
        private OctreeRenderer _octreeRenderer;
        private QuadTreeRenderer _quadTreeRenderer;
        private PolygonRenderer _meshRenderer;

        public BspNodeOptomized[] BspNodes { get; set; }
        public OctreeNode OctreeRootNode { get; set; }
        public QuadTreeNode QuadTreeNode { get; internal set; }

        public int ThreadCount { get; set; }


        public MazeRunner() : base(8.0f, true)
        {
            _wallMaterial = new Material()
            {
                Ambient = new Color4(0.651f, 0.5f, 0.392f, 1.0f),
                Diffuse = new Color4(0.651f, 0.5f, 0.392f, 1.0f),
                Specular = new Color4(4.0f, 0.2f, 0.2f, 0.2f)
            };

            ThreadCount = base.Renderer.MaxThreads;
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
                _frustrum.ConstructFrustrum(Camera.FirstPersonViewMatrix * Renderer.ProjectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(Shader);

            _meshRenderedCount = 0;
            ThreadCount = 4;
            var tasks = new Task[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                int j = i;
                tasks[i] = new Task(() => RenderQuadTree(j));
                tasks[i].Start();
            }
            Task.WaitAll(tasks);

            for (int i = 0; i < ThreadCount; i++)
            {
                var commandList = base.Renderer.CommandLists[i];
                base.Renderer.Device.ImmediateContext.ExecuteCommandList(commandList, false);

                base.Renderer.CommandLists[i].Dispose();
                base.Renderer.CommandLists[i] = null;
            }
        }

        private void RenderQuadTree(int i)
        {
            QuadTreeNode node;

            switch (i)
            {
                case 0:
                    node = ThreadCount == 1 ? QuadTreeNode : QuadTreeNode.Octant1;
                    break;
                case 1:
                    node = QuadTreeNode.Octant2;
                    break;
                case 2:
                    node = QuadTreeNode.Octant3;
                    break;
                case 3:
                    node = QuadTreeNode.Octant4;
                    break;
                default:
                    node = QuadTreeNode;
                    break;
            }

            _quadTreeRenderer.DrawQuadTree(i, node, _frustrum, Camera, ref _meshRenderedCount);
            base.Renderer.CommandLists[i] = base.Renderer.ContextPerThread[i].FinishCommandList(false);
        }

        public override void UpdateScene()
        {
            base.UpdateScene();

            var spotlight = new Spotlight(
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                Camera.EyeAt,
                1000.0f,
                Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                96.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

            LightEngine.AddSpotLight(spotlight);

        }

        public override List<Polygon> GetSceneMeshes()
        {
            return Meshes;
        }

        public override void InitializeScene()
        {
            var textureDictionary = new TextureDictionary(base.Renderer.Device);
            textureDictionary.AddAllTextureFromPath(@"C:\git\DungeonHackPlayTime\FunAndGamesWithSlimDX\Resources");

            var materialDictionary = new MaterialDictionary();
            materialDictionary.AddMaterial(_wallMaterial);

            Camera.SetPosition(0, 16, 0);

            _meshRenderer = new PolygonRenderer(materialDictionary, textureDictionary, base.Renderer.ContextPerThread, Camera, Shader);

            _bspRenderer = new BspRendererOptomized(base.Renderer.Device, _meshRenderer, new PointClassifier(), BspNodes);
            _octreeRenderer = new OctreeRenderer(_meshRenderer);
            _quadTreeRenderer = new QuadTreeRenderer(_meshRenderer);

            Shader.Initialize(base.Renderer.Device, base.Renderer.ContextPerThread);

            _directionalLight = new DirectionalLight(
                new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 1.0f, 0.0f));

            LightEngine.AddDirectionalLight(_directionalLight);

            _pointLight = new PointLight(
                new Color4(0.5f, 0.5f, 0.0f, 1.0f),
                new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                100.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

            LightEngine.AddPointLight(_pointLight);

            _spotlight = new Spotlight(
                new Color4(0.5f, 1.5f, 0.0f, 1.0f),
                new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                new Color4(1.5f, 0.5f, 0.5f, 1.0f),
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

using DungeonHack;
using DungeonHack.BSP;
using DungeonHack.DataDictionaries;
using DungeonHack.Octree;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using DungeonHack.QuadTree;
using DungeonHack.CollisionDetection;
using DungeonHack.Quadtree;
using DungeonHack.Entities;
using DungeonHack.Builders;
using DungeonHack.DirectX;
using DungeonHack.Lights;

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
        private QuadTreeLeafNodeRenderer _quadTreeLeafNodeRenderer;
        private QuadTreeCollisionDetector _quadTreeCollisionDetector;
        private QuadTreeTraverser _quadTreeTraverser;
        private PolygonRenderer _meshRenderer;
        private Matrix _viewProjectionMatrix;
        private ItemRegistry _itemRegistry;

        public BspNodeOptomized[] BspNodes { get; set; }
        public OctreeNode OctreeRootNode { get; set; }
        public QuadTreeNode QuadTreeNode { get; internal set; }
        public IEnumerable<QuadTreeNode> QuadTreeLeafNodes { get; internal set; }

        public Dungeon Dungeon { get; set; }

        private Vector3 initialPos;
        private float timeupdate;

        public MazeRunner() : base(8.0f, true)
        {
            _wallMaterial = new Material()
            {
                Ambient = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f)
            };
        }

        public Device Device
        {
            get
            {
                return base.Renderer.Device;
            }
        }

        public new Shader Shader 
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
                _frustrum.ConstructFrustrum(Camera.ViewProjectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(base.Shader);

            _meshRenderedCount = 0;
            base._stopwatch.Restart();

            //_octreeRenderer.DrawOctree(OctreeRootNode, _frustrum, Camera, ref _meshRenderedCount);
            //_bspRenderer.DrawBspTreeFrontToBack(Camera.EyeAt, _frustrum, ref _meshRenderedCount, Camera);
            _quadTreeRenderer.DrawQuadTree(QuadTreeNode, _frustrum, Camera, ref _meshRenderedCount);
            //_quadTreeLeafNodeRenderer.DrawQuadTree(_frustrum, Camera, ref _meshRenderedCount);

            //Draw items in world;
            foreach (var item in _itemRegistry.GetItems())
            {
                _meshRenderer.Render(item.Polygon, Camera.RenderViewProjectionMatrix, ref _meshRenderedCount);
            }

        }

        public override void UpdateScene()
        {
            _quadTreeCollisionDetector.Camera = Camera;
            _quadTreeCollisionDetector.CurrentNode = _quadTreeTraverser.FindCurrentCameraLeafNode(Camera);

            base.UpdateScene();

            Vector3 lightPos = initialPos;

            if (Timer.TotalTime() - timeupdate >= 0.5f)
            {
                lightPos.Y *= (float) Math.Sin(Camera.FrameTime)*100;

                timeupdate += 0.5f;
            }

            if (ConfigManager.SpotlightOn)
            {
                var spotlight = new Spotlight(
                    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                    Camera.EyeAt,
                    ConfigManager.SpotLightRange, //500.0f,
                    Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                    ConfigManager.SpotLightFactor, //96.0f,
                    //new Vector3(1.0f, 1.0f, 1.0f)
                    new Vector3(ConfigManager.SpotLightAttentuationA, ConfigManager.SpotLightAttentuationB,
                        ConfigManager.SpotLightAttentuationC)
                );

                LightEngine.AddSpotLight(spotlight);
            }

            _pointLight = new PointLight(
                new Color4(0.3f, 0.3f, 0.3f, 0.1f),
                new Color4(0.7f, 0.7f, 0.7f, 0.1f),
                new Color4(0.7f, 0.7f, 0.7f, 0.1f),
                lightPos,
                50.0f,
                new Vector3(0.0f, 1.0f, 0.0f)
            );

            LightEngine.AddPointLight(_pointLight);
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

            var playerStart = Dungeon.GetPlayerStartLocation();

            Camera.SetPosition(playerStart.Item1 * 65, 16, playerStart.Item2 * 65);

            _meshRenderer = new PolygonRenderer(materialDictionary, textureDictionary, base.Renderer.Context, Camera, base.Shader);

            _bspRenderer = new BspRendererOptomized(base.Renderer.Device, _meshRenderer, new PointClassifier(), BspNodes);
            _octreeRenderer = new OctreeRenderer(_meshRenderer);
            _quadTreeRenderer = new QuadTreeRenderer(_meshRenderer, Camera);
            _quadTreeLeafNodeRenderer = new QuadTreeLeafNodeRenderer(_meshRenderer, Camera, QuadTreeLeafNodes);
            _quadTreeTraverser = new QuadTreeTraverser(QuadTreeNode);
            _quadTreeCollisionDetector = new QuadTreeCollisionDetector();
            Camera.CollisionDetector = _quadTreeCollisionDetector;

            base.Shader.Initialize(base.Renderer.Device, base.Renderer.Context);

            ConfigManager.SpotLightAttentuationA = 1.0f;
            ConfigManager.SpotLightAttentuationB = 1.0f;//1.0f;
            ConfigManager.SpotLightAttentuationC = 1.0f;//1.0f;
            ConfigManager.SpotLightFactor = 96.0f;
            ConfigManager.SpotLightRange = 1000;
            ConfigManager.FogStart = 50;
            ConfigManager.FogEnd = 1000;

            _directionalLight = new DirectionalLight(
                new Color4(0.1f, 0.1f, 0.1f, 1.0f),
                new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                new Color4(20.0f, 20.0f, 20.0f, 1.0f),
                new Vector3(0.0f, 1.0f, 0.0f));

            LightEngine.AddDirectionalLight(_directionalLight);

            _pointLight = new PointLight(
                new Color4(0.5f, 0.5f, 0.0f, 1.0f),
                new Color4(0.2f, 0.2f, 0.2f, 1.0f),
                new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                Camera.EyeAt,
                1000.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

           // LightEngine.AddPointLight(_pointLight);

            initialPos = Camera.EyeAt;

             _spotlight = new Spotlight(
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                new Color4(2.0f, 2.0f, 2.0f, 2.0f),
                Camera.EyeAt,
                1000.0f,
                Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                96.0f,
                new Vector3(1.0f, 1.0f, 1.0f)
            );

            LightEngine.AddSpotLight(_spotlight);

            var polygonBuilder = new PolygonBuilder(Device, Shader, new BufferFactory(Device));
            var itemFactory = new ItemFactory(polygonBuilder, textureDictionary, materialDictionary);
            _itemRegistry = new ItemRegistry();

            var itemLocation = Dungeon.GetItemLocation();

            _itemRegistry.AddItem(
                itemFactory.CreateItem("cat.obj-model.txt","cat_diff.png", 
                                    null, 
                                    new Vector3(itemLocation.Item1 * 64, 0, itemLocation.Item2 * 64))
                //itemFactory.CreateItem("treasure_chest.obj-model.txt", "treasure_chest.png")
                );

            foreach (var item in _itemRegistry.GetItems())
            {
                _quadTreeTraverser.PlaceItemInLeafNode(item);
            }
        }

        public new void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();

            base.Shader.Dispose();

            base.Dispose();
        }
    }
}

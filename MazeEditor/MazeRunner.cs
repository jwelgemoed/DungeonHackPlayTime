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
using DungeonHack.CollisionDetection;
using DungeonHack.Quadtree;
using DungeonHack.Entities;
using DungeonHack.Builders;

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

        public MazeRunner() : base(8.0f, true)
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
                _meshRenderer.Render(_frustrum, item.Polygon, Camera.ViewProjectionMatrix, ref _meshRenderedCount);
            }

        }

        public override void UpdateScene()
        {
            _quadTreeCollisionDetector.Camera = Camera;
            _quadTreeCollisionDetector.CurrentNode = _quadTreeTraverser.FindCurrentCameraLeafNode(Camera);

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

            var playerStart = Dungeon.GetPlayerStartLocation();

            Camera.SetPosition(playerStart.Item1 * 64, 16, playerStart.Item2 * 64);

            _meshRenderer = new PolygonRenderer(materialDictionary, textureDictionary, base.Renderer.Context, Camera, base.Shader);

            _bspRenderer = new BspRendererOptomized(base.Renderer.Device, _meshRenderer, new PointClassifier(), BspNodes);
            _octreeRenderer = new OctreeRenderer(_meshRenderer);
            _quadTreeRenderer = new QuadTreeRenderer(_meshRenderer, Camera);
            _quadTreeLeafNodeRenderer = new QuadTreeLeafNodeRenderer(_meshRenderer, Camera, QuadTreeLeafNodes);
            _quadTreeTraverser = new QuadTreeTraverser(QuadTreeNode);
            _quadTreeCollisionDetector = new QuadTreeCollisionDetector();
            Camera.CollisionDetector = _quadTreeCollisionDetector;

            base.Shader.Initialize(base.Renderer.Device, base.Renderer.Context);

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

            var polygonBuilder = new PolygonBuilder(Device, Shader);
            var itemFactory = new ItemFactory(polygonBuilder, textureDictionary, materialDictionary);
            _itemRegistry = new ItemRegistry();

            var itemLocation = Dungeon.GetItemLocation();

            _itemRegistry.AddItem(
                itemFactory.CreateItem("cat.obj-model.txt","cat_diff.png", 
                                    null, 
                                    new Vector3(itemLocation.Item1 * 64, 0, itemLocation.Item2 * 64))
                //itemFactory.CreateItem("treasure_chest.obj-model.txt", "treasure_chest.png")
                );
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

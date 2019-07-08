﻿using DungeonHack.Builders;
using DungeonHack.CollisionDetection;
using DungeonHack.DataDictionaries;
using DungeonHack.DirectX;
using DungeonHack.Engine;
using DungeonHack.Entities;
using DungeonHack.Lights;
using DungeonHack.OcclusionCulling;
using DungeonHack.Quadtree;
using DungeonHack.QuadTree;
using DungeonHack.Renderers;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using SharpDX.Diagnostics;

namespace MazeEditor
{
    public class MazeRunner : CameraEngine, IDisposable
    {
        public List<Polygon> Meshes;
        private Material _wallMaterial;
        private PointLight _pointLight;
        private PointLight _pointLight2;
        private DirectionalLight _directionalLight;
        private QuadTreeRenderer _quadTreeRenderer;
        private QuadTreeCollisionDetector _quadTreeCollisionDetector;
        private QuadTreeTraverser _quadTreeTraverser;
        private QuadTreeDepthRenderer _quadTreeDepthRenderer;
        private PolygonRenderer _polygonRenderer;
        private BoundingBoxRenderer _boundingBoxRenderer;
        private RenderedItems _renderedItems;
        private Matrix _viewProjectionMatrix;
        private ItemRegistry _itemRegistry;
        private ObjectTrackerToFile _objectTracker = new ObjectTrackerToFile("C:\\temp");

        public QuadTreeNode QuadTreeNode { get; internal set; }
        public IEnumerable<QuadTreeNode> QuadTreeLeafNodes { get; internal set; }

        public Dungeon Dungeon { get; set; }

        private Vector3 initialPos;
        private float timeupdate;
        private Item _catItem;

        public MazeRunner() : base(8.0f, true)
        {
            _wallMaterial = new Material()
            {
                Ambient = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                Specular = new Color4(0.1f, 0.1f, 0.1f, 0.1f)
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

        public override void PreRenderScene()
        {
            if (QuadTreeNode == null || _frustrum == null || Camera == null)
                return;

            _quadTreeDepthRenderer.DrawDepthQuadTree(QuadTreeNode, _frustrum, Camera);
        }

        public override void DrawScene()
        {
            //Construct the frustrum
            if (ConfigManager.FrustrumCullingEnabled)
                _frustrum.ConstructFrustrum(Camera.ViewProjectionMatrix);

            _meshRenderedCount = 0;
            base._stopwatch.Restart();

            _quadTreeRenderer.DrawQuadTree(QuadTreeNode, _frustrum, Camera);

            _meshRenderedCount = _quadTreeRenderer.MeshesRendered;

            //Draw items in world;
            foreach (var item in _itemRegistry.GetItems())
            {
                //_polygonRenderer.Render(item.Polygon, ref _meshRenderedCount);
            }

            LightEngine.RenderLights(base.Shader);

            //Track Liveobjects per frame.
            // _objectTracker.TrackLiveObjects();
        }

        public override void UpdateScene()
        {
           // _quadTreeCollisionDetector.Camera = Camera;
            _quadTreeCollisionDetector.CurrentNode = _quadTreeTraverser.FindCurrentCameraLeafNode(Camera);

            base.UpdateScene();

            Vector3 lightPos = initialPos;
            Vector3 lightPos2 = initialPos;
            lightPos2.Y += 8.0f;

            //if (Timer.TotalTime() - timeupdate >= 0.1f)
            {
                var factor = (32 * (float)Math.Sin(timeupdate));
                lightPos.X = lightPos.X + factor;
                lightPos.Y = factor/2;//(timeupdate % 360) * Math.PI/180));
                //initialPos = lightPos;

                var factor2 = (32 * (float)Math.Cos(timeupdate));
                lightPos2.X = lightPos2.X + factor2;
                lightPos2.Y = factor2/2;

                timeupdate += 0.1f;
            }

            if (ConfigManager.SpotlightOn)
            {
                var spotlight = new Spotlight()
                {
                    Ambient = new Color4(5.0f, 4.0f, 0.0f, 5.0f),
                    Diffuse = new Color4(5.0f, 4.0f, 0.0f, 5.0f),
                    Specular = new Color4(0.1f, 0.1f, 0.1f, 1.0f),
                    Position = Camera.EyeAt,
                    Range = ConfigManager.SpotLightRange, //500.0f,
                    Direction = Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
                    Spot = ConfigManager.SpotLightFactor, //96.0f,
                    //new Vector3(1.0f, 1.0f, 1.0f)
                    Attentuation = new Vector3(ConfigManager.SpotLightAttentuationA, ConfigManager.SpotLightAttentuationB,
                        ConfigManager.SpotLightAttentuationC)
                };

                //LightEngine.AddSpotLight(spotlight);
            }

            _pointLight = new PointLight()
            {
                //Diffuse = new Color4(0.5f, 0.0f, 0.0f, 1.0f),
                //Ambient = new Color4(5.0f, 1.0f, 1.0f, 1.0f),
                //Specular = new Color4(5.0f, 1.0f, 1.0f, 1.0f),
                Color = new Color4(15.0f, 0.0f, 0.0f, 1.0f),
                Position = lightPos,
                Range = 48.0f,
                Attentuation = new Vector3(0.0f, 0.1f, 0.0f)
            };

            _pointLight2 = new PointLight()
            {
                //Diffuse = new Color4(0.0f, 0.0f, 0.5f, 1.0f),
                //Ambient = new Color4(1.0f, 1.0f, 5.0f, 1.0f),
                //Specular = new Color4(1.0f, 1.0f, 5.0f, 1.0f),
                Color = new Color4(0.0f, 0.0f, 15.0f, 1.0f),
                Position = lightPos2,
                Range = 48.0f,
                Attentuation = new Vector3(0.0f, 0.1f, 0.0f)
            };

            LightEngine.UpdatePointLight(0, _pointLight);
            LightEngine.UpdatePointLight(1, _pointLight2);
        }

        public override List<Polygon> GetSceneMeshes()
        {
            return Meshes;
        }

        public override RenderedItems GetRenderedItems()
        {
            return _renderedItems;
        }

        public override void InitializeScene()
        {
            var textureDictionary = new TextureDictionary(base.Renderer.Device);
            textureDictionary.AddAllTextureFromPath(@"C:\git\DungeonHackPlayTime\FunAndGamesWithSlimDX\Resources");

            var materialDictionary = new MaterialDictionary();
            materialDictionary.AddMaterial(_wallMaterial);

            var playerStart = Dungeon.GetPlayerStartLocation();

            Camera.SetPosition(playerStart.Item1 * 65, 16, playerStart.Item2 * 65);

            _polygonRenderer = new PolygonRenderer(materialDictionary, textureDictionary,
                Renderer.ImmediateContext, Renderer.DeferredContexts, Renderer.CommandLists, Camera, base.Shader);

            int _threadCount = NumberOfThreads;

            _renderedItems = new RenderedItems(_threadCount);
            DepthBuffer depthBuffer = new DepthBuffer(Camera, _threadCount);
            DepthBufferRenderer.DepthBuffer = depthBuffer;
            _quadTreeRenderer = new QuadTreeRenderer(_polygonRenderer, depthBuffer, _renderedItems, _threadCount);
            _quadTreeDepthRenderer = new QuadTreeDepthRenderer(Camera, depthBuffer);

            _quadTreeTraverser = new QuadTreeTraverser(QuadTreeNode);
            _quadTreeCollisionDetector = new QuadTreeCollisionDetector();
            Camera.CollisionDetector = _quadTreeCollisionDetector;
            _quadTreeCollisionDetector.Camera = Camera;

            Shader.Initialize();

            ConfigManager.SpotLightAttentuationA = 1.0f;
            ConfigManager.SpotLightAttentuationB = 1.0f;//1.0f;
            ConfigManager.SpotLightAttentuationC = 1.0f;//1.0f;
            ConfigManager.SpotLightFactor = 96.0f;
            ConfigManager.SpotLightRange = 1000;
            ConfigManager.FogStart = 50;
            ConfigManager.FogEnd = 1000;
            ConfigManager.UseNormalMap = 1;


            LightEngine.AddAmbientLight(new AmbientLight()
            {
                AmbientDown = new Vector3(0.05f, 0.05f, 0.05f),
                AmbientUp = new Vector3(0.01f, 0.01f, 0.01f)
            });

            _directionalLight = new DirectionalLight()
            {
                Color = new Color4(0.05f, 0.05f, 0.035f, 0.01f),
                Direction = new Vector3(1.0f, 1.0f, 1.0f)
            };

            LightEngine.AddDirectionalLight(_directionalLight);
            //LightEngine.AddDirectionalLight(new DirectionalLight()
            //{
            //    Color = new Color4(1.0f, 0.5f, 0.5f, 0.1f),
            //    Direction = new Vector3(0.0f, 1.0f, 0.0f)
            //});
            //LightEngine.AddDirectionalLight(new DirectionalLight()
            //{
            //    Color = new Color4(0.5f, 0.5f, 2.5f, 1.0f),
            //    Direction = new Vector3(0.0f, 0.0f, 1.0f)
            //});

            LightEngine.AddPointLight(new PointLight());
            LightEngine.AddPointLight(new PointLight());

            //_pointLight = new PointLight(
            //    new Color4(0.0f, 0.0f, 0.0f, 1.0f),
            //    new Color4(0.2f, 0.2f, 0.2f, 1.0f),
            //    new Color4(0.5f, 0.5f, 0.5f, 1.0f),
            //    Camera.EyeAt,
            //    100.0f,
            //    new Vector3(0.0f, 1.0f, 0.0f)
            //);

            // LightEngine.AddPointLight(_pointLight);

            initialPos = Camera.EyeAt;

            // _spotlight = new Spotlight(
            //    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
            //    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
            //    new Color4(2.0f, 2.0f, 2.0f, 2.0f),
            //    Camera.EyeAt,
            //    1000.0f,
            //    Vector3.Normalize(Camera.LookAt - Camera.EyeAt),
            //    96.0f,
            //    new Vector3(1.0f, 1.0f, 1.0f)
            //);

            //LightEngine.AddSpotLight(_spotlight);

            var polygonBuilder = new PolygonBuilder(Device, Shader, new BufferFactory(Device));
            var itemFactory = new ItemFactory(polygonBuilder, textureDictionary, materialDictionary);
            _itemRegistry = new ItemRegistry();

            //var itemLocation = Dungeon.GetItemLocation();

            //_catItem = itemFactory.CreateItem("cat.obj-model.txt", "cat_diff.png",
            //    null,
            //    new Vector3(itemLocation.Item1 * 64, 0, itemLocation.Item2 * 64));

            //_itemRegistry.AddItem(_catItem);

            //foreach (var item in _itemRegistry.GetItems())
            //{
            //    _quadTreeTraverser.PlaceItemInLeafNode(item);
            //}
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

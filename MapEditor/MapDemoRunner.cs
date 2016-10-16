using FunAndGamesWithSlimDX.Engine;
using System;
using System.Collections.Generic;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Lights;
using DungeonHack.BSP;

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
        private PointClassifier _pointClassifier;

        public BspNode BspRootNode { get; set; }

        public MapDemoRunner() : base(8.0f, true)
        {
            _pointClassifier = new PointClassifier();

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
            var viewMatrix = Camera.ViewMatrix;
            var projectionMatrix = Renderer.ProjectionMatrix;

            //Construct the frustrum
            if (ConfigManager.FrustrumCullingEnabled)
                _frustrum.ConstructFrustrum(viewMatrix * projectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(Shader);

            _meshRenderedCount = 0;

           /*  foreach (var mesh in Meshes)
             {
                 mesh.Render(_frustrum, base.Renderer.Context, Camera, ref _meshRenderedCount);
             }
             */
           WalkBspTree(BspRootNode, Camera.LookAt);
        }

        private void WalkBspTree(BspNode node, Vector3 position)
        {
            if (node.IsLeaf)
            {
                return;
            }

            //var worldMatrix = node.Splitter.ScaleMatrix * node.Spliter.RotationMatrix;
            //worldMatrix = worldMatrix * node.Splitter.TranslationMatrix;

            PointClassification result = _pointClassifier.ClassifyPoint(position, node.Splitter);

            if (result == PointClassification.Front)
            {
                if (node.Back != null)
                    WalkBspTree(node.Back, position);

                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Front != null)
                    WalkBspTree(node.Front, position);
            }
            else
            {
                if (node.Front != null)
                    WalkBspTree(node.Front, position);

               // node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Back != null)
                    WalkBspTree(node.Back, position);
            }
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

            /*BspTreeBuilder builder = new BspTreeBuilder(Device, Shader);

            builder.TraverseBspTreeAndPerformAction(BspRootNode, (x) =>
            {
                x.LoadVectorsFromModel();
                x.SetPosition(0.0f, 0.0f, 0.0f);
                x.Material = _wallMaterial;

                x.CollissionEventHandler += y =>
                {
                    Camera.CollidedVertex = (Vertex)y.CollidedObject;
                    Camera.Collided = true;
                    _console.WriteLine("I'm hit!!");
                };
            });*/

            foreach (var mesh in Meshes)
            {
                mesh.LoadVectorsFromModel();
                mesh.SetPosition(0.0f, 0.0f, 0.0f);
                mesh.Material = _wallMaterial;

                mesh.CollissionEventHandler += x => 
                                                    {
                                                        Camera.CollidedVertex = (Vertex) x.CollidedObject;
                                                        Camera.Collided = true;
                                                        _console.WriteLine("I'm hit!!");
                                                    };
            }
            
         
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();

            Shader.Dispose();
        }
    }
}

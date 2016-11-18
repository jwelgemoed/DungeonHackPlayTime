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

        private int _nodesVisited = 0;

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
            //Construct the frustrum
            if (ConfigManager.FrustrumCullingEnabled)
                _frustrum.ConstructFrustrum(Camera.ViewMatrix * Renderer.ProjectionMatrix);

            //Do the light rendering
            LightEngine.RenderLights(Shader);

            _meshRenderedCount = 0;

            /*  foreach (var mesh in Meshes)
              {
                  mesh.Render(_frustrum, base.Renderer.Context, Camera, ref _meshRenderedCount);
              }
              */

            _nodesVisited = 0;
            //DrawBspTreeFrontToBack(BspRootNode, Camera.EyeAt);
            //_console.WriteLine("Nodes visited : " + _nodesVisited);
            DrawBspTreeBackToFront(BspRootNode, Camera.EyeAt);

        }

        /// <summary>
        /// BSP tree traversal inorder to draw polygons in back to front order (painter's algorithm)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        private void DrawBspTreeBackToFront(BspNode node, Vector3 position)
        {
            if (node.IsLeaf)
            {
                return;
            }
                       
            PointClassification result = _pointClassifier.ClassifyPoint(position, node.Splitter);

            if (result == PointClassification.Front)
            {
                if (node.Back != null)
                    DrawBspTreeBackToFront(node.Back, position);

                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Front != null)
                    DrawBspTreeBackToFront(node.Front, position);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeBackToFront(node.Front, position);

                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Back != null)
                    DrawBspTreeBackToFront(node.Back, position);
            }
        }

        private void DrawBspTreeFrontToBack(BspNode node, Vector3 position)
        {
            if (node.IsLeaf)
            {
                return;
            }

            if (node.BoundingVolume.HasValue)
            {
                var BoundingBox = new BoundingBox(
                    Vector3.TransformCoordinate(node.BoundingVolume.Value.Minimum, node.Splitter.WorldMatrix),
                    Vector3.TransformCoordinate(node.BoundingVolume.Value.Maximum, node.Splitter.WorldMatrix));

                if (_frustrum.CheckBoundingBox(BoundingBox) == 0)
                {
                    return;
                }
            }

            PointClassification result = _pointClassifier.ClassifyPoint(position, node.Splitter);

            _nodesVisited++;

            if (result == PointClassification.Back)
            {
                if (node.Back != null)
                    DrawBspTreeFrontToBack(node.Back, position);

                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Front != null)
                    DrawBspTreeFrontToBack(node.Front, position);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeFrontToBack(node.Front, position);

                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Back != null)
                    DrawBspTreeFrontToBack(node.Back, position);
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

            BspTreeBuilder builder = new BspTreeBuilder(Device, Shader);

          /*  builder.TraverseBspTreeAndPerformAction(BspRootNode, (x) =>
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

        internal void UpdateMeshes()
        {
            BspTreeBuilder builder = new BspTreeBuilder(Device, Shader);

            using (var file = new StreamWriter("c:\\temp\\meshes.txt"))
            {
                builder.TraverseBspTreeAndPerformAction(BspRootNode, (x) =>
                {

                    foreach(var model in x.Model)
                    {
                        file.WriteLine("X:{0}, Y:{1}, Y:{2}", model.x, model.y, model.z);
                    }

                    x.LoadVectorsFromModel();
                    x.SetPosition(0.0f, 0.0f, 0.0f);
                    x.Material = _wallMaterial;

                    x.CollissionEventHandler += y =>
                    {
                        Camera.CollidedVertex = (Vertex)y.CollidedObject;
                        Camera.Collided = true;
                        _console.WriteLine("I'm hit!!");
                    };
                });
            }
        }
    }
}

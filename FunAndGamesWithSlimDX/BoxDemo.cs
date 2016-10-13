using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX
{
    public class BoxDemo : Engine
    {
        private VertexBufferBinding _vBufferBinding;
        
        private Effect _fx;
        private EffectTechnique _technique;
        private EffectMatrixVariable _fxWorldProjectionView ;
        private InputLayout _inputLayout;

        private SlimDX.Matrix _world;
        private SlimDX.Matrix _projection;
        private SlimDX.Matrix _view;

        private float _theta;
        private float _phi;
        private float _radius;

        private bool _hasInitialized = false;

        private Point _lastMousePosition;
       
        private BoxMesh _boxMesh;

        public BoxDemo()
        {
            this.Initialize();

            _world = SlimDX.Matrix.Identity;
            _projection = SlimDX.Matrix.Identity;
            _view = SlimDX.Matrix.Identity;
        }

        public void Init()
        {
            this.OnMouseDown += HandleMouseDown;
            this.OnMouseMove += HandleMouseMove;

            _view = Matrix.LookAtLH(
                new Vector3(0f, 0f, - 4f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 1f, 0f));

            _projection = Matrix.PerspectiveFovLH(
                (float) Math.PI*0.5f,
                renderer.Width/(float) renderer.Height,
                0.1f, 100f);

            _world = Matrix.RotationYawPitchRoll(0.85f, 0.85f, 0f);

            var shader = new ColorShader();

            _boxMesh = new BoxMesh(base.renderer.Device, 0.5f) { MeshRenderPrimitive = PrimitiveTopology.TriangleList};

            BuildGeometryBuffers();
            BuildFx();
            BuildVertexLayout();

            

            _hasInitialized = true;
        }

        public override void Run()
        {
            if (!_hasInitialized)
                Init();
            
            base.Run();
        }

        protected override void UpdateScene()
        {
            float x = (float) (_radius*Math.Sin(_phi)*Math.Cos(_theta));
            float z = (float) (_radius*Math.Sin(_phi)*Math.Sin(_theta));
            float y = (float) (_radius*Math.Cos(_phi));

            Vector3 pos = new Vector3(x, y, z);
            Vector3 target = Vector3.Zero;
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

            _view = Matrix.LookAtLH(pos, target, up);
        }

        protected override void InitializeScene()
        {
            //throw new NotImplementedException();
        }

        protected override void DrawScene()
        {
            this.renderer.Context.ClearRenderTargetView(renderer.RenderTarget, Colors.Black);
            /*this.renderer.Context.ClearDepthStencilView(renderer.DepthStencilView,
                                                        DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                                                        1.0f, 0);*/

            this.renderer.Context.InputAssembler.InputLayout = _inputLayout;
            this.renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.renderer.Context.InputAssembler.SetVertexBuffers(0, _vBufferBinding);
            this.renderer.Context.InputAssembler.PrimitiveTopology = _boxMesh.MeshRenderPrimitive;
            this.renderer.Context.InputAssembler.SetIndexBuffer(_boxMesh.IndexBuffer, Format.R16_UInt, 0);


            Matrix world = _world;
            Matrix projection = _projection;
            Matrix view = _view;
            Matrix worldViewProjection = world*view*projection;

            _fxWorldProjectionView.SetMatrixTranspose(worldViewProjection);

            var techDesc = _technique.Description;

            for (int i = 0; i < techDesc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply(this.renderer.Context);

                this.renderer.Context.DrawIndexed(_boxMesh.GetIndexCount(), 0, 0);
            }

            this.renderer.SwapChain.Present(0, PresentFlags.None);

        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            _lastMousePosition.X = e.Location.X;
            _lastMousePosition.Y = e.Location.Y;
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float dx = 0.25f*(e.Location.X - _lastMousePosition.X);
                float dy = 0.25f*(e.Location.Y - _lastMousePosition.Y);

                _theta += dx;
                _phi += dy;
            }
            else if (e.Button == MouseButtons.Right)
            {
                float dx = 0.005f*(e.Location.X - _lastMousePosition.X);
                float dy = 0.005f*(e.Location.Y - _lastMousePosition.Y);

                _radius += dx - dy;

            }

            _lastMousePosition.X = e.Location.X;
            _lastMousePosition.Y = e.Location.Y;
        }

        private void BuildVertexLayout()
        {
            var vElements = new InputElement[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0), 
                };

            var passDescription = _technique.GetPassByIndex(0).Description;
            _inputLayout = new InputLayout(this.renderer.Device,
                                           passDescription.Signature, vElements);
        }

        private void BuildFx()
        {
            var shader = ShaderBytecode.CompileFromFile("FX/color.fx", null, "fx_5_0",
                                                        ShaderFlags.SkipOptimization | ShaderFlags.Debug,
                                                        EffectFlags.None);

            _fx = new Effect(this.renderer.Device, shader);

            _technique = _fx.GetTechniqueByName("ColorTech");
            _fxWorldProjectionView = _fx.GetVariableByName("gWorldViewProj").AsMatrix();
        }

        private void BuildGeometryBuffers()
        {
            var vertices = new Vertex[8];
            /* {
                   /* new Vertex()
                        {
                            Position = new Vector3(-1.0f, -1.0f, -1.0f),
                            Color = Color.White.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(1.0f, -1.0f, -1.0f),
                            Color = Color.Black.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(1.0f, -1.0f, 1.0f),
                            Color = Color.Red.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(-1.0f, -1.0f, 1.0f),
                            Color = Color.Green.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(-1.0f, 1.0f, -1.0f),
                            Color = Color.Blue.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(1.0f, 1.0f, -1.0f),
                            Color = Color.Yellow.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(1.0f, 1.0f, 1.0f),
                            Color = Color.Cyan.ToArgb()
                        },
                    new Vertex()
                        {
                            Position = new Vector3(-1.0f, 1.0f, 1.0f),
                            Color = Color.Magenta.ToArgb()
                        }*/
        

            var verticesStream = new DataStream(8 * Marshal.SizeOf(typeof(Vertex)), true, true);
            verticesStream.WriteRange(vertices);

            verticesStream.Position = 0;

            var buffer = new SlimDX.Direct3D11.Buffer(this.renderer.Device, verticesStream, 8 * Marshal.SizeOf(typeof(Vertex)),
                                                    ResourceUsage.Immutable, BindFlags.VertexBuffer,
                                                    CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _vBufferBinding = new VertexBufferBinding(buffer, Marshal.SizeOf(typeof(Vertex)), 0);

            verticesStream.Close();
        }

        public override void Shutdown()
        {
            _fx.Dispose();
            _inputLayout.Dispose();
            

            base.Shutdown();
        }
    }
}

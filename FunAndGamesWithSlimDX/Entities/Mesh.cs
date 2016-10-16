using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.IO;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

namespace FunAndGamesWithSlimDX.Entities
{
    public class Mesh : IDisposable
    {
        public IShader Shader;

        public SlimDX.Direct3D11.Buffer VertexBuffer
        {
           get
           {
                if (_vertexBuffer == null)
                {
                    _vertexBuffer = GetVertexBuffer();

                    return _vertexBuffer;
                }

               return _vertexBuffer;
           }
        }

        public SlimDX.Direct3D11.Buffer IndexBuffer
        {
            get
            {
                if (_indexBuffer == null)
                {
                    _indexBuffer = GetIndexBuffer();

                    return _indexBuffer;
                }

                return _indexBuffer;
            }
        }

        public Vertex[] VertexData { get; set; }

        public short[] IndexData { get; set; }

        public Model[] Model { get; set; }

        public PrimitiveTopology MeshRenderPrimitive { get; set; }

        public delegate void OnCameraCollisionDetected(CollissionEventArgs e);

        public event OnCameraCollisionDetected CollissionEventHandler;
 
        public int GetIndexCount()
        {
            return IndexData.Length;
        }

        private readonly Device _device;
        private SlimDX.Direct3D11.Buffer _vertexBuffer;
        private SlimDX.Direct3D11.Buffer _indexBuffer;
        protected readonly int _sizeOfVertex = Vertex.SizeOf;
        private const int SizeOfShort = sizeof (uint);
        protected Texture _texture = new Texture();
        private BoundingBox _boundingBox;
        private Vector3 _minimumVector;
        private Vector3 _maximumVector;
        public string TextureFileName { get; set; }

        public Matrix ScaleMatrix { get; private set; }
        public Matrix TranslationMatrix { get; private set; }
        public Matrix RotationMatrix { get; private set; }
        public Matrix WorldMatrix { get; private set; }

        public Material Material { get; set; }

        public Action<float> UpdateMeshAction { get; set; }

        public Mesh(Device device, IShader shader)
        {
            Shader = shader;
            _device = device;
            ScaleMatrix = Matrix.Identity;
            TranslationMatrix = Matrix.Identity;
            RotationMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
        }

        public void UpdateMesh(float time)
        {
            if (UpdateMeshAction != null)
                UpdateMeshAction.Invoke(time);
        }

        public void SetPosition(float x, float y, float z)
        {
            TranslationMatrix = Matrix.Translation(x, y, z);

            RecalculateWorldMatrix();
        }

        public void SetScaling(float scale)
        {
            ScaleMatrix = Matrix.Scaling(scale, scale, scale);

            RecalculateWorldMatrix();
        }

        public void SetScaling(float scaleX, float scaleY, float scaleZ)
        {
            ScaleMatrix = Matrix.Scaling(scaleX, scaleY, scaleZ);

            RecalculateWorldMatrix();
        }

        public void SetRotationMatrix(Matrix rotationMatrix)
        {
            RotationMatrix = rotationMatrix;

            RecalculateWorldMatrix();
        }

        public void SetScaleMatrix(Matrix scaleMatrix)
        {
            ScaleMatrix = scaleMatrix;

            RecalculateWorldMatrix();
        }

        public void SetTranslationMatrix(Matrix translationMatrix)
        {
            TranslationMatrix = translationMatrix;

            RecalculateWorldMatrix();
        }

        private void RecalculateWorldMatrix()
        {
            WorldMatrix = ScaleMatrix * RotationMatrix;
            WorldMatrix = WorldMatrix * TranslationMatrix;
        }
       
        private SlimDX.Direct3D11.Buffer GetVertexBuffer()
        {
            var vertices = new DataStream(_sizeOfVertex * VertexData.Length, true, true);
            vertices.WriteRange(VertexData);
            vertices.Position = 0;

            return new Buffer(_device, vertices, _sizeOfVertex * VertexData.Length
                    , ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, _sizeOfVertex);
        }


        private Buffer GetIndexBuffer()
        {
            var indexStream = new DataStream(SizeOfShort * IndexData.Length, true, true);
            indexStream.WriteRange(IndexData);
            indexStream.Position = 0;

            return new Buffer(_device, indexStream, SizeOfShort * IndexData.Length,
                                                           ResourceUsage.Default, BindFlags.IndexBuffer,
                                                           CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public virtual void Render(Frustrum frustrum, DeviceContext context, Camera camera, ref int meshRenderedCount)
        {
            //Do player collision detection
            _boundingBox = new BoundingBox(
                    Vector3.TransformCoordinate(_minimumVector, WorldMatrix),
                    Vector3.TransformCoordinate(_maximumVector, WorldMatrix));

            if (!ConfigManager.WallClipEnabled &&
                BoundingSphere.Intersects(camera.CameraSphere, _boundingBox))
            {
                //Calculate vertex closest to camera
                Vertex closestVertex = VertexData[0];
                Vector3 cameraPosition = camera.GetPosition();
                float closestDistance = Vector3.Distance(cameraPosition, new Vector3(VertexData[0].Position.X, VertexData[0].Position.Y, VertexData[0].Position.Z));

                for (int i = 1; i < VertexData.Length; i++)
                {
                    float distance = Vector3.Distance(cameraPosition, new Vector3(VertexData[i].Position.X, VertexData[i].Position.Y, VertexData[i].Position.Z));

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestVertex = VertexData[i];
                    }
                }

                RaiseCollissionEvent(new CollissionEventArgs() { CollidedObject = closestVertex });
            }


            //Frustrum culling.
            if (ConfigManager.FrustrumCullingEnabled &&
                frustrum.CheckBoundingBox(_boundingBox) == 0)
            {
                return;
            }
            
            meshRenderedCount++;

            context.InputAssembler.PrimitiveTopology = MeshRenderPrimitive;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, _sizeOfVertex, 0));
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R16_UInt, 0);

            Shader.Render(context, GetIndexCount(), WorldMatrix, camera.ViewMatrix,
                            camera.ProjectionMatrix, _texture.TextureData, camera.GetPosition(), Material);

        }

        private void RaiseCollissionEvent(CollissionEventArgs p)
        {
            if (CollissionEventHandler != null)
                CollissionEventHandler.Invoke(p);
        }

        public ShaderResourceView GetTexture()
        {
            return _texture.TextureData;
        }

        public void LoadTexture(string fileName)
        {
            TextureFileName = fileName;

            if (_texture == null)
                _texture = new Texture();

            var basePath = ConfigManager.ResourcePath;

            var fileNamePath = basePath + @"\Resources\" + fileName;

            _texture.LoadTexture(_device, fileNamePath);
        }

        public void LoadTextureFullPath(string filePath)
        {
            TextureFileName = filePath;

            if (_texture == null)
                _texture = new Texture();

            _texture.LoadTexture(_device, filePath);
        }

        public void LoadModel(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException("Param cannot be empty", "fileName");

            var basePath = ConfigManager.ResourcePath;

            var fileNamePath = basePath + @"\Resources\" + fileName;

            using (var inputStream = new StreamReader(fileNamePath))
            {
                string inputLine = "";

                while (!inputLine.Contains("Vertex Count"))
                {
                    inputLine = inputStream.ReadLine();
                }

                int vertexCount = int.Parse(inputLine.Substring(inputLine.IndexOf(":") + 1).Trim());

                Model = new Model[vertexCount];

                while (!inputLine.Contains("Data"))
                {
                    inputLine = inputStream.ReadLine();
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    inputLine = inputStream.ReadLine();

                    var data = inputLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    Model[i].x = float.Parse(data[0].Trim());
                    Model[i].y = float.Parse(data[1].Trim());
                    Model[i].z = float.Parse(data[2].Trim());
                    Model[i].tx = float.Parse(data[3].Trim());
                    Model[i].ty = float.Parse(data[4].Trim());
                    Model[i].nx = float.Parse(data[5].Trim());
                    Model[i].ny = float.Parse(data[6].Trim());
                    Model[i].nz = float.Parse(data[7].Trim());

                }
            }

            LoadVectorsFromModel();
        }

        

        public void LoadVectorsFromModel()
        {
            LoadVectorsFromModel(null, null);
        }

        public void LoadVectorsFromModel(Model[] model)
        {
            LoadVectorsFromModel(model, null);
        }
       
        public void LoadVectorsFromModel(Model[] model, short[] indexes)
        {
            if (model != null)
            {
                Model = new Model[model.Length];
                model.CopyTo(Model, 0);
            }
            
            VertexData = new Vertex[Model.Length];

            if (indexes == null)
                IndexData = new short[Model.Length];
            else
                IndexData = new short[indexes.Length];

            Vector3[] positions = new Vector3[VertexData.Length];

            for (int i = 0; i < Model.Length; i++)
            {
                VertexData[i] = new Vertex()
                {
                    Position = new Vector4(Model[i].x, Model[i].y, Model[i].z, 1.0f),
                    Texture = new Vector2(Model[i].tx, Model[i].ty),
                    Normal = new Vector3(Model[i].nx, Model[i].ny, Model[i].nz)
                };

                positions[i] = new Vector3(Model[i].x, Model[i].y, Model[i].z);

                if (indexes == null)
                    IndexData[i] = (short)i;
            }

            if (indexes != null)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    IndexData[i] = (short) indexes[i];
                }
            }

            MeshRenderPrimitive = PrimitiveTopology.TriangleList;

            _boundingBox = BoundingBox.FromPoints(positions);
            _minimumVector = _boundingBox.Minimum;
            _maximumVector = _boundingBox.Maximum;
        }

        public void Dispose()
        {
            _texture.Dispose();
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
            if (_indexBuffer != null)
                _indexBuffer.Dispose();
        }
    }
}

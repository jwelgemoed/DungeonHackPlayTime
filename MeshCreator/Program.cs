using System;
using System.IO;

namespace MeshCreator
{
    public class Program
    {
        public struct VertexType
        {
            public float x, y, z;
        }

        public struct FaceType
        {
            public int vIndex1, vIndex2, vIndex3;
            public int tIndex1, tIndex2, tIndex3;
            public int nIndex1, nIndex2, nIndex3;
        }

        public struct FileCounts
        {
            public int VertexCount;
            public int TextureCount;
            public int NormalCount;
            public int FaceCount;
        }

        private static string GetModelFileName(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: MeshCreator.exe <INPUT_FILE>");
            }

            return args[0];
        }

        private static FileCounts ReadFileCounts(string file)
        {
            FileStream fin;
            char input;
            long currentPos = 0;
            FileCounts fileCounts = new FileCounts();

            using (fin = new FileStream(file, FileMode.Open))
            {
                input = (char)fin.ReadByte();
                currentPos++;

                while (currentPos < fin.Length)
                {
                    if (input == 'v')
                    {
                        input = (char)fin.ReadByte();
                        currentPos++;

                        if (input == ' ')
                            fileCounts.VertexCount++;

                        if (input == 't')
                            fileCounts.TextureCount++;

                        if (input == 'n')
                            fileCounts.NormalCount++;
                    }

                    if (input == 'f')
                    {
                        input = (char)fin.ReadByte();
                        currentPos++;

                        if (input == ' ')
                            fileCounts.FaceCount++;
                    }

                    while (input != '\n')
                    {
                        input = (char)fin.ReadByte();
                        currentPos++;
                    }

                    input = (char)fin.ReadByte();
                    currentPos++;
                }

                fin.Close();
            }

            return fileCounts;
        }

        private static void LoadDataStructures(string fileName, FileCounts fileCounts)
        {
            VertexType[] vertices, texcoords, normals;
            FaceType[] faces;
            FileStream fin;
            int vIndex, tIndex, nIndex, fIndex, vIndex1, tIndex1, nIndex1;
            long currentPos = 0;
            char input, input2;
            TextWriter fout;

            vertices = new VertexType[fileCounts.VertexCount];
            texcoords = new VertexType[fileCounts.TextureCount];
            normals = new VertexType[fileCounts.NormalCount];
            faces = new FaceType[fileCounts.FaceCount];
            vIndex = 0;
            tIndex = 0;
            nIndex = 0;
            fIndex = 0;
            vIndex1 = 0;
            tIndex1 = 0;
            nIndex1 = 0;

            using (StreamReader reader = File.OpenText(fileName))
            {
                string line = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    var sublines = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (sublines.Length == 0)
                        continue;

                    if (sublines[0] == "v")
                    {
                        vertices[vIndex].x = Convert.ToSingle(sublines[1]);
                        vertices[vIndex].y = Convert.ToSingle(sublines[2]);
                        vertices[vIndex].z = Convert.ToSingle(sublines[3]) * -1.0f;

                        vIndex++;
                    }

                    if (sublines[0] == "vt")
                    {
                        texcoords[tIndex].x = Convert.ToSingle(sublines[1]);
                        texcoords[tIndex].y = 1.0f - Convert.ToSingle(sublines[2]);

                        tIndex++;
                    }

                    if (sublines[0] == "vn")
                    {
                        normals[nIndex].x = Convert.ToSingle(sublines[1]);
                        normals[nIndex].y = Convert.ToSingle(sublines[2]);
                        normals[nIndex].z = Convert.ToSingle(sublines[3]);

                        normals[nIndex].z = normals[nIndex].z * -1.0f;
                        nIndex++;
                    }

                    if (sublines[0] == "f")
                    {
                        var faceLines = sublines[1].Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                        faces[fIndex].vIndex3 = Convert.ToInt32(faceLines[0]);
                        faces[fIndex].tIndex3 = Convert.ToInt32(faceLines[1]);

                        if (faceLines.Length > 2)
                            faces[fIndex].nIndex3 = Convert.ToInt32(faceLines[2]);

                        faceLines = sublines[2].Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                        faces[fIndex].vIndex2 = Convert.ToInt32(faceLines[0]);
                        faces[fIndex].tIndex2 = Convert.ToInt32(faceLines[1]);

                        if (faceLines.Length > 2)
                            faces[fIndex].nIndex2 = Convert.ToInt32(faceLines[2]);

                        faceLines = sublines[3].Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                        faces[fIndex].vIndex1 = Convert.ToInt32(faceLines[0]);
                        faces[fIndex].tIndex1 = Convert.ToInt32(faceLines[1]);

                        if (faceLines.Length > 2)
                            faces[fIndex].nIndex1 = Convert.ToInt32(faceLines[2]);

                        fIndex++;
                    }
                }
            }

            using (fout = File.CreateText(fileName + "-model.txt"))
            {
                fout.WriteLine("Vertex Count: " + (fileCounts.FaceCount * 3));
                fout.WriteLine();
                fout.WriteLine("Data:");

                // Now loop through all the faces and output the three vertices for each face.
                for (int i = 0; i < fIndex; i++)
                {
                    vIndex = faces[i].vIndex1 - 1;
                    tIndex = faces[i].tIndex1 - 1;
                    nIndex = faces[i].nIndex1 - 1;

                    if (nIndex >= 0)
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, normals[nIndex].x, normals[nIndex].y, normals[nIndex].z);
                    }
                    else
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, 0, 0, 0);
                    }

                    vIndex = faces[i].vIndex2 - 1;
                    tIndex = faces[i].tIndex2 - 1;
                    nIndex = faces[i].nIndex2 - 1;

                    if (nIndex >= 0)
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, normals[nIndex].x, normals[nIndex].y, normals[nIndex].z);
                    }
                    else
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, 0, 0, 0);
                    }

                    vIndex = faces[i].vIndex3 - 1;
                    tIndex = faces[i].tIndex3 - 1;
                    nIndex = faces[i].nIndex3 - 1;

                    if (nIndex >= 0)
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, normals[nIndex].x, normals[nIndex].y, normals[nIndex].z);
                    }
                    else
                    {
                        fout.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", vertices[vIndex].x, vertices[vIndex].y, vertices[vIndex].z,
                        texcoords[tIndex].x, texcoords[tIndex].y, 0, 0, 0);
                    }
                }

                fout.Flush();
            }
        }

        private static void Main(string[] args)
        {
            bool result;
            string fileName;
            FileCounts fileCounts;
            char garbage;

            //args = new[] { "Castle.obj" };

            fileName = GetModelFileName(args);

            fileCounts = ReadFileCounts(fileName);

            Console.WriteLine();
            Console.WriteLine("Vertices: " + fileCounts.VertexCount);
            Console.WriteLine("UVs: " + fileCounts.TextureCount);
            Console.WriteLine("Normals: " + fileCounts.NormalCount);
            Console.WriteLine("Faces: " + fileCounts.FaceCount);

            LoadDataStructures(fileName, fileCounts);

            Console.WriteLine("File has been converted.");
        }
    }
}
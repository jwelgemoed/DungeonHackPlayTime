﻿using FunAndGamesWithSlimDX.Entities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FunAndGamesWithSlimDX.Engine
{
    /// <summary>
    /// Class responsible for saving scenes/levels/objects to file.
    /// </summary>
    public static class Saver
    {
        private static BinaryFormatter binFormatter = new BinaryFormatter();

        public static void SaveScene(string fileName, List<Mesh> scene)
        {
            using (var saveFile = File.Create(fileName, 0))
            {
                binFormatter.Serialize(saveFile, scene);
            }
        }

        public static List<Mesh> LoadScene(string fileName)
        {
            using (var saveFile = File.OpenRead(fileName))
            {
                return (List<Mesh>)binFormatter.Deserialize(saveFile);
            }
        }
    }
}
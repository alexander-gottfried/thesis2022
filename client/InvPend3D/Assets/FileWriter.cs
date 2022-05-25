using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Text;
using System.Runtime.Serialization.Json;

namespace Cereal
{
    class FileWriter
    {
        [Serializable]
        private class ListHolder<T>
        {
            public List<T> list;
        }

        [Serializable]
        private class THolder<T>
        {
            public T content;
        }

        public static void SaveList<T>(
            List<T> values, string fileName)
        {
            var list = new ListHolder<T>
            {
                list = values
            };

            string jsonStr = JsonUtility.ToJson(list);
            File.WriteAllText(fileName, jsonStr);
        }

        public static void DicToJson(Dictionary<string, List<double>> dict, string filename)
        {
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, List<double>>));

            using MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, dict);
            File.WriteAllText(filename, Encoding.Default.GetString(ms.ToArray()));
        }
    }
}
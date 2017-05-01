using System;
using System.IO;
using UnityEngine;

namespace Multiverse
{
    [Serializable]
    public class StorageElement
    {
        public ulong id;
    }

    public sealed class FileStorage<T> where T : StorageElement
    {
        public string databaseDirectory { get; set; }

        public FileStorage(string databaseDirectory)
        {
            this.databaseDirectory = databaseDirectory;
        }

        public void Store(T value)
        {
            System.IO.File.WriteAllText(FilePathFromId(value.id), JsonUtility.ToJson(value));
        }

        public T Load(ulong id)
        {
            string str = null;

            try
            {
               str = System.IO.File.ReadAllText(FilePathFromId(id));
            }
            catch(FileNotFoundException e)
            {
                return null;
            }

            if (str == null || str.Length == 0)
                return null;

            return JsonUtility.FromJson<T>(str);
        }

        public bool Exists(ulong id)
        {
            return System.IO.File.Exists(FilePathFromId(id));
        }

        public T Load(string nonHashedId)
        {
            return Load(MakeId(nonHashedId));
        }

        private string FilePathFromId(ulong id)
        {
            return databaseDirectory + "\\" + id.ToString();
        }

        public static ulong MakeId(string value)
        {
            return HashUtil.FromString64(value);
        }
    }
}

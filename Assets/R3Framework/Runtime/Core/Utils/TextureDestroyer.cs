using System.Collections.Generic;
using UnityEngine;

namespace Netmarble.Core
{
    public class TextureDestroyer : SingletonObject<TextureDestroyer>
    {
        private readonly List<Texture> _textureList = new List<Texture>();

        public void Register(Texture texture)
        {
            _textureList.Add(texture);
        }

        public void Destroy(Texture texture)
        {
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
        }

        public void DestroyCaches()
        {
            foreach (Texture texture in _textureList)
            {
                DestroyImmediate(texture);
            }

            _textureList.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
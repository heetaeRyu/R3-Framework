using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Netmarble.Core
{
    [Serializable]
    public class ImageUtilsLocalData
    {
        private LocalFileStorage<ImageUtilsLocalData> _storage;

        public string fileBrowserStartDirectory = "";

        public void Initialize()
        {
            string path;
#if UNITY_EDITOR
            path = Path.GetFullPath(Path.Combine(Application.persistentDataPath, $"Editor/ImageUtilsLocalData"));
#else
            path = Path.Combine(Application.persistentDataPath, $"ImageUtilsLocalData");
#endif

            _storage = LocalFileStorage<ImageUtilsLocalData>.GetStorage(path);

            var data = _storage.Data;
            if (data == null)
            {
                data = new ImageUtilsLocalData();
                _storage.Data = data;
            }

        }

        public void Update()
        {
            _storage.Data = this;
        }
    }

    public static class StandardImageUtils
    {
        private const long HdByteLength = 3686289;
        private const int ThumbnailMaxHeight = 144;

        private static ImageUtilsLocalData _imageUtilsLocalData;

        public static ImageUtilsLocalData GetImageUtilsLocalData()
        {
            if (_imageUtilsLocalData == null)
            {
                _imageUtilsLocalData = new ImageUtilsLocalData();
                _imageUtilsLocalData.Initialize();
            }

            return _imageUtilsLocalData;
        }

        public static void SetTexture(this Image image, Texture texture, Vector2 size)
        {
            Texture2D compressedOriginalTexture = ScaleTexture((Texture2D)texture, (int)size.x, (int)size.y);
            compressedOriginalTexture.Compress(true);
            Rect rect = new Rect(0, 0, compressedOriginalTexture.width, compressedOriginalTexture.height);
            image.sprite = Sprite.Create(compressedOriginalTexture, rect, new Vector2(0.5f, 0.5f));
            image.preserveAspect = true;
        }

        public static Texture2D ResizeTexture(this Texture texture, Vector2 size)
        {
            if (new Vector2(texture.width, texture.height) == size)
                return (Texture2D)texture;

            Texture2D resizeTexture = ScaleTexture((Texture2D)texture, (int)size.x, (int)size.y);
            return resizeTexture;
        }

        public static Texture2D GetDownSamplingTexture(Texture texture)
        {
            Vector2 downSampleTextureSize = GetDownSamplingTextureSize(texture);
            Texture2D downSampleTexture = texture.ResizeTexture(downSampleTextureSize);
            return downSampleTexture;
        }

        public static byte[] GetThumbnailBytes(Texture texture)
        {
            float resizeThreshHold = Math.Max(1, texture.height / ThumbnailMaxHeight);
            Vector2 thumbnailImageSize = new Vector2(texture.width, texture.height) / resizeThreshHold;
            Texture2D thumbnailTexture = texture.ResizeTexture(thumbnailImageSize);
            return thumbnailTexture.EncodeToPNG();
        }

        public static byte[] GetSquareIconThumbnailBytes(Texture texture, float size)
        {
            Vector2 thumbnailImageSize = new Vector2(size, size);
            Texture2D thumbnailTexture = texture.ResizeTexture(thumbnailImageSize);
            return thumbnailTexture.EncodeToPNG();
        }

        public static Vector2 GetDownSamplingTextureSize(Texture texture2D)
        {

            byte[] byteSize = ((Texture2D)texture2D).GetRawTextureData();
            float downSamplingRate = (float)HdByteLength / byteSize.Length;

            if (downSamplingRate < 1f)
            {
                double rate = Math.Sqrt(downSamplingRate);
                var resizedWidth = (int)Math.Ceiling(texture2D.width * rate);
                var resizedHeight = (int)Math.Ceiling(texture2D.height * rate);

                return new Vector2(resizedWidth, resizedHeight);
            }

            return new Vector2(texture2D.width, texture2D.height);
        }

        public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };

            Color[] pixels = result.GetPixels(0);
            float incX = (1.0f / targetWidth);
            float incY = (1.0f / targetHeight);
            for (int px = 0; px < pixels.Length; px++)
            {
                float width = (float)px / targetWidth;
                pixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * Mathf.Floor(width));
            }

            result.SetPixels(pixels, 0);
            TextureDestroyer.Instance.Register(result);

            return result;
        }

        public static Sprite GetSpriteByBytes(byte[] bytes, bool isCompressHighQuality = false, bool isCompress = true)
        {
            var texture2D = new Texture2D(8, 8);
            texture2D.LoadImage(bytes, false);

            var isPot = texture2D.width % 4 == 0 && texture2D.height % 4 == 0;
            if (isCompress && isPot)
                texture2D.Compress(isCompressHighQuality);

            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Rect tRect = new Rect(0, 0, texture2D.width, texture2D.height);
            Sprite newSprite = Sprite.Create(texture2D, tRect, pivot);

            return newSprite;
        }

        public static Texture2D GetTextureBytes(byte[] bytes, bool isCompressHighQuality = false,
            bool isCompress = true)
        {
            var texture2D = new Texture2D(8, 8);
            texture2D.LoadImage(bytes, false);

            var isPot = texture2D.width % 4 == 0 && texture2D.height % 4 == 0;
            if (isCompress && isPot)
                texture2D.Compress(isCompressHighQuality);

            return texture2D;
        }

        // public async static UniTask GetTextureInFilePanel(Action<Texture> handler)
        // {
        //     ImageUtilsLocalData localData = GetImageUtilsLocalData();
        //     string startDirectory = localData.fileBrowserStartDirectory;
        //     string[] paths = StandaloneFileBrowser.OpenFilePanel("Title", startDirectory,  new[]{new ExtensionFilter(string.Empty, "jpg", "png")}, false);
        //     if (paths.Length > 0)
        //     {
        //         string path = paths.First();
        //         string url = new System.Uri(path).AbsoluteUri;
        //         using (UnityWebRequest loader = UnityWebRequestTexture.GetTexture(url))
        //         {
        //             await loader.SendWebRequest();
        //             if (loader.result != UnityWebRequest.Result.Success)
        //             {
        //                 Debug.Log(loader.error);
        //             }
        //             else
        //             {
        //                 Texture2D texture = DownloadHandlerTexture.GetContent(loader);
        //                 handler?.Invoke(texture);
        //
        //                 localData.fileBrowserStartDirectory = Path.GetDirectoryName(path);
        //                 localData.Update();
        //             }
        //         }
        //     }
        // }

        public async static UniTask<byte[]> DownloadTextureBytes(string url)
        {
            var loader = UnityWebRequest.Get(url);
            try
            {
                await loader.SendWebRequest();

                if (loader.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(loader.error);
                    return null;
                }

                return loader.downloadHandler.data;
            }
            catch (UnityWebRequestException e)
            {
                Debug.LogWarning($"error: {e.Error} message: {e.Message}");
                return null;
            }
        }

        public async static UniTask<byte[]> DownloadLocalFileTextureBytes(string url)
        {
#if (UNITY_ANDROID||UNITY_IOS) && !UNITY_EDITOR
            url = "file://" + url;
#endif
            return await DownloadTextureBytes(url);
        }
    }
}
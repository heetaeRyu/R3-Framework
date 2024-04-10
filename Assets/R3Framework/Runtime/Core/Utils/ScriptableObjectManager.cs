#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace Netmarble.Core
{
    public static class ScriptableObjectManager
    {
#if UNITY_EDITOR
        public static class Editor
        {

            public static bool Exist<T>(string assetFolderPath) where T : ScriptableObject
            {
                Type type = typeof(T);
                string destPath = ScriptableObjectManager.GetTargetPath(assetFolderPath, type.Name) + ".asset";
                return File.Exists(Path.Combine(SystemUtil.AbsPath, destPath));
            }

            public static T Get<T>(string assetFolderPath) where T : ScriptableObject
            {
                Type type = typeof(T);
                T asset;
                string destPath = ScriptableObjectManager.GetTargetPath(assetFolderPath, type.Name) + ".asset";
                if (!EditorApplication.isPlaying &&
                    !Exist<T>(assetFolderPath))
                {
                    asset = Create<T>(destPath);
                }

                if (_dataDic.ContainsKey(type.Name))
                {
                    asset = _dataDic[type.Name] as T;
                }
                else
                {
                    asset = AssetDatabase.LoadAssetAtPath<T>(destPath);
                    if (asset != null)
                    {
                        _dataDic.Add(type.Name, asset);
                    }
                }

                return asset;
            }

            public static void Save<T>() where T : ScriptableObject
            {
                if (EditorApplication.isPlaying)
                {
                    EditorUtility.DisplayDialog("Fail", "Can't save in play mode", "OK");
                }
                else
                {
                    T data;
                    Type type = typeof(T);
                    string name = type.Name;
                    if (_dataDic.ContainsKey(name))
                    {
                        data = _dataDic[name] as T;
                        if (data != null)
                        {
                            EditorUtility.SetDirty(data);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Not exist type of {0} data, You should Get<T> first.", name);
                    }
                }
            }

            public static T Create<T>(string path) where T : ScriptableObject
            {
                int startIndex = path.LastIndexOf("/");
                string folderPath = Path.Combine(SystemUtil.AbsPath, path.Substring(0, startIndex));
                Directory.CreateDirectory(folderPath);
                T asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return asset;
            }
        }
#endif

        private static Dictionary<string, ScriptableObject> _dataDic = new Dictionary<string, ScriptableObject>();

        private static string GetTargetPath(string folderPath, string type)
        {
            if (!string.IsNullOrEmpty(folderPath) &&
                folderPath.LastIndexOf("/") != folderPath.Length - 1)
                folderPath += "/";
            return folderPath + type;
        }

        public static bool Exist<T>(string folderPath = "") where T : ScriptableObject
        {
            Type type = typeof(T);
            T value = Resources.Load<T>(GetTargetPath(folderPath, type.Name));
            return value != null;
        }

        public static T Get<T>(string folderPath = "") where T : ScriptableObject
        {
            Type type = typeof(T);
            T asset;
            if (_dataDic.ContainsKey(type.Name))
            {
                asset = _dataDic[type.Name] as T;
            }
            else
            {
                var path = GetTargetPath(folderPath, type.Name);
                Debug.Log(path);
                asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    _dataDic.Add(type.Name, asset);
                }
            }

            return asset;
        }

        public static bool Dispose<T>() where T : ScriptableObject
        {
            bool success = false;
            Type type = typeof(T);
            string name = type.Name;
            if (_dataDic.ContainsKey(name))
            {
                T target = _dataDic[name] as T;
                _dataDic.Remove(name);
                if (target != null)
                {
                    Resources.UnloadAsset(target);
                    success = true;
                }
            }

            return success;
        }
    }

    public class TestScriptable : ScriptableObject
    {
        public string id;
    }
}
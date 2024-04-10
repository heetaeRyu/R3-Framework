using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Netmarble.Core
{
    public class CacheManager : SingletonObject<CacheManager>
    {
        private string _cacheDirectoryPath;
        private long _maxCacheByteLength;
        private int _expireTimeDay = 3;
        public int ExpireTimeDay => _expireTimeDay;

        private bool _isInit;

        private void Awake()
        {
            Expire();
        }

        public void Initialize(string cacheDirectoryPath, long maxCacheByteLength, int expireTimeDay)
        {
            UpdateMaxCacheByteLength(maxCacheByteLength);
            UpdateCacheDirectoryPath(cacheDirectoryPath);
            UpdateExpireTimeDay(expireTimeDay);

            _isInit = true;
        }

        private void UpdateMaxCacheByteLength(long maxCacheByteLength)
        {
            _maxCacheByteLength = maxCacheByteLength;
        }

        private void UpdateCacheDirectoryPath(string path)
        {
            _cacheDirectoryPath = path;
        }

        private void UpdateExpireTimeDay(int day)
        {
            _expireTimeDay = day;
        }

        public /* async */ void Write(byte[] bytes, string fileName)
        {
            SetDefaultInit();

            string cachePath = _cacheDirectoryPath + $"/{fileName}";
            //await UniTask.SwitchToThreadPool();
            File.WriteAllBytes(cachePath, bytes);
        }

        public async UniTask<byte[]> Read(string fileName)
        {
            SetDefaultInit();

            string cachePath = _cacheDirectoryPath + $"/{fileName}";
            if (File.Exists(cachePath))
            {
                byte[] cacheBytes = await StandardImageUtils.DownloadLocalFileTextureBytes(cachePath);
                return cacheBytes;
            }

            return null;
        }

        public void SetDefaultInit()
        {
            if (_isInit == false)
            {
                _cacheDirectoryPath = Application.persistentDataPath + $"/ImageCaches";
                _maxCacheByteLength = 1 * 1024 * 1024 * 1024; // 1GB
                _expireTimeDay = 14;
                _isInit = true;

                DirectoryInfo directory = new DirectoryInfo(_cacheDirectoryPath);
                if (directory.Exists == false)
                {
                    directory.Create();
                }
            }
        }

        public void ClearCache()
        {
            var files = GetCacheSortedFileList();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }

        public bool IsExpire(long createTimeMillisecond)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(createTimeMillisecond);
            int days = (DateTime.UtcNow - dtDateTime).Days;

            return days >= ExpireTimeDay;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Expire();
        }

        private void Expire()
        {
            SetDefaultInit();

            TimeExpiration();
            ByteLengthExpiration();

            // TODO(희태): 서버에 등록된 시간을 기준으로 expire
            void TimeExpiration()
            {
                DirectoryInfo dirInfo = new DirectoryInfo(_cacheDirectoryPath);
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    TimeSpan lifeTime = DateTime.UtcNow - file.CreationTimeUtc;
                    if (lifeTime.Days >= _expireTimeDay)
                    {
                        file.Delete();
                    }
                }
            }

            void ByteLengthExpiration()
            {
                long cacheByteLength = GetCacheDirectorySize();
                bool isOverCacheMaxSize = cacheByteLength > _maxCacheByteLength;
                if (isOverCacheMaxSize)
                {
                    long howMuchOverByte = cacheByteLength - _maxCacheByteLength;
                    var willDeleteFiles = new List<FileInfo>();
                    var sortedFiles = GetCacheSortedFileList();
                    foreach (FileInfo file in sortedFiles)
                    {
                        if (howMuchOverByte < 0)
                            break;

                        willDeleteFiles.Add(file);
                        howMuchOverByte -= file.Length;
                    }

                    foreach (FileInfo file in willDeleteFiles)
                    {
                        file.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// 디렉토리 안의 파일들을 모두 모은 사이즈를 반환한다.
        /// </summary>
        /// <returns>바이트 단위</returns>
        public long GetCacheDirectorySize()
        {
            SetDefaultInit();

            long size = 0;
            DirectoryInfo dirInfo = new DirectoryInfo(_cacheDirectoryPath);

            foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
            }

            return size;
        }

        private IOrderedEnumerable<FileInfo> GetCacheSortedFileList()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_cacheDirectoryPath);
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            var sortedFiles = files.OrderBy(x => x.CreationTime);

            return sortedFiles;
        }
    }
}
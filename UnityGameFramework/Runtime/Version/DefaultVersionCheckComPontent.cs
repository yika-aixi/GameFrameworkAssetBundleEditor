using Icarus.GameFramework;
using Icarus.GameFramework.Version;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Icarus.UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Default VersionCheck")]
    public class DefaultVersionCheckComPontent : MonoBehaviour, IVersionCheck
    {
        public string Url { get; set; }
        private GameFrameworkAction<string> _errorHandle;
        private GameFrameworkAction<IEnumerable<AssetBundleInfo>, VersionInfo> _completeHandle;
        public void Check(GameFrameworkAction<IEnumerable<AssetBundleInfo>, VersionInfo> completeHandle, GameFrameworkAction<string> errorHandle)
        {
            _completeHandle = completeHandle;
            _errorHandle = errorHandle;
            StartCoroutine(_check());
        }

        private IEnumerator _check()
        {
            var url = GameFramework.Utility.Path.GetRemotePath(Path.Combine(Application.streamingAssetsPath,
                ConstTable.VersionFileName));
            VersionInfo streamInfos;
            VersionInfo persistentInfos;
            VersionInfo serverInfos;
            VersionInfo version;
            using (WWW www = new WWW(url))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    version = _jieMi(www.bytes);
                    if (version == null)
                    {
                        streamInfos = new VersionInfo();
                    }
                    else
                    {
                        streamInfos = version;
                    }
                }
                else
                {
                    streamInfos = new VersionInfo();
                }
            }
           
            var versionInfoFilePath = Path.Combine(Application.persistentDataPath, ConstTable.VersionFileName);
            if (File.Exists(versionInfoFilePath))
            {
                var versionInfoFileBy =
                    File.ReadAllBytes(versionInfoFilePath);
                version = _jieMi(versionInfoFileBy);
                if (version != null)
                {
                    persistentInfos = version;
                }
                else
                {
                    persistentInfos = new VersionInfo();
                }
            }
            else
            {
                persistentInfos = new VersionInfo();
            }
            var localAllInfo = new VersionInfo(persistentInfos.Version,
                persistentInfos.AssetBundleInfos.Union(streamInfos.AssetBundleInfos).ToList());

            using (WWW www = new WWW(Url))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    _errorHandle?.Invoke(www.error);
                    yield break;
                }

                version = _jieMi(www.bytes);
                if (version == null)
                {
                    _errorHandle?.Invoke("解析服务器version.info失败.");
                    yield break;
                }
                serverInfos = version;
            }
            
            List<AssetBundleInfo> result = new List<AssetBundleInfo>();
            foreach (var abInfo in serverInfos.AssetBundleInfos)
            {
                var serverInfo = serverInfos.GetAssetBundleInfo(abInfo.PackFullName);
                var localInfo = localAllInfo.GetAssetBundleInfo(abInfo.PackFullName);
                if (serverInfo.Optional)
                {
                    if (localInfo == null)
                    {
                        continue;
                    }
                }

                if (localInfo != null)
                {
                    if (serverInfo.MD5 == localInfo.MD5)
                    {
                        continue;
                    }
                }

                //加入结果列表
                result.Add(abInfo);
            }

            _completeHandle?.Invoke(result,persistentInfos);

        }

        VersionInfo _jieMi(byte[] bytes)
        {
            try
            {
                VersionInfo version = new VersionInfo();
                version = version.JieMiDeserialize(bytes);
                return version;
            }
            catch (Exception e)
            {
                Debug.LogError("解密失败.\n"+e+"\n"+e.StackTrace);
                return null;
            }
        }

        void Awake()
        {
            GameEntry.RegisterComponent(this);

        }
    }
}
//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Icarus.GameFramework.Resource
{
    internal partial class ResourceManager
    {
        /// <summary>
        /// 资源初始化器。
        /// </summary>
        private sealed class ResourceIniter
        {
            private readonly ResourceManager m_ResourceManager;
            private string m_CurrentVariant;

            public GameFrameworkAction ResourceInitComplete;

            /// <summary>
            /// 初始化资源初始化器的新实例。
            /// </summary>
            /// <param name="resourceManager">资源管理器。</param>
            public ResourceIniter(ResourceManager resourceManager)
            {
                m_ResourceManager = resourceManager;
                m_CurrentVariant = null;

                ResourceInitComplete = null;
            }

            /// <summary>
            /// 关闭并清理资源初始化器。
            /// </summary>
            public void Shutdown()
            {

            }

            /// <summary>
            /// 初始化资源。
            /// </summary>
            public void InitResources(string currentVariant)
            {
                m_CurrentVariant = currentVariant;

                if (m_ResourceManager.m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("Resource helper is invalid.");
                }

                m_ResourceManager.m_ResourceHelper.
                    LoadBytes(
                        Icarus.GameFramework.Utility.Path.GetRemotePath(m_ResourceManager.m_ReadOnlyPath,
                            Icarus.GameFramework.Utility.Path.GetResourceNameWithSuffix(VersionListFileName)),
                        ParsePackageListVersion);
            }

            private int _waitParsePackageList;
            private void ParsePackageListVersion(string fileUri, byte[] bytes, string errorMessage)
            {
                if (bytes == null || bytes.Length <= 0)
                {
                    //StreamingAssets 目录中没有Version文件跳过加载
                    if (errorMessage.Contains("404"))
                    {
                        _initPersistentDataPath();
                        return;
                    }
                    throw new GameFrameworkException(string.Format("Package list Version'{0}' is invalid, error message is '{1}'.", fileUri, string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
                }
                _isPersistent = false;
                _waitParsePackageList = 0;
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes);
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        memoryStream = null;
                        byte[] encryptBytes = binaryReader.ReadBytes(4);
                        byte[] packVersionInfo = binaryReader.ReadBytes(bytes.Length - 4);
                        string str = Icarus.GameFramework.Utility.Converter.GetString(
                            Icarus.GameFramework.Utility.Encryption.GetXorBytes(packVersionInfo, encryptBytes));
                        UnityEngine.Debug.Log("TT:" + str);
                        var packs = str.Split('|');
                        //todo 测试
                        foreach (var pack in packs)
                        {
                            ++_waitParsePackageList;
                            m_ResourceManager.m_ResourceHelper.
                                LoadBytes(
                                    Icarus.GameFramework.Utility.Path.GetRemotePath(m_ResourceManager.m_ReadOnlyPath,
                                        Icarus.GameFramework.Utility.Path.GetResourceNameWithSuffix(pack)),
                                    ParsePackageList);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(string.Format("Parse package list version exception '{0}'.", exception.Message), exception);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private bool _isPersistent;
            /// <summary>
            /// 加载持久化目录下的ab信息文件,如果持久化目录中没有就返回true
            /// </summary>
            /// <returns></returns>
            private bool _initPersistentDataPath()
            {
                _waitParsePackageList = 0;
                _isPersistent = true;
                var files = Directory.GetFiles(m_ResourceManager.ReadWritePath, "*~version.dat",
                    SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string name = Path.GetFileName(file);
                    UnityEngine.Debug.Log(name);
                    ++_waitParsePackageList;
                    m_ResourceManager.m_ResourceHelper.
                        LoadBytes(
                            Icarus.GameFramework.Utility.Path.GetRemotePath(m_ResourceManager.ReadWritePath, name),
                            ParsePackageList);
                }

                return files.Length == 0;
            }

            private class ParsePackageInfo
            {
                public string FileUri;
                public byte[] Bytes;
                public string ErrorMessage;
            }

            private bool _isCompolete = true;
            private Queue<ParsePackageInfo> _parsePackageInfos = new Queue<ParsePackageInfo>();
            //todo 问题会有很多,需要仔细查
            /// <summary>
            /// 解析资源包资源列表。
            /// </summary>
            /// <param name="fileUri">版本资源列表文件路径。</param>
            /// <param name="bytes">要解析的数据。</param>
            /// <param name="errorMessage">错误信息。</param>
            private void ParsePackageList(string fileUri, byte[] bytes, string errorMessage)
            {
                if (bytes == null || bytes.Length <= 0)
                {
                    throw new GameFrameworkException(string.Format("Package list '{0}' is invalid, error message is '{1}'.", fileUri, string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
                }

                //                if (!_isCompolete)
                //                {
                //                    _parsePackageInfos.Enqueue(new ParsePackageInfo()
                //                    {
                //                        FileUri = fileUri,
                //                        Bytes = bytes,
                //                        ErrorMessage = errorMessage
                //                    });
                //                    return;
                //                }
                UnityEngine.Debug.Log("fileUri:" + fileUri + "bytes Lenght:" + bytes.Length);
                //                _isCompolete = false;
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes);
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        memoryStream = null;
                        char[] header = binaryReader.ReadChars(3);
                        UnityEngine.Debug.Log("header:" + header);
                        if (header[0] != PackageListHeader[0] || header[1] != PackageListHeader[1] || header[2] != PackageListHeader[2])
                        {
                            throw new GameFrameworkException("Package list header is invalid.");
                        }

                        byte listVersion = binaryReader.ReadByte();
                        UnityEngine.Debug.Log("listVersion:" + listVersion);
                        if (listVersion == 0)
                        {
                            byte[] encryptBytes = binaryReader.ReadBytes(4);
                            UnityEngine.Debug.Log("encryptBytes:" + encryptBytes);
                            var applicableGameVersionLenght = binaryReader.ReadByte();
                            UnityEngine.Debug.Log("applicableGameVersionLenght:" + applicableGameVersionLenght);
                            m_ResourceManager.m_ApplicableGameVersion =
                                Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(applicableGameVersionLenght), encryptBytes));
                            UnityEngine.Debug.Log("m_ResourceManager.m_ApplicableGameVersion:" + m_ResourceManager.m_ApplicableGameVersion);
                            m_ResourceManager.m_InternalResourceVersion = binaryReader.ReadInt32();
                            UnityEngine.Debug.Log("m_ResourceManager.m_InternalResourceVersion:" + m_ResourceManager.m_InternalResourceVersion);

                            //                            int resourceCount = binaryReader.ReadInt32();
                            //                            string[] names = new string[resourceCount];
                            //                            string[] variants = new string[resourceCount];
                            //                            int[] lengths = new int[resourceCount];

                            var ABNameCount = binaryReader.ReadByte();
                            UnityEngine.Debug.Log("ABNameCount:" + ABNameCount);
                            string ABName = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(ABNameCount), encryptBytes));
                            UnityEngine.Debug.Log("ABName:" + ABName);
                            string variant = null;
                            int ABlength;
                            Dictionary<string, string[]> dependencyAssetNamesCollection = new Dictionary<string, string[]>();
                            //                            for (int i = 0; i < resourceCount; i++)
                            //                            {
                            //                                var nameCount = binaryReader.ReadByte();
                            //                                UnityEngine.Debug.Log(nameCount);
                            //                                ABname = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(nameCount), encryptBytes));
                            //                                UnityEngine.Debug.Log(ABname);
                            byte variantLength = binaryReader.ReadByte();
                            UnityEngine.Debug.Log(variantLength);
                            if (variantLength > 0)
                            {
                                variant = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(variantLength), encryptBytes));
                            }

                            LoadType loadType = (LoadType)binaryReader.ReadByte();
                            UnityEngine.Debug.Log("loadType:" + loadType);
                            ABlength = binaryReader.ReadInt32();
                            UnityEngine.Debug.Log("ABlength:" + ABlength);
                            int hashCode = binaryReader.ReadInt32();
                            UnityEngine.Debug.Log("hashCode:" + hashCode);
                            int assetNamesCount = binaryReader.ReadInt32();
                            UnityEngine.Debug.Log("assetNamesCount:" + assetNamesCount);
                            string[] assetNames = new string[assetNamesCount];
                            for (int j = 0; j < assetNamesCount; j++)
                            {
                                assetNames[j] = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(binaryReader.ReadByte()), Icarus.GameFramework.Utility.Converter.GetBytes(hashCode)));
                                UnityEngine.Debug.Log("assetNames[j]:" + assetNames[j]);
                                int dependencyAssetNamesCount = binaryReader.ReadInt32();
                                string[] dependencyAssetNames = new string[dependencyAssetNamesCount];
                                for (int k = 0; k < dependencyAssetNamesCount; k++)
                                {
                                    dependencyAssetNames[k] = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(binaryReader.ReadByte()), Icarus.GameFramework.Utility.Converter.GetBytes(hashCode)));
                                }

                                if (variant == null || variant == m_CurrentVariant)
                                {
                                    if (dependencyAssetNamesCollection.ContainsKey(assetNames[j]))
                                    {
                                        dependencyAssetNamesCollection[assetNames[j]] = dependencyAssetNames;
                                    }
                                    else
                                    {
                                        dependencyAssetNamesCollection.Add(assetNames[j], dependencyAssetNames);
                                    }
                                }
                            }

                            if (variant == null || variant == m_CurrentVariant)
                            {
                                ResourceName resourceName = new ResourceName(ABName, variant);
                                ProcessAssetInfo(resourceName, assetNames);
                                ProcessResourceInfo(resourceName, loadType, ABlength, hashCode);
                            }
                            //                            }

                            ProcessAssetDependencyInfo(dependencyAssetNamesCollection);

                            ResourceGroup resourceGroupAll = m_ResourceManager.GetResourceGroup(string.Empty);
                            resourceGroupAll.AddResource(ABName, variant, ABlength);
                            //                            for (int i = 0; i < resourceCount; i++)
                            //                            {
                            //                                resourceGroupAll.AddResource(names[i], variants[i], lengths[i]);
                            //                            }

                            //todo 暂时没有资源组
                            //                            int resourceGroupCount = binaryReader.ReadInt32();
                            //                            for (int i = 0; i < resourceGroupCount; i++)
                            //                            {
                            //                                string groupName = Icarus.GameFramework.Utility.Converter.GetString(Icarus.GameFramework.Utility.Encryption.GetXorBytes(binaryReader.ReadBytes(binaryReader.ReadByte()), encryptBytes));
                            //                                ResourceGroup resourceGroup = m_ResourceManager.GetResourceGroup(groupName);
                            //                                int groupResourceCount = binaryReader.ReadInt32();
                            //                                for (int j = 0; j < groupResourceCount; j++)
                            //                                {
                            //                                    ushort versionIndex = binaryReader.ReadUInt16();
                            //                                    if (versionIndex >= resourceCount)
                            //                                    {
                            //                                        throw new GameFrameworkException(string.Format("Package index '{0}' is invalid, resource count is '{1}'.", versionIndex, resourceCount));
                            //                                    }
                            //
                            //                                    resourceGroup.AddResource(names[versionIndex], variants[versionIndex], lengths[versionIndex]);
                            //                                }
                            //                            }
                        }
                        else
                        {
                            throw new GameFrameworkException("Package list version is invalid.");
                        }
                    }
                    //                    _isCompolete = true;
                    UnityEngine.Debug.Log("fileUri:" + fileUri + "完成.");
                    --_waitParsePackageList;
                    if (_waitParsePackageList <= 0)
                    {
                        if (_isPersistent)
                        {
                            ResourceInitComplete();
                        }
                        else
                        {
                            if (_initPersistentDataPath())
                            {
                                UnityEngine.Debug.Log("初始化完成");
                                ResourceInitComplete();
                            }
                        }
                    }
                    //                    else
                    //                    {
                    //                        if (_parsePackageInfos.Count == 0)
                    //                        {
                    //                            return;
                    //                        }
                    //                        var info = _parsePackageInfos.Dequeue();
                    //                        
                    //                        ParsePackageList(info.FileUri,info.Bytes,info.ErrorMessage);
                    //                    }
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(string.Format("Parse package list exception '{0}'.", exception.Message), exception);
                }
                finally
                {

                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void ProcessAssetInfo(ResourceName resourceName, string[] assetNames)
            {
                foreach (string assetName in assetNames)
                {
                    if (m_ResourceManager.m_AssetInfos.ContainsKey(assetName))
                    {
                        m_ResourceManager.m_AssetInfos[assetName] = new AssetInfo(assetName, resourceName);
                    }
                    else
                    {
                        m_ResourceManager.m_AssetInfos.Add(assetName, new AssetInfo(assetName, resourceName));
                    }
                }
            }

            private void ProcessAssetDependencyInfo(Dictionary<string, string[]> dependencyAssetNamesCollection)
            {
                foreach (KeyValuePair<string, string[]> dependencyAssetNamesCollectionItem in dependencyAssetNamesCollection)
                {
                    List<string> dependencyAssetNames = new List<string>();
                    List<string> scatteredDependencyAssetNames = new List<string>();
                    foreach (string dependencyAssetName in dependencyAssetNamesCollectionItem.Value)
                    {
                        AssetInfo? assetInfo = m_ResourceManager.GetAssetInfo(dependencyAssetName);
                        if (assetInfo.HasValue)
                        {
                            dependencyAssetNames.Add(dependencyAssetName);
                        }
                        else
                        {
                            scatteredDependencyAssetNames.Add(dependencyAssetName);
                        }
                    }

                    if (m_ResourceManager.m_AssetDependencyInfos.ContainsKey(dependencyAssetNamesCollectionItem.Key))
                    {
                        m_ResourceManager.m_AssetDependencyInfos[dependencyAssetNamesCollectionItem.Key] =
                            new AssetDependencyInfo(dependencyAssetNamesCollectionItem.Key,
                                dependencyAssetNames.ToArray(), scatteredDependencyAssetNames.ToArray());
                    }
                    else
                    {
                        m_ResourceManager.m_AssetDependencyInfos.Add(dependencyAssetNamesCollectionItem.Key,
                            new AssetDependencyInfo(dependencyAssetNamesCollectionItem.Key,
                                dependencyAssetNames.ToArray(), scatteredDependencyAssetNames.ToArray()));
                    }
                }
            }

            private void ProcessResourceInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode)
            {
                //                if (m_ResourceManager.m_ResourceInfos.ContainsKey(resourceName))
                //                {
                //                    throw new GameFrameworkException(string.Format("Resource info '{0}' is already exist.", resourceName.FullName));
                //                }

                if (m_ResourceManager.m_ResourceInfos.ContainsKey(resourceName))
                {
                    m_ResourceManager.m_ResourceInfos[resourceName] =
                        new ResourceInfo(resourceName, loadType, length, hashCode, !_isPersistent);
                }
                else
                {
                    m_ResourceManager.m_ResourceInfos.Add(resourceName,
                        new ResourceInfo(resourceName, loadType, length, hashCode, !_isPersistent));
                }
            }
        }
    }
}

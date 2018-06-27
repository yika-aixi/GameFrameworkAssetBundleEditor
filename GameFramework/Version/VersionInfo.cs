using System.Collections.Generic;

namespace Icarus.GameFramework.Version
{
    public class VersionInfo
    {
        //Version.info 的版本
        public string Version;
        //最低app版本,低于该版本就跳app更新,否则开始对比资源进行更新
        public string MinAppVersion;
        //当前app中所有的资源包信息:持久化目录及StreamingAssets目录中所有的ab包信息
        //并集 -> 以持久化为主
        public IEnumerable<AssetBundleInfo> AssetBundleInfos;
    }
}
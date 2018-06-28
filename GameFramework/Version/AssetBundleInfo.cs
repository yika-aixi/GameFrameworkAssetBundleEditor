using System.IO;

namespace Icarus.GameFramework.Version
{
    public class AssetBundleInfo
    {
        //所属资源组
        public string GroupTag;
        //资源包名 : xxx.dat 或 xx.xx.dat
        public string PackName;
        //资源包相对路径 : xxx/xxxx
        public string PackPath;
        //资源包路径 : PackPath + "/" + PackName
        public string PackFullName => Path.Combine(PackPath, PackName);
        //资源包MD5
        public string MD5;
        //是否可选,false的话就会一定被下载,true的话可以在游戏中让玩家决定是否下载
        public bool Optional;
    }
}
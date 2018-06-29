using System.Collections.Generic;

namespace Icarus.GameFramework.Version
{
    public interface IVersionCheck
    {
        /// <summary>
        /// version.info远程地址
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// 开始检查
        /// </summary>
        /// <paramref name="completeHandle">参数1：更新列表，参数2：本地版本信息文件</paramref>
        /// <returns>所有需要更新的包</returns>
        void Check(GameFrameworkAction<IEnumerable<AssetBundleInfo>,VersionInfo> completeHandle,GameFrameworkAction<string> errorHandle);
    }
}
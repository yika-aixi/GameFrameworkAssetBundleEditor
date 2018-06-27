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
        /// <returns>所有需要更新的包</returns>
        IEnumerable<AssetBundleInfo> Check();
    }
}
using Icarus.GameFramework.Version;
using System.Collections.Generic;

namespace Icarus.GameFramework.UpdateAssetBundle
{
    public interface IUpdateAssetBundle
    {
        /// <summary>
        /// 更新资源包
        /// </summary>
        /// <param name="updateInfo">更新信息</param>
        /// <param name="assetBundleifInfos">更新列表</param>
        /// <param name="localVersionInfo">本地versionInfo</param>
        /// <param name="anyCompleteHandle">更新好一个</param>
        /// <param name="allCompleteHandle">全部更新完成</param>
        /// <param name="errorHandle">更新出错</param>
        void UpdateAssetBundle(UpdateInfo updateInfo,IEnumerable<AssetBundleInfo> assetBundleifInfos, VersionInfo localVersionInfo,
            GameFrameworkAction<AssetBundleInfo> anyCompleteHandle, GameFrameworkAction allCompleteHandle,GameFrameworkAction<string> errorHandle);
    }
}
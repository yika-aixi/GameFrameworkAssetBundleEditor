# GameFrameworkAssetBundleEditor
该分支我会将UGF的资源打包及加载那块从一个version文件分解为和ab包对应的vetsion文件,还有一个version文件
(用来记录当前打包的所有ab的version文件名,StreamingAssets目录用到,目前这样做会有点问题[就是可能你StreamingAssets你删除了某个不要的
但是因为包名被记录了,去加载时找不到就出错了],我后面会改成外部工具生成)
新的version名为:
	ab包名_变体名_~version.dat

新的加载逻辑为:
	先加载StreamingAssets目录下的version.dat文件,然后解析加载StreamingAssets下的ab包version,加载完成后加载持久化目录下的
	所有ab包version文件,如果资源路径一样的将会被覆盖
	

版本更新的逻辑:
	现在只会生成Package目录下的ab包,该目录可以被复制到StreamingAssets目录(可以手动选择复制,也可以在打包时勾选CopyStreamingAssets选项),
	新增了一个Zip目录.
	目前的规划是:将Zip下对应平台文件夹下所有文件夹及文件复制到远程,然后游戏中会进行资源包对比是否需要更新,可选的资源如果本地version.info中
	没有存在就不下载,存在才会进行比较是否需要更新
	
	Version.info文件的实体结构:
```	
	public class AssetBundleInfo
	{
		//所属资源组
		public string GroupTag;
		//资源包名 : xxx.dat
		public string PackName;
		//资源包相对路径 : xxx/xxxx
		public string PackPath;
		//资源包路径 : PackPath + "/" + PackName
		public string PackFullName;
		//资源包MD5
		public string MD5;
		//是否可选,false的话就会一定被下载,true的话可以在游戏中让玩家决定是否下载
		public bool Optional;
	}
	
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
		
```
我先写出来测测看=-


框架地址:https://github.com/EllanJiang/UnityGameFramework

框架文档:http://gameframework.cn/

使用 AssetBundle 编辑工具教程:http://gameframework.cn/archives/320

使用 AssetBundle 构建工具教程:http://gameframework.cn/archives/356

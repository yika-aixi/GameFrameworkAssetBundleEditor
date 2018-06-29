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
	
目前已经完成,使用非常简单,打包完成后,只需将Zip文件夹中对应平台文件下的所有文件复制到服务器中就好了,然后:
DefaultVersionCheckComPontent.Url = "服务器的version.info地址";
DefaultVersionCheckComPontent.Check()
DefaultUpdateAssetBundle.UpdateAssetBundle()
结束


后面删除一些没用的残留代码后就会加入资源组的功能,目前没有资源组

和E大的AssetBundleTool不同的地方有2个地方:
	1.AssetBundle Editor中的:packed选项变为了标记改资源包是否可选(默认是必须的)
	2.AssetBundle Builder中:增加了一个CopyStreamingAssets的功能

框架地址:https://github.com/EllanJiang/UnityGameFramework

框架文档:http://gameframework.cn/

使用 AssetBundle 编辑工具教程:http://gameframework.cn/archives/320

使用 AssetBundle 构建工具教程:http://gameframework.cn/archives/356

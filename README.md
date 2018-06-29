# GameFrameworkAssetBundleEditor

新的version名为:

	ab包名_变体名_~version.dat

新的加载逻辑为:

	先加载StreamingAssets目录下的version.dat文件,然后解析加载StreamingAssets下的ab包version,加载完成后加载持久化目录下的
	所有ab包version文件,如果资源路径一样的将会被覆盖
	

版本更新的逻辑:

	现在只会生成Package目录下的ab包,该目录可以被复制到StreamingAssets目录(可以手动选择复制,也可以在打包时勾选CopyStreamingAssets选项),
	新增了一个Zip目录.目前的规划是:将Zip下对应平台文件夹下所有文件夹及文件复制到远程,然后游戏中会进行资源包对比是否需要更新,可选
	的资源如果本地version.info中没有存在记录就不比较,存在才会进行比较是否需要更新
	
目前已经完成,使用非常简单,打包完成后,只需将Zip文件夹中对应平台文件下的所有文件复制到服务器中就好了,然后:
```
DefaultVersionCheckComPontent.Url = "服务器的version.info地址";
DefaultVersionCheckComPontent.Check()
DefaultUpdateAssetBundle.UpdateAssetBundle()

```
具体的使用参考`Example\test.cs`

和E大的AssetBundleTool不同的地方有2个地方以及删除了一些东西:
	1.AssetBundle Editor中的:packed选项变为了标记改资源包是否可选(默认是必须的)
	2.AssetBundle Builder中:增加了一个CopyStreamingAssets的功能
	3.删除了zip的压缩相关
	
如果手动增删改过StreamingAssets目录下的资源,请执行一下 ` Icarus/Game Framework/AssetBundle Tools/StreamingAssets Version 生成 `
以便更新资源记录文件和当前修改的同步

框架地址:https://github.com/EllanJiang/UnityGameFramework

框架文档:http://gameframework.cn/

使用 AssetBundle 编辑工具教程:http://gameframework.cn/archives/320

使用 AssetBundle 构建工具教程:http://gameframework.cn/archives/356

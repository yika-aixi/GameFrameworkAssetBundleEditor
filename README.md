# GameFrameworkAssetBundleEditor
该分支我会将UGF的资源打包及加载那块从一个version文件分解为和ab包对应的vetsion文件,还有一个version文件
(用来记录当前打包的所有ab的version文件名,StreamingAssets目录用到,目前这样做会有点问题[就是可能你StreamingAssets你删除了某个不要的
但是因为包名被记录了,去加载时找不到就出错了],我后面会改成外部工具生成)
新的version名为:
	ab包名_变体名_~version.dat

新的加载逻辑为:
	先加载StreamingAssets目录下的version.dat文件,然后解析加载StreamingAssets下的ab包version,加载完成后加载持久化目录下的
	所有ab包version文件,如果资源路径一样的将会被覆盖


框架地址:https://github.com/EllanJiang/UnityGameFramework

框架文档:http://gameframework.cn/

使用 AssetBundle 编辑工具教程:http://gameframework.cn/archives/320

使用 AssetBundle 构建工具教程:http://gameframework.cn/archives/356

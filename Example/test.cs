using System;
using System.ComponentModel;
using Icarus.GameFramework;
using Icarus.GameFramework.Event;
using Icarus.GameFramework.Resource;
using Icarus.GameFramework.UpdateAssetBundle;
using Icarus.GameFramework.Version;
using UnityEngine;
using Icarus.UnityGameFramework.Runtime;

public class test : MonoBehaviour
{
    public string AssetName;
    public UpdateInfo Info;
    private EventComponent _eventComponent;
    private BaseComponent _baseComponent;
    private ResourceComponent _resourceComponent;

    // Use this for initialization
    void Start()
    {
        _eventComponent = GameEntry.GetComponent<EventComponent>();
        _baseComponent = GameEntry.GetComponent<BaseComponent>();
        _resourceComponent = GameEntry.GetComponent<ResourceComponent>();
        if (!_baseComponent)
        {
            Debug.LogError("Base component is invalid.");
            return;
        }
        if (!_baseComponent.EditorResourceMode)
        {
            //进行资源版本检测
            var versionCheck = GameEntry.GetComponent<DefaultVersionCheckComPontent>();
            if (!versionCheck)
            {
                Debug.LogError("Default VersionCheck ComPontent is invalid.");
                return;
            }

            versionCheck.Url = Info.AssetBundleUrl+"/"+ConstTable.VersionFileName;
            versionCheck.Check((x,y) =>
            {
                foreach (var info in x)
                {
                    Debug.Log("需要更新的资源:"+info);
                }

                DefaultUpdateAssetBundle update = GameEntry.GetComponent<DefaultUpdateAssetBundle>();
                update.UpdateAssetBundle(Info, x,y, (pro, str) =>
                {
                    Debug.Log($"下载进度：{pro.Progress}，下载速度：{pro.Speed},下载描述：{str}");
                }, x1 =>
                {
                    Debug.Log("更新完成：" + y);
                }, _loadAsset, ex =>
                {
                    Debug.Log("更新出错："+ex);
                });
            }, Debug.LogError);
            
        }
        else
        {
            _load();
        }
    }

    void _loadAsset()
    {
        var eventArgs = ReferencePool.Acquire<Icarus.UnityGameFramework.Runtime.ResourceInitCompleteEventArgs>();
        _eventComponent.Subscribe(eventArgs.Id, _loadAsset);
        ReferencePool.Release(eventArgs);
        _resourceComponent.InitResources();
    }

    private void _loadAsset(object sender, GameEventArgs e)
    {
        _load();
    }

    [ContextMenu("加载资源")]
    void _load()
    {
        _resourceComponent.LoadAsset(AssetName, new LoadAssetCallbacks(_loadAssetSuccessCallback,  _loadAssetFailureCallback, _loadAssetUpdateCallback));
    }

    private void _loadAssetUpdateCallback(string assetname, float progress, object userdata)
    {
        Debug.LogFormat("加载中.加载进度:{0}", progress);
    }

    private void _loadAssetFailureCallback(string assetname, LoadResourceStatus status, string errormessage, object userdata)
    {
        Debug.LogErrorFormat("加载失败.资源名:{0},状态:{1},错误信息:{2}", assetname, status, errormessage);
    }

    private void _loadAssetSuccessCallback(string assetname, object asset, float duration, object userdata)
    {
        var gameobject = (GameObject) asset;
        Instantiate(gameobject);
        Debug.LogFormat("资源名为:{0},duration:{1}", assetname, duration);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

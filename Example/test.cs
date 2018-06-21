using System;
using System.ComponentModel;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

public class test : MonoBehaviour
{
    public string AssetName;
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
            _resourceComponent.SetResourceMode(ResourceMode.Package);
            var eventArgs = ReferencePool.Acquire<UnityGameFramework.Runtime.ResourceInitCompleteEventArgs>();
            _eventComponent.Subscribe(eventArgs.Id, _loadAsset);
            ReferencePool.Release(eventArgs);
            _resourceComponent.InitResources();
        }
        else
        {
            _load();
        }
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

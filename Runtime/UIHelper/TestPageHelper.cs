﻿using huqiang;
using huqiang.Core.HGUI;
using huqiang.Data;
using huqiang.UIEvent;
using System.IO;
using UnityEngine;


/// <summary>
/// 挂载在Canvas下
/// </summary>
public class TestPageHelper:UIHelper
{
    public virtual void LoadBundle()
    {
#if UNITY_EDITOR
        if (ElementAsset.bundles.Count == 0)
        {
            //var dic = Application.dataPath + "/StreamingAssets";
            var dic = Application.streamingAssetsPath;
            if (Directory.Exists(dic))
            {
                var bs = Directory.GetFiles(dic, "*.unity3d");
                for (int i = 0; i < bs.Length; i++)
                {
                    ElementAsset.bundles.Add(AssetBundle.LoadFromFile(bs[i]));
                }
            }
        } 
#endif
    }
    public TextAsset bytesUI;
    public string AssetName = "baseUI";
    public string dicpath;
    public string CloneName;
    private void Awake()
    {
        Initital();
        HCanvas.MainCanvas = GetComponent<HCanvas>();
        LoadTestPage();
    }
    public void Initital()
    {
        LoadBundle();
        HGUIManager.Initial(transform);
        var db = HGUIManager.GetPrefab(transform);
        HGUIManager.prefabAssets.Clear();
        HGUIManager.LoadModels(db, AssetName);
        var c = transform.childCount;
        for (int i = 0; i < c; i++)
            GameObject.Destroy(transform.GetChild(i).gameObject);
        App.Initial(transform);
    }
    
    public virtual void LoadTestPage()
    {
    }

    private void Update()
    {
        OnUpdate();
    }
    public virtual void OnUpdate()
    {
    }
    public virtual void OnDestroy()
    {
        App.Dispose();
        AssetBundle.UnloadAllAssetBundles(true);
        ElementAsset.bundles.Clear();
    }
}
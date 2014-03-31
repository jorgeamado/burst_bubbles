using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ResourceLoader
{
	AssetBundle bundle;
	public object Value;
	WWW www;

	public AssetBundleRequest Load(string name, Type type)
	{
		Debug.Log("loading " + name);
		if (null == bundle)
		{
			bundle = www.assetBundle;
			www.Dispose();
		}
		return bundle.LoadAsync(name, type);
	}

	public WWW LoadBundle(string path)
	{
		Caching.CleanCache();
		string url = "file://" + Application.dataPath + path;
		Debug.Log("loading bundle " + url);
		www = WWW.LoadFromCacheOrDownload(url, 1);
		return www;
	}

}

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
		Debug.Log("Loading " + name);
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
		#if UNITY_EDITOR
		string url = "file://" + Application.dataPath + path;
		#elif UNITY_STANDALONE
		string url = "file://" + Application.dataPath + "/../.." + path;
		#endif

		Debug.Log("Loading bundle " + url);
		www = WWW.LoadFromCacheOrDownload(url, 1);
		return www;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureFactory
{
	private static TextureFactory instance = null;
	public static TextureFactory Instance
	{
		get 
		{
			if (null == instance)
				instance = new TextureFactory();
			return instance;
		}
	}

	private Dictionary<int, Texture2D> TexturesDictionary = new Dictionary<int, Texture2D>();

	public void GenerateTextures()
	{
		foreach (var texture in TexturesDictionary)
		{
			UnityEngine.Object.DestroyImmediate(texture.Value);
		}
		TexturesDictionary.Clear();

		for (int textureSize = 32; textureSize <= 256; textureSize *= 2)
		{
			Texture2D generatedtTexture = MakePicture(textureSize);
			Texture2D generatedtTextureOpponent = MakePictureCross(textureSize);
			TexturesDictionary.Add(textureSize, generatedtTexture);
			TexturesDictionary.Add(textureSize + 1, generatedtTextureOpponent);
		}
	}

	private Texture2D MakePictureCross(int textureSize)
	{
		var generatedtTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
		var halftexture = generatedtTexture.height / 2;
		float sqrRadius = halftexture * halftexture;
		var transparentColor = new Color(0, 0, 0, 0);
		for (int horiz = 0; horiz < textureSize; ++horiz)
		{
			for (int vert = 0; vert < textureSize; ++vert)
			{
				if (((vert - halftexture) * (vert - halftexture) + (horiz - halftexture) * (horiz - halftexture)) < sqrRadius)
				{
					if (Mathf.Abs(vert - halftexture) <= 2 || Mathf.Abs(horiz - halftexture) <= 2)
						generatedtTexture.SetPixel(horiz, vert, Color.black);
					else
						generatedtTexture.SetPixel(horiz, vert, Color.white);
				}
				else
					generatedtTexture.SetPixel(horiz, vert, transparentColor);
			}
		}
		generatedtTexture.Apply();
		return generatedtTexture;
	}

	private Texture2D MakePicture(int textureSize)
	{
		var generatedtTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
		var halftexture = generatedtTexture.height / 2;
		var transparentColor = new Color(0, 0, 0, 0);
		float sqrRadius = halftexture * halftexture;

		for (int horiz = 0; horiz < textureSize; ++horiz)
		{
			for (int vert = 0; vert < textureSize; ++vert)
			{
				if (((vert - halftexture) * (vert - halftexture) + (horiz - halftexture) * (horiz - halftexture)) < sqrRadius)
				{
					if (Mathf.Abs((vert - halftexture) * (vert - halftexture) -(horiz - halftexture)) <= 2
						|| Mathf.Abs((horiz - halftexture) * (horiz - halftexture) - (vert - halftexture))<= 2)
						generatedtTexture.SetPixel(horiz, vert, Color.black);
					else
						generatedtTexture.SetPixel(horiz, vert, Color.white);
				}
				else
					generatedtTexture.SetPixel(horiz, vert, transparentColor);
			}
		}
		generatedtTexture.Apply();
		return generatedtTexture;
	}

	public Texture2D GetTexture(float normilizedSize, bool opponent = false)
	{
		if (normilizedSize < 0.25f)
			return TexturesDictionary[32 + (opponent ? 1 : 0)];
		else if (normilizedSize < 0.5f)
			return TexturesDictionary[64 + (opponent ? 1 : 0)];
		else if (normilizedSize < 0.75f)
			return TexturesDictionary[128 + (opponent ? 1 : 0)];
		else 
			return TexturesDictionary[256 + (opponent ? 1 : 0)];
	}
}

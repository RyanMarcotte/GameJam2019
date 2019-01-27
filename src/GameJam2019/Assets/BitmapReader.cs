﻿using UnityEngine;

public class BitmapReader
{
	public TileType[,] Read(string levelPath)
	{
		var levelBitmap = Resources.Load<Texture2D>(levelPath);
		var tileMap = new TileType[levelBitmap.height, levelBitmap.width];
		for (int x = 0; x < levelBitmap.width; x++)
		{
			for (int y = 0; y < levelBitmap.height; y++)
			{
				tileMap[y, x] = GetTileType(levelBitmap.GetPixel(x, y));
			}
		}
		return tileMap;
	}

	private TileType GetTileType(Color pixelColour)
	{
		if (pixelColour == Color.black)
			return TileType.Stump;
		if (pixelColour == Color.blue)
			return TileType.Log;
		if (pixelColour == Color.red)
			return TileType.Tree;
		if (pixelColour == Color.green)
			return TileType.Bush;
		return TileType.Ground;
	}
}
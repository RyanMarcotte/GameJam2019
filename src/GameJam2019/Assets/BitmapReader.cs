using System;
using UnityEngine;

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
		if (pixelColour == Color.magenta)
			return TileType.ShakeSprite;
		if (pixelColour == Color.cyan)
			return TileType.NoiseSprite;
		if (pixelColour == new Color(1.0f, 1.0f, 0.0f))
			return TileType.Rock;

		if (new Color(
				(float)Math.Round(pixelColour.r, 1, MidpointRounding.ToEven),
				(float)Math.Round(pixelColour.b, 1, MidpointRounding.ToEven),
				(float)Math.Round(pixelColour.g, 1, MidpointRounding.ToEven)) == Color.grey)
			return TileType.House;
		return TileType.Ground;
	}
}
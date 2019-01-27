using UnityEngine;

public class BitmapReader
{
	public TileType[,] Read()
	{
		var levelBitmap = Resources.Load<Texture2D>( "Levels/lightMapTest" );
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
			return TileType.Obstacle;
		return TileType.Ground;
	}
}
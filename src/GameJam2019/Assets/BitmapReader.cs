using UnityEngine;

public class BitmapReader
{
	public TileType[,] Read()
	{
		var levelBitmap = Resources.Load<Texture2D>( "Levels/test" );
		var tileMap = new TileType[levelBitmap.height, levelBitmap.width];
		for (int i = 0; i < levelBitmap.width; i++)
		{
			for (int j = 0; j < levelBitmap.height; j++)
			{
				tileMap[i, j] = GetTileType(levelBitmap.GetPixel(j, i));
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
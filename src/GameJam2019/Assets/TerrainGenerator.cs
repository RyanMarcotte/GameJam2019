using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TerrainGenerator : MonoBehaviour
{
	public Tile GroundTile;
	public Tile ObstacleTile;

	public Tilemap Floor;
	public Tilemap Obstacles;
	public GameObject LightMap;

	// Start is called before the first frame update
	void Start()
	{
		var bitmapReader = new BitmapReader();
		var levelMap = bitmapReader.Read();

		if (levelMap == null || levelMap.GetLength(0) == 0 || levelMap.GetLength(1) == 0)
		{
			GenerateRandomMap(64, 32);
			return;
		}

		int height = levelMap.GetLength(0);
		int width = levelMap.GetLength(1);

		for (int y = GetLowerBound(height); y < GetUpperBound(height); y++)
		{
			for (int x = GetLowerBound(width); x < GetUpperBound(width); x++)
			{
				var tile = levelMap[y + (height / 2), x + (width / 2)];
				if (y == GetLowerBound(height)
				    || y == GetUpperBound(height) - 1
				    || x == GetUpperBound(width) - 1
				    || x == GetLowerBound(width))
				{
					Obstacles.SetTile(new Vector3Int(x, y, -1), ObstacleTile);
				}

				if (tile == TileType.Ground)
					Floor.SetTile(new Vector3Int(x, y, 0), GroundTile);
				
				if (tile == TileType.Obstacle)
				{
					Obstacles.SetTile(new Vector3Int(x, y, -1), ObstacleTile);
				}
			}
		}
	}

	private void GenerateRandomMap(int height, int width)
	{
		var random = new System.Random();
		for (int y = GetLowerBound(height); y < GetUpperBound(height); y++)
		{
			for (int x = GetLowerBound(width); x < GetUpperBound(width); x++)
			{
				if (y == GetLowerBound(height)
					|| y == GetUpperBound(height) - 1
					|| x == GetUpperBound(width) - 1
					|| x == GetLowerBound(width))
					Obstacles.SetTile(new Vector3Int(x, y, -1), ObstacleTile);
				Floor.SetTile(new Vector3Int(x, y, 0), GroundTile);
				if (random.Next(1, 10) > 8)
					Obstacles.SetTile(new Vector3Int(x, y, -1), ObstacleTile);
			}
		}
	}

	private int GetLowerBound(int value)
	{
		return value / 2 * -1;
	}

	private int GetUpperBound(int value)
	{
		return value / 2;
	}
}
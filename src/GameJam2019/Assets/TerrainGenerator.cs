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

	// Start is called before the first frame update
	void Start()
	{
		var bitmapReader = new BitmapReader();
		var levelMap = bitmapReader.Read();

		var tileXIndex = 0;
		var tileYIndex = 0;

		if (levelMap == null || levelMap.GetLength(0) == 0 || levelMap.GetLength(1) == 0)
		{
			GenerateRandomMap(64, 64);
			return;
		}

		int height = levelMap.GetLength(0);
		int width = levelMap.GetLength(1);

		for (int i = GetLowerBound(height); i < GetUpperBound(height); i++)
		{
			for (int j = GetLowerBound(width); j < GetUpperBound(width); j++)
			{
				var tile = levelMap[tileYIndex, tileXIndex];
				if (i == GetLowerBound(height)
					|| i == GetUpperBound(height) - 1
					|| j == GetUpperBound(width) - 1
					|| j == GetLowerBound(width))
					Obstacles.SetTile(new Vector3Int(i, j, -1), ObstacleTile);
				if (tile == TileType.Ground)
					Floor.SetTile(new Vector3Int(i, j, 0), GroundTile);
				if (tile == TileType.Obstacle)
					Obstacles.SetTile(new Vector3Int(i, j, -1), ObstacleTile);

				tileYIndex++;
			}

			tileXIndex++;
			tileYIndex = 0;
		}
	}

	private void GenerateRandomMap(int height, int width)
	{
		var random = new System.Random();
		for (int i = GetLowerBound(height); i < GetUpperBound(height); i++)
		{
			for (int j = GetLowerBound(width); j < GetUpperBound(width); j++)
			{
				if (i == GetLowerBound(height)
					|| i == GetUpperBound(height) - 1
					|| j == GetUpperBound(width) - 1
					|| j == GetLowerBound(width))
					Obstacles.SetTile(new Vector3Int(i, j, -1), ObstacleTile);
				Floor.SetTile(new Vector3Int(i, j, 0), GroundTile);
				if (random.Next(1, 10) > 8)
					Obstacles.SetTile(new Vector3Int(i, j, -1), ObstacleTile);
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
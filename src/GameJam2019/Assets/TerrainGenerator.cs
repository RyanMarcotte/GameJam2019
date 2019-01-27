using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
	public Tile GroundTile;
	public Tile ObstacleTile;

	public Tilemap Floor;
	public Tilemap Obstacles;
	public GameObject LightMap;

	public int SizeX { get; private set; }
	public int SizeY { get; private set; }

	// Awake is called before Start
	// Start is called before the first frame update
	void Awake()
	{
		var bitmapReader = new BitmapReader();
		var levelMap = bitmapReader.Read();

		if (levelMap == null || levelMap.GetLength(0) == 0 || levelMap.GetLength(1) == 0)
		{
			GenerateRandomMap(64, 32);
			return;
		}

		SizeY = levelMap.GetLength(0);
		SizeX = levelMap.GetLength(1);

		for (int y = GetLowerBound(SizeY); y < GetUpperBound(SizeY); y++)
		{
			for (int x = GetLowerBound(SizeX); x < GetUpperBound(SizeX); x++)
			{
				var tile = levelMap[y + (SizeY / 2), x + (SizeX / 2)];
				if (y == GetLowerBound(SizeY))
					Obstacles.SetTile(new Vector3Int(x, y - 1, -1), ObstacleTile);

				if (y == GetUpperBound(SizeY) - 1)
					Obstacles.SetTile(new Vector3Int(x, y + 1, -1), ObstacleTile);

				if (x == GetUpperBound(SizeX) - 1)
					Obstacles.SetTile(new Vector3Int(x + 1, y, -1), ObstacleTile);

				if (x == GetLowerBound(SizeX))
					Obstacles.SetTile(new Vector3Int(x - 1, y, -1), ObstacleTile);

				if (tile == TileType.Ground)
					Floor.SetTile(new Vector3Int(x, y, 0), GroundTile);
				
				if (tile == TileType.Obstacle)
					Obstacles.SetTile(new Vector3Int(x, y, -1), ObstacleTile);
			}
		}

		if (LightMap == null)
			return;
		
		var lightMapController = LightMap.GetComponent<LightMapController>();
		if (lightMapController == null)
			throw new InvalidOperationException("Could not retrieve LightMapController component!!");

		lightMapController.SetLightmapData(SizeX, SizeY, GenerateLightMap(levelMap));
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

	private LineSegment[] GenerateLightMap(TileType[,] tileMap)
	{
		int width = tileMap.GetLength(1);
		int height = tileMap.GetLength(0);

		var lineSegmentCollection = new List<LineSegment>();
		var topLeftCorner = new Vector2(-width / 2, height / 2);
		var topRightCorner = new Vector2(width / 2, height / 2);
		var bottomLeftCorner = new Vector2(-width / 2, -height / 2);
		var bottomRightCorner = new Vector2(width / 2, -height / 2);

		lineSegmentCollection.Add(new LineSegment(topLeftCorner, bottomLeftCorner));
		lineSegmentCollection.Add(new LineSegment(bottomLeftCorner, bottomRightCorner));
		lineSegmentCollection.Add(new LineSegment(bottomRightCorner, topRightCorner));
		lineSegmentCollection.Add(new LineSegment(topRightCorner, topLeftCorner));

		bool[,] cellVisited = new bool[height, width];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				if (cellVisited[y, x]) { continue; }

				var rectangle = GetRectangleFromCell(ref tileMap, ref cellVisited, x, y);
				if (rectangle == null)
					continue;

				var rect = rectangle.Value;
				var rectTopLeftCorner = new Vector2(rect.xMin - width / 2, rect.yMax - height / 2);
				var rectTopRightCorner = new Vector2(rect.xMax - width / 2, rect.yMax - height / 2);
				var rectBottomLeftCorner = new Vector2(rect.xMin - width / 2, rect.yMin - height / 2);
				var rectBottomRightCorner = new Vector2(rect.xMax - width / 2, rect.yMin - height / 2);

				lineSegmentCollection.Add(new LineSegment(rectTopLeftCorner, rectBottomLeftCorner));
				lineSegmentCollection.Add(new LineSegment(rectBottomLeftCorner, rectBottomRightCorner));
				lineSegmentCollection.Add(new LineSegment(rectBottomRightCorner, rectTopRightCorner));
				lineSegmentCollection.Add(new LineSegment(rectTopRightCorner, rectTopLeftCorner));
			}
		}

		return lineSegmentCollection.ToArray();
	}

	private Rect? GetRectangleFromCell(ref TileType[,] tileMap, ref bool[,] cellVisited, int cellX, int cellY)
	{
		var type = tileMap[cellY, cellX];
		if (type != TileType.Obstacle && type != TileType.Wall)
			return null;

		int widthOfTileMap = tileMap.GetLength(1);
		int heightOfTileMap = tileMap.GetLength(0);

		int width = 0;
		int height = 0;

		// Analyze current row to determine run-length of like-typed tiles
		// (Terminate when a different tile is encountered)
		for (int x = cellX; ((x < widthOfTileMap) && (tileMap[cellY, x] == type) && (!cellVisited[cellY, x])); ++x, ++width)
			cellVisited[cellY, x] = true;

		// Analyze rows below the original run to determine if they also have like-typed tiles
		bool keepGoing = true;
		for (int y = cellY; ((keepGoing) && (y < heightOfTileMap)); ++y)
		{
			for (int x = cellX; x < (cellX + width); ++x)
			{
				if (tileMap[y, x] != type)
					keepGoing = false;
			}

			// Test if tiles to the left and to the right of the run are like-typed
			// (Terminate if BOTH tiles share a type with the current run)
			if (width != widthOfTileMap)
			{
				try
				{
					bool leftCellIsSameType = (cellX > 0) && (tileMap[y, cellX - 1] == type);
					bool leftCellNotVisited = (cellX > 0) && (!cellVisited[y, cellX - 1]);
					bool leftAdjacentIsSame = (cellX <= 0) || ((leftCellIsSameType) && (leftCellNotVisited));

					int rightEdge = (cellX + width);

					bool rightCellIsSameType = (rightEdge < widthOfTileMap - 1) && tileMap[y, rightEdge + 1] == type;
					bool rightCellNotVisited = (rightEdge < widthOfTileMap - 1) && (!cellVisited[y, rightEdge + 1]);
					bool rightAdjacentIsSame = (rightEdge >= widthOfTileMap - 1) || ((rightCellIsSameType) && (rightCellNotVisited));

					if ((leftAdjacentIsSame) && (rightAdjacentIsSame)) { keepGoing = false; }
				}
				catch
				{
					throw (new Exception("For " + cellX + "," + cellY + " - rightEdge = " + (cellX + width) + ", data.Width = " + (widthOfTileMap - 1)));
				}
			}

			// Mark all cells on new row as visited
			if (!keepGoing) continue;
			
			++height;
			for (int x = cellX; x < (cellX + width); ++x)
				cellVisited[y, x] = true;
		}

		return new Rect(cellX, cellY, width, height);
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class TerrainGenerator : MonoBehaviour
{
	//public Tile GroundTile;
	//public Tile ObstacleTile;
	//public Tile[] ObstacleTiles;

	public Tilemap Floor;
	public Tilemap Obstacles;
	public GameObject LightMap;

	public GameObject ShakeSprite;
	public GameObject NoiseSprite;

	private readonly System.Random _random = new System.Random();
	private Tile _tree_topleft;
	private Tile _tree_topright;
	private Tile _tree_bottomleft;
	private Tile _tree_bottomright;
	private Tile _groundTile;
	private Tile _stump;
	private Tile _log;
	private Tile _logv;
	private Tile _bush;

	public int SizeX { get; private set; }
	public int SizeY { get; private set; }

	// Awake is called before Start
	// Start is called before the first frame update
	void Awake()
	{
		_groundTile = Resources.Load<Tile>("tiles/floor");
		_stump = Resources.Load<Tile>("tiles/stump");
		_log = Resources.Load<Tile>("tiles/log");
		_logv = Resources.Load<Tile>("tiles/logv");
		_bush = Resources.Load<Tile>("tiles/bush");
		_tree_topleft = Resources.Load<Tile>("tiles/treetopleft");
		_tree_topright = Resources.Load<Tile>("tiles/treetopright");
		_tree_bottomleft = Resources.Load<Tile>("tiles/treebottomleft");
		_tree_bottomright = Resources.Load<Tile>("tiles/treebottomright");

		var bitmapReader = new BitmapReader();
		var levelMap = bitmapReader.Read("Levels/map01");

		if (levelMap == null || levelMap.GetLength(0) == 0 || levelMap.GetLength(1) == 0)
		{
			throw new InvalidOperationException("Missing valid map");
		}

		SizeY = levelMap.GetLength(0);
		SizeX = levelMap.GetLength(1);

		for (int y = GetLowerBound(SizeY); y < GetUpperBound(SizeY); y++)
		{
			for (int x = GetLowerBound(SizeX); x < GetUpperBound(SizeX); x++)
			{
				var tile = levelMap[y + (SizeY / 2), x + (SizeX / 2)];
				if (y == GetLowerBound(SizeY))
					Obstacles.SetTile(new Vector3Int(x, y - 1, -1), _stump);

				if (y == GetUpperBound(SizeY) - 1)
					Obstacles.SetTile(new Vector3Int(x, y + 1, -1), _stump);

				if (x == GetUpperBound(SizeX) - 1)
					Obstacles.SetTile(new Vector3Int(x + 1, y, -1), _stump);

				if (x == GetLowerBound(SizeX))
					Obstacles.SetTile(new Vector3Int(x - 1, y, -1), _stump);

				Floor.SetTile(new Vector3Int(x, y, 0), _groundTile);

				if (tile == TileType.NoiseSprite)
				{
					PlaceSprite(x, y, NoiseSprite);
				}

				if (tile == TileType.ShakeSprite)
				{
					PlaceSprite(x, y, ShakeSprite);
				}

				if (tile != TileType.Ground)
					Obstacles.SetTile(
						new Vector3Int(x, y, -1),
						GetTile(tile, x, y));
			}
		}

		if (LightMap == null)
			return;

		var lightMapController = LightMap.GetComponent<LightMapController>();
		if (lightMapController == null)
			throw new InvalidOperationException("Could not retrieve LightMapController component!!");

		lightMapController.SetLightmapData(SizeX, SizeY, GenerateLightMap(levelMap));
	}

	private void PlaceSprite(int x, int y, GameObject gObject)
	{
		var sprite = Instantiate(gObject);
		sprite.transform.position = new Vector3(x, y, 0);
		sprite.SetActive(true);
	}

	private TileBase GetTile(
		TileType tile,
		int x,
		int y)
	{
		if (tile == TileType.Stump)
			return _stump;
		if (tile == TileType.Bush)
			return _bush;

		if (tile == TileType.Tree)
		{
			if (!_treeLookAhead.ContainsKey((x, y)))
			{
				_treeLookAhead.Add((x, y), _tree_bottomleft);
				if (BoundsCheck(x, y + 1))
					_treeLookAhead.Add((x, y + 1), _tree_topleft);
				if (BoundsCheck(x + 1, y))
					_treeLookAhead.Add((x + 1, y), _tree_bottomright);
				if (BoundsCheck(x + 1, y + 1))
					_treeLookAhead.Add((x + 1, y + 1), _tree_topright);
			}

			if (_treeLookAhead.ContainsKey((x, y)))
				return _treeLookAhead[(x, y)];
		}

		if (tile == TileType.Log)
		{
			if (!_logLookAhead.ContainsKey((x, y)))
			{
				_logLookAhead.Add((x, y), null);
				if (BoundsCheck(x, y + 1))
					_logLookAhead.Add((x, y + 1), _logv);
				if (BoundsCheck(x + 1, y))
					_logLookAhead.Add((x + 1, y), _log);
				if (BoundsCheck(x + 2, y))
					_logLookAhead.Add((x + 2, y), null);
				if (BoundsCheck(x, y + 2))
					_logLookAhead.Add((x, y + 2), null);
			}

			if (_logLookAhead.ContainsKey((x, y)))
				return _logLookAhead[(x, y)];
		}

		return null;
	}

	private bool BoundsCheck(
		int x,
		int y)
	{
		if (x < GetLowerBound(SizeX) || x > GetUpperBound(SizeX))
			return false;
		if (y < GetLowerBound(SizeY) || y > GetUpperBound(SizeY))
			return false;
		return true;
	}

	private IDictionary<(int X, int Y), Tile> _treeLookAhead = new Dictionary<(int X, int Y), Tile>();
	private IDictionary<(int X, int Y), Tile> _logLookAhead = new Dictionary<(int X, int Y), Tile>();

	//private void GenerateRandomMap(int height, int width)
	//{
	//	var random = new System.Random();
	//	for (int y = GetLowerBound(height); y < GetUpperBound(height); y++)
	//	{
	//		for (int x = GetLowerBound(width); x < GetUpperBound(width); x++)
	//		{
	//			if (y == GetLowerBound(height)
	//				|| y == GetUpperBound(height) - 1
	//				|| x == GetUpperBound(width) - 1
	//				|| x == GetLowerBound(width))
	//				Obstacles.SetTile(new Vector3Int(x, y, -1), GetRandomObstacle());
	//			Floor.SetTile(new Vector3Int(x, y, 0), GroundTile);
	//			if (random.Next(1, 10) > 8)
	//				Obstacles.SetTile(new Vector3Int(x, y, -1), GetRandomObstacle());
	//		}
	//	}
	//}

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

		var rectangles = new List<Rect>();
		bool[,] cellVisited = new bool[height, width];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				if (cellVisited[y, x])
				{
					continue;
				}

				var rectangle = GetRectangleFromCell(ref tileMap, ref cellVisited, x, y);
				if (rectangle != null)
					rectangles.Add(rectangle.Value);
			}
		}

		var rectGroups = new List<Rect[]>();
		var rectanglesLeftToProcess = new LinkedList<Tuple<Rect, Bounds>>(rectangles.Select(r => Tuple.Create(r, new Bounds(r.center, r.size * 1.1f))));
		while (rectanglesLeftToProcess.Count > 0)
		{
			var firstRect = rectanglesLeftToProcess.First;
			var currentGroup = new List<Tuple<Rect, Bounds>>(new [] { firstRect.Value });
			var nodesToRemove = new List<LinkedListNode<Tuple<Rect, Bounds>>>(new [] { firstRect });
			var current = firstRect.Next;
			while (current != null)
			{
				if (currentGroup.Any(x => x.Item2.Intersects(current.Value.Item2)))
				{
					currentGroup.Add(current.Value);
					nodesToRemove.Add(current);
				}

				current = current.Next;
			}

			rectGroups.Add(currentGroup.Select(x => x.Item1).ToArray());
			foreach (var nodeToRemove in nodesToRemove)
				rectanglesLeftToProcess.Remove(nodeToRemove);
		}

		lineSegmentCollection.AddRange(rectangles.SelectMany(rect => GetLineSegmentsFromRectangle(rect, width, height)));
		return lineSegmentCollection.ToArray();
	}

	private static Rect? GetRectangleFromCell(
		ref TileType[,] tileMap,
		ref bool[,] cellVisited,
		int cellX,
		int cellY)
	{
		var type = tileMap[cellY, cellX];
		if (type == TileType.Ground)
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

					if ((leftAdjacentIsSame) && (rightAdjacentIsSame))
					{
						keepGoing = false;
					}
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

	private static IEnumerable<LineSegment> GetLineSegmentsFromRectangle(Rect rect, int width, int height)
	{
		var rectTopLeftCorner = new Vector2(rect.xMin - width / 2, rect.yMax - height / 2);
		var rectTopRightCorner = new Vector2(rect.xMax - width / 2, rect.yMax - height / 2);
		var rectBottomLeftCorner = new Vector2(rect.xMin - width / 2, rect.yMin - height / 2);
		var rectBottomRightCorner = new Vector2(rect.xMax - width / 2, rect.yMin - height / 2);

		yield return new LineSegment(rectTopLeftCorner, rectBottomLeftCorner);
		yield return new LineSegment(rectBottomLeftCorner, rectBottomRightCorner);
		yield return new LineSegment(rectBottomRightCorner, rectTopRightCorner);
		yield return new LineSegment(rectTopRightCorner, rectTopLeftCorner);
	}
}
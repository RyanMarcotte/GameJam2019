using System;
using System.Collections.Generic;
using System.Linq;
using Hull;
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

		lineSegmentCollection.AddRange(rectGroups.SelectMany(rectGroup => GetConcaveHullFromRectangleGroup(rectGroup, width, height)));
		return lineSegmentCollection.ToArray();
	}

	private static Rect? GetRectangleFromCell(
		ref TileType[,] tileMap,
		ref bool[,] cellVisited,
		int cellX,
		int cellY)
	{
		var type = tileMap[cellY, cellX];
		if (type != TileType.Tree)
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

	private IEnumerable<LineSegment> GetConcaveHullFromRectangleGroup(Rect[] rectGroup, int width, int height)
	{
		int halfWidth = width / 2;
		int halfHeight = height / 2;
		var points = rectGroup.SelectMany(r => new[]
		{
			new Vector2(r.xMin - halfWidth, r.yMax - halfHeight), // rectTopLeftCorner
			new Vector2(r.xMax - halfWidth, r.yMax - halfHeight), // rectTopRightCorner
			new Vector2(r.xMin - halfWidth, r.yMin - halfHeight), // rectBottomLeftCorner
			new Vector2(r.xMax - halfWidth, r.yMin - halfHeight) // rectBottomRightCorner
		}).Distinct(new Vector2EqualityComparer()).Select((item, index) => new Node(item.x, item.y, index)).ToList();

		Hull.Hull.setConcaveHull(points, -1m, 100, true);
		foreach (var line in Hull.Hull.hull_concave_edges)
		{
			Vector2 left = new Vector2((float)line.nodes[0].x, (float)line.nodes[0].y);
			Vector2 right = new Vector2((float)line.nodes[1].x, (float)line.nodes[1].y);
			yield return new LineSegment(left, right);
			/*var rectTopLeftCorner = new Vector2(rect.xMin - halfWidth, rect.yMax - halfHeight);
			var rectTopRightCorner = new Vector2(rect.xMax - halfWidth, rect.yMax - halfHeight);
			var rectBottomLeftCorner = new Vector2(rect.xMin - halfWidth, rect.yMin - halfHeight);
			var rectBottomRightCorner = new Vector2(rect.xMax - halfWidth, rect.yMin - halfHeight);

			yield return new LineSegment(rectTopLeftCorner, rectBottomLeftCorner);
			yield return new LineSegment(rectBottomLeftCorner, rectBottomRightCorner);
			yield return new LineSegment(rectBottomRightCorner, rectTopRightCorner);
			yield return new LineSegment(rectTopRightCorner, rectTopLeftCorner);*/
		}

		Hull.Hull.Clear();
	}
}



#region Concave Hull

namespace Hull
{
	public static class Hull
	{
		public static List<Node> unused_nodes = new List<Node>();
		public static List<Line> hull_edges = new List<Line>();
		public static List<Line> hull_concave_edges = new List<Line>();

		public static void Clear()
		{
			unused_nodes.Clear();
			hull_edges.Clear();
			hull_concave_edges.Clear();
		}

		public static List<Line> getHull(List<Node> nodes)
		{
			List<Node> convexH = new List<Node>();
			List<Line> exitLines = new List<Line>();

			convexH = new List<Node>();
			convexH.AddRange(GrahamScan.convexHull(nodes));
			for (int i = 0; i < convexH.Count - 1; i++)
			{
				exitLines.Add(new Line(convexH[i], convexH[i + 1]));
			}
			exitLines.Add(new Line(convexH[0], convexH[convexH.Count - 1]));
			return exitLines;
		}

		public static void setConcaveHull(List<Node> nodes, decimal concavity, int scaleFactor, bool isSquareGrid)
		{
			unused_nodes.AddRange(nodes);
			hull_edges.AddRange(getHull(nodes));
			foreach (Line line in hull_edges)
			{
				foreach (Node node in line.nodes)
					unused_nodes.RemoveAll(a => a.id == node.id);
			}

			/* Run setConvexHull before! 
             * Concavity is a value used to restrict the concave angles 
             * it can go from -1 to 1 (it wont crash if you go further)
             * */
			hull_concave_edges = new List<Line>(hull_edges.OrderByDescending(a => Line.getLength(a.nodes[0], a.nodes[1])).ToList());
			Line selected_edge;
			List<Line> aux = new List<Line>(); ;
			int list_original_size;
			int count = 0;
			bool listIsModified = false;
			do
			{
				listIsModified = false;
				count = 0;
				list_original_size = hull_concave_edges.Count;
				while (count < list_original_size)
				{
					selected_edge = hull_concave_edges[0];
					hull_concave_edges.RemoveAt(0);
					aux = new List<Line>();
					if (!selected_edge.isChecked)
					{
						List<Node> nearby_points = HullFunctions.getNearbyPoints(selected_edge, unused_nodes, scaleFactor);
						aux.AddRange(HullFunctions.setConcave(selected_edge, nearby_points, hull_concave_edges, concavity, isSquareGrid));
						listIsModified = listIsModified || (aux.Count > 1);

						if (aux.Count > 1)
						{
							foreach (Node node in aux[0].nodes)
								unused_nodes.RemoveAll(a => a.id == node.id);
						}
						else
						{
							aux[0].isChecked = true;
						}
					}
					else
					{
						aux.Add(selected_edge);
					}
					hull_concave_edges.AddRange(aux);
					count++;
				}
				hull_concave_edges = hull_concave_edges.OrderByDescending(a => Line.getLength(a.nodes[0], a.nodes[1])).ToList();
				list_original_size = hull_concave_edges.Count;
			} while (listIsModified);
		}
	}

	public static class GrahamScan
	{
		const int TURN_LEFT = 1;
		const int TURN_RIGHT = -1;
		const int TURN_NONE = 0;
		public static int turn(Node p, Node q, Node r)
		{
			return ((q.x - p.x) * (r.y - p.y) - (r.x - p.x) * (q.y - p.y)).CompareTo(0);
		}

		public static void keepLeft(List<Node> hull, Node r)
		{
			while (hull.Count > 1 && turn(hull[hull.Count - 2], hull[hull.Count - 1], r) != TURN_LEFT)
			{
				hull.RemoveAt(hull.Count - 1);
			}
			if (hull.Count == 0 || hull[hull.Count - 1] != r)
			{
				hull.Add(r);
			}
		}

		public static double getAngle(Node p1, Node p2)
		{
			double xDiff = p2.x - p1.x;
			double yDiff = p2.y - p1.y;
			return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
		}

		public static List<Node> MergeSort(Node p0, List<Node> arrPoint)
		{
			if (arrPoint.Count == 1)
			{
				return arrPoint;
			}
			List<Node> arrSortedInt = new List<Node>();
			int middle = (int)arrPoint.Count / 2;
			List<Node> leftArray = arrPoint.GetRange(0, middle);
			List<Node> rightArray = arrPoint.GetRange(middle, arrPoint.Count - middle);
			leftArray = MergeSort(p0, leftArray);
			rightArray = MergeSort(p0, rightArray);
			int leftptr = 0;
			int rightptr = 0;
			for (int i = 0; i < leftArray.Count + rightArray.Count; i++)
			{
				if (leftptr == leftArray.Count)
				{
					arrSortedInt.Add(rightArray[rightptr]);
					rightptr++;
				}
				else if (rightptr == rightArray.Count)
				{
					arrSortedInt.Add(leftArray[leftptr]);
					leftptr++;
				}
				else if (getAngle(p0, leftArray[leftptr]) < getAngle(p0, rightArray[rightptr]))
				{
					arrSortedInt.Add(leftArray[leftptr]);
					leftptr++;
				}
				else
				{
					arrSortedInt.Add(rightArray[rightptr]);
					rightptr++;
				}
			}
			return arrSortedInt;
		}

		public static List<Node> convexHull(List<Node> points)
		{
			Node p0 = null;
			foreach (Node value in points)
			{
				if (p0 == null)
					p0 = value;
				else
				{
					if (p0.y > value.y)
						p0 = value;
				}
			}
			List<Node> order = new List<Node>();
			foreach (Node value in points)
			{
				if (p0 != value)
					order.Add(value);
			}

			order = MergeSort(p0, order);
			List<Node> result = new List<Node>();
			result.Add(p0);
			result.Add(order[0]);
			result.Add(order[1]);
			order.RemoveAt(0);
			order.RemoveAt(0);
			foreach (Node value in order)
			{
				keepLeft(result, value);
			}
			return result;
		}
	}

	public class HullFunctions
	{

		public static bool verticalIntersection(Line lineA, Line lineB)
		{
			/* lineA is vertical */
			double y_intersection;
			if ((lineB.nodes[0].x > lineA.nodes[0].x) && (lineA.nodes[0].x > lineB.nodes[1].x) ||
					((lineB.nodes[1].x > lineA.nodes[0].x) && (lineA.nodes[0].x > lineB.nodes[0].x)))
			{
				y_intersection = (((lineB.nodes[1].y - lineB.nodes[0].y) * (lineA.nodes[0].x - lineB.nodes[0].x)) / (lineB.nodes[1].x - lineB.nodes[0].x)) + lineB.nodes[0].y;
				return ((lineA.nodes[0].y > y_intersection) && (y_intersection > lineA.nodes[1].y))
					|| ((lineA.nodes[1].y > y_intersection) && (y_intersection > lineA.nodes[0].y));
			}
			else
			{
				return false;
			}
		}

		public static bool intersection(Line lineA, Line lineB)
		{
			/* Returns true if segments collide
             * If they have in common a segment edge returns false
             * Algorithm obtained from: 
             * http://stackoverflow.com/questions/3838329/how-can-i-check-if-two-segments-intersect
             * Thanks OMG_peanuts !
             * */
			double dif;
			double A1, A2;
			double b1, b2;
			decimal X;

			if (Math.Max(lineA.nodes[0].x, lineA.nodes[1].x) < Math.Min(lineB.nodes[0].x, lineB.nodes[1].x))
			{
				return false; //Not a chance of intersection
			}

			dif = lineA.nodes[0].x - lineA.nodes[1].x;
			if (dif != 0)
			{ //Avoids dividing by 0
				A1 = (lineA.nodes[0].y - lineA.nodes[1].y) / dif;
			}
			else
			{
				//Segment is vertical
				A1 = 9999999;
			}

			dif = lineB.nodes[0].x - lineB.nodes[1].x;
			if (dif != 0)
			{ //Avoids dividing by 0
				A2 = (lineB.nodes[0].y - lineB.nodes[1].y) / dif;
			}
			else
			{
				//Segment is vertical
				A2 = 9999999;
			}

			if (A1 == A2)
			{
				return false; //Parallel
			}
			else if (A1 == 9999999)
			{
				return verticalIntersection(lineA, lineB);
			}
			else if (A2 == 9999999)
			{
				return verticalIntersection(lineB, lineA);
			}

			b1 = lineA.nodes[0].y - (A1 * lineA.nodes[0].x);
			b2 = lineB.nodes[0].y - (A2 * lineB.nodes[0].x);
			X = Math.Round(System.Convert.ToDecimal((b2 - b1) / (A1 - A2)), 4);
			if ((X <= System.Convert.ToDecimal(Math.Max(Math.Min(lineA.nodes[0].x, lineA.nodes[1].x), Math.Min(lineB.nodes[0].x, lineB.nodes[1].x)))) ||
				(X >= System.Convert.ToDecimal(Math.Min(Math.Max(lineA.nodes[0].x, lineA.nodes[1].x), Math.Max(lineB.nodes[0].x, lineB.nodes[1].x)))))
			{
				return false; //Out of bound
			}
			else
			{
				return true;
			}
		}

		public static List<Line> setConcave(Line line, List<Node> nearbyPoints, List<Line> concave_hull, decimal concavity, bool isSquareGrid)
		{
			/* Adds a middlepoint to a line (if there can be one) to make it concave */
			List<Line> concave = new List<Line>();
			decimal cos1, cos2;
			decimal sumCos = -2;
			Node middle_point = null;
			bool edgeIntersects;
			int count = 0;
			int count_line = 0;

			while (count < nearbyPoints.Count)
			{
				edgeIntersects = false;
				cos1 = getCos(nearbyPoints[count], line.nodes[0], line.nodes[1]);
				cos2 = getCos(nearbyPoints[count], line.nodes[1], line.nodes[0]);
				if (cos1 + cos2 >= sumCos && (cos1 > concavity && cos2 > concavity))
				{
					count_line = 0;
					while (!edgeIntersects && count_line < concave_hull.Count)
					{
						edgeIntersects = (intersection(concave_hull[count_line], new Line(nearbyPoints[count], line.nodes[0]))
							|| (intersection(concave_hull[count_line], new Line(nearbyPoints[count], line.nodes[1]))));
						count_line++;
					}
					if (!edgeIntersects)
					{
						// Prevents from getting sharp angles between middlepoints
						Node[] nearNodes = getHullNearbyNodes(line, concave_hull);
						if ((getCos(nearbyPoints[count], nearNodes[0], line.nodes[0]) < -concavity) &&
							(getCos(nearbyPoints[count], nearNodes[1], line.nodes[1]) < -concavity))
						{
							// Prevents inner tangent lines to the concave hull
							if (!(tangentToHull(line, nearbyPoints[count], cos1, cos2, concave_hull) && isSquareGrid))
							{
								sumCos = cos1 + cos2;
								middle_point = nearbyPoints[count];
							}
						}
					}
				}
				count++;
			}
			if (middle_point == null)
			{
				concave.Add(line);
			}
			else
			{
				concave.Add(new Line(middle_point, line.nodes[0]));
				concave.Add(new Line(middle_point, line.nodes[1]));
			}
			return concave;
		}

		public static bool tangentToHull(Line line_treated, Node node, decimal cos1, decimal cos2, List<Line> concave_hull)
		{
			/* A new middlepoint could (rarely) make a segment that's tangent to the hull.
             * This method detects these situations
             * I suggest turning this method of if you are not using square grids or if you have a high dot density
             * */
			bool isTangent = false;
			decimal current_cos1;
			decimal current_cos2;
			double edge_length;
			List<int> nodes_searched = new List<int>();
			Line line;
			Node node_in_hull;
			int count_line = 0;
			int count_node = 0;

			edge_length = Line.getLength(node, line_treated.nodes[0]) + Line.getLength(node, line_treated.nodes[1]);


			while (!isTangent && count_line < concave_hull.Count)
			{
				line = concave_hull[count_line];
				while (!isTangent && count_node < 2)
				{
					node_in_hull = line.nodes[count_node];
					if (!nodes_searched.Contains(node_in_hull.id))
					{
						if (node_in_hull.id != line_treated.nodes[0].id && node_in_hull.id != line_treated.nodes[1].id)
						{
							current_cos1 = getCos(node_in_hull, line_treated.nodes[0], line_treated.nodes[1]);
							current_cos2 = getCos(node_in_hull, line_treated.nodes[1], line_treated.nodes[0]);
							if (current_cos1 == cos1 || current_cos2 == cos2)
							{
								isTangent = (Line.getLength(node_in_hull, line_treated.nodes[0]) + Line.getLength(node_in_hull, line_treated.nodes[1]) < edge_length);
							}
						}
					}
					nodes_searched.Add(node_in_hull.id);
					count_node++;
				}
				count_node = 0;
				count_line++;
			}
			return isTangent;
		}

		public static decimal getCos(Node a, Node b, Node o)
		{
			/* Law of cosines */
			double aPow2 = Math.Pow(a.x - o.x, 2) + Math.Pow(a.y - o.y, 2);
			double bPow2 = Math.Pow(b.x - o.x, 2) + Math.Pow(b.y - o.y, 2);
			double cPow2 = Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2);
			double cos = (aPow2 + bPow2 - cPow2) / (2 * Math.Sqrt(aPow2 * bPow2));
			return Math.Round(System.Convert.ToDecimal(cos), 4);
		}

		public static int[] getBoundary(Line line, int scaleFactor)
		{
			/* Giving a scaleFactor it returns an area around the line 
             * where we will search for nearby points 
             * */
			int[] boundary = new int[4];
			Node aNode = line.nodes[0];
			Node bNode = line.nodes[1];
			int min_x_position = (int)Math.Floor(Math.Min(aNode.x, bNode.x) / scaleFactor);
			int min_y_position = (int)Math.Floor(Math.Min(aNode.y, bNode.y) / scaleFactor);
			int max_x_position = (int)Math.Floor(Math.Max(aNode.x, bNode.x) / scaleFactor);
			int max_y_position = (int)Math.Floor(Math.Max(aNode.y, bNode.y) / scaleFactor);

			boundary[0] = min_x_position;
			boundary[1] = min_y_position;
			boundary[2] = max_x_position;
			boundary[3] = max_y_position;

			return boundary;
		}

		public static List<Node> getNearbyPoints(Line line, List<Node> nodeList, int scaleFactor)
		{
			/* The bigger the scaleFactor the more points it will return
             * Inspired by this precious algorithm:
             * http://www.it.uu.se/edu/course/homepage/projektTDB/ht13/project10/Project-10-report.pdf
             * Be carefull: if it's too small it will return very little points (or non!), 
             * if it's too big it will add points that will not be used and will consume time
             * */
			List<Node> nearbyPoints = new List<Node>();
			int[] boundary;
			int tries = 0;
			int node_x_rel_pos;
			int node_y_rel_pos;

			while (tries < 2 && nearbyPoints.Count == 0)
			{
				boundary = getBoundary(line, scaleFactor);
				foreach (Node node in nodeList)
				{
					//Not part of the line
					if (!(node.x == line.nodes[0].x && node.y == line.nodes[0].y ||
						node.x == line.nodes[1].x && node.y == line.nodes[1].y))
					{
						node_x_rel_pos = (int)Math.Floor(node.x / scaleFactor);
						node_y_rel_pos = (int)Math.Floor(node.y / scaleFactor);
						//Inside the boundary
						if (node_x_rel_pos >= boundary[0] && node_x_rel_pos <= boundary[2] &&
							node_y_rel_pos >= boundary[1] && node_y_rel_pos <= boundary[3])
						{
							nearbyPoints.Add(node);
						}
					}
				}
				//if no points are found we increase the area
				scaleFactor = scaleFactor * 4 / 3;
				tries++;
			}
			return nearbyPoints;
		}

		public static Node[] getHullNearbyNodes(Line line, List<Line> concave_hull)
		{
			/* Return previous and next nodes to a line in the hull */
			Node[] nearbyHullNodes = new Node[2];
			int leftNodeID = line.nodes[0].id;
			int rightNodeID = line.nodes[1].id;
			int currentID;
			int nodesFound = 0;
			int line_count = 0;
			int position = 0;
			int opposite_position = 1;

			while (nodesFound < 2)
			{
				position = 0;
				opposite_position = 1;
				while (position < 2)
				{
					currentID = concave_hull[line_count].nodes[position].id;
					if (currentID == leftNodeID &&
						concave_hull[line_count].nodes[opposite_position].id != rightNodeID)
					{
						nearbyHullNodes[0] = concave_hull[line_count].nodes[opposite_position];
						nodesFound++;
					}
					else if (currentID == rightNodeID &&
					   concave_hull[line_count].nodes[opposite_position].id != leftNodeID)
					{
						nearbyHullNodes[1] = concave_hull[line_count].nodes[opposite_position];
						nodesFound++;
					}
					position++;
					opposite_position--;
				}
				line_count++;
			}
			return nearbyHullNodes;
		}
	}

	public class Line
	{
		public bool isChecked = false;
		public Node[] nodes = new Node[2];
		public Line(Node n1, Node n2)
		{
			nodes[0] = n1;
			nodes[1] = n2;
		}
		public static double getLength(Node node1, Node node2)
		{
			/* It actually calculates relative length */
			double length;
			length = Math.Pow(node1.y - node2.y, 2) + Math.Pow(node1.x - node2.x, 2);
			//length = Math.sqrt(Math.Pow(node1.y - node2.y, 2) + Math.Pow(node1.x - node2.x, 2));
			return length;
		}
	}

	public class Node
	{
		public int id;
		public double x;
		public double y;
		public Node(double x, double y)
		{
			this.x = x;
			this.y = y;
		}
		public Node(double x, double y, int id)
		{
			this.x = x;
			this.y = y;
			this.id = id;
		}
	}
}

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public class LightMapController : MonoBehaviour
{
	public GameObject Player;

	private int _mapWidth;
	private int _mapHeight;
	private QuadTree<LineSegmentAndAssociatedRenderer> _quadTree;
	private LineSegment[] _lineSegmentCollection = {};

	private Bounds GetCameraBounds()
	{
		var camera = Camera.main.GetComponent<Camera>();
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		var boundsSize = new Vector2(cameraHeight * screenAspect, cameraHeight);
		return new Bounds(new Vector2(camera.transform.position.x, camera.transform.position.y), boundsSize);
	}

	private Bounds GetPlayerBounds()
	{
		var position = Player.transform.position;

		var camera = Camera.main.GetComponent<Camera>();
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		var boundsSize = new Vector2(cameraHeight * screenAspect, cameraHeight);
		return new Bounds(new Vector2(position.x, position.y), boundsSize);
	}

	void Start()
	{
		// DEBUGGING ONLY (for camera bounds)
		var bounds = GetPlayerBounds();
		var lineRenderer = Player.AddComponent<LineRenderer>();
		lineRenderer.widthMultiplier = 0.1f;
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.startColor = Color.red;
		lineRenderer.endColor = Color.red;
		lineRenderer.sortingOrder = 105;
	}

	// Update is called once per frame
	void Update()
	{
		foreach (var item in _quadTree.Items)
			item.LineRenderer.enabled = false;

		var cameraBounds = GetCameraBounds();
		
		var linesToTest = _quadTree.Query(cameraBounds).ToArray();
		foreach (var item in linesToTest)
			item.LineRenderer.enabled = true;

		// TODO: calculate the intersections of the lines and wall edges
		var lineRenderer = Player.GetComponent<LineRenderer>();
		var playerBounds = GetPlayerBounds();
		var origin = new Vector2(playerBounds.center.x, playerBounds.center.y);
		/*lineRenderer.SetPosition(0, origin);
		lineRenderer.SetPosition(1, new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, 10f));
		lineRenderer.SetPosition(2, origin);
		lineRenderer.SetPosition(3, new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, 10f));
		lineRenderer.SetPosition(4, origin);
		lineRenderer.SetPosition(5, new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, 10f));
		lineRenderer.SetPosition(6, origin);
		lineRenderer.SetPosition(7, new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, 10f));*/

		var lineSegmentsToTest = linesToTest.Select(x => x.LineSegment).ToArray();
		/*var direction1 = (new Vector2(playerBounds.center.x - playerBounds.extents.x, playerBounds.center.y - playerBounds.extents.y) - origin).normalized;
		var direction2 = (new Vector2(playerBounds.center.x + playerBounds.extents.x, playerBounds.center.y - playerBounds.extents.y) - origin).normalized;
		var direction3 = (new Vector2(playerBounds.center.x - playerBounds.extents.x, playerBounds.center.y + playerBounds.extents.y) - origin).normalized;
		var direction4 = (new Vector2(playerBounds.center.x + playerBounds.extents.x, playerBounds.center.y + playerBounds.extents.y) - origin).normalized;*/

		int count = 0;
		var uniquePoints = lineSegmentsToTest.SelectMany(x => new[] { x.Start, x.End }).Distinct(new Vector2EqualityComparer()).ToArray();
		lineRenderer.positionCount = uniquePoints.Length * 2;
		var anglesForUniquePoints = uniquePoints.SelectMany(uniquePoint => new[] { Math.Atan2(uniquePoint.y - playerBounds.center.y, uniquePoint.x - playerBounds.center.x) }).OrderBy(x => x).ToArray();
		foreach (var angle in anglesForUniquePoints)
		{
			lineRenderer.SetPosition(count, origin);
			lineRenderer.SetPosition(count + 1, origin + GetDirectionalLightVector2(origin, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)), lineSegmentsToTest));

			count += 2;
		}

		/*foreach (var lineSegment in lineSegmentsToTest)
		{
			var direction = (new Vector2(lineSegment.StartplayerBounds.center.x - playerBounds.extents.x, playerBounds.center.y - playerBounds.extents.y) - origin).normalized;
		}

		lineRenderer.SetPosition(0, origin);
		lineRenderer.SetPosition(1, origin + GetDirectionalLightVector2(origin, direction1, lineSegmentsToTest));

		lineRenderer.SetPosition(2, origin);
		lineRenderer.SetPosition(3, origin + GetDirectionalLightVector2(origin, direction2, lineSegmentsToTest));

		lineRenderer.SetPosition(4, origin);
		lineRenderer.SetPosition(5, origin + GetDirectionalLightVector2(origin, direction3, lineSegmentsToTest));

		lineRenderer.SetPosition(6, origin);
		lineRenderer.SetPosition(7, origin + GetDirectionalLightVector2(origin, direction4, lineSegmentsToTest));*/
	}

	private const float MAXIMUM_LIGHT_CAST = 205f;
	private static Vector2 GetDirectionalLightVector2(Vector2 rayOrigin, Vector2 rayDirection, IEnumerable<LineSegment> lineSegments)
	{
		float t1 = MAXIMUM_LIGHT_CAST;
		foreach (var lineSegment in lineSegments)
		{
			var t1a = GetT1(rayOrigin, rayDirection, lineSegment.Start + Vector2.up * 0.1f, (lineSegment.End - lineSegment.Start));
			if (t1a < t1)
				t1 = t1a;

			var t1b = GetT1(rayOrigin, rayDirection, lineSegment.Start + Vector2.right * 0.1f, (lineSegment.End - lineSegment.Start));
			if (t1b < t1)
				t1 = t1b;

			var t1c = GetT1(rayOrigin, rayDirection, lineSegment.Start + Vector2.down * 0.1f, (lineSegment.End - lineSegment.Start));
			if (t1c < t1)
				t1 = t1c;

			var t1d = GetT1(rayOrigin, rayDirection, lineSegment.Start + Vector2.left * 0.1f, (lineSegment.End - lineSegment.Start));
			if (t1d < t1)
				t1 = t1d;

		}

		return rayDirection * t1;
	}

	private static float GetT1(Vector2 rayOrigin, Vector2 rayDirection, Vector2 lineSegmentOrigin, Vector2 lineSegmentDirection)
	{
		/*
			*Ray X = r_px+r_dx*T1
			Ray Y = r_py+r_dy*T1
			Segment X = s_px+s_dx*T2
			Segment Y = s_py+s_dy*T2
			*/

		//T2 = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
		//T1 = (s_px + s_dx * T2 - r_px) / r_dx
		float t2 = (rayDirection.x * (lineSegmentOrigin.y - rayOrigin.y) + rayDirection.y * (rayOrigin.x - lineSegmentOrigin.x)) / (lineSegmentDirection.x * rayDirection.y - lineSegmentDirection.y * rayDirection.x);
		float t1 = (lineSegmentOrigin.x + lineSegmentDirection.x * t2 - rayOrigin.x) / rayDirection.x;
		if ((t1 > 0f) && (0f < t2) && (t2 < 1f))
			return t1;
		
		return MAXIMUM_LIGHT_CAST;
	}

	public void SetLightmapData(int mapWidth, int mapHeight, IEnumerable<LineSegment> lineSegmentCollection)
	{
		_mapWidth = mapWidth;
		_mapHeight = mapHeight;
		_lineSegmentCollection = lineSegmentCollection.ToArray();

		var objects = _lineSegmentCollection.Select(lineSegment =>
		{
			GameObject myLine = GameObject.Instantiate(Resources.Load("Line")) as GameObject;
			myLine.transform.position = lineSegment.Start;

			// TODO: remove after debugging is complete
			var lineRenderer = myLine.GetComponent<LineRenderer>();
			lineRenderer.SetPosition(0, new Vector3(lineSegment.Start.x, lineSegment.Start.y, 10f));
			lineRenderer.SetPosition(1, new Vector3(lineSegment.End.x, lineSegment.End.y, 10f));
			lineRenderer.startColor = Color.magenta;
			lineRenderer.endColor = Color.magenta;

			return new LineSegmentAndAssociatedRenderer(lineSegment, lineRenderer);
		}).ToArray();

		_quadTree = new QuadTree<LineSegmentAndAssociatedRenderer>(0, 0, _mapWidth + 3.3f, _mapHeight + 3.3f, 25);
		foreach (var obj in objects)
			_quadTree.Insert(obj);
	}

	private class LineSegmentAndAssociatedRenderer : IHasAABB
	{
		public LineSegmentAndAssociatedRenderer(LineSegment lineSegment, LineRenderer lineRenderer)
		{
			LineSegment = lineSegment;
			LineRenderer = lineRenderer;
		}

		public LineSegment LineSegment { get; }
		public LineRenderer LineRenderer { get; }
		
		public Bounds AABB => LineSegment.AABB;
	}
}

internal class Vector2EqualityComparer : IEqualityComparer<Vector2>
{
	public bool Equals(Vector2 v1, Vector2 v2) => Math.Abs(v1.x - v2.x) < 0.01f && Math.Abs(v1.y - v2.y) < 0.01f;
	public int GetHashCode(Vector2 obj) => obj.GetHashCode();
}

public class LineSegment : Tuple<Vector2, Vector2>, IHasAABB
{
	public LineSegment(Vector2 start, Vector2 end)
		: base(start, end)
	{
		var width = Math.Abs(end.x - start.x);
		if (width < 1)
			width = 0.1f;

		var height = Math.Abs(end.y - start.y);
		if (height < 1)
			height = 0.1f;

		var centerX = (start.x + end.x) / 2;
		var centerY = (start.y + end.y) / 2;
		AABB = new Bounds(new Vector3(centerX, centerY), new Vector3(width, height));
	}

	public Vector2 Start => Item1;
	public Vector2 End => Item2;
	public Bounds AABB { get; }
}

public interface IHasAABB
{
	Bounds AABB { get; }
}

public class QuadTree<T>
		where T : class, IHasAABB
{
	private readonly int _minimumArea;

	private readonly Bounds _bounds;
	private readonly QuadTreeNode _root;
	private readonly List<T> _allItems;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="centerX">The X-coordinate of the quadtree center.</param>
	/// <param name="centerY">The Y-coordinate of the quadtree center.</param>
	/// <param name="width">The width of the quadtree boundary.</param>
	/// <param name="height">The height of the quadtree boundary.</param>
	/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
	public QuadTree(float centerX, float centerY, float width, float height, int minimumArea = 100)
	{
		_minimumArea = minimumArea;
		_bounds = new Bounds(new Vector3(centerX, centerY), new Vector3(width, height, 2));
		_root = new QuadTreeNode(_bounds, minimumArea);
		_allItems = new List<T>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="bounds">The boundary of the quadtree.</param>
	/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
	public QuadTree(Rect bounds, int minimumArea = 100)
		: this(bounds.x, bounds.y, bounds.width, bounds.height, minimumArea)
	{
		
	}

	/// <summary>
	/// Gets the bounds.
	/// </summary>
	public Bounds Bounds => _bounds;

	/// <summary>
	/// Gets the number of items in the quadtree.
	/// </summary>
	public int Count => _root.Count;

	public IEnumerable<T> Items => _allItems;

	/// <summary>
	/// Insert an item into the quadtree.
	/// </summary>
	/// <param name="item">The item to insert.</param>
	public void Insert(T item)
	{
		_root.Insert(item);
		_allItems.Add(item);
	}

	/// <summary>
	/// Query the quadtree, returning the item whose AABB contains the specified point.
	/// </summary>
	/// <param name="x">The x-coordinate of the point to test.</param>
	/// <param name="y">The y-coordinate of the point to test.</param>
	/// <returns>The item whose AABB contains the specified point; if no such item exists, return null.</returns>
	public T Query(float x, float y)
	{
		return _root.Query(new Vector2(x, y));
	}

	/// <summary>
	/// Query the quadtree, returning the item whose AABB contains the specified point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The item whose AABB contains the specified point; if no such item exists, return null.</returns>
	public T Query(Vector2 point)
	{
		return _root.Query(point);
	}

	/// <summary>
	/// Query the quadtree, returning a collection of items that are in the given area.
	/// </summary>
	/// <param name="area">The area to test.</param>
	/// <returns>A collection of items that are in the given area.</returns>
	public IEnumerable<T> Query(Bounds area)
	{
		return _root.Query(area);
	}

	private class QuadTreeNode
	{
		private readonly int _minimumArea;
		private readonly List<T> _contents;
		private readonly List<QuadTreeNode> _nodes;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bounds">The node bounds.</param>
		/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
		public QuadTreeNode(Bounds bounds, int minimumArea)
		{
			_minimumArea = minimumArea;
			Bounds = bounds;
			_contents = new List<T>();
			_nodes = new List<QuadTreeNode>(4);
		}

		/// <summary>
		/// Indicates if the node is empty.
		/// </summary>
		public bool IsEmpty => (Math.Abs(Bounds.size.x) < 0.01f && Math.Abs(Bounds.size.y) < 0.01f) || (!_nodes.Any() && !_contents.Any());

		/// <summary>
		/// Gets the AABB of the quadtree node.
		/// </summary>
		public Bounds Bounds { get; }

		/// <summary>
		/// Gets the number of items in this node and all subnodes.
		/// </summary>
		public int Count
		{
			get
			{
				int count = _nodes.Sum(node => node.Count);
				count += _contents.Count;
				return count;
			}
		}

		/// <summary>
		/// Gets the contents of this node and all subnodes.
		/// </summary>
		public IEnumerable<T> Contents
		{
			get
			{
				var results = new List<T>();
				foreach (var node in _nodes)
					results.AddRange(node.Contents);

				results.AddRange(_contents);
				return results;
			}
		}

		/// <summary>
		/// Gets the contents of this node only.
		/// </summary>
		public IEnumerable<T> OwnContents => _contents;

		/// <summary>
		/// Create the subnodes (partition space), provided that it is larger than the minimum-allowed size.
		/// </summary>
		private void CreateSubNodes()
		{
			// The smallest subnode has an area
			if (Bounds.size.y * Bounds.size.x <= _minimumArea)
				return;

			var nodeSize = new Vector3(Bounds.extents.x, Bounds.extents.y, Bounds.size.z);
			_nodes.Add(new QuadTreeNode(new Bounds(Bounds.center + new Vector3(-Bounds.extents.x / 2, -Bounds.extents.y / 2), nodeSize), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Bounds(Bounds.center + new Vector3(Bounds.extents.x / 2, -Bounds.extents.y / 2), nodeSize), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Bounds(Bounds.center + new Vector3(-Bounds.extents.x / 2, Bounds.extents.y / 2), nodeSize), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Bounds(Bounds.center + new Vector3(Bounds.extents.x / 2, Bounds.extents.y / 2), nodeSize), _minimumArea));
		}

		/// <summary>
		/// Insert an item into this node.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		public void Insert(T item)
		{
			if (!Bounds.Contains(item.AABB))
				return;

			// Partition this node (if the size of this node is below the minimum-allowed size, this node is not partitioned)
			if (!_nodes.Any())
				CreateSubNodes();

			// If the node contains the item, add the item to that node and return
			// The item is stored in the node just large enough to fit it
			foreach (var node in _nodes.Where(n => n.Bounds.Contains(item.AABB)))
			{
				node.Insert(item);
				return;
			}

			// Either the subnodes cannot completely contain the item, or this node is the minimum-allowed size => add the item to this node
			_contents.Add(item);
		}

		/// <summary>
		/// Query the quadtree, returning the item whose AABB contains the specified point.
		/// </summary>
		/// <param name="point">The point to test.</param>
		/// <returns>The item whose AABB contains the specified point; if no such item exists, return null.</returns>
		public T Query(Vector2 point)
		{
			// Search this node first; if no results, then search sub-nodes
			return _contents.FirstOrDefault(i => i.AABB.Contains(point)) ?? _nodes.Where(n => !n.IsEmpty).Select(n => n.Query(point)).FirstOrDefault(i => i != null);
		}

		/// <summary>
		/// Query the quadtree, returning a collection of items that are in the given area.
		/// </summary>
		/// <param name="area">The area to test.</param>
		/// <returns>A collection of items that are in the given area.</returns>
		public IEnumerable<T> Query(Bounds area)
		{
			// This node contains items that are not entirely contained by it's four sub-nodes
			// Check if any items in this node intersect with the specified area
			var results = _contents.Where(item => item.AABB.Intersects(area) || area.Intersects(item.AABB) || area.Contains(item.AABB)).ToList();

			foreach (var node in _nodes.Where(n => !n.IsEmpty))
			{
				// CASE 1: Search area completely contained by sub-node
				// If a node completely contains the query area, go down that branch and skip the remaining nodes
				if (node.Bounds.Contains(area))
				{
					results.AddRange(node.Query(area));
					break;
				}

				// CASE 2: Sub-node completely contained by search area
				// If the query area completely contains a sub-node, just add all the contents of that node and its children to the result set
				if (area.Contains(node.Bounds))
				{
					results.AddRange(node.Contents);
					continue;
				}

				// CASE 3: Search area intersects with sub-node
				// Traverse into this node, then search other quads
				if (node.Bounds.Intersects(area))
					results.AddRange(node.Query(area));
			}

			return results;
		}
	}
}

public static class BoundsExtensions
{
	public static bool Contains(this Bounds source, Bounds bounds)
	{
		return source.Contains(bounds.min)
			&& source.Contains(new Vector3(bounds.min.x, bounds.max.y))
			&& source.Contains(new Vector3(bounds.max.x, bounds.min.y))
			&& source.Contains(bounds.max);
	}
}
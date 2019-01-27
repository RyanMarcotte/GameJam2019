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

	// Update is called once per frame
	void Update()
	{
		/*var camera = Camera.main.GetComponent<Camera>();
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		var bounds = new Bounds(camera.transform.position, new Vector3(cameraHeight * screenAspect, cameraHeight, 0));*/

		var position = Player.transform.position;
	}

	public void SetLightmapData(int mapWidth, int mapHeight, IEnumerable<LineSegment> lineSegmentCollection)
	{
		_mapWidth = mapWidth;
		_mapHeight = mapHeight;
		_lineSegmentCollection = lineSegmentCollection.ToArray();

		// TODO: remove after debugging is complete
		var objects = _lineSegmentCollection.Select(lineSegment =>
		{
			GameObject myLine = GameObject.Instantiate(Resources.Load("Line")) as GameObject;
			myLine.transform.position = lineSegment.Start;

			var lineRenderer = myLine.GetComponent<LineRenderer>();
			lineRenderer.SetPosition(0, new Vector3(lineSegment.Start.x, lineSegment.Start.y, 10f));
			lineRenderer.SetPosition(1, new Vector3(lineSegment.End.x, lineSegment.End.y, 10f));
			lineRenderer.startColor = Color.yellow;
			lineRenderer.endColor = Color.yellow;

			return new LineSegmentAndAssociatedRenderer(lineSegment, lineRenderer);
		}).ToArray();

		_quadTree = new QuadTree<LineSegmentAndAssociatedRenderer>(-_mapWidth / 2, -_mapHeight / 2, _mapWidth, _mapHeight, 9);
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
		
		public Rect AABB => LineSegment.AABB;
	}
}

public class LineSegment : Tuple<Vector2, Vector2>, IHasAABB
{
	private readonly Rect _aabb;

	public LineSegment(Vector2 start, Vector2 end)
		: base(start, end)
	{
		var height = Math.Abs(end.y - start.y);
		if (height < 1)
			height = 0.1f;

		var width = Math.Abs(end.x - start.x);
		if (width < 1)
			width = 0.1f;

		var topLeftX = Math.Min(start.x, end.x);
		var topLeftY = Math.Max(start.y, end.y);
		_aabb = new Rect(topLeftX, topLeftY, width, height);
	}

	public Vector2 Start => Item1;
	public Vector2 End => Item2;
	public Rect AABB => _aabb;
}

public interface IHasAABB
{
	Rect AABB { get; }
}

public class QuadTree<T>
		where T : class, IHasAABB
{
	private readonly int _minimumArea;

	private Rect _bounds;
	private QuadTreeNode _root;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="x">The X-coordinate of the top-left corner of the quadtree boundary.</param>
	/// <param name="y">The Y-coordinate of the top-left corner of the quadtree boundary.</param>
	/// <param name="width">The width of the quadtree boundary.</param>
	/// <param name="height">The height of the quadtree boundary.</param>
	/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
	public QuadTree(float x, float y, float width, float height, int minimumArea = 100)
	{
		_minimumArea = minimumArea;
		_bounds = new Rect(x, y, width, height);
		_root = new QuadTreeNode(_bounds, minimumArea);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="bounds">The boundary of the quadtree.</param>
	/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
	public QuadTree(Rect bounds, int minimumArea = 100)
	{
		_minimumArea = minimumArea;
		_bounds = bounds;
		_root = new QuadTreeNode(_bounds, minimumArea);
	}

	/// <summary>
	/// Gets the bounds.
	/// </summary>
	public Rect Bounds => _bounds;

	/// <summary>
	/// Gets the number of items in the quadtree.
	/// </summary>
	public int Count => _root.Count;

	/// <summary>
	/// Insert an item into the quadtree.
	/// </summary>
	/// <param name="item">The item to insert.</param>
	public void Insert(T item)
	{
		_root.Insert(item);
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
	public IEnumerable<T> Query(Rect area)
	{
		return _root.Query(area);
	}

	/// <summary>
	/// Removes an item from the quadtree.
	/// </summary>
	/// <param name="item">The item to remove.</param>
	public void Remove(T item)
	{
		_root.Remove(item);
	}

	/// <summary>
	/// Removes all items from the quadtree.
	/// </summary>
	public void Clear()
	{
		_root.Clear();
	}

	private class QuadTreeNode
	{
		#region PRIVATE MEMBERS

		private readonly int _minimumArea;
		private readonly Rect _bounds;
		private readonly List<T> _contents;
		private readonly List<QuadTreeNode> _nodes;

		#endregion

		#region INITIALIZATION

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bounds">The node bounds.</param>
		/// <param name="minimumArea">The minimum area (in pixels) that nodes must occupy.</param>
		public QuadTreeNode(Rect bounds, int minimumArea)
		{
			_minimumArea = minimumArea;
			_bounds = bounds;
			_contents = new List<T>();
			_nodes = new List<QuadTreeNode>(4);
		}

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Indicates if the node is empty.
		/// </summary>
		public bool IsEmpty => (Math.Abs(_bounds.width) < 0.01f && Math.Abs(_bounds.height) < 0.01f) || !_nodes.Any();

		/// <summary>
		/// Gets the AABB of the quadtree node.
		/// </summary>
		public Rect Bounds => _bounds;

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

		#endregion

		#region PRIVATE METHODS

		/// <summary>
		/// Create the subnodes (partition space), provided that it is larger than the minimum-allowed size.
		/// </summary>
		private void CreateSubNodes()
		{
			// The smallest subnode has an area
			if (_bounds.height * _bounds.width <= _minimumArea)
				return;

			float halfWidth = (_bounds.width / 2f);
			float halfHeight = (_bounds.height / 2f);

			_nodes.Add(new QuadTreeNode(new Rect(new Vector2(_bounds.xMin, _bounds.yMin), new Vector2(halfWidth, halfHeight)), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Rect(new Vector2(_bounds.xMin, _bounds.yMin + halfHeight), new Vector2(halfWidth, halfHeight)), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Rect(new Vector2(_bounds.xMin + halfWidth, _bounds.yMin), new Vector2(halfWidth, halfHeight)), _minimumArea));
			_nodes.Add(new QuadTreeNode(new Rect(new Vector2(_bounds.xMin + halfWidth, _bounds.yMin + halfHeight), new Vector2(halfWidth, halfHeight)), _minimumArea));
		}

		#endregion

		#region PUBLIC METHODS

		/// <summary>
		/// Insert an item into this node.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		public void Insert(T item)
		{
			if (!_bounds.Overlaps(item.AABB))
				return;

			// Partition this node (if the size of this node is below the minimum-allowed size, this node is not partitioned)
			if (!_nodes.Any())
				CreateSubNodes();

			// If the node contains the item, add the item to that node and return
			// The item is stored in the node just large enough to fit it
			foreach (var node in _nodes.Where(n => n.Bounds.Overlaps(item.AABB)))
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
		public IEnumerable<T> Query(Rect area)
		{
			// This node contains items that are not entirely contained by it's four sub-nodes
			// Check if any items in this node intersect with the specified area
			var results = _contents.Where(item => area.Overlaps(item.AABB)).ToList();

			foreach (var node in _nodes.Where(n => !n.IsEmpty))
			{
				// CASE 1: Search area completely contained by sub-node
				// If a node completely contains the query area, go down that branch and skip the remaining nodes
				if (node.Bounds.Overlaps(area))
				{
					results.AddRange(node.Query(area));
					break;
				}

				// CASE 2: Sub-node completely contained by search area
				// If the query area completely contains a sub-node, just add all the contents of that node and its children to the result set
				if (area.Overlaps(node.Bounds))
				{
					results.AddRange(node.Contents);
					continue;
				}

				// CASE 3: Search area intersects with sub-node
				// Traverse into this node, then search other quads
				if (node.Bounds.Overlaps(area))
					results.AddRange(node.Query(area));
			}

			return results;
		}

		/// <summary>
		/// Perform the specified action for each item in the quadtree.
		/// </summary>
		/// <param name="action">Action to perform.</param>
		public void ForEach(Action<QuadTreeNode> action)
		{
			action(this);

			foreach (var node in _nodes)
				node.ForEach(action);
		}

		/// <summary>
		/// Removes an item from the quadtree.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		public void Remove(T item)
		{
			if (!_bounds.Overlaps(item.AABB))
				return;

			foreach (var node in _nodes.Where(n => n.Bounds.Overlaps(item.AABB)))
			{
				node.Remove(item);
				return;
			}

			_contents.Remove(item);
		}

		/// <summary>
		/// Remove all contents from the node.
		/// </summary>
		internal void Clear()
		{
			_contents.Clear();
			foreach (var node in _nodes)
				node.Clear();
		}

		#endregion
	}
}
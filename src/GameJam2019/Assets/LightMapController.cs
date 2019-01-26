using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightMapController : MonoBehaviour
{
	public GameObject Player;

	private LineSegment[] _lineSegmentCollection = {};

	// Start is called before the first frame update
	void Start()
    {
		
	}

	// Update is called once per frame
    void Update()
    {
        
    }

	public void SetLightmapData(IEnumerable<LineSegment> lineSegmentCollection)
	{
		_lineSegmentCollection = lineSegmentCollection.ToArray();

		// TODO: remove after debugging is complete
		foreach (var lineSegment in _lineSegmentCollection)
		{
			var startPoint = GameObject.Instantiate(Resources.Load("Point")) as GameObject;
			startPoint.transform.position = lineSegment.Start;

			var endPoint = GameObject.Instantiate(Resources.Load("Point")) as GameObject;
			endPoint.transform.position = lineSegment.Start;
		}
	}
}

public class LineSegment : Tuple<Vector2, Vector2>
{
	public LineSegment(Vector2 start, Vector2 end)
		: base(start, end)
	{
	}

	public Vector2 Start => Item1;
	public Vector2 End => Item2;
}
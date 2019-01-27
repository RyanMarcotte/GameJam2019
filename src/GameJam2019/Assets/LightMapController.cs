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
			GameObject myLine = GameObject.Instantiate(Resources.Load("Line")) as GameObject;
			myLine.transform.position = lineSegment.Start;

			var lr = myLine.GetComponent<LineRenderer>();
			lr.SetPosition(0, new Vector3(lineSegment.Start.x, lineSegment.Start.y, 10f));
			lr.SetPosition(1, new Vector3(lineSegment.End.x, lineSegment.End.y, 10f));
			lr.startColor = Color.yellow;
			lr.endColor = Color.yellow;
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
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
	public Tile GroundTile;
	public Tile ObstacleTile;

	public Tilemap Floor;
	public Tilemap Obstacles;

	private int length = 64;
	private int width = 64;

	// Start is called before the first frame update
	void Start()
	{
		var random = new System.Random();
		for (int i = GetLowerBound(length); i < GetUpperBound(length); i++)
		{
			for (int j = GetLowerBound(width); j < GetUpperBound(width); j++)
			{
				if (i == GetLowerBound(length)
					|| i == GetUpperBound(length) - 1
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
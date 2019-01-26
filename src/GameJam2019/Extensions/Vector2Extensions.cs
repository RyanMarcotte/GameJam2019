using UnityEngine;

namespace Extensions
{
	public static class Vector2Extensions
	{
		public static Vector2 ToNormalizedVector2(this Vector2 source)
		{
			var result = new Vector2(source.x, source.y);
			result.Normalize();
			return result;
		}
	}
}
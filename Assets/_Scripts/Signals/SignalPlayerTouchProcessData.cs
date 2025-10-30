using UnityEngine;

public class SignalPlayerTouchProcessData
{
	public Vector2 DirectionVector { get; private set; }

	public SignalPlayerTouchProcessData(Vector2 directionVector)
	{
		DirectionVector = directionVector;
	}
}
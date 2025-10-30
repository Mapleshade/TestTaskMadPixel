using UnityEngine;

public class PlayerStandaloneInputProvider : BasePlayerInputProvider
{
	public override void Tick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 mousePosition = Input.mousePosition;
			MovingStartPosition = mousePosition;
			CheckUiTouch(mousePosition);
			return;
		}

		if (Input.GetMouseButton(0))
		{
			Vector2 mousePosition = Input.mousePosition;

			MovingVector = Utils.GetClampedMovingVector(mousePosition - MovingStartPosition);
			Debug.Log($"_movingVector: {MovingVector}");
		}

		if (Input.GetMouseButtonUp(0))
			MovingVector = Vector2.zero;
	}
}
using UnityEngine;

public struct InputTouchData
{
	public Touch Data { get; private set; }
	public bool HasTouch { get; private set; }

	public void SetTouch(Touch touch)
	{
		Data = touch;
		HasTouch = true;
	}

	public void ResetTouch()
	{
		HasTouch = false;
	}
}
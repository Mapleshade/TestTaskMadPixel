using UnityEngine;

public class PlayerMobileInputProvider : BasePlayerInputProvider
{
	private const int MaxTouchesCount = 1;
	private InputTouchData _moveTouch;

	public override void Tick()
	{
		if (Input.touchCount > MaxTouchesCount)
		{
			Reset();
			return;
		}

		for (var i = 0; i < Input.touchCount; i++)
		{
			ProcessTouch(Input.GetTouch(i));
		}
	}

	private void Reset()
	{
		if (!_moveTouch.HasTouch)
			return;

		_moveTouch.ResetTouch();
		MovingVector = Vector2.zero;
	}

	private void CalculateMoveVector()
	{
		var x = _moveTouch.Data.position.x - MovingStartPosition.x;
		var y = _moveTouch.Data.position.y - MovingStartPosition.y;

		MovingVector = new Vector2(x, y);
		MovingVector = Utils.GetClampedMovingVector(MovingVector);
	}

	private void ProcessTouch(Touch touch)
	{
		if (touch.phase == TouchPhase.Began)
		{
			CheckUiTouch(touch.position);

			_moveTouch.SetTouch(touch);
			MovingStartPosition = new Vector2(touch.position.x, touch.position.y);
			return;
		}

		if (_moveTouch.HasTouch && touch.fingerId == _moveTouch.Data.fingerId)
		{
			if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				_moveTouch.ResetTouch();
				MovingVector = Vector2.zero;
				return;
			}

			CalculateMoveVector();
			Debug.Log($"_movingVector: {MovingVector}");
			_moveTouch.SetTouch(touch);
		}
	}
}
using UnityEngine;
using Zenject;

public class PlayerStandaloneInputProvider : BasePlayerInputProvider
{
	public PlayerStandaloneInputProvider(SignalBus signalBus) : base(signalBus)
	{
	}
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
			SignalBus.Fire(new SignalPlayerTouchProcessData(MovingVector));
		}

		if (!Input.GetMouseButtonUp(0))
			return;

		MovingVector = Vector2.zero;
		SignalBus.Fire(new SignalResetPlayerInputData());
	}

}
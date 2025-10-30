using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class PlayerInputProvider : ITickable
{
	private const float SCREEN_MOVING_MAX_DELTA_PERSENT = .15f;
	private Vector2 _movingStartPosition;
	private Vector2 _movingVector;
	private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
	private EventSystem _eventSystem;
	private PointerEventData _eventData;
	private float ScreenMovingMaxDelta { get; }

	public PlayerInputProvider()
	{
		ScreenMovingMaxDelta = Screen.height * SCREEN_MOVING_MAX_DELTA_PERSENT;
	}

	public void Tick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			OnGetMouseButtonDown();
			return;
		}

		if (Input.GetMouseButton(0))
		{
			Vector2 mousePosition = Input.mousePosition;

			_movingVector = GetClampedMovingVector(mousePosition - _movingStartPosition);
			Debug.Log($"_movingVector: {_movingVector}");
			// MoveJoystick(_movingVector);

		}

		if (Input.GetMouseButtonUp(0))
		{
			OnGetMouseButtonUp();
		}
	}
	
	private bool IsUiTouch(Vector2 position)
	{
		_raycastResults.Clear();

		var currentEventSystem = EventSystem.current;

		if (_eventData == null || _eventSystem != currentEventSystem)
		{
			_eventSystem = currentEventSystem;
			_eventData = new PointerEventData(EventSystem.current);
		}

		_eventData.position = position;
		_eventData.clickCount = 1;

		currentEventSystem.RaycastAll(_eventData, _raycastResults);

		for (int i = 0; i < _raycastResults.Count; i++)
		{
			RaycastResult raycastResult = _raycastResults[i];
			var component = raycastResult.gameObject.GetComponent<ViewCellRoot>();
			Debug.Log($"cellData: {component.transform.name}");
		}

		Debug.Log($"touch ui: {_raycastResults.Count > 0}");
		return _raycastResults.Count > 0;
	}
	
	private void OnGetMouseButtonDown()
	{
		Vector2 mousePosition = UnityEngine.Input.mousePosition;
		_movingStartPosition = mousePosition;
		IsUiTouch(mousePosition);
	}

	private void OnGetMouseButtonUp()
	{
		_movingVector = Vector2.zero;
	}
	private Vector2 GetClampedMovingVector(Vector2 movingVector)
	{
		movingVector = Vector2.ClampMagnitude(movingVector, ScreenMovingMaxDelta);
		return Vector2.ClampMagnitude(movingVector, movingVector.magnitude / ScreenMovingMaxDelta);
	}
}

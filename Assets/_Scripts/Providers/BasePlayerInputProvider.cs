using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class BasePlayerInputProvider : ITickable
{
	private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
	private EventSystem _eventSystem;
	private PointerEventData _eventData;
	protected Vector2 MovingStartPosition;
	protected Vector2 MovingVector { get; set; }

	public virtual void Tick()
	{
	}

	protected bool CheckUiTouch(Vector2 position)
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
			if (component != null)
				Debug.Log($"cellData: {component.transform.name}");
		}

		Debug.Log($"touch ui: {_raycastResults.Count > 0}");
		return _raycastResults.Count > 0;
	}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class BasePlayerInputProvider : ITickable
{
	protected Vector2 MovingVector { get; set; }
	protected Vector2 MovingStartPosition { get; set; }
	protected SignalBus SignalBus { get; private set; }
	private readonly List<RaycastResult> _raycastResults = new();
	private EventSystem _eventSystem;
	private PointerEventData _eventData;

	protected BasePlayerInputProvider(SignalBus signalBus)
	{
		SignalBus = signalBus;
	}

	public virtual void Tick()
	{
	}

	protected void CheckUiTouch(Vector2 position)
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

		for (var i = 0; i < _raycastResults.Count; i++)
		{
			var raycastResult = _raycastResults[i];
			var component = raycastResult.gameObject.GetComponent<ViewCellRoot>();

			if (component != null)
				SignalBus.Fire(new SignalPlayerTouchCellData(component));
		}
	}
}
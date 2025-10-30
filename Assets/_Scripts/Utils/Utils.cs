using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	public const int PlateSizeX = 8;
	public const int PlateSizeY = 8;
	private const float ScreenMovingMaxDeltaPersent = .15f;
	private static float ScreenMovingMaxDelta { get; } = Screen.height * ScreenMovingMaxDeltaPersent;

	public static Transform GetCanvas(this BaseUIView view, List<ViewCanvas> viewCanvases)
	{
		var viewCanvas = viewCanvases.Find(f => f.SortingOrder == view.CanvasSortOrder);

		if (viewCanvas == null)
		{
			throw new Exception($"Can't find canvas for view {view} with sort order {view.CanvasSortOrder}");
		}

		return viewCanvas.Root;
	}
	
	public static Vector2 GetClampedMovingVector(Vector2 movingVector)
	{
		movingVector = Vector2.ClampMagnitude(movingVector, ScreenMovingMaxDelta);
		return Vector2.ClampMagnitude(movingVector, movingVector.magnitude / ScreenMovingMaxDelta);
	}
}

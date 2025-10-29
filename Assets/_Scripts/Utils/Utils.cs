using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	public static Transform GetCanvas(this BaseUIView view, List<ViewCanvas> viewCanvases)
	{
		var viewCanvas = viewCanvases.Find(f => f.SortingOrder == view.CanvasSortOrder);

		if (viewCanvas == null)
		{
			throw new Exception($"Can't find canvas for view {view} with sort order {view.CanvasSortOrder}");
		}

		return viewCanvas.Root;
	}
}

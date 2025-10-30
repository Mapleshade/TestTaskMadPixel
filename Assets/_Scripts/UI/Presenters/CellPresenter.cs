using UnityEngine;

public class CellPresenter : UiPresenter
{
	private readonly ViewCellRootPool _viewCellRootPool;
	private ViewCellRoot View { get; }
	public CellTypeEnum CellType { get; private set; }
	public Transform ViewTransform => View.transform;

	public CellPresenter(ViewCellRootPool pool)
	{
		_viewCellRootPool = pool;
		View = _viewCellRootPool.Spawn();
	}

	public void SetType(CellTypeEnum cellType)
	{
		CellType = cellType;

		foreach (var cellTypeData in View.ImagesCellType)
			cellTypeData.PanelImage.CheckSetActive(cellTypeData.CellType == cellType);
	}

	public void InitCell(bool isLight, CellTypeEnum cellType, Transform parentTransform)
	{
		View.transform.SetParent(parentTransform);
		View.transform.localScale = Vector3.one;
		View.ImagesCellBackground[0].PanelBackground.CheckSetActive(isLight);
		View.ImagesCellBackground[1].PanelBackground.CheckSetActive(!isLight);
		SetType(cellType);
	}

	public void SetViewName(int xIndex, int yIndex)
	{
		View.name = $"Cell_{xIndex}_{yIndex}";
	}

	public override void Dispose()
	{
		base.Dispose();
		_viewCellRootPool.Despawn(View);
	}
}
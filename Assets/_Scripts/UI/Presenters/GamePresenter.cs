using System.Collections.Generic;
using UnityEngine;

public class GamePresenter : BaseUIPresenter<ViewGame>
{
	private readonly CellPresenterFactory _cellPresenterFactory;
	private readonly List<CellPresenter> _allPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _rowsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _columnsPresenters = new();
	private readonly int _cellTypesCount;

	public GamePresenter(ViewGame view, CellPresenterFactory cellPresenterFactory) : base(view)
	{
		_cellPresenterFactory = cellPresenterFactory;
		_cellTypesCount = System.Enum.GetValues(typeof(CellTypeEnum)).Length;

		for (var i = 0; i < Utils.PlateSizeY; i++)
			_rowsPresenters.Add(i, new List<CellPresenter>());

		for (var i = 0; i < Utils.PlateSizeX; i++)
			_columnsPresenters.Add(i, new List<CellPresenter>());
	}

	public override void Initialize()
	{
		base.Initialize();

		var isOdd = false;
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			for (var y = 0; y < Utils.PlateSizeY; y++)
			{
				var cellPresenter = _cellPresenterFactory.Create();
				_allPresenters.Add(cellPresenter);
				_rowsPresenters[x].Add(cellPresenter);
				_columnsPresenters[y].Add(cellPresenter);
				
				var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
				var randomCellType = (CellTypeEnum) randomCellTypeIndex;
				cellPresenter.InitCell(isOdd, randomCellType, View.PanelPlate);
				cellPresenter.SetViewName(x, y);
				isOdd = !isOdd;
			}

			isOdd = !isOdd;
		}

		CheckStartPlate();
	}

	private void CheckStartPlate()
	{
		//check rows
		for (var y = 0; y < Utils.PlateSizeY; y++)
		{
			var rowList = _rowsPresenters[y];
			for (var x = 0; x < rowList.Count - 3; x++)
			{
				if (rowList[x].CellType == rowList[x + 1].CellType
					&& rowList[x].CellType == rowList[x + 2].CellType)
				{
					if (rowList[x + 2].CellType != rowList[x + 3].CellType)
					{
						(rowList[x + 2], rowList[x + 3]) = (rowList[x + 3], rowList[x + 2]);
						(_columnsPresenters[x + 2][y], _columnsPresenters[x + 3][y]) = (_columnsPresenters[x + 3][y], _columnsPresenters[x + 2][y]);
						SwitchTransformsInHierarchy(rowList[x + 2].ViewTransform, rowList[x + 3].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(rowList[x + 2].CellType);
						Debug.Log($"switch type from {rowList[x + 2].CellType} to {newCellType} for {rowList[x + 2].ViewTransform.name}.");
						rowList[x + 2].SetType(newCellType);
					}
				}
			}
		}

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 0; y < columnList.Count - 3; y++)
			{
				if (columnList[y].CellType == columnList[y + 1].CellType
					&& columnList[y].CellType == columnList[y + 2].CellType)
				{
					if (columnList[y + 2].CellType != columnList[y + 3].CellType)
					{
						(columnList[y + 2], columnList[y + 3]) = (columnList[y + 3], columnList[y + 2]);
						(_rowsPresenters[y + 2][x], _rowsPresenters[y + 3][x]) = (_rowsPresenters[y + 3][x], _rowsPresenters[y + 2][x]);
						SwitchTransformsInHierarchy(columnList[y + 2].ViewTransform, columnList[y + 3].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(columnList[y + 2].CellType);
						Debug.Log($"switch type from {columnList[y + 2].CellType} to {newCellType} for {columnList[y + 2].ViewTransform.name}.");
						columnList[y + 2].SetType(newCellType);
					}
				}
			}
		}
	}

	private void SwitchTransformsInHierarchy(Transform t1, Transform t2)
	{
		var index1 = t1.GetSiblingIndex();
		var index2 = t2.GetSiblingIndex();

		t1.SetSiblingIndex(index2);
		t2.SetSiblingIndex(index1);
		
		Debug.Log($"Swapped hierarchy positions of {t1.name} and {t2.name}.");
	}

	private CellTypeEnum GetRandomCellType(CellTypeEnum exclusionType)
	{
		
		var randomCellType = exclusionType;
		while (randomCellType == exclusionType)
		{
			var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
			randomCellType = (CellTypeEnum) randomCellTypeIndex;
		}

		return randomCellType;
	}
	
	public override void Dispose()
	{
		base.Dispose();

		foreach (var cellPresenter in _allPresenters)
			cellPresenter.Dispose();
	}
}
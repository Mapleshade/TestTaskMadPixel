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
		for (var i = 0; i < Utils.PlateSizeX; i++)
		{
			for (var j = 0; j < Utils.PlateSizeY; j++)
			{
				var cellPresenter = _cellPresenterFactory.Create();
				_allPresenters.Add(cellPresenter);
				_rowsPresenters[i].Add(cellPresenter);
				_columnsPresenters[j].Add(cellPresenter);
				
				var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
				var randomCellType = (CellTypeEnum) randomCellTypeIndex;
				cellPresenter.InitCell(isOdd, randomCellType, View.PanelPlate);
				cellPresenter.SetViewName(i, j);
				isOdd = !isOdd;
			}

			isOdd = !isOdd;
		}

		CheckStartPlate();
	}

	private void CheckStartPlate()
	{
		//check rows
		for (var i = 0; i < Utils.PlateSizeY; i++)
		{
			var rowList = _rowsPresenters[i];
			for (var j = 0; j < rowList.Count - 3; j++)
			{
				if (rowList[j].CellType == rowList[j + 1].CellType
					&& rowList[j].CellType == rowList[j + 2].CellType)
				{
					if (rowList[j + 2].CellType != rowList[j + 3].CellType)
					{
						(rowList[j + 2], rowList[j + 3]) = (rowList[j + 3], rowList[j + 2]);
						(_columnsPresenters[j + 2][i], _columnsPresenters[j + 3][i]) = (_columnsPresenters[j + 3][i], _columnsPresenters[j + 2][i]);
						SwitchTransformsInHierarchy(rowList[j + 2].ViewTransform, rowList[j + 3].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(rowList[j + 2].CellType);
						rowList[j + 2].SetType(newCellType);
					}
				}
			}
		}

		//checkColumns
		for (var i = 0; i < Utils.PlateSizeX; i++)
		{
			var columnList = _columnsPresenters[i];
			for (var j = 0; j < columnList.Count - 3; j++)
			{
				if (columnList[j].CellType == columnList[j + 1].CellType
					&& columnList[j].CellType == columnList[j + 2].CellType)
				{
					if (columnList[j + 2].CellType != columnList[j + 3].CellType)
					{
						(columnList[j + 2], columnList[j + 3]) = (columnList[j + 3], columnList[j + 2]);
						(_rowsPresenters[j + 2][i], _rowsPresenters[j + 3][i]) = (_rowsPresenters[j + 3][i], _rowsPresenters[j + 2][i]);
						SwitchTransformsInHierarchy(columnList[j + 2].ViewTransform, columnList[j + 3].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(columnList[j + 2].CellType);
						columnList[j + 2].SetType(newCellType);
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
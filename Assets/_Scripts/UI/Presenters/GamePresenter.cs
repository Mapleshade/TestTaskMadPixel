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
			for (var x = 1; x < rowList.Count - 3; x++)
			{
				var indexFirstSubsequent = x + 1;
				var indexSecondSubsequent = x + 2;
				if (rowList[x].CellType == rowList[indexFirstSubsequent].CellType
					&& rowList[x].CellType == rowList[indexSecondSubsequent].CellType)
				{
					var indexThirdSubsequent = x + 3;
					if (rowList[indexSecondSubsequent].CellType != rowList[indexThirdSubsequent].CellType)
					{
						(rowList[indexSecondSubsequent], rowList[indexThirdSubsequent]) = (rowList[indexThirdSubsequent], rowList[indexSecondSubsequent]);
						(_columnsPresenters[indexSecondSubsequent][y], _columnsPresenters[indexThirdSubsequent][y]) = (_columnsPresenters[indexThirdSubsequent][y], _columnsPresenters[indexSecondSubsequent][y]);
						SwitchTransformsInHierarchy(rowList[indexSecondSubsequent].ViewTransform, rowList[indexThirdSubsequent].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(rowList[indexSecondSubsequent].CellType);
						Debug.Log($"switch type from {rowList[indexSecondSubsequent].CellType} to {newCellType} for {rowList[indexSecondSubsequent].ViewTransform.name}.");
						rowList[indexSecondSubsequent].SetType(newCellType);
					}
				}

				var indexFirstPrevious = x - 1;
				if (rowList[x].CellType == rowList[indexFirstSubsequent].CellType
					&& rowList[x].CellType == rowList[indexFirstPrevious].CellType)
				{
					if (rowList[indexFirstSubsequent].CellType != rowList[indexSecondSubsequent].CellType)
					{
						(rowList[indexFirstSubsequent], rowList[indexSecondSubsequent]) = (rowList[indexSecondSubsequent], rowList[indexFirstSubsequent]);
						(_columnsPresenters[indexFirstSubsequent][y], _columnsPresenters[indexSecondSubsequent][y]) = (_columnsPresenters[indexSecondSubsequent][y], _columnsPresenters[indexFirstSubsequent][y]);
						SwitchTransformsInHierarchy(rowList[indexFirstSubsequent].ViewTransform, rowList[indexSecondSubsequent].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(rowList[indexFirstSubsequent].CellType);
						Debug.Log($"switch type from {rowList[indexSecondSubsequent].CellType} to {newCellType} for {rowList[indexFirstSubsequent].ViewTransform.name}.");
						rowList[indexFirstSubsequent].SetType(newCellType);
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

	private void SwitchTransformsInHierarchy(Transform firstTransform, Transform secondTransform)
	{
		var firstIndex = firstTransform.GetSiblingIndex();
		var secondIndex = secondTransform.GetSiblingIndex();

		firstTransform.SetSiblingIndex(secondIndex);
		secondTransform.SetSiblingIndex(firstIndex);
		
		Debug.Log($"Swapped hierarchy positions of {firstTransform.name} and {secondTransform.name}.");
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
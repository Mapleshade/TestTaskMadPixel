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
						Debug.Log($"rows switch type from {rowList[indexSecondSubsequent].CellType} to {newCellType} for {rowList[indexSecondSubsequent].ViewTransform.name}.");
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
						Debug.Log($"rows switch type from {rowList[indexSecondSubsequent].CellType} to {newCellType} for {rowList[indexFirstSubsequent].ViewTransform.name}.");
						rowList[indexFirstSubsequent].SetType(newCellType);
					}
				}
			}
		}

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 1; y < columnList.Count - 3; y++)
			{
				var indexFirstSubsequent = y + 1;
				var indexSecondSubsequent = y + 2;
				if (columnList[y].CellType == columnList[indexFirstSubsequent].CellType
					&& columnList[y].CellType == columnList[indexSecondSubsequent].CellType)
				{
					var indexThirdSubsequent = y + 3;
					if (columnList[indexSecondSubsequent].CellType != columnList[indexThirdSubsequent].CellType)
					{
						(columnList[indexSecondSubsequent], columnList[indexThirdSubsequent]) = (columnList[indexThirdSubsequent], columnList[indexSecondSubsequent]);
						(_rowsPresenters[indexSecondSubsequent][x], _rowsPresenters[indexThirdSubsequent][x]) = (_rowsPresenters[indexThirdSubsequent][x], _rowsPresenters[indexSecondSubsequent][x]);
						SwitchTransformsInHierarchy(columnList[indexSecondSubsequent].ViewTransform, columnList[indexThirdSubsequent].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(columnList[indexSecondSubsequent].CellType);
						Debug.Log($"columns switch type from {columnList[indexSecondSubsequent].CellType} to {newCellType} for {columnList[indexSecondSubsequent].ViewTransform.name}.");
						columnList[indexSecondSubsequent].SetType(newCellType);
					}
				}
				
				var indexFirstPrevious = y - 1;
				if (columnList[y].CellType == columnList[indexFirstSubsequent].CellType
					&& columnList[y].CellType == columnList[indexFirstPrevious].CellType)
				{
					if (columnList[indexFirstSubsequent].CellType != columnList[indexSecondSubsequent].CellType)
					{
						(columnList[indexFirstSubsequent], columnList[indexSecondSubsequent]) = (columnList[indexSecondSubsequent], columnList[indexFirstSubsequent]);
						(_rowsPresenters[indexFirstSubsequent][x], _rowsPresenters[indexSecondSubsequent][x]) = (_rowsPresenters[indexSecondSubsequent][x], _rowsPresenters[indexFirstSubsequent][x]);
						SwitchTransformsInHierarchy(columnList[indexFirstSubsequent].ViewTransform, columnList[indexSecondSubsequent].ViewTransform);
					}
					else
					{
						var newCellType = GetRandomCellType(columnList[indexFirstSubsequent].CellType);
						Debug.Log($"columns switch type from {columnList[indexSecondSubsequent].CellType} to {newCellType} for {columnList[indexFirstSubsequent].ViewTransform.name}.");
						columnList[indexFirstSubsequent].SetType(newCellType);
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
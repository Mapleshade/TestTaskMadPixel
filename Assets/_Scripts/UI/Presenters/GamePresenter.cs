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
				CheckTwoNextCells(x, y, rowList, _columnsPresenters);
				CheckPreviousAndNextCells(x, y, rowList, _columnsPresenters);
			}
		}

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 1; y < columnList.Count - 3; y++)
			{
				CheckTwoNextCells(y, x, columnList, _rowsPresenters);
				CheckPreviousAndNextCells(y, x, columnList, _rowsPresenters);
			}
		}
	}

	private void CheckTwoNextCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList, Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;

		if (checkList[currentIndex].CellType == checkList[indexFirstSubsequent].CellType
			&& checkList[currentIndex].CellType == checkList[indexSecondSubsequent].CellType)
		{
			var indexThirdSubsequent = currentIndex + 3;
			if (checkList[indexSecondSubsequent].CellType != checkList[indexThirdSubsequent].CellType)
			{
				(checkList[indexSecondSubsequent], checkList[indexThirdSubsequent]) = (checkList[indexThirdSubsequent], checkList[indexSecondSubsequent]);
				(additionalDictionary[indexSecondSubsequent][additionalIndex], additionalDictionary[indexThirdSubsequent][additionalIndex])
					= (additionalDictionary[indexThirdSubsequent][additionalIndex], additionalDictionary[indexSecondSubsequent][additionalIndex]);
				
				SwitchTransformsInHierarchy(checkList[indexSecondSubsequent].ViewTransform, checkList[indexThirdSubsequent].ViewTransform);
			}
			else
			{
				var newCellType = GetRandomCellType(checkList[indexSecondSubsequent].CellType);
				Debug.Log($"switch type from {checkList[indexSecondSubsequent].CellType} to {newCellType} for {checkList[indexSecondSubsequent].ViewTransform.name}.");
				checkList[indexSecondSubsequent].SetType(newCellType);
			}
		}
	}

	private void CheckPreviousAndNextCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList, Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;
		var indexFirstPrevious = currentIndex - 1;

		if (checkList[currentIndex].CellType == checkList[indexFirstSubsequent].CellType
			&& checkList[currentIndex].CellType == checkList[indexFirstPrevious].CellType)
		{
			if (checkList[indexFirstSubsequent].CellType != checkList[indexSecondSubsequent].CellType)
			{
				(checkList[indexFirstSubsequent], checkList[indexSecondSubsequent]) = (checkList[indexSecondSubsequent], checkList[indexFirstSubsequent]);
				(additionalDictionary[indexFirstSubsequent][additionalIndex], additionalDictionary[indexSecondSubsequent][additionalIndex])
					= (additionalDictionary[indexSecondSubsequent][additionalIndex], additionalDictionary[indexFirstSubsequent][additionalIndex]);
				
				SwitchTransformsInHierarchy(checkList[indexFirstSubsequent].ViewTransform, checkList[indexSecondSubsequent].ViewTransform);
			}
			else
			{
				var newCellType = GetRandomCellType(checkList[indexFirstSubsequent].CellType);
				Debug.Log($"switch type from {checkList[indexSecondSubsequent].CellType} to {newCellType} for {checkList[indexFirstSubsequent].ViewTransform.name}.");
				checkList[indexFirstSubsequent].SetType(newCellType);
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
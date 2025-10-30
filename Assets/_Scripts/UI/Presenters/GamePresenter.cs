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

			CheckLastThreeCells(rowList, _columnsPresenters, y);
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

			CheckLastThreeCells(columnList, _rowsPresenters, x);
		}
	}

	private void CheckTwoNextCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList,
		Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;

		if (checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
			|| checkList[currentIndex].CellType != checkList[indexSecondSubsequent].CellType)
			return;

		var indexThirdSubsequent = currentIndex + 3;
		if (checkList[indexSecondSubsequent].CellType != checkList[indexThirdSubsequent].CellType)
		{
			(checkList[indexSecondSubsequent], checkList[indexThirdSubsequent])
				= (checkList[indexThirdSubsequent], checkList[indexSecondSubsequent]);

			(additionalDictionary[indexSecondSubsequent][additionalIndex], additionalDictionary[indexThirdSubsequent][additionalIndex])
				= (additionalDictionary[indexThirdSubsequent][additionalIndex], additionalDictionary[indexSecondSubsequent][additionalIndex]);

			SwitchTransformsInHierarchy(checkList[indexSecondSubsequent].ViewTransform,
				checkList[indexThirdSubsequent].ViewTransform, "CheckTwoNextCells");
			return;
		}

		var newCellType = GetRandomCellType(checkList[indexSecondSubsequent].CellType);
		Debug.Log($"CheckTwoNextCells switch type from {checkList[indexSecondSubsequent].CellType} to {newCellType} for {checkList[indexSecondSubsequent].ViewTransform.name}.");
		checkList[indexSecondSubsequent].SetType(newCellType);
	}

	private void CheckPreviousAndNextCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList,
		Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;
		var indexFirstPrevious = currentIndex - 1;

		if (checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
			|| checkList[currentIndex].CellType != checkList[indexFirstPrevious].CellType)
			return;

		if (checkList[indexFirstSubsequent].CellType != checkList[indexSecondSubsequent].CellType)
		{
			(checkList[indexFirstSubsequent], checkList[indexSecondSubsequent])
				= (checkList[indexSecondSubsequent], checkList[indexFirstSubsequent]);

			(additionalDictionary[indexFirstSubsequent][additionalIndex], additionalDictionary[indexSecondSubsequent][additionalIndex])
				= (additionalDictionary[indexSecondSubsequent][additionalIndex], additionalDictionary[indexFirstSubsequent][additionalIndex]);

			SwitchTransformsInHierarchy(checkList[indexFirstSubsequent].ViewTransform,
				checkList[indexSecondSubsequent].ViewTransform, "CheckPreviousAndNextCells");

			CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexFirstSubsequent]);
			CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondSubsequent]);
			return;
		}

		var newCellType = GetRandomCellType(checkList[indexFirstSubsequent].CellType);
		Debug.Log($"CheckPreviousAndNextCells switch type from {checkList[indexSecondSubsequent].CellType} to {newCellType} for {checkList[indexFirstSubsequent].ViewTransform.name}.");
		checkList[indexFirstSubsequent].SetType(newCellType);
	}

	private void CheckCellAfterSwitch(int index, List<CellPresenter> checkList)
	{
		var indexFirstSubsequent = index + 1;
		var indexSecondSubsequent = index + 2;
		var indexFirstPrevious = index - 1;
		var indexSecondPrevious = index - 2;

		if (indexFirstSubsequent < checkList.Count
			&& indexSecondSubsequent < checkList.Count
			&& checkList[index].CellType == checkList[indexFirstSubsequent].CellType
			&& checkList[index].CellType == checkList[indexSecondSubsequent].CellType)
		{
			var newCellType = GetRandomCellType(checkList[index].CellType);
			Debug.Log($"CheckCellAfterSwitch switch type from {checkList[index].CellType} to {newCellType} for {checkList[index].ViewTransform.name}.");
			checkList[index].SetType(newCellType);
		}

		if (indexFirstPrevious >= 0
			&& indexSecondPrevious >= 0
			&& checkList[index].CellType == checkList[indexFirstPrevious].CellType
			&& checkList[index].CellType == checkList[indexSecondPrevious].CellType)
		{
			var newCellType = GetRandomCellType(checkList[index].CellType);
			Debug.Log($"CheckCellAfterSwitch switch type from {checkList[index].CellType} to {newCellType} for {checkList[index].ViewTransform.name}.");
			checkList[index].SetType(newCellType);
		}
	}

	private void CheckLastThreeCells(List<CellPresenter> checkList, Dictionary<int, List<CellPresenter>> additionalDictionary, int additionalIndex)
	{
		var indexLastCell = checkList.Count - 1;
		var indexFirstPrevious = checkList.Count - 2;
		var indexSecondPrevious = checkList.Count - 3;
		var indexThirdPrevious = checkList.Count - 4;

		if (checkList[indexLastCell].CellType != checkList[indexFirstPrevious].CellType
			|| checkList[indexLastCell].CellType != checkList[indexSecondPrevious].CellType)
			return;

		if (checkList[indexSecondPrevious].CellType != checkList[indexThirdPrevious].CellType)
		{
			(checkList[indexSecondPrevious], checkList[indexThirdPrevious])
				= (checkList[indexThirdPrevious], checkList[indexSecondPrevious]);

			(additionalDictionary[indexSecondPrevious][additionalIndex], additionalDictionary[indexThirdPrevious][additionalIndex])
				= (additionalDictionary[indexThirdPrevious][additionalIndex], additionalDictionary[indexSecondPrevious][additionalIndex]);

			SwitchTransformsInHierarchy(checkList[indexSecondPrevious].ViewTransform,
				checkList[indexThirdPrevious].ViewTransform, "CheckLastThreeCells");

			CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondPrevious]);
			CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexThirdPrevious]);
			return;
		}

		var newCellType = GetRandomCellType(checkList[indexSecondPrevious].CellType);
		Debug.Log($"CheckLastThreeCells switch type from {checkList[indexSecondPrevious].CellType} to {newCellType} for {checkList[indexSecondPrevious].ViewTransform.name}.");
		checkList[indexSecondPrevious].SetType(newCellType);
	}

	private void SwitchTransformsInHierarchy(Transform firstTransform, Transform secondTransform, string debugStr)
	{
		var firstIndex = firstTransform.GetSiblingIndex();
		var secondIndex = secondTransform.GetSiblingIndex();

		firstTransform.SetSiblingIndex(secondIndex);
		secondTransform.SetSiblingIndex(firstIndex);

		Debug.Log($"{debugStr} Swapped hierarchy positions of {firstTransform.name} and {secondTransform.name}.");
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
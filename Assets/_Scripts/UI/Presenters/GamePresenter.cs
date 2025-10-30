using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class GamePresenter : BaseUIPresenter<ViewGame>
{
	private readonly CellPresenterFactory _cellPresenterFactory;
	private readonly SignalBus _signalBus;
	private readonly CompositeDisposable _disposables = new ();
	private readonly Dictionary<string, CellPresenter> _allPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _rowsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _columnsPresenters = new();
	private readonly int _cellTypesCount;
	private CellPresenter _selectedPresenter;

	public GamePresenter(ViewGame view,
		CellPresenterFactory cellPresenterFactory,
		SignalBus signalBus) : base(view)
	{
		_cellPresenterFactory = cellPresenterFactory;
		_signalBus = signalBus;
		_cellTypesCount = System.Enum.GetValues(typeof(CellTypeEnum)).Length;

		for (var i = 0; i < Utils.PlateSizeY; i++)
			_rowsPresenters.Add(i, new List<CellPresenter>());

		for (var i = 0; i < Utils.PlateSizeX; i++)
			_columnsPresenters.Add(i, new List<CellPresenter>());
	}

	public override void Initialize()
	{
		base.Initialize();

		_signalBus.GetStream<SignalPlayerTouchCellData>()
			.Subscribe(OnSignalPlayerTouchCellData)
			.AddTo(_disposables);
		
		_signalBus.GetStream<SignalPlayerTouchProcessData>()
			.Subscribe(OnSignalPlayerTouchProcessData)
			.AddTo(_disposables);
		
		_signalBus.GetStream<SignalResetPlayerInputData>()
			.Subscribe(OnSignalResetPlayerInputData)
			.AddTo(_disposables);

		GenerateGamePlate();
		CheckStartPlate();
	}

	#region Generate Game Plate

	private void GenerateGamePlate()
	{
		var isOdd = false;
		for (var y = 0; y < Utils.PlateSizeY; y++)
		{
			for (var x = 0; x < Utils.PlateSizeX; x++)
			{
				var cellPresenter = _cellPresenterFactory.Create();
				var cellName = $"Cell_{x}_{y}";
				cellPresenter.SetViewName(cellName);

				_allPresenters.Add(cellName, cellPresenter);
				_rowsPresenters[y].Add(cellPresenter);
				_columnsPresenters[x].Add(cellPresenter);

				var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
				var randomCellType = (CellTypeEnum) randomCellTypeIndex;
				cellPresenter.InitCell(isOdd, randomCellType, View.PanelPlate, x, y);
				isOdd = !isOdd;
			}

			isOdd = !isOdd;
		}
	}

	private void CheckStartPlate()
	{
		//check rows
		for (var y = 0; y < Utils.PlateSizeY; y++)
		{
			var rowList = _rowsPresenters[y];
			for (var x = 0; x < rowList.Count; x++)
			{
				CheckRightCells(x, y, rowList, _columnsPresenters);
				CheckLeftCells(x, y, rowList, _columnsPresenters);
			}
		}

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 0; y < columnList.Count; y++)
			{
				CheckRightCells(y, x, columnList, _rowsPresenters);
				CheckLeftCells(y, x, columnList, _rowsPresenters);
			}
		}

		var isPossible = CheckForPossibilityOfMergingOnGamePlate();
		Debug.Log($"plate has at least one option for merger: {isPossible}");
	}

	private void CheckRightCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList,
		Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;
		var indexThirdSubsequent = currentIndex + 3;

		var isIndexesValid = indexFirstSubsequent < checkList.Count
							&& indexSecondSubsequent < checkList.Count
							&& indexThirdSubsequent < checkList.Count;

		if (!isIndexesValid)
			return;

		if (checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
			|| checkList[currentIndex].CellType != checkList[indexSecondSubsequent].CellType)
			return;

		if (checkList[indexSecondSubsequent].CellType == checkList[indexThirdSubsequent].CellType)
		{
			SetNewRandomCellType(checkList, indexSecondSubsequent);
		}
		else
		{
			var secondCellType = checkList[indexSecondSubsequent].CellType;
			var thirdCellType = checkList[indexThirdSubsequent].CellType;

			checkList[indexSecondSubsequent].SetType(thirdCellType);
			checkList[indexThirdSubsequent].SetType(secondCellType);
		}

		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondSubsequent]);
		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexThirdSubsequent]);

		Debug.Log($"switch type for {checkList[indexSecondSubsequent].ViewTransform.name} and {checkList[indexThirdSubsequent].ViewTransform.name}.");
	}

	private void CheckLeftCells(int currentIndex, int additionalIndex, List<CellPresenter> checkList,
		Dictionary<int, List<CellPresenter>> additionalDictionary)
	{
		var indexFirstPrevious = currentIndex - 1;
		var indexSecondPrevious = currentIndex - 2;
		var indexThirdPrevious = currentIndex - 3;

		var isIndexesValid = indexFirstPrevious >= 0 && indexSecondPrevious >= 0 && indexThirdPrevious >= 0;

		if (!isIndexesValid)
			return;

		if (checkList[currentIndex].CellType != checkList[indexFirstPrevious].CellType
			|| checkList[currentIndex].CellType != checkList[indexSecondPrevious].CellType)
			return;

		if (checkList[indexSecondPrevious].CellType == checkList[indexThirdPrevious].CellType)
		{
			SetNewRandomCellType(checkList, indexSecondPrevious);
		}
		else
		{
			var secondCellType = checkList[indexSecondPrevious].CellType;
			var thirdCellType = checkList[indexThirdPrevious].CellType;

			checkList[indexSecondPrevious].SetType(thirdCellType);
			checkList[indexThirdPrevious].SetType(secondCellType);
		}

		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondPrevious]);
		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexThirdPrevious]);

		Debug.Log($"switch type for {checkList[indexSecondPrevious].ViewTransform.name} and {checkList[indexThirdPrevious].ViewTransform.name}.");
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
			&& checkList[index].CellType == checkList[indexSecondSubsequent].CellType
			|| indexFirstPrevious >= 0
			&& indexSecondPrevious >= 0
			&& checkList[index].CellType == checkList[indexFirstPrevious].CellType
			&& checkList[index].CellType == checkList[indexSecondPrevious].CellType)
		{
			SetNewRandomCellType(checkList, index);
		}
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

	private void SetNewRandomCellType(List<CellPresenter> checkList, int cellIndex)
	{
		var newCellType = GetRandomCellType(checkList[cellIndex].CellType);
		Debug.Log($"switch type from {checkList[cellIndex].CellType} to {newCellType} for {checkList[cellIndex].ViewTransform.name}.");
		checkList[cellIndex].SetType(newCellType);
	}

	#endregion

	#region Check For Possibility Of Merging

	private bool CheckForPossibilityOfMergingOnGamePlate()
	{
		var isPossible = false;
		for (var y = 0; y < Utils.PlateSizeY; y++)
		{
			var rowList = _rowsPresenters[y];
			for (var x = 0; x < rowList.Count; x++)
			{
				if (CheckRightCellsAvailableToMerge(x, rowList)
					|| CheckLeftCellsAvailableToMerge(x, rowList))
				{
					isPossible = true;
					break;
				}

				if (isPossible)
					break;
			}
		}

		if (isPossible)
			return true;

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 0; y < columnList.Count; y++)
			{
				if (CheckRightCellsAvailableToMerge(y, columnList)
					|| CheckLeftCellsAvailableToMerge(y, columnList))
				{
					isPossible = true;
					break;
				}

				if (isPossible)
					break;
			}
		}

		return isPossible;
	}

	private bool CheckRightCellsAvailableToMerge(int currentIndex, List<CellPresenter> checkList)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;
		var indexThirdSubsequent = currentIndex + 3;

		var isIndexesValid = indexFirstSubsequent < checkList.Count && indexSecondSubsequent < checkList.Count &&
							indexThirdSubsequent < checkList.Count;
		return isIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
				&& checkList[currentIndex].CellType == checkList[indexSecondSubsequent].CellType
				&& checkList[currentIndex].CellType == checkList[indexThirdSubsequent].CellType;
	}

	private bool CheckLeftCellsAvailableToMerge(int currentIndex, List<CellPresenter> checkList)
	{
		var indexFirstPrevious = currentIndex - 1;
		var indexSecondPrevious = currentIndex - 2;
		var indexThirdPrevious = currentIndex - 3;

		var isIndexesValid = indexFirstPrevious >= 0 && indexSecondPrevious >= 0 && indexThirdPrevious >= 0;
		return isIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstPrevious].CellType
				&& checkList[currentIndex].CellType == checkList[indexSecondPrevious].CellType
				&& checkList[currentIndex].CellType == checkList[indexThirdPrevious].CellType;
	}

	#endregion

	private void OnSignalPlayerTouchCellData(SignalPlayerTouchCellData signalData)
	{
		if (_allPresenters.TryGetValue(signalData.SelectedViewCellRoot.name, out var presenter))
		{
			presenter.SetActiveSelectedAnimation(true);
			_selectedPresenter = presenter;
		}
	}

	private void OnSignalPlayerTouchProcessData(SignalPlayerTouchProcessData signalData)
	{
		if (_selectedPresenter == null)
			return;

		var dotProductRight = Vector3.Dot(signalData.DirectionVector, Vector2.right);
		var dotProductUp = Vector3.Dot(signalData.DirectionVector, Vector2.up);

		if (dotProductRight > 0 && dotProductUp > 0)
		{
			//right side
			if (dotProductRight > dotProductUp)
			{
				CheckRightSideCells();
			}
			//up side
			else
			{
				_selectedPresenter.ActivateUpAnimation();
			}
		} else if (dotProductRight < 0 && dotProductUp < 0)
		{
			//left side
			if (dotProductRight < dotProductUp)
			{
				CheckLeftSideCells();
			}
			//down side
			else
			{
				_selectedPresenter.ActivateDownAnimation();
			}
		} else if (dotProductRight < 0 && dotProductUp > 0)
		{
			//left side
			if (Mathf.Abs(dotProductRight) > dotProductUp)
			{
				CheckLeftSideCells();
			}
			//up side
			else
			{
				_selectedPresenter.ActivateUpAnimation();
			}
		} else if (dotProductRight > 0 && dotProductUp < 0)
		{
			//right side
			if (dotProductRight > Mathf.Abs(dotProductUp))
			{
				CheckRightSideCells();
			}
			//down side
			else
			{
				_selectedPresenter.ActivateDownAnimation();
			}
		}
		//staying on one place
		else
		{
		}
	}

	private void CheckRightSideCells()
	{
		if (_selectedPresenter.IndexX == _rowsPresenters[_selectedPresenter.IndexY].Count - 1)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckRightCellsAvailableToMerge(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY]))
		{
			_selectedPresenter.ActivateRightAnimation();
		}
		else
		{
			_selectedPresenter.ActivateRightBadAnimation();
		}
	}

	private void CheckLeftSideCells()
	{
		if (_selectedPresenter.IndexX == 0)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckLeftCellsAvailableToMerge(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY]))
		{
			_selectedPresenter.ActivateLeftAnimation();
		}
		else
		{
			_selectedPresenter.ActivateLeftBadAnimation();
		}
	}

	private void OnSignalResetPlayerInputData(SignalResetPlayerInputData signalData)
	{
		if (_selectedPresenter == null)
			return;

		_selectedPresenter.SetActiveSelectedAnimation(false);
		_selectedPresenter = null;
	}

	public override void Dispose()
	{
		base.Dispose();

		_disposables.Dispose();

		foreach (var cellPresenter in _allPresenters)
			cellPresenter.Value.Dispose();
	}
}
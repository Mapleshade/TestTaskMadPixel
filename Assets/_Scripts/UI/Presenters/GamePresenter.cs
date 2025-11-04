using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

public class GamePresenter : BaseUIPresenter<ViewGame>
{
	private readonly CellPresenterFactory _cellPresenterFactory;
	private readonly SignalBus _signalBus;
	private readonly CompositeDisposable _disposables = new();
	private readonly Dictionary<string, CellPresenter> _allPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _rowsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _columnsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _affectedColumns = new();

	private readonly List<CellPresenter> _firstCellsBuffer = new();
	private readonly List<CellPresenter> _secondCellsBuffer = new();
	private readonly List<CellPresenter> _thirdCellsBuffer = new();
	private readonly List<CellPresenter> _fourthCellsBuffer = new();
	private readonly List<CellPresenter> _generalCellsBuffer = new();

	private readonly int _cellTypesCount;
	private readonly Tween _timerForBlockPlate;
	private CellPresenter _selectedPresenter;
	private bool _isPlateAvailable = true;

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

		for (var i = 0; i < Utils.PlateSizeX; i++)
			_affectedColumns.Add(i, new List<CellPresenter>());

		_timerForBlockPlate = DOVirtual
			.DelayedCall(1.5f, OnEndTimerForBlockPlate)
			.SetAutoKill(false)
			.SetId(this)
			.Pause();
	}

	private void OnEndTimerForBlockPlate()
	{
		_isPlateAvailable = true;
		ResetPlayerInput();
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
				cellPresenter.InitCell(isOdd, randomCellType, View.PanelPlate, View.PanelBackgrounds, x, y);
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

			checkList[indexSecondSubsequent].SetType(thirdCellType, true);
			checkList[indexThirdSubsequent].SetType(secondCellType, true);
		}

		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondSubsequent]);
		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexThirdSubsequent]);

		Debug.Log(
			$"switch type for {checkList[indexSecondSubsequent].ViewTransform.name} and {checkList[indexThirdSubsequent].ViewTransform.name}.");
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

			checkList[indexSecondPrevious].SetType(thirdCellType, true);
			checkList[indexThirdPrevious].SetType(secondCellType, true);
		}

		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexSecondPrevious]);
		CheckCellAfterSwitch(additionalIndex, additionalDictionary[indexThirdPrevious]);

		Debug.Log(
			$"switch type for {checkList[indexSecondPrevious].ViewTransform.name} and {checkList[indexThirdPrevious].ViewTransform.name}.");
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
		Debug.Log(
			$"switch type from {checkList[cellIndex].CellType} to {newCellType} for {checkList[cellIndex].ViewTransform.name}.");
		checkList[cellIndex].SetType(newCellType, true);
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
		var indexFirstPrevious = currentIndex - 1;
		var indexSecondPrevious = currentIndex - 2;

		var isForwardIndexesValid = indexFirstSubsequent < checkList.Count
									&& indexSecondSubsequent < checkList.Count
									&& indexThirdSubsequent < checkList.Count;

		var isBackIndexesValid = indexFirstSubsequent < checkList.Count
								&& indexFirstPrevious >= 0
								&& indexSecondPrevious >= 0;

		return isForwardIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
				&& checkList[currentIndex].CellType == checkList[indexSecondSubsequent].CellType
				&& checkList[currentIndex].CellType == checkList[indexThirdSubsequent].CellType
				|| isBackIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstSubsequent].CellType
				&& checkList[indexFirstSubsequent].CellType == checkList[indexFirstPrevious].CellType
				&& checkList[indexFirstSubsequent].CellType == checkList[indexSecondPrevious].CellType;
	}

	private bool CheckLeftCellsAvailableToMerge(int currentIndex, List<CellPresenter> checkList)
	{
		var indexFirstPrevious = currentIndex - 1;
		var indexSecondPrevious = currentIndex - 2;
		var indexThirdPrevious = currentIndex - 3;
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;

		var isBackIndexesValid = indexFirstPrevious
			>= 0 && indexSecondPrevious >= 0
				&& indexThirdPrevious >= 0;

		var isForwardIndexesValid = indexFirstSubsequent < checkList.Count
									&& indexSecondSubsequent < checkList.Count
									&& indexFirstPrevious >= 0;

		return isBackIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstPrevious].CellType
				&& checkList[currentIndex].CellType == checkList[indexSecondPrevious].CellType
				&& checkList[currentIndex].CellType == checkList[indexThirdPrevious].CellType
				|| isForwardIndexesValid
				&& checkList[currentIndex].CellType != checkList[indexFirstPrevious].CellType
				&& checkList[indexFirstPrevious].CellType == checkList[indexFirstSubsequent].CellType
				&& checkList[indexFirstPrevious].CellType == checkList[indexSecondSubsequent].CellType;
	}

	private bool CheckCellsFromParallelLineAvailableToMerge(int currentIndex, List<CellPresenter> checkList,
		List<CellPresenter> parallelList)
	{
		var indexFirstSubsequent = currentIndex + 1;
		var indexSecondSubsequent = currentIndex + 2;
		var indexFirstPrevious = currentIndex - 1;
		var indexSecondPrevious = currentIndex - 2;

		var isNearestOnCheckIndexesValid = indexFirstPrevious >= 0 && indexFirstSubsequent < checkList.Count;
		var isNearestOnParallelIndexesValid = indexFirstPrevious >= 0 && indexFirstSubsequent < parallelList.Count;

		var isBackIndexesValid =
			indexFirstPrevious >= 0 && indexSecondPrevious >= 0;

		var isForwardOnCheckIndexesValid =
			indexFirstSubsequent < checkList.Count && indexSecondSubsequent < checkList.Count;

		var isForwardOnParallelIndexesValid =
			indexFirstSubsequent < parallelList.Count && indexSecondSubsequent < parallelList.Count;

		var isMergePossible = isNearestOnCheckIndexesValid
							&& checkList[currentIndex].CellType != parallelList[currentIndex].CellType
							&& checkList[currentIndex].CellType == parallelList[indexFirstSubsequent].CellType
							&& checkList[currentIndex].CellType == parallelList[indexFirstPrevious].CellType;

		if (isMergePossible)
			return true;

		isMergePossible = isNearestOnParallelIndexesValid
						&& checkList[currentIndex].CellType != parallelList[currentIndex].CellType
						&& parallelList[currentIndex].CellType == checkList[indexFirstSubsequent].CellType
						&& parallelList[currentIndex].CellType == checkList[indexFirstPrevious].CellType;

		if (isMergePossible)
			return true;

		isMergePossible = isBackIndexesValid
						&& (checkList[currentIndex].CellType != parallelList[currentIndex].CellType
							&& checkList[currentIndex].CellType == parallelList[indexFirstPrevious].CellType
							&& checkList[currentIndex].CellType == parallelList[indexSecondPrevious].CellType
							|| checkList[currentIndex].CellType != parallelList[currentIndex].CellType
							&& parallelList[currentIndex].CellType == checkList[indexFirstPrevious].CellType
							&& parallelList[currentIndex].CellType == checkList[indexSecondPrevious].CellType);

		if (isMergePossible)
			return true;

		isMergePossible = isForwardOnCheckIndexesValid
						&& checkList[currentIndex].CellType != parallelList[currentIndex].CellType
						&& checkList[currentIndex].CellType == parallelList[indexFirstSubsequent].CellType
						&& checkList[currentIndex].CellType == parallelList[indexSecondSubsequent].CellType;

		if (isMergePossible)
			return true;

		isMergePossible = isForwardOnParallelIndexesValid
						&& checkList[currentIndex].CellType != parallelList[currentIndex].CellType
						&& parallelList[currentIndex].CellType == checkList[indexFirstSubsequent].CellType
						&& parallelList[currentIndex].CellType == checkList[indexSecondSubsequent].CellType;

		return isMergePossible;
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

		if (!_isPlateAvailable)
			return;

		var dotProductRight = Vector3.Dot(signalData.DirectionVector, Vector2.right);
		var dotProductUp = Vector3.Dot(signalData.DirectionVector, Vector2.up);

		var isAnimationsStarted = true;
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
				CheckUpSideCells();
			}
		}
		else if (dotProductRight < 0 && dotProductUp < 0)
		{
			//left side
			if (dotProductRight < dotProductUp)
			{
				CheckLeftSideCells();
			}
			//down side
			else
			{
				CheckDownSideCells();
			}
		}
		else if (dotProductRight < 0 && dotProductUp > 0)
		{
			//left side
			if (Mathf.Abs(dotProductRight) > dotProductUp)
			{
				CheckLeftSideCells();
			}
			//up side
			else
			{
				CheckUpSideCells();
			}
		}
		else if (dotProductRight > 0 && dotProductUp < 0)
		{
			//right side
			if (dotProductRight > Mathf.Abs(dotProductUp))
			{
				CheckRightSideCells();
			}
			//down side
			else
			{
				CheckDownSideCells();
			}
		}
		//staying on one place
		else
		{
			isAnimationsStarted = false;
		}

		if (!isAnimationsStarted)
			return;

		_isPlateAvailable = false;
		_timerForBlockPlate.Restart();
	}

	private void CheckRightSideCells()
	{
		if (_selectedPresenter.IndexX == _rowsPresenters[_selectedPresenter.IndexY].Count - 1)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckRightCellsAvailableToMerge(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY])
				|| _selectedPresenter.IndexX + 1 < _columnsPresenters.Count && CheckCellsFromParallelLineAvailableToMerge
				(_selectedPresenter.IndexY, _columnsPresenters[_selectedPresenter.IndexX],
					_columnsPresenters[_selectedPresenter.IndexX + 1]))
		{
			_selectedPresenter.ActivateRightAnimation();
			var presenter = _rowsPresenters[_selectedPresenter.IndexY][_selectedPresenter.IndexX + 1];
			presenter.ActivateLeftAnimation();
			SwapCellsTypesAndDestroyCells(presenter);
		}
		else
		{
			_selectedPresenter.ActivateRightBadAnimation();
			var presenter = _rowsPresenters[_selectedPresenter.IndexY][_selectedPresenter.IndexX + 1];
			presenter.ActivateLeftBadAnimation();
		}
	}

	private void CheckLeftSideCells()
	{
		if (_selectedPresenter.IndexX == 0)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckLeftCellsAvailableToMerge(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY])
				|| _selectedPresenter.IndexX - 1 >= 0 && CheckCellsFromParallelLineAvailableToMerge
				(_selectedPresenter.IndexY, _columnsPresenters[_selectedPresenter.IndexX],
					_columnsPresenters[_selectedPresenter.IndexX - 1]))
		{
			_selectedPresenter.ActivateLeftAnimation();
			var presenter = _rowsPresenters[_selectedPresenter.IndexY][_selectedPresenter.IndexX - 1];
			presenter.ActivateRightAnimation();
			SwapCellsTypesAndDestroyCells(presenter);
		}
		else
		{
			_selectedPresenter.ActivateLeftBadAnimation();
			var presenter = _rowsPresenters[_selectedPresenter.IndexY][_selectedPresenter.IndexX - 1];
			presenter.ActivateRightBadAnimation();
		}
	}

	private void CheckUpSideCells()
	{
		if (_selectedPresenter.IndexY == 0)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckLeftCellsAvailableToMerge(_selectedPresenter.IndexY,
					_columnsPresenters[_selectedPresenter.IndexX])
				|| _selectedPresenter.IndexY - 1 >= 0 && CheckCellsFromParallelLineAvailableToMerge
				(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY],
					_rowsPresenters[_selectedPresenter.IndexY - 1]))
		{
			_selectedPresenter.ActivateUpAnimation();
			var presenter = _columnsPresenters[_selectedPresenter.IndexX][_selectedPresenter.IndexY - 1];
			presenter.ActivateDownAnimation();
			SwapCellsTypesAndDestroyCells(presenter);
		}
		else
		{
			_selectedPresenter.ActivateUpBadAnimation();
			var presenter = _columnsPresenters[_selectedPresenter.IndexX][_selectedPresenter.IndexY - 1];
			presenter.ActivateDownBadAnimation();
		}
	}

	private void CheckDownSideCells()
	{
		if (_selectedPresenter.IndexY == _columnsPresenters[_selectedPresenter.IndexX].Count - 1)
		{
			_selectedPresenter.ActivateBadWayAnimation();
		}
		else if (CheckRightCellsAvailableToMerge(_selectedPresenter.IndexY, _columnsPresenters[_selectedPresenter.IndexX])
				|| _selectedPresenter.IndexY + 1 < _rowsPresenters.Count && CheckCellsFromParallelLineAvailableToMerge
				(_selectedPresenter.IndexX, _rowsPresenters[_selectedPresenter.IndexY],
					_rowsPresenters[_selectedPresenter.IndexY + 1]))
		{
			_selectedPresenter.ActivateDownAnimation();
			var presenter = _columnsPresenters[_selectedPresenter.IndexX][_selectedPresenter.IndexY + 1];
			presenter.ActivateUpAnimation();
			SwapCellsTypesAndDestroyCells(presenter);
		}
		else
		{
			_selectedPresenter.ActivateDownBadAnimation();
			var presenter = _columnsPresenters[_selectedPresenter.IndexX][_selectedPresenter.IndexY + 1];
			presenter.ActivateUpBadAnimation();
		}
	}

	private void SwapCellsTypesAndDestroyCells(CellPresenter presenter)
	{
		var tempCellType = _selectedPresenter.CellType;
		_selectedPresenter.SetType(presenter.CellType, false);
		presenter.SetType(tempCellType, false);
		DestroyCells(_selectedPresenter, presenter);
	}

	private void OnSignalResetPlayerInputData(SignalResetPlayerInputData signalData)
	{
		ResetPlayerInput();
	}

	private void ResetPlayerInput()
	{
		if (_selectedPresenter == null)
			return;

		_selectedPresenter.SetActiveSelectedAnimation(false);
		_selectedPresenter = null;
	}

	private void DestroyCells(CellPresenter firstPresenter, CellPresenter secondPresenter)
	{
		_firstCellsBuffer.Clear();
		_secondCellsBuffer.Clear();
		_thirdCellsBuffer.Clear();
		_fourthCellsBuffer.Clear();
		_generalCellsBuffer.Clear();

		var needToDisappearFirstPresenter = false;
		var needToDisappearSecondPresenter = false;

		if (firstPresenter.IndexX == secondPresenter.IndexX)
		{
			var cellsFromFirstRow = _rowsPresenters[firstPresenter.IndexY];
			var cellsFromSecondRow = _rowsPresenters[secondPresenter.IndexY];
			var cellsFromColumn = _columnsPresenters[firstPresenter.IndexX];

			SelectCellsForDestroying(cellsFromFirstRow, cellsFromSecondRow, cellsFromColumn,
				firstPresenter.IndexX, secondPresenter.IndexX,
				firstPresenter.IndexY, secondPresenter.IndexY,
				secondPresenter.CellType, firstPresenter.CellType);
		}

		if (firstPresenter.IndexY == secondPresenter.IndexY)
		{
			var cellsFromFirstColumn = _columnsPresenters[firstPresenter.IndexX];
			var cellsFromSecondColumn = _columnsPresenters[secondPresenter.IndexX];
			var cellsFromRow = _rowsPresenters[firstPresenter.IndexY];

			SelectCellsForDestroying(cellsFromFirstColumn, cellsFromSecondColumn, cellsFromRow,
				firstPresenter.IndexY, secondPresenter.IndexY,
				firstPresenter.IndexX, secondPresenter.IndexX,
				secondPresenter.CellType, firstPresenter.CellType);
		}

		if (_firstCellsBuffer.Count >= 2)
		{
			Debug.Log($"destroy cellsFirst");
			foreach (var presenter in _firstCellsBuffer)
				presenter.ActivateDisappearAnimation();

			needToDisappearSecondPresenter = true;
			_generalCellsBuffer.AddRange(_firstCellsBuffer);
		}

		if (_secondCellsBuffer.Count >= 2)
		{
			Debug.Log($"destroy cellsSecond");
			foreach (var presenter in _secondCellsBuffer)
				presenter.ActivateDisappearAnimation();

			needToDisappearFirstPresenter = true;
			_generalCellsBuffer.AddRange(_secondCellsBuffer);
		}

		if (_thirdCellsBuffer.Count >= 2)
		{
			Debug.Log($"destroy cellsThird");
			foreach (var presenter in _thirdCellsBuffer)
				presenter.ActivateDisappearAnimation();

			needToDisappearFirstPresenter = true;
			_generalCellsBuffer.AddRange(_thirdCellsBuffer);
		}

		if (_fourthCellsBuffer.Count >= 2)
		{
			Debug.Log($"destroy cellsFourth");
			foreach (var presenter in _fourthCellsBuffer)
				presenter.ActivateDisappearAnimation();

			needToDisappearSecondPresenter = true;
			_generalCellsBuffer.AddRange(_fourthCellsBuffer);
		}

		Debug.Log($"cellsFirst.Count: {_firstCellsBuffer.Count}, cellsSecond.Count: {_secondCellsBuffer.Count}, " +
				$"cellsThird.Count: {_thirdCellsBuffer.Count}, cellsFourth.Count: {_fourthCellsBuffer.Count}");
		if (needToDisappearFirstPresenter)
		{
			firstPresenter.ActivateDisappearAnimation();
			_generalCellsBuffer.Add(firstPresenter);
		}
		else
		{
			// firstPresenter.SetCachedNewType(secondPresenter.CellType, false, 1);
		}

		if (needToDisappearSecondPresenter)
		{
			secondPresenter.ActivateDisappearAnimation();
			_generalCellsBuffer.Add(secondPresenter);
		}
		else
		{
			// secondPresenter.SetCachedNewType(firstPresenter.CellType, false, 1);
		}

		// DropCells();
	}

	private void SelectCellsForDestroying
	(List<CellPresenter> firstParallelLineCells,
		List<CellPresenter> secondParallelLineCells,
		List<CellPresenter> perpendicularLineCells,
		int firstPresenterFirstIndex,
		int secondPresenterFirstIndex,
		int firstPresenterSecondIndex,
		int secondPresenterSecondIndex,
		CellTypeEnum firstCellType,
		CellTypeEnum secondCellType)
	{
		for (var i = firstPresenterFirstIndex + 1; i < firstParallelLineCells.Count; i++)
		{
			if (firstParallelLineCells[i].CellType == secondCellType)
				_firstCellsBuffer.Add(firstParallelLineCells[i]);
			else
				break;
		}

		for (var i = firstPresenterFirstIndex - 1; i >= 0; i--)
		{
			if (firstParallelLineCells[i].CellType == secondCellType)
				_firstCellsBuffer.Add(firstParallelLineCells[i]);
			else
				break;
		}

		for (var i = secondPresenterFirstIndex + 1; i < secondParallelLineCells.Count; i++)
		{
			if (secondParallelLineCells[i].CellType == firstCellType)
				_secondCellsBuffer.Add(secondParallelLineCells[i]);
			else
				break;
		}

		for (var i = firstPresenterFirstIndex - 1; i >= 0; i--)
		{
			if (secondParallelLineCells[i].CellType == firstCellType)
				_secondCellsBuffer.Add(secondParallelLineCells[i]);
			else
				break;
		}

		if (secondPresenterSecondIndex > firstPresenterSecondIndex)
		{
			for (var i = secondPresenterSecondIndex + 1; i < perpendicularLineCells.Count; i++)
			{
				if (perpendicularLineCells[i].CellType == firstCellType)
					_thirdCellsBuffer.Add(perpendicularLineCells[i]);
				else
					break;
			}

			for (var i = firstPresenterSecondIndex - 1; i >= 0; i--)
			{
				if (perpendicularLineCells[i].CellType == secondCellType)
					_fourthCellsBuffer.Add(perpendicularLineCells[i]);
				else
					break;
			}
		}
		else
		{
			for (var i = firstPresenterSecondIndex + 1; i < perpendicularLineCells.Count; i++)
			{
				if (perpendicularLineCells[i].CellType == secondCellType)
					_fourthCellsBuffer.Add(perpendicularLineCells[i]);
				else
					break;
			}

			for (var i = secondPresenterSecondIndex - 1; i >= 0; i--)
			{
				if (perpendicularLineCells[i].CellType == firstCellType)
					_thirdCellsBuffer.Add(perpendicularLineCells[i]);
				else
					break;
			}
		}
	}


	// private void DropCells()
	// {
	// 	foreach (var cellsList in _affectedColumns.Values)
	// 		cellsList.Clear();
	//
	// 	foreach (var cellPresenter in _generalCellsBuffer)
	// 	{
	// 		if (!_affectedColumns[cellPresenter.IndexX].Contains(cellPresenter))
	// 			_affectedColumns[cellPresenter.IndexX].Add(cellPresenter);
	// 	}
	//
	// 	foreach (var affectedColumn in _affectedColumns)
	// 	{
	// 		foreach (var cellPresenter in affectedColumn.Value)
	// 		{
	// 			Debug.Log($"DropCells column: {affectedColumn.Key}, count: {affectedColumn.Value.Count}, cell x: {cellPresenter.IndexX}, cell y: {cellPresenter.IndexY}");
	// 		}
	// 	}
	//
	// 	for (var i = 0; i < _affectedColumns.Count; i++)
	// 	{
	// 		var affectedCells = _affectedColumns[i];
	// 		if (affectedCells.Count == 0)
	// 				continue;
	// 	
	// 		var allCellsInColumn = _columnsPresenters[i];
	// 		var affectedCellsCount = affectedCells.Count;
	// 		var maxAffectedIndex = 0;
	// 	
	// 		foreach (var cell in affectedCells)
	// 		{
	// 			if (maxAffectedIndex < cell.IndexY)
	// 				maxAffectedIndex = cell.IndexY;
	// 		}
	// 	
	// 		Debug.Log($"DropCells column: {i}, maxAffectedIndex: {maxAffectedIndex}");
	//
	// 		for (var j = 0; j <= maxAffectedIndex; j++)
	// 		{
	// 			if (j - affectedCellsCount < 0)
	// 			{
	// 				var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
	// 				var randomCellType = (CellTypeEnum) randomCellTypeIndex;
	// 				allCellsInColumn[j].SetCachedNewType(randomCellType, true, j + 1);
	// 				continue;
	// 			}
	// 			
	// 			allCellsInColumn[j].SetCachedNewType(allCellsInColumn[j - affectedCellsCount].CellType, false, j - affectedCellsCount + 1);
	// 		}
	// 	}
	// }
	//
	
	public override void Dispose()
	{
		base.Dispose();

		_disposables.Dispose();
		DOTween.Kill(this);

		foreach (var cellPresenter in _allPresenters)
			cellPresenter.Value.Dispose();
	}
}
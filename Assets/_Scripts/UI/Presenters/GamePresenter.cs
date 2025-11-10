using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class GamePresenter : BaseUIPresenter<ViewGame>
{
	private readonly CellPresenterFactory _cellPresenterFactory;
	private readonly SignalBus _signalBus;
	private readonly CompositeDisposable _disposables = new();
	private readonly Dictionary<string, CellPresenter> _allPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _rowsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _columnsPresenters = new();
	private readonly Dictionary<int, List<CellPresenter>> _affectedColumns = new();
	private readonly CellPresenter[,] _gameBoard;

	private readonly List<CellPresenter> _matchesBuffer = new();
	private readonly List<CellPresenter> _matchPositionsBuffer = new();

	private readonly int _cellTypesCount;
	private readonly Tween _timerForBlockPlate;
	private readonly Tween _timerWaitingMoveAnimations;
	private readonly Tween _timerWaitingDisappearAnimations;
	private readonly Tween _timerWaitingDropAnimations;
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

		_gameBoard = new CellPresenter[Utils.PlateSizeX, Utils.PlateSizeY];

		_timerForBlockPlate = DOVirtual
			.DelayedCall(10f, OnEndTimerForBlockPlate)
			.SetAutoKill(false)
			.SetId(this)
			.Pause();

		_timerWaitingMoveAnimations = DOVirtual
			.DelayedCall(1.1f, OnEndTimerWaitingMoveAnimations)
			.SetAutoKill(false)
			.SetId(this)
			.Pause();

		_timerWaitingDisappearAnimations = DOVirtual
			.DelayedCall(1.1f, OnEndTimerWaitingDisappearAnimations)
			.SetAutoKill(false)
			.SetId(this)
			.Pause();

		_timerWaitingDropAnimations = DOVirtual
			.DelayedCall(1.1f, OnEndTimerWaitingDropAnimations)
			.SetAutoKill(false)
			.SetId(this)
			.Pause();
	}

	private void OnEndTimerForBlockPlate()
	{
		_isPlateAvailable = true;
		ResetPlayerInput();
	}

	private void OnEndTimerWaitingMoveAnimations()
	{
		StartDisappearAnimations();
	}

	private void StartDisappearAnimations()
	{
		foreach (var cellPresenter in _matchesBuffer)
			cellPresenter.ActivateDisappearAnimation();

		_timerWaitingDisappearAnimations.Restart();
	}

	private void OnEndTimerWaitingDisappearAnimations()
	{
		DropCells();
		_timerWaitingDropAnimations.Restart();
	}

	private void OnEndTimerWaitingDropAnimations()
	{
		CheckForMatches();
		if (_matchesBuffer.Count <= 0)
		{
			_isPlateAvailable = true;
			ResetPlayerInput();
			var canContinueGame = CanContinueGame();
			if (!canContinueGame)
				Debug.Log($"Game over!");
		}
		else
		{
			StartDisappearAnimations();
		}
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

		View.PanelBackgrounds.GetComponent<RectTransform>().sizeDelta =
			new Vector2(100 * Utils.PlateSizeX, 100 * Utils.PlateSizeY);
		View.PanelPlate.GetComponent<RectTransform>().sizeDelta =
			new Vector2(100 * Utils.PlateSizeX, 100 * Utils.PlateSizeY);

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
				_gameBoard[x, y] = cellPresenter;

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
				|| _selectedPresenter.IndexX + 1 < _columnsPresenters.Count &&
				CheckCellsFromParallelLineAvailableToMerge
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
		else if (CheckRightCellsAvailableToMerge(_selectedPresenter.IndexY,
					_columnsPresenters[_selectedPresenter.IndexX])
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
		CheckForMatches();
		var debugStr = string.Empty;
		foreach (var cellPresenter in _matchesBuffer)
		{
			debugStr += $"cell: {cellPresenter.IndexX}, {cellPresenter.IndexY}; ";
		}

		Debug.Log($"_matchesBuffer.count: {_matchesBuffer.Count} {debugStr}");
		// DestroyCells(_selectedPresenter, presenter);


		_timerWaitingMoveAnimations.Restart();
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

	private void CheckForMatches()
	{
		_matchesBuffer.Clear();
		for (var y = 0; y < Utils.PlateSizeY; y++)
		{
			var rowList = _rowsPresenters[y];
			for (var x = 0; x < rowList.Count; x++)
			{
				CheckLine(x, 0, 0, 1);
			}
		}

		//checkColumns
		for (var x = 0; x < Utils.PlateSizeX; x++)
		{
			var columnList = _columnsPresenters[x];
			for (var y = 0; y < columnList.Count; y++)
			{
				CheckLine(0, y, 1, 0);
			}
		}
	}
	
	private bool CanContinueGame()
	{
		int rows = _gameBoard.GetLength(0);
		int cols = _gameBoard.GetLength(1);

		// Проверяем каждую клетку на возможность перемещения
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				// Проверяем возможные перемещения: вправо и вниз
				if (IsMovePossible(i, j, i, j + 1) || IsMovePossible( i, j, i + 1, j))
				{
					return true; // Если хотя бы одно перемещение возможно, возвращаем true
				}
			}
		}

		return false; // Если не найдено ни одного возможного перемещения
	}

	private bool IsMovePossible(int x1, int y1, int x2, int y2)
	{
		// Проверяем границы доски
		if (x2 >= _gameBoard.GetLength(0) || y2 >= _gameBoard.GetLength(1))
			return false;

		// Меняем местами элементы
		var temp = _gameBoard[x1, y1];
		_gameBoard[x1, y1] = _gameBoard[x2, y2];
		_gameBoard[x2, y2] = temp;

		// Проверяем на наличие последовательностей
		CheckForMatches();
	
		bool hasMatches = _matchesBuffer.Count > 0;

		// Возвращаем обратно элементы на место
		_gameBoard[x2, y2] = _gameBoard[x1, y1];
		_gameBoard[x1, y1] = temp;

		return hasMatches;
	}
	
	private void CheckLine(int startX, int startY, int deltaX, int deltaY)
	{
		_matchPositionsBuffer.Clear();
		var count = 1;
		var currentCellType = _gameBoard[startX, startY].CellType;
		_matchPositionsBuffer.Add(_gameBoard[startX, startY]);

		for (var i = 1; i < Math.Max(_gameBoard.GetLength(0), _gameBoard.GetLength(1)); i++)
		{
			var x = startX + i * deltaX;
			var y = startY + i * deltaY;

			if (x >= _gameBoard.GetLength(0) || y >= _gameBoard.GetLength(1))
				break;

			if (_gameBoard[x, y].CellType == currentCellType)
			{
				count++;
				_matchPositionsBuffer.Add(_gameBoard[x, y]);
			}
			else
			{
				if (count >= 3)
				{
					// Добавляем все позиции текущей последовательности
					AddRangeExceptDuplicates(_matchesBuffer, _matchPositionsBuffer);
				}

				currentCellType = _gameBoard[x, y].CellType;
				count = 1;
				_matchPositionsBuffer.Clear(); // очищаем список для новой последовательности
				_matchPositionsBuffer.Add(_gameBoard[x, y]); // добавляем новую начальную позицию
			}
		}

		// Проверяем последний сегмент
		if (count >= 3)
			AddRangeExceptDuplicates(_matchesBuffer, _matchPositionsBuffer);
	}

	private void AddRangeExceptDuplicates(List<CellPresenter> mainList, List<CellPresenter> additionalList)
	{
		foreach (var cellPresenter in additionalList)
		{
			if (!mainList.Contains(cellPresenter))
				mainList.Add(cellPresenter);
		}
	}

	private void DropCells()
	{
		foreach (var cellsList in _affectedColumns.Values)
			cellsList.Clear();

		foreach (var cellPresenter in _matchesBuffer)
		{
			if (!_affectedColumns[cellPresenter.IndexX].Contains(cellPresenter))
				_affectedColumns[cellPresenter.IndexX].Add(cellPresenter);
		}

		foreach (var affectedColumn in _affectedColumns)
		{
			foreach (var cellPresenter in affectedColumn.Value)
			{
				Debug.Log(
					$"DropCells column: {affectedColumn.Key}, count: {affectedColumn.Value.Count}, cell x: {cellPresenter.IndexX}, cell y: {cellPresenter.IndexY}");
			}
		}

		for (var i = 0; i < _affectedColumns.Count; i++)
		{
			var affectedCells = _affectedColumns[i];
			if (affectedCells.Count == 0)
				continue;

			var allCellsInColumn = _columnsPresenters[i];
			var affectedCellsCount = affectedCells.Count;
			var maxAffectedIndex = 0;

			foreach (var cell in affectedCells)
			{
				if (maxAffectedIndex < cell.IndexY)
					maxAffectedIndex = cell.IndexY;
			}

			Debug.Log($"DropCells column: {i}, maxAffectedIndex: {maxAffectedIndex}");

			for (var j = maxAffectedIndex; j >= 0; j--)
			{
				if (j - affectedCellsCount < 0)
				{
					var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
					var randomCellType = (CellTypeEnum) randomCellTypeIndex;
					allCellsInColumn[j].SetType(randomCellType, true);
					allCellsInColumn[j].ActivateDropAnimation(true, j + 1);
					continue;
				}

				allCellsInColumn[j].SetType(allCellsInColumn[j - affectedCellsCount].CellType, true);
				allCellsInColumn[j].ActivateDropAnimation(false, affectedCellsCount);
			}
		}
	}


	public override void Dispose()
	{
		base.Dispose();

		_disposables.Dispose();
		DOTween.Kill(this);

		foreach (var cellPresenter in _allPresenters)
			cellPresenter.Value.Dispose();
	}
}
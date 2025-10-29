using System.Collections.Generic;
using UnityEngine;

public class GamePresenter : BaseUIPresenter<ViewGame>
{
	private readonly CellPresenterFactory _cellPresenterFactory;
	private readonly List<CellPresenter> _presenters = new();
	private readonly int _cellTypesCount;

	public GamePresenter(ViewGame view, CellPresenterFactory cellPresenterFactory) : base(view)
	{
		_cellPresenterFactory = cellPresenterFactory;
		_cellTypesCount = System.Enum.GetValues(typeof(CellTypeEnum)).Length;
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
				_presenters.Add(cellPresenter);
				
				var randomCellTypeIndex = Random.Range(0, _cellTypesCount);
				var randomCellType = (CellTypeEnum) randomCellTypeIndex;
				cellPresenter.InitCell(isOdd, randomCellType, View.PanelPlate);
				isOdd = !isOdd;
			}

			isOdd = !isOdd;
		}
	}

	public override void Dispose()
	{
		base.Dispose();

		foreach (var cellPresenter in _presenters)
			cellPresenter.Dispose();
	}
}
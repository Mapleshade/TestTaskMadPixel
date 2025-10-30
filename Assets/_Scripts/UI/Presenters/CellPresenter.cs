using DG.Tweening;
using UnityEngine;

public class CellPresenter : UiPresenter
{
	private readonly ViewCellRootPool _viewCellRootPool;
	private readonly Tween _selectedAnimation;
	private readonly Tween _badWayAnimation;
	private readonly Tween _disappearAnimation;
	private readonly Tween _leftMoveAnimation;
	private readonly Tween _rightMoveAnimation;
	private readonly Tween _upMoveAnimation;
	private readonly Tween _downMoveAnimation;
	private ViewCellRoot View { get; }
	public CellTypeEnum CellType { get; private set; }
	public Transform ViewTransform => View.transform;

	public CellPresenter(ViewCellRootPool pool)
	{
		_viewCellRootPool = pool;
		View = _viewCellRootPool.Spawn();

		var viewPanelFruitsRoot = View.PanelFruitsRoot;

		_selectedAnimation = DOTween.Sequence()
			.Append(View.ImageShine.DOFade(0.3f, 1.5f))
			.SetLoops(-1, LoopType.Yoyo)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_badWayAnimation = DOTween.Sequence()
			.Append(viewPanelFruitsRoot.DOShakePosition(1.5f, 2))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_disappearAnimation = DOTween.Sequence()
			.Append(View.CanvasGroupFruitsRoot.DOFade(0, 1f))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		var rootSizeDelta = View.PaneRoot.sizeDelta;
		var cellWidth = rootSizeDelta.x;
		var cellHeight = rootSizeDelta.y;

		_leftMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(-cellWidth, 0f), 1.5f))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_rightMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(cellWidth, 0f), 1.5f))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_upMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, cellHeight), 1.5f))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_downMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, -cellHeight), 1.5f))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();
	}

	public void SetType(CellTypeEnum cellType)
	{
		CellType = cellType;

		foreach (var cellTypeData in View.ImagesCellType)
			cellTypeData.PanelImage.CheckSetActive(cellTypeData.CellType == cellType);
	}

	public void InitCell(bool isLight, CellTypeEnum cellType, Transform parentTransform)
	{
		View.transform.SetParent(parentTransform);
		View.transform.localScale = Vector3.one;
		View.ImagesCellBackground[0].PanelBackground.CheckSetActive(isLight);
		View.ImagesCellBackground[1].PanelBackground.CheckSetActive(!isLight);
		SetType(cellType);
	}

	public void SetViewName(string cellName)
	{
		View.name = cellName;
	}

	public void SetActiveSelectedAnimation(bool isActive)
	{
		View.ImageShine.enabled = isActive;

		if (isActive)
			_selectedAnimation.Play();
		else
			_selectedAnimation.Rewind();
	}

	public override void Dispose()
	{
		base.Dispose();
		DOTween.Kill(this);
		_viewCellRootPool.Despawn(View);
	}
}
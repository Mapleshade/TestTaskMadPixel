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
	private readonly Tween _leftBadMoveAnimation;
	private readonly Tween _rightBadMoveAnimation;
	private readonly Tween _upBadMoveAnimation;
	private readonly Tween _downBadMoveAnimation;
	private ViewCellRoot View { get; }
	public CellTypeEnum CellType { get; private set; }
	public Transform ViewTransform => View.transform;
	public int IndexX { get; private set; }
	public int IndexY { get; private set; }
	private bool _needToDisappear;

	public CellPresenter(ViewCellRootPool pool)
	{
		_viewCellRootPool = pool;
		View = _viewCellRootPool.Spawn();

		#region service animations

		_selectedAnimation = DOTween.Sequence()
			.Append(View.ImageShine.DOFade(0.3f, 1.5f))
			.SetLoops(-1, LoopType.Yoyo)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_badWayAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOShakePosition(1.5f, 2))
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_disappearAnimation = DOTween.Sequence()
			.Append(View.CanvasGroupFruitsRoot.DOFade(0, 1f))
			.AppendCallback(AfterDisappear)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		#endregion

		#region move animations

		var rootSizeDelta = View.PaneRoot.sizeDelta;
		var cellWidth = rootSizeDelta.x;
		var cellHeight = rootSizeDelta.y;

		_leftMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(-cellWidth, 0f), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_rightMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(cellWidth, 0f), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_upMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, cellHeight), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_downMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, -cellHeight), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		#endregion

		#region bad move animations

		_leftBadMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(-cellWidth, 0f), 0.5f))
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, 0f), 0.5f))
			.AppendCallback(AfterBadMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_rightBadMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(cellWidth, 0f), 0.5f))
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, 0f), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_upBadMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, cellHeight), 0.5f))
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, 0f), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		_downBadMoveAnimation = DOTween.Sequence()
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, -cellHeight), 0.5f))
			.Append(View.PanelFruitsRoot.DOAnchorPos(new Vector2(0f, 0f), 0.5f))
			.AppendCallback(AfterMove)
			.SetId(this)
			.SetAutoKill(false)
			.Pause();

		#endregion
	}

	public void SetType(CellTypeEnum cellType)
	{
		CellType = cellType;

		foreach (var cellTypeData in View.ImagesCellType)
			cellTypeData.PanelImage.CheckSetActive(cellTypeData.CellType == cellType);
	}

	public void InitCell(bool isLight, CellTypeEnum cellType, Transform parentTransform, int indexX, int indexY)
	{
		View.transform.SetParent(parentTransform);
		View.transform.localScale = Vector3.one;
		View.ImagesCellBackground[0].PanelBackground.CheckSetActive(isLight);
		View.ImagesCellBackground[1].PanelBackground.CheckSetActive(!isLight);
		SetType(cellType);
		IndexX = indexX;
		IndexY = indexY;
	}

	public void SetViewName(string cellName)
	{
		View.name = cellName;
	}

	#region animations methods
	
	public void SetActiveSelectedAnimation(bool isActive)
	{
		View.ImageShine.enabled = isActive;

		if (isActive)
			_selectedAnimation.Play();
		else
			_selectedAnimation.Rewind();
	}

	public void ActivateLeftAnimation()
	{
		if (!_leftMoveAnimation.IsPlaying())
			_leftMoveAnimation.Restart();
	}

	public void ActivateRightAnimation()
	{
		if (!_rightMoveAnimation.IsPlaying())
			_rightMoveAnimation.Restart();
	}

	public void ActivateUpAnimation()
	{
		if (!_upMoveAnimation.IsPlaying())
			_upMoveAnimation.Restart();
	}

	public void ActivateDownAnimation()
	{
		if (!_downMoveAnimation.IsPlaying())
			_downMoveAnimation.Restart();
	}

	public void ActivateLeftBadAnimation()
	{
		if (!_leftBadMoveAnimation.IsPlaying())
			_leftBadMoveAnimation.Restart();
	}

	public void ActivateRightBadAnimation()
	{
		if (!_rightBadMoveAnimation.IsPlaying())
			_rightBadMoveAnimation.Restart();
	}

	public void ActivateUpBadAnimation()
	{
		if (!_upBadMoveAnimation.IsPlaying())
			_upBadMoveAnimation.Restart();
	}

	public void ActivateDownBadAnimation()
	{
		if (!_downBadMoveAnimation.IsPlaying())
			_downBadMoveAnimation.Restart();
	}

	private void AfterMove()
	{
		if (_needToDisappear)
		{
			_needToDisappear = false;
			_disappearAnimation.Restart();
			return;
		}

		_rightMoveAnimation.Rewind();
		_leftMoveAnimation.Rewind();
		_upMoveAnimation.Rewind();
		_downMoveAnimation.Rewind();
	}

	private void AfterBadMove()
	{
		_rightBadMoveAnimation.Rewind();
		_leftBadMoveAnimation.Rewind();
		_upBadMoveAnimation.Rewind();
		_downBadMoveAnimation.Rewind();
	}

	private void AfterDisappear()
	{
		_disappearAnimation.Rewind();
		_rightMoveAnimation.Rewind();
		_leftMoveAnimation.Rewind();
		_upMoveAnimation.Rewind();
		_downMoveAnimation.Rewind();
	}

	#endregion

	public override void Dispose()
	{
		base.Dispose();
		DOTween.Kill(this);
		_viewCellRootPool.Despawn(View);
	}
}
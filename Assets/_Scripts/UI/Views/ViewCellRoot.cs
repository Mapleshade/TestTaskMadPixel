using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewCellRoot : BaseUIView
{
	[SerializeField]
	private RectTransform _panelFruitsRoot;
	[SerializeField]
	private Image _imageShine;
	[SerializeField]
	private List<CellBackgroundData> _imagesCellBackground;
	[SerializeField]
	private List<CellTypeData> _imagesCellType;

	public RectTransform PanelFruitsRoot => _panelFruitsRoot;
	public List<CellBackgroundData> ImagesCellBackground => _imagesCellBackground;
	public List<CellTypeData> ImagesCellType => _imagesCellType;
	public Image ImageShine => _imageShine;
}
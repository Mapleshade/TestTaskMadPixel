using System;
using UnityEngine;

[Serializable]
public class CellTypeData
{
	[SerializeField]
	private CellTypeEnum _cellType;
	[SerializeField]
	private GameObject _panelImage;

	public CellTypeEnum CellType => _cellType;
	public GameObject PanelImage => _panelImage;
}

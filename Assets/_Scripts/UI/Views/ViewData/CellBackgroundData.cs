using System;
using UnityEngine;

[Serializable]
public class CellBackgroundData
{
	[SerializeField]
	private CellBackgroundEnum _cellBackground;
	[SerializeField]
	private GameObject _panelBackground;

	public CellBackgroundEnum CellBackground => _cellBackground;
	public GameObject PanelBackground => _panelBackground;
}
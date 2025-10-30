using UnityEngine;

public class ViewGame  : BaseUIView
{
    [SerializeField]
	private Transform _panelPlate;
    [SerializeField]
	private Transform _panelBackgrounds;
	public Transform PanelPlate => _panelPlate;
	public Transform PanelBackgrounds => _panelBackgrounds;
}

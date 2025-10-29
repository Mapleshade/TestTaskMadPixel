using UnityEngine;

[CreateAssetMenu(fileName = "GameUISettings", menuName = "ScriptableObjects/Create Game UI Settings")]
public class GameUISettings : ScriptableObject
{
	[SerializeField]
	private ViewGame _viewGame;
	[SerializeField]
	private ViewCellRoot _viewCellRoot;

	public ViewGame ViewGame => _viewGame;
	public ViewCellRoot ViewCellRoot => _viewCellRoot;
}

using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuUISettings", menuName = "ScriptableObjects/Create Main Menu UI Settings")]
public class MainMenuUISettings : ScriptableObject
{
	[SerializeField]
	private ViewMainMenu _viewMainMenu;

	public ViewMainMenu ViewMainMenu => _viewMainMenu;
}
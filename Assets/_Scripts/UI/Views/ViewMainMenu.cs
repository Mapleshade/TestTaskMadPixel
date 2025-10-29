using UnityEngine;
using UnityEngine.UI;

//disable warning CS0649:Field is never assigned to, and will always have its default value 
#pragma warning disable CS0649
public class ViewMainMenu : BaseUIView
{
	[SerializeField]
	private Button _newGameButton;
	[SerializeField]
	private Button _loadGameButton;
	[SerializeField]
	private Button _settingsButton;
	[SerializeField]
	private Button _exitButton;

	public Button NewGameButton => _newGameButton;
	public Button LoadGameButton => _loadGameButton;
	public Button SettingsButton => _settingsButton;
	public Button ExitButton => _exitButton;
}
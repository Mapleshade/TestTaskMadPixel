using UnityEngine;
using UnityEngine.UI;

//disable warning CS0649:Field is never assigned to, and will always have its default value 
#pragma warning disable CS0649
public class ViewMainMenu : BaseUIView
{
	[SerializeField]
	private Button _newGameButton;

	public Button NewGameButton => _newGameButton;
}
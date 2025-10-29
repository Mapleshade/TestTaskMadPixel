using UnityEngine;
using Zenject;

public class testZen : MonoBehaviour
{
	private MainMenuPresenter _mainMenuPresenter;

	[Inject]
	private void Init(MainMenuPresenter mainMenuPresenter)
	{
		_mainMenuPresenter = mainMenuPresenter;
	}
}
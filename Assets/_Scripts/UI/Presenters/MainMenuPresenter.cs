using UniRx;
using UnityEngine;

public class MainMenuPresenter : BaseUIPresenter<ViewMainMenu>
{
	public override void Initialize()
	{
		base.Initialize();
		
		View.NewGameButton.OnClickAsObservable().Subscribe(_ => Debug.Log($"NewGameButton")).AddTo(View.NewGameButton);
		View.ExitButton.OnClickAsObservable().Subscribe(_ => Debug.Log($"ExitButton")).AddTo(View.ExitButton);
		View.SettingsButton.OnClickAsObservable().Subscribe(_ => Debug.Log($"SettingsButton")).AddTo(View.SettingsButton);
		View.LoadGameButton.OnClickAsObservable().Subscribe(_ => Debug.Log($"LoadGameButton")).AddTo(View.LoadGameButton);
	}

	public MainMenuPresenter(ViewMainMenu view) : base(view)
	{
	}
}
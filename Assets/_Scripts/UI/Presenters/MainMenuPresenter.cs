using UniRx;
using UnityEngine;

public class MainMenuPresenter : BaseUIPresenter<ViewMainMenu>
{
	public override void Initialize()
	{
		base.Initialize();
		
		View.NewGameButton.OnClickAsObservable().Subscribe(_ => Debug.Log($"NewGameButton")).AddTo(View.NewGameButton);
	}

	public MainMenuPresenter(ViewMainMenu view) : base(view)
	{
	}
}
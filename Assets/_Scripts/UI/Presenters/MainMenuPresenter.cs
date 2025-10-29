using UniRx;
using UnityEngine.SceneManagement;

public class MainMenuPresenter : BaseUIPresenter<ViewMainMenu>
{
	public override void Initialize()
	{
		base.Initialize();
		
		View.NewGameButton.OnClickAsObservable().Subscribe(_ => SceneManager.LoadScene("Game")).AddTo(View.NewGameButton);
	}

	public MainMenuPresenter(ViewMainMenu view) : base(view)
	{
	}
}
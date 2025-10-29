using System.Linq;
using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller<MainMenuInstaller>
{
	[SerializeField]
	private MainMenuUISettings _uiSettings;

	public override void InstallBindings()
	{
		var viewCanvases = transform.GetComponentsInChildren<ViewCanvas>().ToList();

		Container.BindInstance(_uiSettings).AsSingle();

		Container.Bind<ViewMainMenu>()
			.FromComponentInNewPrefab(_uiSettings.ViewMainMenu)
			.UnderTransform(_uiSettings.ViewMainMenu.GetCanvas(viewCanvases))
			.AsSingle();
		Container.BindInterfacesAndSelfTo<MainMenuPresenter>().AsSingle().NonLazy();
	}
}
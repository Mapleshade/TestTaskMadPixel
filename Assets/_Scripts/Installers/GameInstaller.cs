using System.Linq;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
    [SerializeField]
    private GameUISettings _uiSettings;

    public override void InstallBindings()
    {
        #region Signals

        SignalBusInstaller.Install(Container);

        Container.DeclareSignal<SignalPlayerTouchCellData>();
        Container.DeclareSignal<SignalPlayerTouchProcessData>();
        Container.DeclareSignal<SignalResetPlayerInputData>();

        #endregion

        #region UI

        var viewCanvases = transform.GetComponentsInChildren<ViewCanvas>().ToList();

		Container.BindInstance(_uiSettings).AsSingle();

		Container.Bind<ViewGame>()
			.FromComponentInNewPrefab(_uiSettings.ViewGame)
			.UnderTransform(_uiSettings.ViewGame.GetCanvas(viewCanvases))
			.AsSingle();
		Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle().NonLazy();
        
		Container
			.BindMemoryPool<ViewCellRoot, ViewCellRootPool>()
			.WithInitialSize(Utils.PlateSizeX * Utils.PlateSizeY)
			.FromComponentInNewPrefab(_uiSettings.ViewCellRoot)
			.UnderTransformGroup("ViewCellRootPool");
        
		Container.BindFactory<CellPresenter, CellPresenterFactory>();

        #endregion

#if UNITY_EDITOR
        Container.Bind(typeof(ITickable)).To<PlayerStandaloneInputProvider>().AsSingle().NonLazy();
#else
        Container.Bind(typeof(ITickable)).To<PlayerMobileInputProvider>().AsSingle().NonLazy();
#endif
    }
}
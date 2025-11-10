using Zenject;

public class GameController
{
	private readonly GamePresenter _gamePresenter;
	private readonly SignalBus _signalBus;

	public GameController(GamePresenter gamePresenter, SignalBus signalBus)
	{
		_gamePresenter = gamePresenter;
		_signalBus = signalBus;
	}
}

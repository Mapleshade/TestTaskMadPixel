public class SignalPlayerTouchCellData
{
	public ViewCellRoot SelectedViewCellRoot { get; private set; }

	public SignalPlayerTouchCellData(ViewCellRoot selectedViewCellRoot)
	{
		SelectedViewCellRoot = selectedViewCellRoot;
	}
}
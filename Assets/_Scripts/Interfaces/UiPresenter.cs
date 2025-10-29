using System;
using UniRx;
using Zenject;

public class UiPresenter : IInitializable, IDisposable, IUIPresenter
{
	protected readonly BoolReactiveProperty IsShownProperty = new();

	public bool IsShown => IsShownProperty.Value;

	public virtual void InitialEnable()
	{
		IsShownProperty.Value = true;
	}

	public virtual void InitialDisable()
	{
		IsShownProperty.Value = false;
	}

	public virtual void Show()
	{
		IsShownProperty.Value = true;
	}

	public virtual void Hide()
	{
		IsShownProperty.Value = false;
	}

	/// <summary>
	/// Call this AFTER ancestor's logic
	/// </summary>
	public virtual void Dispose()
	{
	}

	public virtual void Initialize()
	{
	}
}
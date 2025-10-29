using System;
using Zenject;

public abstract class BaseUIPresenter<T> : IInitializable, IDisposable, IUIPresenter where T : BaseUIView
{
	public readonly T View;
	
	public BaseUIPresenter(T view)
	{
		View = view;
	}

	public virtual void Show()
	{
	}

	public virtual void Hide()
	{
	}

	public virtual void Dispose()
	{
	}

	public virtual void Initialize()
	{
	}
}
using UnityEngine;

public class BaseUIView : MonoBehaviour
{
	public int CanvasSortOrder;

	public virtual void Show(bool isShow = true)
	{
		gameObject.CheckSetActive(isShow);
	}

	public virtual void Hide()
	{
		Show(false);
	}
}
using UnityEngine;

public static class GameObjectExtensions
{
	public static void CheckSetActive(this GameObject gameObject, bool active)
	{
		if (gameObject.activeSelf != active)
			gameObject.SetActive(active);
	}

	public static T GetOrAddComponent<T>(this GameObject obj)
		where T : Behaviour
	{
		var component = obj.GetComponent<T>();

		if (!component)
			component = obj.AddComponent<T>();

		return component;
	}
}
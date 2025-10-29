using UnityEngine;

//disable warning CS0649:Field is never assigned to, and will always have its default value 
#pragma warning disable CS0649

[RequireComponent(typeof(Canvas))]
public class ViewCanvas : MonoBehaviour
{
	[SerializeField] 
	private RectTransform _root;

	public RectTransform Root => _root;
	public int SortingOrder => GetComponent<Canvas>().sortingOrder;
}
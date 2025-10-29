using System.Collections.Generic;
using UnityEngine;

public class ViewCellRoot : BaseUIView
{
    [SerializeField]
    private List<CellBackgroundData> _imagesCellBackground;
    [SerializeField]
    private List<CellTypeData> _imagesCellType;

    public List<CellBackgroundData> ImagesCellBackground => _imagesCellBackground;
    public List<CellTypeData> ImagesCellType => _imagesCellType;
}

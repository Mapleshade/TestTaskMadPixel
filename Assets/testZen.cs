using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class testZen : MonoBehaviour
{
    private MainMenuPresenter _mainMenuPresenter;

    [Inject]
    private void Init(MainMenuPresenter mainMenuPresenter)
    {
        _mainMenuPresenter = mainMenuPresenter;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

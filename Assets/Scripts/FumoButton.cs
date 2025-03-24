using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FumoButton : MonoBehaviour
{
    public GameObject fum;
    public Button but;
    // Start is called before the first frame update
    void Start()
    {
        fum.SetActive(false);
        but.onClick.AddListener(() => { fum.SetActive(true); Invoke("Fumhide", 3f); });
    }
    void Fumhide()
    {
        fum.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

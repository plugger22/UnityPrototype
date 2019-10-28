using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// test method to scroll background vertically
/// </summary>
public class ScrollingBackground : MonoBehaviour
{

    public float scrollSpeed;
    //public Renderer renderer;
    private Material material;

    public void OnEnable()
    {
        //material derived from objects renderer
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update ()
    {
        material.mainTextureOffset += new Vector2(0f, scrollSpeed * Time.deltaTime);
	}
}

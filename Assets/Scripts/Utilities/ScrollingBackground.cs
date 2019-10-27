using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// test method to scroll background vertically
/// </summary>
public class ScrollingBackground : MonoBehaviour
{

    public float scrollSpeed;
    public Renderer renderer;
	
	// Update is called once per frame
	void Update ()
    {
        renderer.material.mainTextureOffset += new Vector2(0f, scrollSpeed * Time.deltaTime);
	}
}

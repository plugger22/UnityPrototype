﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for type of Organisation
/// </summary>
[CreateAssetMenu(menuName = "Game / Organisation / OrgType")]
public class OrgType : ScriptableObject
{

    public string tag;
    [TextArea(1,2)] public string services;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(services) == false, "Invalid services (Null or Empty) for {0}", name);       
    }
}

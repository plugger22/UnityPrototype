using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum SO class for contact type, eg. Street, Corporate, Utility, Research, etc.
/// </summary>
[CreateAssetMenu(menuName = "Contact / ContactType")]
public class ContactType : ScriptableObject
{

    public TextList pickList;


    public void OnEnable()
    {
        Debug.Assert(pickList != null, string.Format("Invalid pickList (Null) for ContactType \"{0}\"", this.name));
        Debug.Assert(pickList.category.name.Equals("Contacts", System.StringComparison.Ordinal) == true, "Invalid pickList TextList (wrong Category)");
    }

}

using gameAPI;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Resistance Contact, non-Unity data class
/// </summary>
public class Contact
{
    public int contactID;
    public string contactName;
    public string job;
    public string dataText0;              //multipurpose, could be level of job
    public string dataText1;              //multipurpose, could be name of corporation
    public int actorID;
    public int nodeID;
    public int turnStart;               //turn started as a contact
    public int turnFinish;              //turn finished as a contact
    public ContactStatus status;
    public bool isTurned;               //working for Authority as an informant
    

}

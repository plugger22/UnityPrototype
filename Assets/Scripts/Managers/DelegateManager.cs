using modalAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace delegateAPI
{
    //used to for Actor 'Manage' actions in ActionManager.cs -> ProcessHandleActor
    public delegate void manageDelegate(ModalActionDetails d);

}

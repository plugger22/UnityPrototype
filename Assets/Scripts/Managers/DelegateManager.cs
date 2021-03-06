﻿using gameAPI;
using modalAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace delegateAPI
{
    //used to for Actor 'Manage' actions in ActionManager.cs -> ProcessHandleActor
    public delegate void manageDelegate(ModalActionDetails d);

    //used for ModalInventoryUI.cs to pass the method needed to refresh the option line-up once an Action has been taken
    public delegate InventoryInputData InventoryDelegate();

    //used for ActionMenu's to pass a method to be run once that button is pressed, if null ignore
    public delegate void ActionButtonDelegate();

    //used for GameManager Initialisation
    public delegate void InitialisationDelegate(GameState state);

}

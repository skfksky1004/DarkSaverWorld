using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControlType
{
    Idle,
    Move,
    Attack,
    Magic,
}

public class SgtControlManager : MonoSingleton<SgtControlManager>
{
    protected override void OnExit()
    {
    }

    protected override void OnInit()
    {
    }
}
 
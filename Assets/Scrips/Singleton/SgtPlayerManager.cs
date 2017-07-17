using UnityEngine;
using System.Collections;

public class SgtPlayerManager : MonoSingleton<SgtPlayerManager> {

    private planeClick m_player;

    public override void Init()
    {
        base.Init();

        if(m_player == null)
        {
            m_player = FindObjectOfType<planeClick>();
        }
    }
}

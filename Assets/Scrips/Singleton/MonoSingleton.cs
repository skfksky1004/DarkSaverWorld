using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T m_Instance = null;

    public static T instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

                if(m_Instance == null)
                {
                    Debug.LogError("No Instance of" + typeof(T).ToString());
                }
                m_Instance.Init();
            }
            return m_Instance;
        }
    }

    private void Awake()
    {
        if(m_Instance == null)
        {
            m_Instance = this as T;
            m_Instance.Init();
        }
    }

    public virtual void Init() { }

    private void OnApplicationQuit()
    {
        m_Instance = null;
    }
}
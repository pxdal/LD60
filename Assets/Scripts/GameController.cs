using UnityEngine;

public class GameController : MonoBehaviour
{
    // singleton stuff //

    private static GameController m_instance = null;

    public static GameController Instance
    {
        get
        {
            if(!m_instance) throw new System.Exception("no GameController instance available");

            return m_instance;
        }
    }


    // private //

    private TapperBehaviour m_tapper;


    public TapperBehaviour Tapper
    {
        get
        {
            if(m_tapper == null) m_tapper = GetComponentInChildren<TapperBehaviour>();

            return m_tapper;
        }
    }

    // event listeners //

    void Awake()
    {
        if(m_instance != null) throw new System.Exception("GameController instance already exists!");

        m_instance = this;
    }
}

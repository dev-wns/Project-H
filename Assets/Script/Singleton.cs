using System.Collections;
using UnityEngine;

public abstract class SingletonMonoBehaviour<type> : MonoBehaviour where type : MonoBehaviour
{
    static type instance;

    public static type Instance
    {
        get
        {
            if ( null == instance )
            {
                instance = FindObjectOfType( typeof( type ) ) as type;
                if( null == instance )
                {
                    GameObject obj = new GameObject( typeof( type ).Name );
                    instance = obj.AddComponent<type>();
                }
            }
            return instance;
        }
    }
}

public abstract class SingletonDefault<type> where type : class, new()
{
    static type instance;
    public static type Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new type();
            }

            return instance;
        }
    }
}

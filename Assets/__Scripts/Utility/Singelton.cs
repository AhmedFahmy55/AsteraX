using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singelton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instannce {get;private set;} 




    private void Awake() 
    {
        if(Instannce != null && Instannce != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instannce = this as T ;
            DontDestroyOnLoad(gameObject);
        }
    }
}

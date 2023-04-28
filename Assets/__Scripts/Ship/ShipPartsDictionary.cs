using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPartsDictionary : MonoBehaviour
{
    
    public static Dictionary<ShipPart.eShipPartType, ShipPartsScriptableObject> DICT;

 
    public ShipPartsScriptableObject[] shipPartSOs;

    private void Awake()
    {
        
        DICT = new Dictionary<ShipPart.eShipPartType, ShipPartsScriptableObject>();
        foreach (ShipPartsScriptableObject spso in shipPartSOs)
        {
            DICT.Add(spso.type, spso);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCustomizationPanel : MonoBehaviour
{
    static bool inited = false;
    static Dictionary<ShipPart.eShipPartType, ShipPartToggle[]> TOGGLE_DICT;


    void Awake() {
        InitShipPartToggles();
	}


  
    void InitShipPartToggles() {
       
        TOGGLE_DICT = new Dictionary<ShipPart.eShipPartType, ShipPartToggle[]>();

       
        ShipPartToggle[] shipPartToggles = GetComponentsInChildren<ShipPartToggle>();
        int len = shipPartToggles.Length/2;

        TOGGLE_DICT.Add(ShipPart.eShipPartType.body, new ShipPartToggle[len]);
        TOGGLE_DICT.Add(ShipPart.eShipPartType.turret, new ShipPartToggle[len]);

   
        inited = true;

        foreach (ShipPartToggle spt in shipPartToggles)
        {
            if (TOGGLE_DICT.ContainsKey(spt.partType)) {
                if (TOGGLE_DICT[spt.partType].Length > spt.partNum) {
                    TOGGLE_DICT[spt.partType][spt.partNum] = spt;
                } else {
                    Debug.LogWarning("ShipCustomizationPanel.InitShipPartToggles() - "
                        + "A ShipPartType has a partNum that is greater than allowed: "
                        + "part: " + spt.partType + " #: " + spt.partNum + ".");
                    inited = false; 
                }
            } else {
                Debug.LogError("ShipCustomizationPanel.InitShipPartToggles() - "
                               +"A ShipPartToggle has an unexpected eShipPartType of "
                               + spt.partType + " on GameObject: " + spt.gameObject.name);
                inited = false; 
            }

        }
    }


    private void Start()
    {
        UnlockPart(ShipPart.eShipPartType.body, 0);
        UnlockPart(ShipPart.eShipPartType.turret, 0);
    }


    static public void LockPart(ShipPart.eShipPartType type, int num) {
        if (!inited)
        {
            Debug.LogWarning("ShipCustomizationPanel.LockPart("+type+", "+num+") - "
                             +"inited = false!");
            return;
        }

        if (TOGGLE_DICT.ContainsKey(type)) {
            if (TOGGLE_DICT[type].Length > num) {
                TOGGLE_DICT[type][num].LockPart();
            }
            else 
            {
                Debug.LogWarning("ShipCustomizationPanel.LockPart("+type+", "+num+") - "
                                 +"num is out of bounds.");
            }
        }
        else
        {
            Debug.LogWarning("ShipCustomizationPanel.LockPart("+type+", "+num+") - "
                                 +"Unexpected type.");
        }
    }


    static public void UnlockPart(ShipPart.eShipPartType type, int num) {
        if (!inited)
        {
            Debug.LogWarning("ShipCustomizationPanel.UnlockPart("+type+", "+num+") - "
                             +"inited = false!");
            return;
        }

        if (TOGGLE_DICT.ContainsKey(type)) {
            if (TOGGLE_DICT[type].Length > num) {
                TOGGLE_DICT[type][num].UnlockPart();
            }
            else 
            {
                Debug.LogWarning("ShipCustomizationPanel.UnlockPart("+type+", "+num+") - "
                                 +"num is out of bounds.");
            }
        }
        else
        {
            Debug.LogWarning("ShipCustomizationPanel.UnlockPart("+type+", "+num+") - "
                             +"Unexpected type.");
        }
    }


    static public int GetSelectedPart(ShipPart.eShipPartType type) {
        if (TOGGLE_DICT.ContainsKey(type)) {
            foreach (ShipPartToggle spt in TOGGLE_DICT[type]) {
                if (spt.isOn) {
                    return spt.partNum;
                }
            }

        } else {
            Debug.LogWarning("ShipCustomizationPanel.GetSelectedPart("+type+") - "
                             + " Unexpected type!");
        }

        // Returns 0 because that is the default if nothing is selected.
        return 0;
    }


    static public void SelectPart(ShipPart.eShipPartType type, int num) {
        if (TOGGLE_DICT.ContainsKey(type)) {
            if (TOGGLE_DICT[type].Length > num) {
                TOGGLE_DICT[type][num].isOn = true;
            } else {
                Debug.LogWarning("ShipCustomizationPanel.SelectPart("+type+", "+num+") - "
                                 +"num is out of bounds.");
            }
        } else {
            Debug.LogWarning("ShipCustomizationPanel.SelectPart("+type+", "+num+") - "
                             + " Unexpected type!");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(Toggle) )]
public class ShipPartToggle : MonoBehaviour {
    public ShipPart.eShipPartType partType;
    public int      partNum;

    [HideInInspector]
    public Toggle   toggle;


    Text            label;
    Image           backgroundImage;


	void Awake () {
        toggle = GetComponent<Toggle>();

 
        toggle.onValueChanged.AddListener(ValueChanged);

        label = GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = ShipPartsDictionary.DICT[partType].partInfos[partNum].name;
        }

        try {
            backgroundImage = transform.Find("Background").GetComponent<Image>();
        }
        catch
        {
            Debug.LogError("ShipPartToggle:Awake() – Could not find a child named " +
                           "Background that had an Image component.");
        }
	}

    void ValueChanged(bool tf)
    {
        if (toggle.isOn)
        {
            Debug.Log("Assigning ship "+partType+" to #:"+partNum);
            ShipCustomization.SetPart(partType, partNum);
        }
    }

    public bool isOn
    {
        get { return toggle.isOn; }
        set 
        {
            toggle.isOn = value;
        }
    }

    public void UnlockPart() {
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            backgroundImage.color = Color.white;
        }
    }


    public void LockPart() {
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
            backgroundImage.color = Color.red;
            toggle.isOn = false;
        }
    }

}

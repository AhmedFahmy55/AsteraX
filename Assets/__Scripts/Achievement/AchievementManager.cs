using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    static private AchievementManager _S; 

    [Header("Set in Inspector")]
    public AchievementPopUp popUp;
    public StepRecord[]     stepRecords;
    public Achievement[]    achievements;


   
    static private Dictionary<Achievement.eStepType, StepRecord> STEP_REC_DICT;

    private void Awake()
    {
        S = this;

      
        STEP_REC_DICT = new Dictionary<Achievement.eStepType, StepRecord>();
        foreach (StepRecord sRec in stepRecords)
        {
            STEP_REC_DICT.Add(sRec.type, sRec);
        }
    }


    void TriggerPopUp(string achievementName, string achievementDescription = "")
    {
        popUp.PopUp(achievementName, achievementDescription);
    }


    void UnlockPartsAfterLoadingGame(){

        foreach (Achievement ach in achievements) {
            if (ach.complete) {
                ShipCustomizationPanel.UnlockPart(ach.partType, ach.partNum);
            } else {
                ShipCustomizationPanel.LockPart(ach.partType, ach.partNum);
            }
        }

	}


 
    static private AchievementManager S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AchievementManager:S getter - Attempt to get "
                               + "value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AchievementManager:S setter - Attempt to set S "
                               + "when it has already been set.");
            }
            _S = value;
        }
    }


 
    static public void AchievementStep(Achievement.eStepType stepType, int num = 1)
    {
        StepRecord sRec = STEP_REC_DICT[stepType];
        if (sRec != null)
        {
            sRec.Progress(num);

           
            foreach (Achievement ach in S.achievements)
            {
                if (!ach.complete)
                {
                   
                    if (ach.CheckCompletion(stepType, sRec.num))
                    {
                       
                        AnnounceAchievementCompletion(ach);

                        
                        

                        SaveGameManager.Save();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("AchievementManager:AchievementStep( " + stepType + ", " + num + " )"
                             + "was passed a stepType that is not in STEP_REC_DICT.");
        }
    }


    static public void AnnounceAchievementCompletion(Achievement ach)
    {
        ShipCustomizationPanel.UnlockPart(ach.partType, ach.partNum);

        string desc = ach.description.Replace("#", ach.stepCount.ToString("N0"));
        S.TriggerPopUp(ach.name, desc);
    }


    static public StepRecord[] GetStepRecords()
    {
        return S.stepRecords;
    }


    static public Achievement[] GetAchievements()
    {
        return S.achievements;
    }

 
    internal static void ClearStepsAndAchievements()
    {
        
        foreach (StepRecord sRec in S.stepRecords) {
            sRec.num = 0;
        }

        foreach (Achievement ach in S.achievements) {
            ach.complete = false;
		}

       
        S.UnlockPartsAfterLoadingGame();
    }


    internal static void LoadDataFromSaveFile(SaveFile saveFile)
    {
        // Handle StepRecords
        foreach (StepRecord sRec in saveFile.stepRecords) {
            if (STEP_REC_DICT.ContainsKey(sRec.type)) {
                STEP_REC_DICT[sRec.type].num = sRec.num;
            }
        }

        // Handle Achievements
        foreach (Achievement achSF in saveFile.achievements) {
            
            foreach (Achievement achAM in S.achievements) {
                if (achSF.name == achAM.name) {
                    
                    achAM.complete = achSF.complete;
                }
            }
        }

       
        S.UnlockPartsAfterLoadingGame();
    }
}


[System.Serializable]
public class Achievement
{
   
    public enum eStepType
    {
        levelUp,
        bulletFired,
        hitAsteroid,
        luckyShot,
        scoreAttained,
    }

    public string       name;              
    [Tooltip("A # in the description will be replaced by the stepCount value.")]
    public string       description;       
    public eStepType    stepType;         
    public int          stepCount;         
    public ShipPart.eShipPartType   partType; 
    public int          partNum;           
    [SerializeField]
    private bool        _complete = false; 

    
    public bool complete
    {
        get { return _complete; }
        internal set { _complete = value; }
    }

   
    public bool CheckCompletion(eStepType type, int num)
    {
        if (type != stepType || complete)
        {
            return false;
        }

        
        if (num >= stepCount)
        {
            
            complete = true;
            return true;
        }
        return false;
    }

}


[System.Serializable]
public class StepRecord
{
    public Achievement.eStepType type;
    [Tooltip("Is this cumulative over time (like bullets fired) or based on an individual event (like reaching a certain level)?")]
    public bool     cumulative = false;
    [Tooltip("The current count of this step type. Only modify for testing purposes.")]
    [SerializeField]
    private int     _num = 0;

    public void Progress(int n)
    {
        if (cumulative)
        {
            _num += n;
        }
        else
        {
            _num = n;
        }
    }

    public int num
    {
        get { return _num; }
        internal set { _num = value; }
    }
}
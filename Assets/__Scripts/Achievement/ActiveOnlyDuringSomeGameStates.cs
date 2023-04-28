using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnlyDuringSomeGameStates : MonoBehaviour {
    
    public enum ePauseEffect
    {
        ignorePause,
        activeWhenPaused,
        activeWhenNotPaused
    }

    
    [EnumFlags] 
    public AsteraX.eGameState   activeStates = AsteraX.eGameState.all;
    public ePauseEffect         pauseEffect = ePauseEffect.ignorePause;
    [Tooltip("Check editorOnly to make this GameObject active only in the Unity editor.")]
    public bool                 editorOnly = false;

	
	public virtual void Awake () {

       
        DetermineActive();

        
        AsteraX.GAME_STATE_CHANGE_DELEGATE += DetermineActive;
        AsteraX.PAUSED_CHANGE_DELEGATE += DetermineActive;
    }

    protected void OnDestroy()
    {
        
        AsteraX.GAME_STATE_CHANGE_DELEGATE -= DetermineActive;
        AsteraX.PAUSED_CHANGE_DELEGATE -= DetermineActive;

    }


    protected virtual void DetermineActive()
    {
       
        bool shouldBeActive = (activeStates & AsteraX.GAME_STATE) == AsteraX.GAME_STATE;
        if (shouldBeActive)
        {
            
            switch (pauseEffect)
            {
                case ePauseEffect.activeWhenNotPaused:
                    shouldBeActive = !AsteraX.PAUSED;
                    break;
                case ePauseEffect.activeWhenPaused:
                    shouldBeActive = AsteraX.PAUSED;
                    break;
            }

            if (editorOnly && !Application.isEditor) {
                shouldBeActive = false;
            }
        }
        gameObject.SetActive(shouldBeActive);
    }
    
}

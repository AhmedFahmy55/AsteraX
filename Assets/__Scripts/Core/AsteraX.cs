//#define DEBUG_AsteraX_LogMethods
//#define DEBUG_AsteraX_RespawnNotifications

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AsteraX : MonoBehaviour
{
    static private AsteraX _S;
    static public List<LevelInfo>   LEVEL_LIST;
    static List<Asteroid>           ASTEROIDS;
    static List<Bullet>             BULLETS;
    static private bool             _PAUSED = false;
    static private eGameState       _GAME_STATE = eGameState.mainMenu;
    static public bool              GOT_HIGH_SCORE = false;
    
    static UnityEngine.UI.Text  	SCORE_GT;
    public static int           	SCORE { get; private set; }
    
    const float MIN_ASTEROID_DIST_FROM_PLAYER_SHIP = 5;
    const float DELAY_BEFORE_RELOADING_SCENE = 4;

	public delegate void CallbackDelegate(); 
    static public event CallbackDelegate GAME_STATE_CHANGE_DELEGATE;
    static public event CallbackDelegate PAUSED_CHANGE_DELEGATE;
    
	public delegate void CallbackDelegateV3(Vector3 v); 


    [System.Flags]
    public enum eGameState
    {
        // Decimal      // Binary
        none = 0,       // 00000000
        mainMenu = 1,   // 00000001
        preLevel = 2,   // 00000010
        level = 4,      // 00000100
        postLevel = 8,  // 00001000
        gameOver = 16,  // 00010000
        all = 0xFFFFFFF // 11111111111111111111111111111111
    }

    [Header("Set in Inspector")]
    [Tooltip("This sets the AsteroidsScriptableObject to be used throughout the game.")]
    public AsteroidsScriptableObject asteroidsSO;

    [Header("Set by Remote Settings")]
    public string levelProgression = "1:3/2,2:4/2,3:3/3,4:4/3,5:5/3,6:3/4,7:4/4,8:5/4,9:6/4,10:3/5";


    [Header("These reflect static fields and are otherwise unused")]
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
        + "GAME_STATE_CHANGE_DELEGATE whenever GAME_STATE changes.")]
    protected eGameState  _gameState;
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
    + "PAUSED_CHANGE_DELEGATE whenever PAUSED changes.")]
    protected bool        _paused;

    private void Awake()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:Awake()");
#endif

        S = this;
        
   
        GAME_STATE_CHANGE_DELEGATE += GameStateChanged;
        PAUSED_CHANGE_DELEGATE += PauseChanged;
        
       
        _gameState = eGameState.mainMenu;
        GAME_STATE = _gameState;
        _paused = true;
        PauseGame(_paused);
    }

    void GameStateChanged() {
        this._gameState = AsteraX.GAME_STATE;
    }

    void PauseChanged() {
        this._paused = AsteraX.PAUSED;
    }

    private void OnDestroy()
    {
        GAME_STATE_CHANGE_DELEGATE -= GameStateChanged;
        PAUSED_CHANGE_DELEGATE -= PauseChanged;
        AsteraX.GAME_STATE = AsteraX.eGameState.none;
    }

    void Start()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:Start()");
#endif
        
        RemoteSettings.Updated += RemoteSettingsUpdated;
		
		ParseLevelProgression();

        ASTEROIDS = new List<Asteroid>();
        AddScore(0);

        SaveGameManager.Load();
    }


    void StartLevel(int levelNum)
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:StartLevel("+levelNum+")");
#endif
        if (LEVEL_LIST.Count == 0)
        {
            Debug.LogError("AsteraX:StartLevel(" + levelNum + ") - LEVEL_LIST is empty!");
            return;
        }
        if (levelNum >= LEVEL_LIST.Count)
        {
            levelNum = 1; 
        }

        GAME_STATE = eGameState.preLevel;
        GAME_LEVEL = levelNum;
        LevelInfo info = LEVEL_LIST[levelNum - 1];

       
        ClearAsteroids();
        ClearBullets();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("DestroyWithLevelChange"))
        {
            Destroy(go);
        }

        
        asteroidsSO.numSmallerAsteroidsToSpawn = info.numSubAsteroids;
        
        
        for (int i = 0; i < info.numInitialAsteroids; i++)
        {
            SpawnParentAsteroid(i);
        }

        AchievementManager.AchievementStep(Achievement.eStepType.levelUp, levelNum);

    }

    void EndLevel()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:EndLevel()");
#endif
        if (GAME_STATE != eGameState.none)
        {
            PauseGame(true);
            GAME_LEVEL++;
            GAME_STATE = eGameState.postLevel;
            LevelAdvancePanel.AdvanceLevel(LevelAdvanceDisplayCallback, LevelAdvanceIdleCallback);
        }
    }

    void LevelAdvanceDisplayCallback()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:LevelAdvanceDisplayCallback()");
#endif
        StartLevel(GAME_LEVEL);
    }

    void LevelAdvanceIdleCallback()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:LevelAdvanceIdleCallback()");
#endif
        GAME_STATE = eGameState.level;

        PauseGame(false); 
    }

    void SpawnParentAsteroid(int i)
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:SpawnParentAsteroid("+i+")");
#endif

        Asteroid ast = Asteroid.SpawnAsteroid();
        ast.gameObject.name = "Asteroid_" + i.ToString("00");
        
        Vector3 pos;
        do
        {
            pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
        } while ((pos - PlayerShip.POSITION).magnitude < MIN_ASTEROID_DIST_FROM_PLAYER_SHIP);

        ast.transform.position = pos;
        ast.size = asteroidsSO.initialSize;
    }

    void ClearAsteroids()
    {
  
        Asteroid ast;
        for (int i = ASTEROIDS.Count - 1; i >= 0; i--)
        {
            ast = ASTEROIDS[i];
            ast.transform.SetParent(null); // De-parent the Asteroid
            Destroy(ast.gameObject);
        }

       
        ASTEROIDS.Clear();
    }

    void ClearBullets()
    {
        if (BULLETS == null)
        {
            return;
        }
      
        for (int i = BULLETS.Count - 1; i >= 0; i--)
        {
            Destroy(BULLETS[i].gameObject);
        }
    }


    void ParseLevelProgression()
    {
    
        LEVEL_LIST = new List<LevelInfo>();


        string[] levelStrings = levelProgression.Split(',');
        for (int i = 0; i < levelStrings.Length; i++)
        {
            string[] levelBits = levelStrings[i].Split(':');
            string levelName = "Level " + levelBits[0];
            string[] asteroidStrings = levelBits[1].Split('/');
            int numInitialAsteroids, numSubAsteroids;
            if (!int.TryParse(asteroidStrings[0], out numInitialAsteroids)
                || !int.TryParse(asteroidStrings[1], out numSubAsteroids))
            {
                Debug.LogError("AsteraX:ParseLevelProgression() - Attempt to parse bad asteroid numbers" +
                               "for " + levelName + ": " + levelStrings[i]);
                return; 
            }
            LevelInfo levelInfo = new LevelInfo(i + 1, levelName, numInitialAsteroids, numSubAsteroids);
            LEVEL_LIST.Add(levelInfo);
        }
        Debug.Log("AsteraX:ParseLevelProgression() - Parsed levelProgression:\n" + levelProgression);
    }

    void RemoteSettingsUpdated()
    {
        string newLevelProgression = RemoteSettings.GetString("levelProgression", "");
        if (newLevelProgression != "")
        {
            levelProgression = newLevelProgression;
            Debug.Log("AsteraX:RemoteSettingsUpdated() - Calling ParseLevelProgression() "
                  + "with levelProgression:\n" + levelProgression);
            ParseLevelProgression();
        }
        else
        {
            Debug.Log("AsteraX:RemoteSettingsUpdated() - Did not receive proper " +
                      "levelProgression from RemoteSettings.");
        }
    }

    public void PauseGameToggle()
    {
        PauseGame(!PAUSED);
    }

    public void PauseGame(bool toPaused)
    {
        PAUSED = toPaused;
        if (PAUSED)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }


    private void Update()
    {
        if (GAME_STATE == eGameState.level && ASTEROIDS.Count == 0)
        {
            
            if (_S != null)
            {
                _S.EndLevel();
            }
        }
    }
    
    
	public void EndGame()
    {
        GAME_STATE = eGameState.gameOver;
        Invoke("ReloadScene", DELAY_BEFORE_RELOADING_SCENE);
    }
    

    void ReloadScene()
    {
    
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }





    static public void AddAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) == -1)
        {
            ASTEROIDS.Add(asteroid);
        }
    }
    static public void RemoveAsteroid(Asteroid asteroid)
    {
        if (GAME_STATE != eGameState.level)
        {
         
            return;
        }
        if (ASTEROIDS.IndexOf(asteroid) != -1)
        {
            ASTEROIDS.Remove(asteroid);
        }
    }

    static public void AddBullet(Bullet bullet)
    {
        if (BULLETS == null)
        {
            BULLETS = new List<Bullet>();
        }
        if (BULLETS.IndexOf(bullet) == -1)
        {
            BULLETS.Add(bullet);
            
          
            AchievementManager.AchievementStep(Achievement.eStepType.bulletFired, 1);
        }
    }

    static public void RemoveBullet(Bullet bullet)
    {
        if (BULLETS == null)
        {
            return;
        }
        BULLETS.Remove(bullet);
    }




    static private AsteraX S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AsteraX:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AsteraX:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public AsteroidsScriptableObject AsteroidsSO
    {
        get
        {
            if (S != null)
            {
                return S.asteroidsSO;
            }
            return null;
        }
    }

    static public bool PAUSED
    {
        get
        {
            return _PAUSED;
        }
        private set
        {
            if (value != _PAUSED)
            {
                _PAUSED = value;
         
                if (PAUSED_CHANGE_DELEGATE != null)
                {
                    PAUSED_CHANGE_DELEGATE();
                }
            }

        }
    }

    static public eGameState GAME_STATE
    {
        get
        {
            return _GAME_STATE;
        }
        set
        {
            if (value != _GAME_STATE)
            {
                _GAME_STATE = value;
             
                if (GAME_STATE_CHANGE_DELEGATE != null)
                {
                    GAME_STATE_CHANGE_DELEGATE();
                }
            }
        }
    }

    static public int GAME_LEVEL
    {
        get; private set;
    }

    static public void StartGame()
    {
        GOT_HIGH_SCORE = false;
        GAME_LEVEL = 0;
        _S.EndLevel();
    }

    static public void GameOver()
    {
        SaveGameManager.CheckHighScore(SCORE);
        SaveGameManager.Save();
        _S.EndGame();
    }


	[System.Serializable]
    public struct LevelInfo
    {
        public int levelNum;
        public string levelName;
        public int numInitialAsteroids;
        public int numSubAsteroids;

        public LevelInfo(int lNum, string name, int initial, int sub)
        {
            levelNum = lNum;
            levelName = name;
            numInitialAsteroids = initial;
            numSubAsteroids = sub;
        }
    }


    static public LevelInfo GetLevelInfo(int lNum = -1)
    {
        if (lNum == -1)
        {
            lNum = GAME_LEVEL;
        }
        // lNum is 1-based where LEVEL_LIST is 0-based, so LEVEL_LIST[0] is lNum 1
        if (lNum < 1 || lNum > LEVEL_LIST.Count)
        {
            Debug.LogError("AsteraX:GetLevelInfo() - Requested level number of " + lNum + " does not exist.");
            return new LevelInfo(-1, "NULL", 1, 1);
        }
        return (LEVEL_LIST[lNum - 1]);
    }

    
	static public void AddScore(int num)
    {
        // Find the ScoreGT Text field only once.
        if (SCORE_GT == null)
        {
            GameObject go = GameObject.Find("ScoreGT");
            if (go != null)
            {
                SCORE_GT = go.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                Debug.LogError("AsteraX:AddScore() - Could not find a GameObject named ScoreGT.");
                return;
            }
            SCORE = 0;
        }
        // SCORE holds the definitive score for the game.
        SCORE += num;

        if ( !GOT_HIGH_SCORE && SaveGameManager.CheckHighScore(SCORE) ) {
            // We just got the high score
            GOT_HIGH_SCORE = true;
            // Announce it using the AchievementPopUp
            AchievementPopUp.ShowPopUp("High Score!","You've achieved a new high score.");
        }

        // Show the score on screen. For info on numeric formatting like "N0", see:
        //  https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        SCORE_GT.text = SCORE.ToString("N0");
        
		AchievementManager.AchievementStep(Achievement.eStepType.scoreAttained, SCORE);
    }


    const int RESPAWN_DIVISIONS = 8;
    const int RESPAWN_AVOID_EDGES = 2; // Note: This number must be greater than 0!
    static Vector3[,] RESPAWN_POINTS;

    static public IEnumerator FindRespawnPointCoroutine(Vector3 prevPos, CallbackDelegateV3 callback)
    {
# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine( "+prevPos+", [CallbackDelegateV3] )");
#endif
        // Spawn particle effect for disappearing
        Instantiate(PlayerShip.DISAPPEAR_PARTICLES, prevPos, Quaternion.identity);

        // Set up the RESPAWN_POINTS once
        if (RESPAWN_POINTS == null)
        {
            RESPAWN_POINTS = new Vector3[RESPAWN_DIVISIONS + 1, RESPAWN_DIVISIONS + 1];
            Bounds playAreaBounds = ScreenBounds.BOUNDS;
            float dX = playAreaBounds.size.x / RESPAWN_DIVISIONS;
            float dY = playAreaBounds.size.y / RESPAWN_DIVISIONS;
            for (int i = 0; i <= RESPAWN_DIVISIONS; i++)
            {
                for (int j = 0; j <= RESPAWN_DIVISIONS; j++)
                {
                    RESPAWN_POINTS[i, j] = new Vector3(
                        playAreaBounds.min.x + i * dX,
                        playAreaBounds.min.y + j * dY,
                        0);
                }
            }
        }

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() yielding for "+PlayerShip.RESPAWN_DELAY+" seconds.");
#endif

        // Wait a few seconds before choosing the nextPos
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.8f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() back from yield.");
#endif

        float distSqr, closestDistSqr = float.MaxValue;
        int prevI = 0, prevJ = 0;

        // Check points against prevPos (avoiding edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                // sqrMagnitude avoids doing a needless (and costly) square root operation
                distSqr = (RESPAWN_POINTS[i, j] - prevPos).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    prevI = i;
                    prevJ = j;
                }
            }
        }

        float furthestDistSqr = 0;
        Vector3 nextPos = prevPos;
        // Now, iterate through each of the RESPAWN_POINTS to find the one with 
        //  the largest distance to the closest Asteroid (again avoid edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                if (i == prevI && j == prevJ)
                {
                    continue;
                }
                closestDistSqr = float.MaxValue;
                // Find distance to the closest Asteroid
                for (int k = 0; k < ASTEROIDS.Count; k++)
                {
                    distSqr = (ASTEROIDS[k].transform.position - RESPAWN_POINTS[i, j]).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                    }
                }

                // If this is further than before, this is the best spawn loc
                if (closestDistSqr > furthestDistSqr)
                {
                    furthestDistSqr = closestDistSqr;
                    nextPos = RESPAWN_POINTS[i, j];
                }
            }
        }

        // Spawn particle effect for appearing
        Instantiate(PlayerShip.APPEAR_PARTICLES, nextPos, Quaternion.identity);

        // Give the particle effect just a bit of time before the ship respawns
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.2f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() calling back!");
#endif
        callback(nextPos);
    }

}

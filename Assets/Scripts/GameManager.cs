using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Game Manager handles time and basic game attributes.
public class GameManager : Singleton<GameManager>
{
    /// The total number of lives. --No need to use at first. Later implement a usable item. TODO
    //public int TotalLives = 1;
    /// The current number of lives. --No need to use at first.
    //public int CurrentLives {get; protected set;}
    /// Is the game paused or not?
    public bool Paused {get; set;}
    /// The current game points in number(Coins + passive increase).
    public float Points;
    public float TotalPoints;
    /// The current number of catnips(Special currency).
    public float CatNips;
    public float TotalCatnips;
    /// The stored player character.
    public Player storedCharacter{get; set;}
    /// The current timescale.
    public float TimeScale = 1f;
    /// A number of game statuses. Valuable infos to use later.
    public enum GameStatus {BeforeGameStart, GameInProgress, Paused, UnPaused, GameOver} // Add respawn + playerDeath. TODO
    /// Bring that status to me!
    public GameStatus Status {get; protected set;}

    // Basic storage function
    protected float _savedTimeScale;
    protected GameStatus _statusPrePause;


    /// Initialization
    protected virtual void Start()
    {
        //CurrentLives = TotalLives;
        _savedTimeScale = TimeScale;
        Time.timeScale = TimeScale;
        if (GUIManager.Instance != null)
        {
            GUIManager.Instance.Initialize();
        }
    }


    /// Defines/Sets the status. Can be used by other classes, for different ends.

    public virtual void SetStatus (GameStatus newStatus)
    {
        Status = newStatus;
    }

    /// Resets the Game Manager.

    public virtual void Reset()
    {
        Points = 0;
        TimeScale = 1f;
        GameManager.Instance.SetStatus(GameStatus.GameInProgress);
        EventManager.TriggerEvent(new GameEvent("GameStart"));
    }

    /// Adds points to the current game points.
    public virtual void AddPoints (float pointsToAdd)
    {
        Points += pointsToAdd;
        if (GUIManager.Instance != null)
        {
            GUIManager.Instance.RefreshPoints();
        }
    }

    /// Sets the current points.
    public virtual void SetPoints (float points)
    {
        Points = points;
        if (GUIManager.Instance != null)
        {
            GUIManager.Instance.RefreshPoints();
        }
    }

    public virtual void AddCatnips (float points)
    {
        CatNips += points;
        if (GUIManager.Instance != null)
        {
            GUIManager.Instance.RefreshCatnips();
        }
    }

    public virtual void SetCatnips (float pointsSet)
    {
        CatNips = pointsSet;
        if (GUIManager.Instance != null)
        {
            GUIManager.Instance.RefreshCatnips();
        }
    }

    /*public virtual void SetLives (int lives)
    {
        CurrentLives = lives;
    }

    public virtual void LoseLivePt (int lives)
    {
        CurrentLives -= lives;
    }*/

    /// Sets-Resets time scale.
    public virtual void SetTimeScale (float newTimeScale)
    {
        _savedTimeScale = Time.timeScale;
        Time.timeScale = newTimeScale;
    }

    public virtual void ResetTimeScale ()
    {
        Time.timeScale = _savedTimeScale;
    }

    /// Pauses the game.
    public virtual void PauseOn () 
    {
        // Check if it is already paused.
        if (Time.timeScale > 0.0f)
        {
            Instance.SetTimeScale(0.0f);
            _statusPrePause = Instance.Status;
            Instance.Paused = true;
            Instance.SetStatus(GameStatus.Paused);

            EventManager.TriggerEvent(GameStatus.Paused);
        }
        else
        {
            PauseOff();
        }
    }

    /// Unpauses the game.
    public virtual void PauseOff ()
    {
        Instance.ResetTimeScale();
        Instance.Paused = false;
        Instance.SetStatus(_statusPrePause);

        EventManager.TriggerEvent(GameStatus.UnPaused);
    }

    protected virtual void OnDestroy ()
    {
        if (_instance != null)
        {
            _instance = null;
        }
    }
}

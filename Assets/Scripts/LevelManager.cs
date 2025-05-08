using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>, IEventListener<LevelEvent>
{
  public enum Controls {LeftRight, MidAir, Joystick }
  /// Current level speed.
  public float LevelSpeed  {get; protected set;}
  /// Distance traveled since the start.
  public float DistanceTraveled {get; protected set;}

  public GameObject StartingPos;
  /// Player character. A placeholder for now.
  public Player Player;

  /// Time elapsed since the start.
  public float ElapsedTime {get; protected set;}
  /// Amount of points a player get per second.
  public float PointsPerSec = 5;
  /// Level-Start text.
  public string HeadsUpTxt;

  /// Hill generation.
  public Bounds RecycleBounds;

  /// Can player fly?
  public bool FlightActive;
  
  public float IntroFadeDuration = 1f;

  public float OuterSpaceFade = 1f;
  public float InnerSpaceFade = 1f;
  public float OutroFadeDuration = 1f;

  /// Initial countdown duration.
  public int StartCountdown;
  /// The text, player sees at the end of countdown.
  public string StartText;

  public Controls ControlScheme;

  /// Life-Lost effect.
  public GameObject LifeLostExplosion;

  // Data holders.
  protected DateTime _started;
  protected float _savedPoints;
  // Placeholder, these two down here \/
  protected float _recycleXVal;
  protected Bounds _tmpRecycleBounds;

  protected bool _tempSpeedFactorActive;
  protected float _tempSpeedFactor;
  protected float _tempSpeedFactorRemainingTime;
  protected float _tempSavedSpeed;

  /// Initialization.
  protected virtual void Start ()
  {
      DistanceTraveled = 0;
      LevelSpeed = 0f;

      Player = Player.PlayerIns;

      // Storage
      _savedPoints = GameManager.Instance.Points;
      _started = DateTime.UtcNow;
      GameManager.Instance.SetStatus(GameManager.GameStatus.BeforeGameStart);

      if (GUIManager.Instance != null)
      {
          GUIManager.Instance.SetLevel(SceneManager.GetActiveScene().name);
          //GUIManager.Instance.FaderOn(false, IntroFadeDuration);
      }

      PrepareStart();
  }

  /// Handles everything before the game start.
  protected virtual void PrepareStart ()
  {
      if (StartCountdown > 0)
      {
          GameManager.Instance.SetStatus(GameManager.GameStatus.BeforeGameStart);
          StartCoroutine(PrepareStartCountdown());
      }
      else
      {
          LevelStart();
      }
  }

  /// Handles the countdown display.
  protected virtual IEnumerator PrepareStartCountdown()
  {
      int countdown = StartCountdown;
      GUIManager.Instance.SetCountdownActive(true);

      // Display the active countdown.
      while (countdown > 0)
      {
          if (GUIManager.Instance.CountdownTxt != null)
          {
              GUIManager.Instance.SetCountdownTxt(countdown.ToString());
          }
          countdown--;
          yield return new WaitForSeconds(1f);
      }

      // Display the start message.
      if ((countdown == 0) && (StartText != ""))
      {
          GUIManager.Instance.SetCountdownTxt(StartText);
          yield return new WaitForSeconds(1f);
      }

      // Make the countdown inactive and start the level.
      GUIManager.Instance.SetCountdownActive(false);
      LevelStart();
  }

  protected virtual void LevelStart ()
{
    GameManager.Instance.SetStatus(GameManager.GameStatus.GameInProgress);
    EventManager.TriggerEvent(new GameEvent("GameStart"));
}

public virtual void ResetLevel ()
{
    PrepareStart();
}

public virtual void Update ()
{
    _savedPoints = GameManager.Instance.Points;
    _started = DateTime.UtcNow;

    DistanceTraveled = DistanceTraveled + LevelSpeed * Time.fixedDeltaTime;

    ElapsedTime += Time.deltaTime;
    
    
}

public virtual void FixedUpdate ()
{
    LevelSpeed = Player.CurrentSpeed;
}

public virtual void OnEvent (LevelEvent eventType)
{

}

public virtual void OnEnable ()
{
    this.StartListeningEvent<LevelEvent>();
}

public virtual void OnDisable ()
{
    this.StopListeningEvent<LevelEvent>();
}


}



/******************************************************************************
*  @file       RailroadLane.cs
*  @brief      
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion // Namespaces

public class RailroadLane : Lane
{
    #region Public Interface

    /// <summary>
    /// Activate this lane
    /// </summary>
    /// <param name="rowNumber">Indacates how far into the map the lane is</param>
    /// <param name="prevPassableTileArray">Passable tile array of the previous lane</param>
    /// <param name="prevDir">Direction of previous lane</param>
    public override void Activate(int rowNumber, bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        base.Activate(rowNumber, prevPassableTileArray, prevDir);
        DeactivateWarningLights();
        CreateTrain();
        SpawnCoin();
    }

    /// <summary>
    /// Deactivate this lane
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        DeactivateWarningLights();
    }

    /// <summary>
    /// Determine wheteher the tile is open and the player can move to it.
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    public override bool IsTileOpen(int tileCoord)
    {
        return base.IsTileOpen(tileCoord);
    }

    /// <summary>
    /// Pause this instance
    /// </summary>
    public override void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;

        if (m_train != null)
        {
            m_train.Pause();
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Pause();
        }
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public override void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;

        if (m_train != null)
        {
            m_train.Unpause();
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Unpause();
        }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     float           m_minTrainInterval  = 3f;
    [SerializeField]private     float           m_maxTrainInterval  = 8f;
    [SerializeField]private     float           m_warningDuration   = 2f;
    [SerializeField]private     Transform       m_lightsBox         = null;
    [SerializeField]private     Light           m_lightTracks       = null;
    [SerializeField]private     Light           m_light1            = null;
    [SerializeField]private     Light           m_light2            = null;
    [SerializeField]private     float           m_alternateLightDur = 0.3f;
    [SerializeField]private     Vector3         m_lightsBoxOffPos   = Vector3.zero;
    [SerializeField]private     Vector3         m_lightsBoxOnPos    = Vector3.zero;

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (!m_isActivated)
        {
            return;
        }

        if (m_isPaused)
        {
            return;
        }

        UpdateTrainTimer();
    }

    #endregion // MonoBehaviour

    #region State

    private enum State
    {
        WAITING,
        WARNING,
        MOVING
    }
    private         State               m_state             = State.WAITING;

    #endregion // State

    #region Items and Obstacles

    private         Vehicle             m_train             = null;
    private         float               m_trainTimer        = 0f;
    private         float               m_trainInterval     = 0f;
    private         float               m_trainSpeed        = 0f;
    private         float               m_moveDuration      = 0f;
    private         Vector3             m_startPos          = Vector3.zero;
    private         Vector3             m_endPos            = Vector3.zero;
    private         float               m_lightTimer        = 0f;
    private         SoundObject         m_warningSound      = null;

    /// <summary>
    /// Initializes items and obstacles
    /// </summary>
    protected override void InitializeItems()
    {

    }

    /// <summary>
    /// Returns all items to the pool
    /// </summary>
    protected override void ReturnItems()
    {
        // Return the train instance
        if (m_train != null)
        {
            m_train.Deactivate();
            m_train = null;
        }

        // Return the coin instance
        if (m_coinInstance != null)
        {
            m_coinInstance.Deactivate();
            m_coinInstance = null;
        }
    }

    /// <summary>
    /// Creates vehicles
    /// </summary>
    private void CreateTrain()
    {
        // Get a train instance from the pool
        m_train = m_mapAssetPool.GetVehicleAssetPool(VehicleType.Train).GetAsset();

        // Randomly determine lane direction
        m_direction = (Random.Range(0, 2) == 1) ? LaneDirection.RIGHT : LaneDirection.LEFT;

        // Randomly determine time and interval
        m_trainInterval = Random.Range(m_minTrainInterval, m_maxTrainInterval);
        m_trainTimer = Random.Range(0f, m_trainInterval);

        // Determine train position
        float moveDistance = (m_activeTileCount + (m_edgeTileCount * 2f)) * m_tileSize;
        moveDistance += m_train.VehicleLength;
        m_startPos = new Vector3(moveDistance * -0.5f, m_height, 0f) + transform.position;
        m_endPos = new Vector3(moveDistance * 0.5f, m_height, 0f) + transform.position;
        if (m_direction == LaneDirection.LEFT)
        {
            Vector3 temp = m_startPos;
            m_startPos = m_endPos;
            m_endPos = temp;
        }

        // Set the train speed and movement duration
        m_trainSpeed = m_train.GetRandomSpeed();
        m_moveDuration = moveDistance / m_trainSpeed;
    }

    /// <summary>
    /// Updates the train timer
    /// </summary>
    private void UpdateTrainTimer()
    {
        m_trainTimer += Time.deltaTime;

        switch(m_state)
        {
            case State.WAITING:
                if (m_trainTimer >= m_trainInterval)
                {
                    // TODO: Activate warning lights
                    ActivateWarningLights();
                    m_trainTimer = 0f;
                    m_state = State.WARNING;
                }
                break;

            case State.WARNING:
                UpdateWarningLights();
                if (m_trainTimer >= m_warningDuration)
                {
                    DeactivateWarningLights();
                    MoveTrain();
                    m_trainTimer = 0f;
                    m_state = State.MOVING;
                }
                break;

            case State.MOVING:
                if (m_trainTimer >= m_moveDuration)
                {
                    m_trainTimer = 0f;
                    m_state = State.WAITING;
                }
                break;
        }
    }

    /// <summary>
    /// Moves the train
    /// </summary>
    private void MoveTrain()
    {
        if (m_train != null)
        {
            m_train.gameObject.SetActive(true);
            m_train.SetMapManagerInstance(m_mapManager);
            m_train.StartMovement(m_trainSpeed, m_startPos, m_endPos, 0f, false);
        }
    }

    /// <summary>
    /// Activates the warning lights
    /// </summary>
    private void ActivateWarningLights()
    {
        m_lightsBox.localPosition = m_lightsBoxOnPos;
        m_lightTracks.gameObject.SetActive(true);
        m_light1.gameObject.SetActive(true);
        m_light2.gameObject.SetActive(false);
        m_lightTimer = 0f;

        if (m_warningSound == null)
        {
            m_warningSound = Locator.GetSoundManager().PlaySound(SoundInfo.SFXID.TrainBell);
            m_warningSound.transform.parent = transform;
            m_warningSound.transform.localPosition = Vector3.up * m_height;
        }
    }

    /// <summary>
    /// Updates the warning lights
    /// </summary>
    private void UpdateWarningLights()
    {
        m_lightTimer += Time.deltaTime;
        if (m_lightTimer >= m_alternateLightDur)
        {
            m_light1.gameObject.SetActive(!m_light1.gameObject.activeSelf);
            m_light2.gameObject.SetActive(!m_light2.gameObject.activeSelf);
            m_lightTimer = 0f;
        }
    }

    /// <summary>
    /// Deactivates the warning lights
    /// </summary>
    private void DeactivateWarningLights()
    {
        m_lightsBox.localPosition = m_lightsBoxOffPos;
        m_lightTracks.gameObject.SetActive(false);
        m_light1.gameObject.SetActive(false);
        m_light2.gameObject.SetActive(false);

        if (m_warningSound != null)
        {
            m_warningSound.Delete();
            m_warningSound = null;
        }
    }

    #endregion // Items

    #region Coins

    /// <summary>
    /// Spawns a coin
    /// </summary>
    protected override void SpawnCoin()
    {
        // Determine whether to spawn a coin
        if (Random.value <= m_coinSpawnProb)
        {
            // Get a random spawnable tile
            int tileIndex = Random.Range(0, m_activeTileCount);

            // Get a coin instance
            m_coinInstance = m_mapAssetPool.GetCoinAssetPool().GetAsset();

            // Position the coin
            float leftmostXPos = Mathf.FloorToInt(m_activeTileCount * 0.5f) * -m_tileSize;
            Vector3 leftmostPosition = new Vector3(leftmostXPos, m_height, 0f) + transform.position;
            m_coinInstance.transform.position = leftmostPosition + (Vector3.right * tileIndex * m_tileSize);

            // Activate the coin
            m_coinInstance.gameObject.SetActive(true);
            m_coinInstance.SetOnCoinGetDelegate(RemoveCoinInstance);
            m_coinInstance.Activate();
        }
    }

    #endregion // Coins
}
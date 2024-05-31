using System;
using UnityEngine;

public class LobbyID
{
    private const int REFRESH_INTERVAL_SECOND = 5;
    public static LobbyID Instance { get; private set; } = new LobbyID();

    public enum ValidationStep
    {
        /// <summary>
        /// The ID has not been set.
        /// </summary>
        None,

        /// <summary>
        /// The ID is pending validation.
        /// </summary>
        Pending,

        /// <summary>
        /// The ID is not validated.
        /// </summary>
        Invalid,

        /// <summary>
        /// The ID is validated, but the lobby is full.
        /// </summary>
        Full,

        /// <summary>
        /// The ID is validated.
        /// </summary>
        Valid
    }

    private int id;
    private ValidationStep validationStep;
    private float? nextRefreshTime;
    public event Action<ValidationStep> OnValidationChange;
    public ValidationStep Validation
    {
        get => validationStep;
        private set
        {
            validationStep = value;
            OnValidationChange?.Invoke(validationStep);
        }
    }
    public bool Validated
    {
        get => validationStep == ValidationStep.Valid;
        set
        {
            if (value)
            {
                Validation = ValidationStep.Valid;
                nextRefreshTime = Time.time + REFRESH_INTERVAL_SECOND;
            }
            else
            {
                Validation = ValidationStep.Invalid;
                nextRefreshTime = null;
            }
        }
    }
    public int ID
    {
        get => id;
        set
        {
            id = value;
            RefreshValidation();
        }
    }

    public void ValidateID(int validLobbyID, ValidationStep validationStep)
    {
        if (validLobbyID != ID) return;
        Validation = validationStep;
    }

    public void Reset()
    {
        Validation = ValidationStep.None;
        nextRefreshTime = null;
        id = -1;
    }

    public void DoPeriodicValidation()
    {
        if (Validated && nextRefreshTime != null && Time.time > nextRefreshTime)
        {
            RefreshValidation();
        }
    }

    private void RefreshValidation()
    {
        Validation = ValidationStep.Pending;
        LobbiesManager.RequestLobbyValidation(id);
    }
}
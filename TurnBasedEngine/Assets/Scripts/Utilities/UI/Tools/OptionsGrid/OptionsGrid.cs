﻿using System.Collections.Generic;
using UnityEngine;
using System;
using BF2D.Enums;
using BF2D.Attributes;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BF2D.UI
{
    public class OptionsGrid : UIUtility
    {
        [Header("Grid")]
        [SerializeField] private GridOption optionPrefab = null;
        [Tooltip("The container for grid options")]
        [SerializeField] protected Transform container = null;
        [Tooltip("Determines the direction that elements will be populated in")]
        [SerializeField] private Axis instantiationAxis = Axis.Horizontal;
        [SerializeField] protected int gridWidth = 1;
        [SerializeField] protected int gridHeight = 1;

        public OnNavigate OnNavigateEvent
        {
            get
            {
                return this.onNavigateEvent;
            }
        }

        [Serializable]
        public class OnNavigate : UnityEvent<int> { }
        [SerializeField]
        private OnNavigate onNavigateEvent = new();


        [SerializeField] private AudioSource navigateAudioSource = null;
        [Tooltip("Enable/disable use of the confirm button with the grid")]
        [SerializeField] private bool confirmEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(confirmEnabled))]
        [SerializeField] private AudioSource confirmAudioSource = null;
        [Tooltip("Enable/disable use of the back button with the grid")]
        [SerializeField] private bool backEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(backEnabled))]
        [SerializeField] private AudioSource backAudioSource = null;
        [Tooltip("Enable/disable use of the menu button with the grid")]
        [SerializeField] private bool menuEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(menuEnabled))]
        [SerializeField] private AudioSource menuAudioSource = null;
        [Tooltip("Enable/disable use of the attack button with the grid")]
        [SerializeField] private bool attackEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(attackEnabled))]
        [SerializeField] private AudioSource attackAudioSource = null;
        [Tooltip("Enable/disable use of the pause button with the grid")]
        [SerializeField] private bool pauseEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(pauseEnabled))]
        [SerializeField] private AudioSource pauseAudioSource = null;
        [Tooltip("Enable/disable use of the select button with the grid")]
        [SerializeField] private bool selectEnabled = false;
        [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.AND, nameof(selectEnabled))]
        [SerializeField] private AudioSource selectAudioSource = null;

        /// <summary>
        /// The area of the grid (width * height)
        /// </summary>
        public int Size { get { return this.gridWidth * this.gridHeight; } }

        /// <summary>
        /// Width of the grid
        /// </summary>
        public int Width { get { return this.gridWidth; } }

        /// <summary>
        /// Height of the grid
        /// </summary>
        public int Height { get { return this.gridHeight; } }

        /// <summary>
        /// The number of options in the grid
        /// </summary>
        public int Count { get { return this.count; } }

        /// <summary>
        /// Enable/disable use of the confirm button
        /// </summary>
        public bool ConfirmEnabled { get { return this.confirmEnabled; } set { this.confirmEnabled = value; } }

        /// <summary>
        /// Enable/disable use of the menu button
        /// </summary>
        public bool BackEnabled { get { return this.backEnabled; } set { this.backEnabled = value; } }

        /// <summary>
        /// Enable/disable use of the menu button
        /// </summary>
        public bool MenuEnabled { get { return this.menuEnabled; } set { this.menuEnabled = value; } }
        
        /// <summary>
        /// Enable/disable use of the menu button
        /// </summary>
        public bool AttackEnabled { get { return this.attackEnabled; } set { this.attackEnabled = value; } }

        /// <summary>
        /// Enable/disable use of the menu button
        /// </summary>
        public bool PauseEnabled { get { return this.pauseEnabled; } set { this.pauseEnabled = value; } }

        /// <summary>
        /// Enable/disable use of the menu button
        /// </summary>
        public bool SelectEnabled { get { return this.selectEnabled; } set { this.selectEnabled = value; } }

        private GridOption CurrentOption { get { return this.grid[this.cursorPosition.x, this.cursorPosition.y]; } set { this.grid[this.cursorPosition.x, this.cursorPosition.y] = value; } }

        protected GridOption[,] grid = null;
        protected int count = 0;
        public Vector2Int CursorPosition { get { return this.cursorPosition; } }
        public int CursorPosition1D
        {
            get
            {
                switch (this.instantiationAxis)
                {
                    case Axis.Horizontal:
                        return (this.gridWidth * this.cursorPosition.y) + this.cursorPosition.x;
                    case Axis.Vertical:
                        return (this.gridHeight * this.cursorPosition.x) + this.cursorPosition.y;
                    default:
                        throw new Exception($"[OptionsGrid:CursorPosition1D] The instantiation axis is set to an invalid value: {this.instantiationAxis}");
                }
            }
        }
        private Vector2Int cursorPosition = new(0, 0);
        protected Vector2Int head = new(0, 0);

        #region Public Methods
        /// <summary>
        /// Sets up a new grid, clearing any previous data
        /// </summary>
        /// <param iconID="width">The new grid width</param>
        /// <param iconID="height">The new grid height</param>
        public void Setup(int width, int height)
        {
            //Clean up anything that could be left over
            Clear();

            //Set the width and height
            this.gridWidth = width;
            this.gridHeight = height;

            //Create the element data structure
            this.grid = new GridOption[this.gridWidth, this.gridHeight];
        }

        /// <summary>
        /// Instantiates and adds an option to the grid
        /// </summary>
        /// <param iconID="optionData">The data for the option</param>
        /// <returns>The UI option object</returns>
        /// 
        public GridOption Add(GridOption.Data optionData)
        {

            //Base case
            if (!this.optionPrefab)
            {
                Debug.LogError("[OptionsGrid:Add] Tried to add an option to the grid from a GridOption.Data but the option prefabID was null");
                return null;
            }

            if (this.count + 1 > Size)
            {
                Debug.LogWarning("[OptionsGrid:Add] Tried to add but the grid was full");
                return null;
            }

            //Create and set up the added element
            GridOption option = Instantiate(this.optionPrefab);
            this.grid[this.head.x, this.head.y] = option;
            option.transform.SetParent(this.container.transform);
            option.transform.localScale = Vector3.one;
            option.Setup(optionData);

            this.head = Increment(this.head);

            //Increase the count
            this.count++;

            return option;
        }

        public bool Add(GridOption option)
        {

            //Base case
            if (this.count + 1 > Size)
            {
                Debug.LogWarning("[OptionsGrid:Add] Tried to add but the grid was full");
                return false;
            }

            //Create and set up the added element
            this.grid[this.head.x, this.head.y] = option;

            this.head = Increment(this.head);

            //Increase the count
            this.count++;

            return true;
        }


        /// <summary>
        /// Removes the option selected by the cursor from the grid
        /// </summary>
        /// <returns>True if the option was removed successfully, otherwise returns false</returns>
        public void Remove()
        {
            //Base Case
            if (this.count < 1)
            {
                Debug.LogWarning("[OptionsGrid:Remove] Tried to remove but the grid was empty");
                return;
            }

            Destroy(this.CurrentOption.gameObject);
            this.CurrentOption = null;
            this.count--;

            if (this.count < 1)
            {
                this.cursorPosition = new Vector2Int(0, 0);
                this.head = new Vector2Int(0, 0);
            }


            int maxi = this.gridHeight;
            int maxj = this.gridWidth;
            if (this.instantiationAxis == Axis.Vertical)
            {
                maxi = this.gridWidth;
                maxj = this.gridHeight;
            }

            Queue<GridOption> queue = new Queue<GridOption>();
            for (int i = 0; i < maxi; i++)
            {
                for (int j = 0; j < maxj; j++)
                {
                    int x = j;
                    int y = i;
                    if (this.instantiationAxis == Axis.Vertical)
                    {
                        x = i;
                        y = j;
                    }

                    if (this.grid[x, y] != null)
                    {
                        queue.Enqueue(this.grid[x, y]);
                    }
                }
            }

            this.grid = new GridOption[this.gridWidth, this.gridHeight];

            for (int i = 0; i < maxi; i++)
            {
                for (int j = 0; j < maxj; j++)
                {
                    int x = j;
                    int y = i;
                    if (this.instantiationAxis == Axis.Vertical)
                    {
                        x = i;
                        y = j;
                    }

                    if (queue.Count > 0)
                    {
                        this.grid[x, y] = queue.Dequeue();
                    }
                }
            }

            this.head = Decrement(this.head);

            if (this.cursorPosition == this.head)
            {
                this.cursorPosition = Decrement(this.cursorPosition);
            }

            SetCursorAtPosition(this.cursorPosition, true);
        }

        /// <summary>
        /// Clears all options and resets all option dependent data from the grid
        /// </summary>
        public void Clear() {
            if (this.grid is null)
                return;

            //Remove all elements in the grid
            for (int i = 0; i < this.gridWidth; i++)
                for (int j = 0; j < this.gridHeight; j++)
                    if (this.grid[i, j] != null)
                        Destroy(this.grid[i, j].gameObject);

            //Reset all private members that are dependent on grid elements
            this.count = 0;
            this.cursorPosition = new Vector2Int(0, 0);
            this.head = new Vector2Int(0, 0);
        }

        /// <summary>
        /// Reset the cursor to be at the head of the grid
        /// </summary>
        public void SetCursorToFirst()
        {
            if (this.grid is null)
            {
                Debug.LogError("[OptionsGrid:SetCursorAtHead] Tried to set the cursor at head but the grid was null");
                return;
            }

            if (this.Count < 1)
                return;

            foreach (GridOption option in this.grid)
            {
                if (!option)
                    continue;
                option.SetCursor(false);
            }

            SetCursorToFirstInternal();

            this.CurrentOption.SetCursor(true);
        }

        public void SetCursorAtPosition(Vector2Int cursorPosition, bool value)
        {
            if (this.grid is null)
            {
                Debug.LogWarning($"[OptionsGrid:SetCursorAtPosition] Tried to set the cursor to {value} at position ({cursorPosition.x}, {cursorPosition.y}) but the grid was null");
                return;
            }

            this.grid[cursorPosition.x, cursorPosition.y].SetCursor(value);
        }

        public void InvokeEvent(InputButton inputButton)
        {
            if (this.grid is null)
            {
                Debug.LogError($"[OptionsGrid:InvokeEvent] Tried to invoke the {inputButton} event at position ({this.cursorPosition.x}, {this.cursorPosition.y}) but the grid was null");
                return;
            }

            if (this.Interactable && this.gameObject.activeSelf && ButtonEnabled(inputButton) && this.count > 0)
            {
                this.CurrentOption.InvokeEvent(inputButton);
                BF2D.Utilities.Audio.PlayAudioSource(GetAudioSource(inputButton));
            }
        }

        /// <summary>
        /// Navigate through the grid
        /// </summary>
        /// <param iconID="direction">The direction of navigation</param>
        public void Navigate(InputDirection direction)
        {
            if (this.Interactable && this.gameObject.activeSelf && this.count > 0)
            {
                if (this.CurrentOption != null)
                    SetCursorAtPosition(this.cursorPosition, false);


                GridOption option = null;
                switch (direction)
                {
                    case InputDirection.Up:
                        while (option == null || !option.Interactable)
                            option = this.grid[this.cursorPosition.x, this.cursorPosition.y = Decrement(this.cursorPosition.y, this.gridHeight)];
                        break;
                    case InputDirection.Left:
                        while (option == null || !option.Interactable)
                            option = this.grid[this.cursorPosition.x = Decrement(this.cursorPosition.x, this.gridWidth), this.cursorPosition.y];
                        break;
                    case InputDirection.Down:
                        while (option == null || !option.Interactable)
                            option = this.grid[this.cursorPosition.x, this.cursorPosition.y = Increment(this.cursorPosition.y, this.gridHeight)];
                        break;
                    case InputDirection.Right:
                        while (option == null || !option.Interactable)
                            option = this.grid[this.cursorPosition.x = Increment(this.cursorPosition.x, this.gridWidth), this.cursorPosition.y];
                        break;
                    default:
                        Debug.LogError("[OptionsGrid:Navigate] Invalid direction");
                        break;
                }

                SetCursorAtPosition(this.cursorPosition, true);

                BF2D.Utilities.Audio.PlayAudioSource(this.navigateAudioSource);

                this.onNavigateEvent.Invoke(CursorPosition1D);
            }
        }
        #endregion

        #region Public Overrides
        public override void UtilityInitialize()
        {
            base.UtilityInitialize();
            SetCursorToFirst();
        }
        #endregion

        #region Private Methods
        private int Increment(int value, int size)
        {
            int field = value;

            if (field == size - 1)
            {
                field = 0;
            }
            else
            {
                field += 1;
            }

            return field;
        }

        private Vector2Int Increment(Vector2Int vector)
        {
            Vector2Int v = vector;
            switch (this.instantiationAxis)
            {
                case Axis.Horizontal:
                    if (v.x + 1 >= this.gridWidth)
                    {
                        v.x = 0;
                        v.y++;
                    }
                    else
                    {
                        v.x++;
                    }
                    break;
                case Axis.Vertical:
                    if (v.y + 1 >= this.gridHeight)
                    {
                        v.y = 0;
                        v.x++;
                    }
                    else
                    {
                        v.y++;
                    }
                    break;
            }

            return v;
        }

        private int Decrement(int value, int size)
        {
            int field = value;

            if (field == 0)
            {
                field = size - 1;
            }
            else
            {
                field -= 1;
            }

            return field;

        }

        private Vector2Int Decrement(Vector2Int vector)
        {
            Vector2Int v = vector;
            switch (this.instantiationAxis)
            {
                case Axis.Horizontal:
                    if (v.x - 1 < 0)
                    {
                        v.x = this.gridWidth - 1;
                        v.y--;
                    }
                    else
                    {
                        v.x--;
                    }
                    break;
                case Axis.Vertical:
                    if (v.y - 1 < 0)
                    {
                        v.y = this.gridHeight - 1;
                        v.x--;
                    }
                    else
                    {
                        v.y--;
                    }
                    break;
            }

            return v;
        }

        private void SetCursorToFirstInternal()
        {
            if (this.Count < 1)
                return;

            this.cursorPosition = new Vector2Int(0, 0);
            GridOption option = this.CurrentOption;
            while (!option.Interactable)
            {
                this.cursorPosition = Increment(this.cursorPosition);
                option = this.CurrentOption;
            }
        }

        private AudioSource GetAudioSource(InputButton inputButton)
        {
            return inputButton switch
            {
                InputButton.Confirm => this.confirmAudioSource,
                InputButton.Back => this.backAudioSource,
                InputButton.Menu => this.menuAudioSource,
                InputButton.Attack => this.attackAudioSource,
                InputButton.Pause => this.pauseAudioSource,
                InputButton.Select => this.selectAudioSource,
                _ => throw new ArgumentException("[OptionsGrid:GetAudioSource] InputButton was null or invalid")
            };
        }

        private bool ButtonEnabled(InputButton inputButton)
        {
            return inputButton switch
            {
                InputButton.Confirm => this.confirmEnabled,
                InputButton.Back => this.backEnabled,
                InputButton.Menu => this.menuEnabled,
                InputButton.Attack => this.attackEnabled,
                InputButton.Pause => this.pauseEnabled,
                InputButton.Select => this.selectEnabled,
                _ => throw new ArgumentException("[OptionsGrid:ButtonEnabled] InputButton was null or invalid")
            };
        }
        #endregion
    }
}

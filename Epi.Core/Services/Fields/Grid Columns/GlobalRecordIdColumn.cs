using System;
using System.Data;
using System.Drawing;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Fields
{
    /// <summary>
    /// Global Record Id Grid Column
    /// </summary>
    public class GlobalRecordIdColumn : PredefinedColumn, IMirrorable
    {
        #region Constructors

        /// <summary>
        /// Constructor for the class. Instantiates a grid field.
        /// </summary>
        /// <param name="grid">Grid field object.</param>
        public GlobalRecordIdColumn(GridField grid)
            : base(grid)
        {
            Construct();
        }
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField the row belongs to</param>
        public GlobalRecordIdColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            Construct();
        }

        /// <summary>
        /// Common constructor code to provide a name for the class.
        /// </summary>
        private void Construct()
        {
            this.Name = ColumnNames.GLOBAL_RECORD_ID;
            this.GridColumnType = MetaFieldType.GlobalRecordId;
            genericDbColumnType = GenericDbColumnType.Int64;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Returns/sets the prompt text.
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.GLOBAL_RECORD_ID;
            }
            set
            {
                throw new GeneralException("Text for Global Record Id is pre-defined.");
            }
        }

        #endregion Public Properties

        #region protected Methods
        /// <summary>
        /// Saves the field to the database
        /// </summary>
        protected override void InsertColumn()
        {
            if (Id == 0)
            {
                Id = GetMetadata().CreateGridColumn(this);
                base.OnColumnInserted();
            }
            else
            {
                throw new System.ApplicationException("Global Record Id column already exists.");
            }
        }

        /// <summary>
        /// Deletes the grid column from the database.
        /// </summary>
        protected override void DeleteColumn()
        {
            GetMetadata().DeleteGridColumn(this);
        }

        /// <summary>
        /// Non implemented member to update of the Unique Key column.
        /// </summary>
        protected override void UpdateColumn()
        {
            return;
        }

        #endregion

        #region
        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueString;
        }
        #endregion

    }
}
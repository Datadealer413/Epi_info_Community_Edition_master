#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class EnableDialog : CheckCodeDesignDialog
    {
        #region Constructors
        
        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public EnableDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Enable dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public EnableDialog(MainForm frm)
            : base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        #endregion //Constructors

        #region Private Event Handlers        
        
        /// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handles the Click event of the Ok button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = CommandNames.ENABLE + StringLiterals.SPACE;            

            for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
            {
                Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
            }

            Output = Output.Trim();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Handles the index selection change event of the listbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbxFields.SelectedItem != null);
        }

        #endregion  //Private Event Handlers

        #region Public Properties

        /// <summary>
        /// Sets the View for the dialog
        /// </summary>
        public override View View
        {
            set
            {
                foreach (Fields.Field field in value.Fields)
                {
                    if (field is Fields.RenderableField && !(field is Fields.LabelField))
                    {
                        lbxFields.Items.Add(field.Name);
                    }
                }
            }
        }
        #endregion  //Public Properties
    }
}


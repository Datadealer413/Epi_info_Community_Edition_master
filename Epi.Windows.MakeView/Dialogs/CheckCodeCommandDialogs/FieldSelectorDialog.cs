using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
    public partial class FieldSelectorDialog : CheckCodeDesignDialog
    {
        protected EpiInfo.Plugin.IEnterInterpreter EpiInterpreter;

         #region Constructors
        [Obsolete("Use of default constructor not allowed", true)]
        public FieldSelectorDialog()
        {
            InitializeComponent();
        }
          /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public FieldSelectorDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            this.EpiInterpreter = frm.EpiInterpreter;
        }

        #endregion  //Constructors


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
            Output = StringLiterals.SPACE;

            for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
            {
                Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
            }

            Output = Output.Trim();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Handles the selection change of the fields in the list box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbxFields.SelectedItem != null);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-field-selector.html");
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
                VariableType scopeWord = VariableType.Standard;//User defined variables
                List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope((EpiInfo.Plugin.VariableScope)scopeWord);              
                 foreach (EpiInfo.Plugin.IVariable var in vars)
                 {
                     if (!(var is Epi.Fields.PredefinedDataField))
                     {
                         lbxFields.Items.Add(var.Name.ToString());
                     }
                 }
            }
        }
        #endregion  //Public Properties
    }
}

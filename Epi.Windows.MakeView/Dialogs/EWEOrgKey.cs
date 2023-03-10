using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
 
namespace Epi.Windows.MakeView.Dialogs
{
    public partial class EWEOrgKey : Form
    {
        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        private string _SurveyId="";
        private string _SuccessMessage="";
        private string _Dialogprompt = "";
        private string _OrgKey;
        private bool _ShowSuccessMessage;



        public string  OrganizationKey 
        {
            get { return _OrgKey; }
    
        }
        public EWEOrgKey(string SurveyId, bool ShowSuccessMessage ,string SuccessMessage, string DialogPrompt)
        {
            InitializeComponent();
            pnlError.Visible = false;
            pnlOrgKey.Visible = true;
            pnlSuccess.Visible = false;
            pnlSuccessMsg.Visible = false;
            this._SurveyId = SurveyId;
            _Dialogprompt = DialogPrompt;
            DialogPromptLabel.Text = _Dialogprompt; 
            this._SuccessMessage = SuccessMessage;
            _ShowSuccessMessage = ShowSuccessMessage;
        }
        public EWEOrgKey( )
        {
            InitializeComponent();
            pnlError.Visible = false;
            pnlOrgKey.Visible = true;
            pnlSuccess.Visible = false;
            pnlSuccessMsg.Visible = false;
         
        }
        private void button1_Click(object sender, EventArgs e)
         {
        
            //tbOrgKey
             Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No;
             IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(tbOrgKey.Text.ToString(), this._SurveyId);

            //this.DialogResult = DialogResult.OK;
            switch(IsValidOKey)
            {
                case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No:
                    pnlError.Visible = true;
                    pnlOrgKey.Visible = true;
                    pnlSuccess.Visible = false;
                    pnlSuccessMsg.Visible = false;
                    break;
                case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                    pnlError.Visible = true;
                    pnlOrgKey.Visible = true;
                    pnlSuccess.Visible = false;
                    pnlSuccessMsg.Visible = false;
                    WebEnterOptions wso = new WebEnterOptions();
                     
                    wso.Show();
                    break;
                case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                    pnlError.Visible = true;
                    pnlOrgKey.Visible = true;
                    pnlSuccess.Visible = false;
                    pnlSuccessMsg.Visible = false;
                    break;
                case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.Yes:
                    this._OrgKey = tbOrgKey.Text.ToString();
                    pnlError.Visible = false;
                    pnlOrgKey.Visible = false;
                    pnlSuccess.Visible = true;
                    pnlSuccessMsg.Visible = true;
                    this.Text = "Success";
                    this.DialogPromptLabel.Text = _Dialogprompt;
                   // lblSuccess.Text = " Your survey has been published successfully!";
                    lblSuccess.Text = this._SuccessMessage;
                    if(!this._ShowSuccessMessage)
                    {
                        this.DialogResult = DialogResult.OK;
                    }
                break;
            }
        }


        public static bool IsGuid(string Guid)
        {
            bool isValid = false;


            if (Guid != null)
            {

                if (isGuid.IsMatch(Guid))
                {

                    isValid = true;
                }
            }

            return isValid;
        }



        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this._ShowSuccessMessage)
            {
                this.DialogResult = DialogResult.OK;
            }
            this.Close();
        }

        private void tbOrgKey_TextChanged(object sender, EventArgs e)
        {
            if (IsGuid(this.tbOrgKey.Text))
            {
                btnSubmit.Enabled = true;
            }
             else
            {
                btnSubmit.Enabled = false;
            }
        }

       
    }
}

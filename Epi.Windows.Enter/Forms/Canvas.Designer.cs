namespace Epi.Windows.Enter
{
    partial class Canvas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {

            this.currentPanel = null;
            this.currentView = null;
            this.panelsList = null;
            this.bufferGraphics = null;
            this.bufferBitmap = null;
            this.config = null;
            this.mainFrm = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Canvas));
            this.canvasPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // baseImageList
            // 
            this.baseImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("baseImageList.ImageStream")));
            this.baseImageList.Images.SetKeyName(0, "");
            this.baseImageList.Images.SetKeyName(1, "");
            this.baseImageList.Images.SetKeyName(2, "");
            this.baseImageList.Images.SetKeyName(3, "");
            this.baseImageList.Images.SetKeyName(4, "");
            this.baseImageList.Images.SetKeyName(5, "");
            this.baseImageList.Images.SetKeyName(6, "");
            this.baseImageList.Images.SetKeyName(7, "");
            this.baseImageList.Images.SetKeyName(8, "");
            this.baseImageList.Images.SetKeyName(9, "");
            this.baseImageList.Images.SetKeyName(10, "");
            this.baseImageList.Images.SetKeyName(11, "");
            this.baseImageList.Images.SetKeyName(12, "");
            this.baseImageList.Images.SetKeyName(13, "");
            this.baseImageList.Images.SetKeyName(14, "");
            this.baseImageList.Images.SetKeyName(15, "");
            this.baseImageList.Images.SetKeyName(16, "");
            this.baseImageList.Images.SetKeyName(17, "");
            this.baseImageList.Images.SetKeyName(18, "");
            this.baseImageList.Images.SetKeyName(19, "");
            this.baseImageList.Images.SetKeyName(20, "");
            this.baseImageList.Images.SetKeyName(21, "");
            this.baseImageList.Images.SetKeyName(22, "");
            this.baseImageList.Images.SetKeyName(23, "");
            this.baseImageList.Images.SetKeyName(24, "");
            this.baseImageList.Images.SetKeyName(25, "");
            this.baseImageList.Images.SetKeyName(26, "");
            this.baseImageList.Images.SetKeyName(27, "");
            this.baseImageList.Images.SetKeyName(28, "");
            this.baseImageList.Images.SetKeyName(29, "");
            this.baseImageList.Images.SetKeyName(30, "");
            this.baseImageList.Images.SetKeyName(31, "");
            this.baseImageList.Images.SetKeyName(32, "");
            this.baseImageList.Images.SetKeyName(33, "");
            this.baseImageList.Images.SetKeyName(34, "");
            this.baseImageList.Images.SetKeyName(35, "");
            this.baseImageList.Images.SetKeyName(36, "");
            this.baseImageList.Images.SetKeyName(37, "");
            this.baseImageList.Images.SetKeyName(38, "");
            this.baseImageList.Images.SetKeyName(39, "");
            this.baseImageList.Images.SetKeyName(40, "");
            this.baseImageList.Images.SetKeyName(41, "");
            this.baseImageList.Images.SetKeyName(42, "");
            this.baseImageList.Images.SetKeyName(43, "");
            this.baseImageList.Images.SetKeyName(44, "");
            this.baseImageList.Images.SetKeyName(45, "");
            this.baseImageList.Images.SetKeyName(46, "");
            this.baseImageList.Images.SetKeyName(47, "");
            this.baseImageList.Images.SetKeyName(48, "");
            this.baseImageList.Images.SetKeyName(49, "");
            this.baseImageList.Images.SetKeyName(50, "");
            this.baseImageList.Images.SetKeyName(51, "");
            this.baseImageList.Images.SetKeyName(52, "");
            this.baseImageList.Images.SetKeyName(53, "");
            this.baseImageList.Images.SetKeyName(54, "");
            this.baseImageList.Images.SetKeyName(55, "");
            this.baseImageList.Images.SetKeyName(56, "");
            this.baseImageList.Images.SetKeyName(57, "");
            this.baseImageList.Images.SetKeyName(58, "");
            this.baseImageList.Images.SetKeyName(59, "");
            this.baseImageList.Images.SetKeyName(60, "");
            this.baseImageList.Images.SetKeyName(61, "");
            this.baseImageList.Images.SetKeyName(62, "");
            this.baseImageList.Images.SetKeyName(63, "");
            this.baseImageList.Images.SetKeyName(64, "");
            this.baseImageList.Images.SetKeyName(65, "");
            this.baseImageList.Images.SetKeyName(66, "");
            this.baseImageList.Images.SetKeyName(67, "");
            this.baseImageList.Images.SetKeyName(68, "");
            this.baseImageList.Images.SetKeyName(69, "");
            this.baseImageList.Images.SetKeyName(70, "");
            this.baseImageList.Images.SetKeyName(71, "");
            this.baseImageList.Images.SetKeyName(72, "");
            this.baseImageList.Images.SetKeyName(73, "");
            this.baseImageList.Images.SetKeyName(74, "");
            this.baseImageList.Images.SetKeyName(75, "");
            this.baseImageList.Images.SetKeyName(76, "");
            this.baseImageList.Images.SetKeyName(77, "");
            this.baseImageList.Images.SetKeyName(78, "");
            this.baseImageList.Images.SetKeyName(79, "");
            this.baseImageList.Images.SetKeyName(80, "");
            this.baseImageList.Images.SetKeyName(81, "");
            this.baseImageList.Images.SetKeyName(82, "");
            this.baseImageList.Images.SetKeyName(83, "");
            this.baseImageList.Images.SetKeyName(84, "");
            this.baseImageList.Images.SetKeyName(85, "");
            this.baseImageList.Images.SetKeyName(86, "");
            this.baseImageList.Images.SetKeyName(87, "");
            this.baseImageList.Images.SetKeyName(88, "");
            this.baseImageList.Images.SetKeyName(89, "");
            this.baseImageList.Images.SetKeyName(90, "");
            this.baseImageList.Images.SetKeyName(91, "");
            this.baseImageList.Images.SetKeyName(92, "");
            this.baseImageList.Images.SetKeyName(93, "");
            this.baseImageList.Images.SetKeyName(94, "");
            this.baseImageList.Images.SetKeyName(95, "");
            this.baseImageList.Images.SetKeyName(96, "");
            // 
            // pnlDesigner
            // 
            resources.ApplyResources(this.canvasPanel, "pnlDesigner");
            this.canvasPanel.BackColor = System.Drawing.Color.White;
            this.canvasPanel.Name = "pnlDesigner";
            // 
            // Canvas
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.canvasPanel);
            this.DockType = Epi.Windows.Docking.DockContainerType.Document;
            this.IsVisible = true;
            this.Name = "Canvas";
            this.ResumeLayout(false);

        }

        #endregion


        public System.Windows.Forms.Panel canvasPanel;
    }
}
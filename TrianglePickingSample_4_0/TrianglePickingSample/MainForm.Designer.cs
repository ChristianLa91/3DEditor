namespace TrianglePicking
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonMove = new System.Windows.Forms.Button();
            this.buttonCreateCube = new System.Windows.Forms.Button();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            this.contextMenuMove = new System.Windows.Forms.ContextMenu();
            this.menuItemMoveX = new System.Windows.Forms.MenuItem();
            this.menuItemMoveY = new System.Windows.Forms.MenuItem();
            this.menuItemMoveZ = new System.Windows.Forms.MenuItem();
            this.trianglePickingGame1 = new TrianglePicking.TrianglePickingGame();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.CausesValidation = false;
            this.panel1.Controls.Add(this.buttonMove);
            this.panel1.Controls.Add(this.buttonCreateCube);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(80, 414);
            this.panel1.TabIndex = 0;
            // 
            // buttonMove
            // 
            this.buttonMove.CausesValidation = false;
            this.buttonMove.Location = new System.Drawing.Point(3, 50);
            this.buttonMove.Name = "buttonMove";
            this.buttonMove.Size = new System.Drawing.Size(72, 41);
            this.buttonMove.TabIndex = 1;
            this.buttonMove.TabStop = false;
            this.buttonMove.Text = "Move";
            this.buttonMove.UseVisualStyleBackColor = true;
            this.buttonMove.Click += new System.EventHandler(this.buttonMove_Click);
            // 
            // buttonCreateCube
            // 
            this.buttonCreateCube.Location = new System.Drawing.Point(3, 3);
            this.buttonCreateCube.Name = "buttonCreateCube";
            this.buttonCreateCube.Size = new System.Drawing.Size(72, 41);
            this.buttonCreateCube.TabIndex = 0;
            this.buttonCreateCube.TabStop = false;
            this.buttonCreateCube.Text = "Cube";
            this.buttonCreateCube.UseVisualStyleBackColor = true;
            this.buttonCreateCube.Click += new System.EventHandler(this.buttonCreateCube_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemExit});
            this.menuItem1.Text = "File";
            // 
            // menuItemExit
            // 
            this.menuItemExit.Index = 0;
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // contextMenuMove
            // 
            this.contextMenuMove.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemMoveX,
            this.menuItemMoveY,
            this.menuItemMoveZ});
            // 
            // menuItemMoveX
            // 
            this.menuItemMoveX.Index = 0;
            this.menuItemMoveX.Text = "X";
            this.menuItemMoveX.Click += new System.EventHandler(this.menuItemMoveX_Click);
            // 
            // menuItemMoveY
            // 
            this.menuItemMoveY.Index = 1;
            this.menuItemMoveY.Text = "Y";
            this.menuItemMoveY.Click += new System.EventHandler(this.menuItemMoveY_Click);
            // 
            // menuItemMoveZ
            // 
            this.menuItemMoveZ.Index = 2;
            this.menuItemMoveZ.Text = "Z";
            this.menuItemMoveZ.Click += new System.EventHandler(this.menuItemMoveZ_Click);
            // 
            // trianglePickingGame1
            // 
            this.trianglePickingGame1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trianglePickingGame1.Location = new System.Drawing.Point(80, 0);
            this.trianglePickingGame1.Name = "trianglePickingGame1";
            this.trianglePickingGame1.Size = new System.Drawing.Size(704, 414);
            this.trianglePickingGame1.TabIndex = 1;
            this.trianglePickingGame1.Text = "trianglePickingGame1";
            this.trianglePickingGame1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.trianglePickingGame1_KeyDown);
            this.trianglePickingGame1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trianglePickingGame1_MouseDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 414);
            this.Controls.Add(this.trianglePickingGame1);
            this.Controls.Add(this.panel1);
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "3D Models";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private TrianglePickingGame trianglePickingGame1;
        private System.Windows.Forms.Button buttonCreateCube;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.Button buttonMove;
        private System.Windows.Forms.ContextMenu contextMenuMove;
        private System.Windows.Forms.MenuItem menuItemMoveX;
        private System.Windows.Forms.MenuItem menuItemMoveY;
        private System.Windows.Forms.MenuItem menuItemMoveZ;
    }
}
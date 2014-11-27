using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrianglePicking
{
    public partial class MainForm : Form
    {
        public enum EditMode
        {
            NORMAL,
            MOVEX,
            MOVEY,
            MOVEZ,
        }

        private EditMode mEditMode = EditMode.NORMAL;

        public MainForm()
        {
            InitializeComponent();
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void trianglePickingGame1_MouseDown(object sender, MouseEventArgs e)
        {
            if (mEditMode == EditMode.NORMAL && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                trianglePickingGame1.UpdateSelection();
            }

            mEditMode = EditMode.NORMAL;

            
            trianglePickingGame1.SetEditMode(mEditMode);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                trianglePickingGame1.ResetMovement();
            }
            else
            {
                trianglePickingGame1.StoreToVertexModelBefore();
            }
        }

        private void menuItemMoveX_Click(object sender, EventArgs e)
        {
            mEditMode = EditMode.MOVEX;

            trianglePickingGame1.SetEditMode(mEditMode);
        }

        private void menuItemMoveY_Click(object sender, EventArgs e)
        {
            mEditMode = EditMode.MOVEY;

            trianglePickingGame1.SetEditMode(mEditMode);
        }
        
        private void menuItemMoveZ_Click(object sender, EventArgs e)
        {
            mEditMode = EditMode.MOVEZ;

            trianglePickingGame1.SetEditMode(mEditMode);
        }

        private void buttonMove_Click(object sender, EventArgs e)
        {
            contextMenuMove.Show((Button)sender, ((Button)sender).PointToClient(MousePosition));
        }

        private void buttonCreateCube_Click(object sender, EventArgs e)
        {
            trianglePickingGame1.CreateCube();
        }

        private void trianglePickingGame1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
            {
                trianglePickingGame1.ClearSelection();
            }
        }

    }
}

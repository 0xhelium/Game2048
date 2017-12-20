using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game2048
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            Container.BackColor = Color.FromArgb(217, 255, 221);
            
            _controller = Controller.Create(Container);
            _controller._scoreBoard = this.lblScore;
            _controller.Begin();

            this.Container.Paint += Container_Paint;            
            this.btnRestart.Click += BtnRestart_Click;
            this.KeyDown += Form1_KeyDown;
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            _controller.Restart();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            var keyCode = e.KeyCode;
            if (e.KeyCode == Keys.W)
                keyCode = Keys.Up;
            if (e.KeyCode == Keys.S)
                keyCode = Keys.Down;
            if (e.KeyCode == Keys.A)
                keyCode = Keys.Left;
            if (e.KeyCode == Keys.D)
                keyCode = Keys.Right;
            _controller.OnKeyUp(keyCode);
        }

        private void Container_Paint(object sender, PaintEventArgs e)
        {            
            Container.Invalidate();
            _controller.Draw(e.Graphics);
        }

        private Controller _controller;

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down ||
              keyData == Keys.Left || keyData == Keys.Right)
                return false;
            return base.ProcessDialogKey(keyData);
        }
    }
}

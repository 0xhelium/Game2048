using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game2048
{
    public abstract class BaseObj
    {
        public BaseObj(int x, int y, int len)
        {
            this.XIndex = x;
            this.YIndex = y;
            this.Len = len;
            this.Size = new Size(Len, Len);
            this.Location = new Point(x * Len, y * Len);
        }

        private Color[] _colors
        {
            get
            {
                return new[]{
                    ColorTranslator.FromHtml("#2196f3"),
                    ColorTranslator.FromHtml("#03a9f4"),
                    ColorTranslator.FromHtml("#00bcd4"),
                    ColorTranslator.FromHtml("#009688"),
                    ColorTranslator.FromHtml("#4caf50"),
                    ColorTranslator.FromHtml("#8bc34a"),
                    ColorTranslator.FromHtml("#cddc39"),
                    ColorTranslator.FromHtml("#ffeb3b"),
                    ColorTranslator.FromHtml("#ffc107"),
                    ColorTranslator.FromHtml("#ff9800"),
                    ColorTranslator.FromHtml("#ff5722"),    
                    ColorTranslator.FromHtml("#f44336"),
                    ColorTranslator.FromHtml("#e91e63"),
                    ColorTranslator.FromHtml("#9c27b0"),
                    ColorTranslator.FromHtml("#673ab7"),
                    ColorTranslator.FromHtml("#3f51b5"),
                };
            }
        }

        public int Len { get; set; }

        public int XIndex { get; set; }

        public int YIndex { get; set; }

        public Size Size { get; set; }

        public Point Location { get; set; }

        public int Number { get; set; } = 2;

        public Color BackColor
        {
            get {
                return _colors[(int)Math.Log(Number, 2)];
            }
        }

        public Rectangle Rec
        {
            get { return new Rectangle(Location, Size); }
        }

        public virtual void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(this.BackColor), this.Rec);
            g.DrawRectangle(new Pen(Color.FromArgb(217, 255, 221), 5), this.Rec);
            g.DrawString(this.Number.ToString(),
                new Font("微软雅黑", 22),
                new SolidBrush(Color.White),
                this.Rec,
                new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                });            
        }
    }
}

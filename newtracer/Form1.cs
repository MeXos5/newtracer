using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace newtracer
{
    public partial class Form1 : Form
    {
        scene world;
        List<Illuminance> illums;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            world = new scene();
            illums = interactions.generateColors();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Size = new System.Drawing.Size(world.Width, world.Height);
            Graphics graphics = this.CreateGraphics();

            for (int j = 0; j < world.Height; j++)
            {
                for (int i = 0; i < world.Width; i++)
                {
                    var idx = i + j * world.Width;    //list index [0; width*height]
                    var r = (int)illums[idx].getRGB().X < 255 ? (int)illums[idx].getRGB().X : 255;
                    var g = (int)illums[idx].getRGB().Y < 255 ? (int)illums[idx].getRGB().Y : 255;
                    var b = (int)illums[idx].getRGB().Z < 255 ? (int)illums[idx].getRGB().Z : 255;

                    this.Text = "Loading: %" + (idx * 100 / (world.Height * world.Width)).ToString();
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb(r, g, b)), i, j, 1, 1);
                }
            }
            this.Text = "Rendering done.";
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            //gettin position
            var pos = new Point(Form1.MousePosition.Y - this.Location.Y - SystemInformation.CaptionHeight, Form1.MousePosition.X - this.Location.X - SystemInformation.CaptionHeight);
            var pt = this.PointToClient(Cursor.Position);
            //gettin element
            int idx;
            idx = pos.Y + pos.X * world.Height;
            //obtaining color
            Vector3 color = new Vector3();
            color.X = illums[idx].getRGB().X;
            color.Y = illums[idx].getRGB().Y;
            color.Z = illums[idx].getRGB().Z;
            this.Text = idx.ToString() + " | (" + pt.X.ToString() + "; " + pt.Y.ToString() + ") | R: " + illums[idx].getRGB().X.ToString() + " | G: " + illums[idx].getRGB().Y.ToString() + " | B: " + illums[idx].getRGB().Z.ToString();
        }
    }
}

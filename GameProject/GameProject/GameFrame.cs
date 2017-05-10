using System.Windows.Forms;
using System.Collections.Generic;
using GameProject.Objects;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System;

namespace GameProject
{
    public partial class GameFrame : Form
    {
        private List<Drawable> objects = new List<Drawable>();

        public GameFrame()
        {
            InitializeComponent();

            // Write Code Below Here

            FixFlicker();

            objects.Add(new Character(new Point(0, 0)));
            objects.Add(new Character(new Point(150, 150)));

            RunThread();
        }

        private void FixFlicker()
        {
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        private void RunThread()
        {
            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                while (true)
                {
                    this.Invalidate();
                    Thread.Sleep(1000 / 60);
                }

            });

            bw.RunWorkerAsync();
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            long microseconds = Extensions.GetMicroSeconds();

            foreach (Drawable obj in objects)
            {
                obj.Draw(e.Graphics, microseconds);
            }
        }
    }
}

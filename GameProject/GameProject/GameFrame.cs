using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GameProject.Objects;

namespace GameProject
{
    public partial class GameFrame : Form
    {
        private const int FPS = 60;
        private List<Layer> layers = new List<Layer>();

        public GameFrame()
        {
            InitializeComponent();

            // Write Code Below Here

            FixFlicker();

            layers.Add(new Layer());

            AddObjectToLayer(new Character(new Point(0, 0)), 0);
            Thread.Sleep(300);
            AddObjectToLayer(new Character(new Point(100, 0)), 0);

            RunGameThread();
        }

        private void AddObjectToLayer(Drawable obj, int layerNo)
        {
            Layer layer = layers[layerNo];

            layer.Add(obj);
            //obj.layerNo = layerNo;
            //obj.layersRef = layers;
        }

        private void FixFlicker()
        {
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        private void RunGameThread()
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
                    Thread.Sleep(1000 / FPS);
                }

            });

            bw.RunWorkerAsync();
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            long microseconds = Extensions.GetMicroSeconds();

            foreach (Layer layer in layers)
            {
                foreach (Drawable obj in layer.objects)
                {
                    if (obj.WillBeDrawn)
                        obj.Draw(e.Graphics, microseconds);
                }
            }
        }
    }
}

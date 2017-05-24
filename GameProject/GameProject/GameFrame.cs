using System;
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
        private KeyBindings keyBindings = new KeyBindings();

        // Z-index of layers, lower index means closer to the back
        private enum Layers
        {
            Top = 0
        }

        public GameFrame()
        {
            InitializeComponent();

            // Write Code Below Here

            FixFlicker();

            // Set up the layers 
            foreach (var layerName in Enum.GetNames(typeof(Layers)))
                layers.Add(new Layer());

            // Add the objects to layers
            AddObjectToLayer(new Character(new Point(100, 100)), Layers.Top);

            // Start the game
            RunGameThread();
        }

        private void AddObjectToLayer<T>(T obj, Layers layerNo) where T : Drawable, KeyListener
        {
            keyBindings.listeners.Add(obj);

            AddObjectToLayer((Drawable)obj, layerNo);
        }

        private void AddObjectToLayer(Drawable obj, Layers layerNo)
        {
            Layer layer = layers[(int)layerNo];

            layer.Add(obj);
            obj.frameRef = this;
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

        private void GameFrame_KeyDown(object sender, KeyEventArgs e)
        {
            keyBindings.SetKeyDown(e.KeyCode);
        }

        private void GameFrame_KeyUp(object sender, KeyEventArgs e)
        {
            keyBindings.SetKeyUp(e.KeyCode);
        }

        public bool IsKeyDown(KeyBindings.GameInput key)
        {
            return keyBindings.IsKeyDown(key);
        }
    }
}

using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GameProject
{
    public class KeyBindings
    {
        public enum GameInput
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private bool[] keyDown = new bool[Enum.GetNames(typeof(GameInput)).Length];
        public List<KeyListener> listeners = new List<KeyListener>();

        public bool IsKeyDown(GameInput key)
        {
            return keyDown[(int)key];
        }

        public void SetKeyDown(Keys keys)
        {
            GameInput key = BindKey(keys);

            if (key != GameInput.None)
            {
                SetGameKeyDown(key);
                foreach (KeyListener listener in listeners)
                {
                    listener.OnKeyDown(key);
                }
            }
        }

        public void SetKeyUp(Keys keys)
        {
            GameInput key = BindKey(keys);

            if (key != GameInput.None)
            {
                SetGameKeyUp(key);
                foreach (KeyListener listener in listeners)
                {
                    listener.OnKeyUp(key);
                }
            }
        }

        #region Private Methods
        private void SetGameKeyDown(GameInput key)
        {
            keyDown[(int)key] = true;
        }

        private void SetGameKeyUp(GameInput key)
        {
            keyDown[(int)key] = false;
        }

        private static GameInput BindKey(Keys keys)
        {
            switch (keys)
            {
                case Keys.Right: return GameInput.Right;
                case Keys.Left: return GameInput.Left;
                default: return GameInput.None;
            }
        }
        #endregion
    }
}

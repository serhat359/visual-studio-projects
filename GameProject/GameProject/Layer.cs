using System.Collections.Generic;

namespace GameProject
{
    public class Layer
    {
        public List<Drawable> objects = new List<Drawable>();

        public void Add(Drawable obj)
        {
            objects.Add(obj);
        }
    }
}

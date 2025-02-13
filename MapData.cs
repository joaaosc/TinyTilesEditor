using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyEditor
{
    public class AnimatedSpriteData
    {
        public string TextureID { get; set; } // (nome do arquivo ou outro ID)

        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int FrameCount { get; set; }
        public float FrameTime { get; set; }
    }

    public class MapData
    {
        public List<AnimatedSpriteData> AnimatedSprites { get; set; } = new List<AnimatedSpriteData>();


    }
}


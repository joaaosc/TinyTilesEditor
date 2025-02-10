using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TinyEditor
{
    /// <summary>
    /// Representa uma entrada de textura, contendo o identificador (nome),
    /// a textura propriamente dita e o retângulo usado para exibir sua miniatura.
    /// </summary>
    public class TextureEntry
    {
        /// <summary>
        /// Nome ou identificador da textura.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A textura importada.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Retângulo que define a posição e tamanho da miniatura na interface.
        /// </summary>
        public Rectangle ThumbnailRectangle { get; set; }
    }
}

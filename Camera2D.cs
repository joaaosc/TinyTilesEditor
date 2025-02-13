using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TinyEditor
{
    public class Camera2D
    {
        ///<summary>
        /// Posição da câmera no mundo
        /// </summary>
        public Vector2 PositionVector2 { get; private set; }

        ///<summary>
        /// Fator de zoom (1f = 100%)
        /// </summary>
        public float Zoom { get; set; } = 1f;

        ///<summary>
        /// Rotação da câmera (em radianos)
        /// </summary>
        public float Rotation { get; set; } = 0f;

        public Vector2 Position { get; set; }

        private int viewportWidth;
        private int viewportHeight;

        public Camera2D(int viewportWidth, int viewportHeight)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            PositionVector2 = Vector2.Zero;
        }

        ///<summary>
        /// Retorna a matriz de transformação da câmera, a ser usada
        /// no SpriteBatch.Begin
        /// </summary>
        public Matrix GetTransformation()
        {
            return Matrix.CreateTranslation(new Vector3(-PositionVector2, 0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(Zoom) *
                   Matrix.CreateTranslation(new Vector3(viewportWidth * 0.5f, viewportHeight * 0.5f, 0f));
        }

        ///<summary>
        /// Move a câmera de acordo com o param passado.
        /// </summary>
        public void Move(Vector2 param)
        {
            PositionVector2 += param;
        }


    }
}

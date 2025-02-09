using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TinyEditor
{

    public class InputManager
    {
        private MouseState previousMouseState;
        private Camera2D camera;
        private Map map;

        // Fator para ajustar a sensibilidade do zoom.
        private const float ZoomSensitivity = 0.001f;
        // Limites do zoom
        private const float ZoomMin = 0.1f;
        private const float ZoomMax = 5f;

        public InputManager(Camera2D camera, Map map)
        {
            this.camera = camera;
            this.map = map;
            previousMouseState = Mouse.GetState();
        }
        /// <summary>
        /// Processa as entradas do usuário para atualizar a câmera e a seleção dos tiles.
        /// </summary>
        public void Update()
        {
            MouseState currentMouseState = Mouse.GetState();

            // --- Sistema de Zoom com a Roda do Mouse ---
            int scrollDelta = currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                camera.Zoom += scrollDelta * ZoomSensitivity;
                camera.Zoom = MathHelper.Clamp(camera.Zoom, ZoomMin, ZoomMax);
            }
            // --------------------------------------------

            // --- Sistema de Mover a Câmera com Shift + Botão Esquerdo do Mouse ---
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                // Calcula a variação (delta) da posição do mouse
                Vector2 mouseDelta = new Vector2(
                    currentMouseState.X - previousMouseState.X,
                    currentMouseState.Y - previousMouseState.Y);

                // Move a câmera na direção contrária ao delta para "arrastar" o cenário
                camera.Move(-mouseDelta / camera.Zoom);
            }
            // ---------------------------------------------------------------------

            // Atualiza a seleção dos tiles:
            // Converte a posição do mouse (na tela) para coordenadas do mundo usando a matriz inversa da câmera
            Vector2 worldMousePosition = Vector2.Transform(
                new Vector2(currentMouseState.X, currentMouseState.Y),
                Matrix.Invert(camera.GetTransformation()));

            map.UpdateSelectedTile(new Point((int)worldMousePosition.X, (int)worldMousePosition.Y));

            // Atualiza o estado anterior do mouse para o próximo frame
            previousMouseState = currentMouseState;
        }
    }
}
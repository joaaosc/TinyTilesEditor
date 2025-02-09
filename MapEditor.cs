using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TinyEditor
{
    public class MapEditor
    {
        private Map map;
        private Camera2D camera;
        private GUIManager guiManager;
        private MouseState previousMouseState;

        public MapEditor(Map map, Camera2D camera, GUIManager guiManager)
        {
            this.map = map;
            this.camera = camera;
            this.guiManager = guiManager;
            previousMouseState = Mouse.GetState();
        }

        public void Update()
        {
            MouseState currentMouseState = Mouse.GetState();

            // Se o modo de edição estiver ativo, o botão esquerdo estiver pressionado,
            // o mouse estiver fora da área da GUI e não estivermos usando shift (para movimentar a câmera)
            if (guiManager.EditModeActive &&
                currentMouseState.LeftButton == ButtonState.Pressed &&
                currentMouseState.X > guiManager.SidebarWidth &&
                !Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                // Converte a posição do mouse (na tela) para as coordenadas do mundo, usando a matriz inversa da câmera
                Vector2 worldMousePosition = Vector2.Transform(
                    new Vector2(currentMouseState.X, currentMouseState.Y),
                    Matrix.Invert(camera.GetTransformation()));

                // Determina qual tile foi selecionado com base na posição do mouse
                int col = (int)(worldMousePosition.X / map.TileSize);
                int row = (int)(worldMousePosition.Y / map.TileSize);

                if (row >= 0 && row < map.Rows && col >= 0 && col < map.Columns)
                {
                    // Altera a cor do tile para a cor selecionada na GUI
                    map.Tiles[row, col].Color = guiManager.SelectedColor;
                }
            }

            // Atualiza o estado anterior do mouse para o próximo frame
            previousMouseState = currentMouseState;
        }
    }
    
}

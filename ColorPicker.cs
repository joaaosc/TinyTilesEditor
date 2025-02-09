using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TinyEditor
{
    /// <summary>
    /// Permite ao usuário selecionar uma cor através de uma paleta de cores.
    /// </summary>
    public class ColorPicker
    {
        private const int SliderWidth = 130;
        private const int SliderHeight = 10;
        private const int SliderHandleWidth = 8;
        private const int MaxRGB = 255;

        // Retângulos para     cada slider
        private Rectangle sliderRectangleR;
        private Rectangle sliderRectangleG;
        private Rectangle sliderRectangleB;

        // Posições (0..255) dos sliders
        private int valueR;
        private int valueG;
        private int valueB;

        // Posicionamento geral do ColorPicker
        private Rectangle backgroundRectangle;

        // Textura 1x1 e fonte para desenho
        private Texture2D pixel;
        private SpriteFont font;

        // Indica qual slider (se algum) está atualmente sendo arrastado
        private SliderType? currentDraggingSlider = null;

        private enum SliderType
        {
            R,
            G,
            B
        }

        public ColorPicker(Texture2D pixel, SpriteFont font, int x, int y, int width, int height)
        {
            this.pixel = pixel;
            this.font = font;

            // Exemplo: area de fundo (backgroundRectangle) do ColorPicker
            backgroundRectangle = new Rectangle(x, y, width, height);

            // Inicia valores padrão
            valueR = 255;
            valueG = 255;
            valueB = 255;

            // Define a posição de cada slider
            // Cada slider tem 10px de altura e 130px de largura
            sliderRectangleR = new Rectangle(x + 10, y + 30, SliderWidth, SliderHeight);
            sliderRectangleG = new Rectangle(x + 10, y + 60, SliderWidth, SliderHeight);
            sliderRectangleB = new Rectangle(x + 10, y + 90, SliderWidth, SliderHeight);
        }

        /// <summary>
        /// Retorna a cor resultante RGB escolhida no picker
        /// </summary>
        public Color SelectedColor => new Color(valueR, valueG, valueB);

        /// <summary>
        /// Atualiza a interação do mouse com os sliders 
        /// </summary>
        public void Update()
        {
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;
            bool leftButtonPressed = mouseState.LeftButton == ButtonState.Pressed;

            // Se o botão esquerdo do mouse foi solto, paramos de arrastar
            if (!leftButtonPressed)
            {
                currentDraggingSlider = null;
            }
            else
            {
                //Se não estamos arrastando nenhum slider, verifica se o mouse está sobre um deles
                if (currentDraggingSlider.HasValue)
                {
                    UpdateSliderValue(currentDraggingSlider.Value, mouseX);
                }
                else
                {
                    if (sliderRectangleR.Contains(mouseX, mouseY))
                    {
                        currentDraggingSlider = SliderType.R;
                        UpdateSliderValue(SliderType.R, mouseX);
                    }
                    else if (sliderRectangleG.Contains(mouseX, mouseY))
                    {
                        currentDraggingSlider = SliderType.G;
                        UpdateSliderValue(SliderType.G, mouseX);
                    }
                    else if (sliderRectangleB.Contains(mouseX, mouseY))
                    {
                        currentDraggingSlider = SliderType.B;
                        UpdateSliderValue(SliderType.B, mouseX);
                    }
                }
            }
        }

        /// <summary>
        /// Desenha o retângulo de fundo, os sliders, e o preview da cor final.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, backgroundRectangle, Color.DimGray);

            // Texto "Color Picker"
            spriteBatch.DrawString(font, "Color Picker", new Vector2(backgroundRectangle.X + 10, backgroundRectangle.Y + 10), Color.Black);

            // Desenha os sliders
            DrawSlider(spriteBatch, sliderRectangleR, Color.Red,valueR);
            DrawSlider(spriteBatch, sliderRectangleG, Color.Green,valueG);
            DrawSlider(spriteBatch, sliderRectangleB, Color.Blue,valueB);

            // Preview da cor
            Rectangle previewRectangle = new Rectangle(backgroundRectangle.X + 10, backgroundRectangle.Y + 130, 60, 60);
            spriteBatch.Draw(pixel, previewRectangle, SelectedColor);
            spriteBatch.DrawString(font, $"R:{valueR} | G: {valueG} | B {valueB}",
                new Vector2(previewRectangle.Right + 10,previewRectangle.Y+20),Color.Black);
        }

        /// <summary>
        /// Desenha um slider colorido, com um handle indicando o valor atual.
        /// </summary>
        private void DrawSlider(SpriteBatch spriteBatch, Rectangle rect, Color color, int value)
        {
            // Fundo do slider
            spriteBatch.Draw(pixel, rect, Color.LightGray);

            // Cor principal (gradiente simples da esquerda(0) pra direita(255) seria ideal, mas aqui faremos básico)
            // Para simplificar: desenharemos um retângulo do slider com a cor e opacidade
            // (Para um gradient real, precisaria de um procedimento extra)
            // Aqui, vamos apenas desenhar algo para ilustrar.
            spriteBatch.Draw(pixel, rect, color * 0.3f);

            // Desenha o "handle" (pequeno retângulo vertical)
            float pct = value / 255f;
            int handleX = rect.X + (int)(pct * rect.Width) - (SliderHandleWidth / 2);
            Rectangle handleRect = new Rectangle(handleX, rect.Y, SliderHandleWidth, rect.Height);
            spriteBatch.Draw(pixel, handleRect, Color.White);
        }

        /// <summary>
        ///  Atualiza o valor do slider (0..255) baseado na posição do mouseX dentro do sliderRect
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="mouseX"></param>
        private void UpdateSliderValue(SliderType slider, int mouseX)
        {
            Rectangle sliderRect = GetSliderRect(slider);
            // Posição relativa do mouse dentro do slider
            float relativeX = mouseX - sliderRect.X;
            // Convertendo para 0..255
            float t = MathHelper.Clamp(relativeX / sliderRect.Width, 0f, 1f);
            int newValue = (int)(t * MaxRGB);

            switch (slider)
            {
                case SliderType.R: valueR = newValue; break;
                case SliderType.G: valueG = newValue; break;
                case SliderType.B: valueB = newValue; break;
            }

        }

        private Rectangle GetSliderRect(SliderType slider)
        {
            return slider switch
            {
                SliderType.R => sliderRectangleR,
                SliderType.G => sliderRectangleG,
                SliderType.B => sliderRectangleB,
                _ => sliderRectangleR
            };
        }
    }
}
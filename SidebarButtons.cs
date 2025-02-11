using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TinyEditor
{
    /// <summary>
    /// Representa um botão simples.
    /// </summary>
    public class SimpleButton
    {
        public Rectangle Bounds;      // Área do botão na tela
        public string Text;           // Texto a ser exibido
        public Action OnClick;        // Ação a ser executada quando clicado

        // Cores e fonte (você pode personalizar ou alterar via construtor)
        public Color BackgroundColor = Color.Gray;
        public Color HoverColor = Color.DarkGray;
        public Color TextColor = Color.White;
        public SpriteFont Font;

        public SimpleButton(Rectangle bounds, string text, SpriteFont font, Action onClick)
        {
            Bounds = bounds;
            Text = text;
            Font = font;
            OnClick = onClick;
        }

        /// <summary>
        /// Atualiza o botão: verifica se o mouse está sobre ele e se foi clicado.
        /// </summary>
        public void Update(MouseState currentMouse, MouseState previousMouse)
        {
            if (Bounds.Contains(currentMouse.Position) &&
                currentMouse.LeftButton == ButtonState.Pressed &&
                previousMouse.LeftButton == ButtonState.Released)
            {
                OnClick?.Invoke();
            }
        }

        /// <summary>
        /// Desenha o botão usando a textura pixel (1x1) para preencher o fundo.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Determina a cor de fundo com base se o mouse está sobre o botão.
            Color bgColor = Bounds.Contains(Mouse.GetState().Position) ? HoverColor : BackgroundColor;
            spriteBatch.Draw(pixel, Bounds, bgColor);

            // Centraliza o texto dentro do botão.
            Vector2 textSize = Font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2);
            spriteBatch.DrawString(Font, Text, textPos, TextColor);
        }
    }

    /// <summary>
    /// Representa uma sidebar (painel) que gerencia uma coleção de botões.
    /// </summary>
    public class Sidebar
    {
        public Rectangle PanelArea;           // Área do painel na tela
        public List<SimpleButton> Buttons;    // Lista de botões que pertencem a este painel

        public Sidebar(Rectangle area)
        {
            PanelArea = area;
            Buttons = new List<SimpleButton>();
        }

        /// <summary>
        /// Adiciona um novo botão à sidebar.
        /// </summary>
        public void AddButton(SimpleButton button)
        {
            Buttons.Add(button);
        }

        /// <summary>
        /// Atualiza todos os botões da sidebar.
        /// </summary>
        public void Update(MouseState currentMouse, MouseState previousMouse)
        {
            foreach (var btn in Buttons)
            {
                btn.Update(currentMouse, previousMouse);
            }
        }

        /// <summary>
        /// Desenha a sidebar: primeiro desenha o fundo (opcional) e depois os botões.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Desenha um fundo para o painel (opcional)
            spriteBatch.Draw(pixel, PanelArea, Color.DarkSlateGray);

            // Desenha cada botão.
            foreach (var btn in Buttons)
            {
                btn.Draw(spriteBatch, pixel);
            }
        }
    }
}

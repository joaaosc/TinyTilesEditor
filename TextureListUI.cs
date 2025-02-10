using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TinyEditor
{


    public class TextureListUI
    {
        public List<TextureEntry> TextureEntries { get; private set; }

        /// <summary>
        /// Área do painel onde as miniaturas serão exibidas.
        /// </summary>
        public Rectangle PanelArea { get; set; }

        /// <summary>
        /// Tamanho da miniatura (largura e altura).
        /// </summary>
        public int ThumbnailSize { get; set; } = 64;

        /// <summary>
        /// Espaço entre as miniaturas.
        /// </summary>
        public int Margin { get; set; } = 5;

        public TextureListUI(Rectangle panelArea)
        {
            PanelArea = panelArea;
            TextureEntries = new List<TextureEntry>();
        }

        /// <summary>
        /// Adiciona uma nova textura à lista e atualiza as posições das miniaturas.
        /// </summary>
        public void AddTexture(TextureEntry entry)
        {
            TextureEntries.Add(entry);
            UpdateThumbnailPositions();
        }

        /// <summary>
        /// Atualiza a posição de cada miniatura para organizá-las no painel.
        /// </summary>
        private void UpdateThumbnailPositions()
        {
            // Exemplo: disposição vertical
            int x = PanelArea.X + Margin;
            int y = PanelArea.Y + Margin;

            foreach (var entry in TextureEntries)
            {
                entry.ThumbnailRectangle = new Rectangle(x, y, ThumbnailSize, ThumbnailSize);
                y += ThumbnailSize + Margin;
            }
        }

        /// <summary>
        /// Desenha o painel e as miniaturas.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font = null)
        {
            // Opcional: desenha o fundo do painel (por exemplo, cinza)
            spriteBatch.Draw(pixel, PanelArea, Color.DarkGray);

            foreach (var entry in TextureEntries)
            {
                // Desenha a miniatura da textura
                spriteBatch.Draw(entry.Texture, entry.ThumbnailRectangle, Color.White);

                // Opcional: desenha o nome da textura abaixo da miniatura
                if (font != null)
                {
                    Vector2 textPosition = new Vector2(entry.ThumbnailRectangle.X, entry.ThumbnailRectangle.Bottom + 2);
                    spriteBatch.DrawString(font, entry.Name, textPosition, Color.White);
                }
            }
        }

        /// <summary>
        /// Retorna a entrada de textura cujo retângulo contém o ponto informado, ou null se nenhum.
        /// </summary>
        public TextureEntry GetTextureEntryAtPoint(Point mousePoint)
        {
            foreach (var entry in TextureEntries)
            {
                if (entry.ThumbnailRectangle.Contains(mousePoint))
                {
                    return entry;
                }
            }

            return null;
        }

        public Texture2D GetTextureByID(string id)
        {
            foreach (var entry in TextureEntries)
            {
                if (entry.Name == id)
                {
                    return entry.Texture;
                }
            }

            return null;
        }

    }
}
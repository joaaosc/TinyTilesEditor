using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using static TinyEditor.Tile;

namespace TinyEditor
{
    public class TileTypeEditForm : Form
    {
        public ComboBox cmbTextureNames;
        public ComboBox cmbTileTypes;
        public Button btnOK;
        public Button btnCancel;

        public TileTypeEditForm(List<string> textureNames)
        {
            this.Text = "TileType Edit";
            this.Width = 300;
            this.Height = 170;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Label e ComboBox para Textura
            Label lblTexture = new Label();
            lblTexture.Text = "Texture:";
            lblTexture.Location = new System.Drawing.Point(10, 15);
            lblTexture.AutoSize = true;
            this.Controls.Add(lblTexture);

            cmbTextureNames = new ComboBox();
            cmbTextureNames.Location = new System.Drawing.Point(80, 10);
            cmbTextureNames.Width = 180;
            cmbTextureNames.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTextureNames.Items.AddRange(textureNames.ToArray());
            if (cmbTextureNames.Items.Count > 0)
                cmbTextureNames.SelectedIndex = 0;
            this.Controls.Add(cmbTextureNames);

            // Label e ComboBox para TileType
            Label lblType = new Label();
            lblType.Text = "TileType:";
            lblType.Location = new System.Drawing.Point(10, 55);
            lblType.AutoSize = true;
            this.Controls.Add(lblType);

            cmbTileTypes = new ComboBox();
            cmbTileTypes.Location = new System.Drawing.Point(80, 50);
            cmbTileTypes.Width = 180;
            cmbTileTypes.DropDownStyle = ComboBoxStyle.DropDownList;
            // Preenche com os nomes do enum
            cmbTileTypes.Items.AddRange(Enum.GetNames(typeof(TileTypeEnum)));
            cmbTileTypes.SelectedIndex = 0;
            this.Controls.Add(cmbTileTypes);

            // Botão OK
            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Location = new System.Drawing.Point(50, 90);
            btnOK.DialogResult = DialogResult.OK;
            this.Controls.Add(btnOK);

            // Botão Cancel
            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(150, 90);
            btnCancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
    }
}

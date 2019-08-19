using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX.Direct3D11;
using System;
using System.Windows.Forms;
using DungeonHack.DirectX;

namespace FunAndGamesWithSharpDX.Menus
{
    public partial class OptionsWindow : Form
    {
        private readonly Renderer _renderer;
        private readonly Camera _camera;
        private readonly LightShader _shader;

        public OptionsWindow(Renderer renderer, LightShader shader, DungeonHack.Engine.Engine.FrameRateStats frameRateStats, Camera camera, int MouseSensitivity)
        {
            InitializeComponent();
            _renderer = renderer;
            valFps.Text = string.Format("{0} fps", frameRateStats.FramesPerSecond);

            chkMSA.Enabled = _renderer.Check4XMSAAQualitySupport();
            comboBox1.Items.Add("LightTech");
            comboBox1.Items.Add("TextureTech");

            _camera = camera;
            _shader = shader;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();

            ApplicationStateEngine.CurrentState = ApplicationStateEnum.Normal;
        }

        private void chkMSA_CheckedChanged(object sender, EventArgs e)
        {
            var chkBox = (CheckBox)sender;

            if (chkBox.Checked)
            {
                if (!_renderer.Check4XMSAAQualitySupport())
                    MessageBox.Show("The current device does not support 4XMSAA");
            }
        }

        private void OptionsWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            ApplicationStateEngine.CurrentState = ApplicationStateEnum.Normal;
        }

        private void chkWireframe_CheckedChanged(object sender, EventArgs e)
        {
            var chkBox = (CheckBox)sender;

            if (chkBox.Checked)
            {
                _renderer.SetRasterizerState(FillMode.Wireframe, CullMode.Back);
            }
            else
            {
                _renderer.SetRasterizerState(FillMode.Solid, CullMode.Back);
            }
        }

        private void chkRestrictMovementXZ_CheckedChanged(object sender, EventArgs e)
        {
            var chkBox = (CheckBox)sender;

            if (chkBox.Checked)
            {
                _camera.RestrictMovementPlaneXZ = true;
            }
            else
            {
                _camera.RestrictMovementPlaneXZ = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;

            //_shader.SetSelectedShaderEffect(_renderer.Device, comboBox.SelectedItem.ToString());
        }
    }
}
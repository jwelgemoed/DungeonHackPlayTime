using DungeonHack.Properties;
using SharpDX;

namespace FunAndGamesWithSharpDX.Engine
{
    public static class ConfigManager
    {
        public static bool FullScreen
        {
            get { return Settings.Default.FullScreen; }
        }

        public static int ScreenWidth
        {
            get { return Settings.Default.ScreenWidth; }
        }

        public static int ScreenHeight
        {
            get { return Settings.Default.ScreenHeight; }
        }

        public static float ScreenNear
        {
            get { return Settings.Default.ScreenNear; }
        }

        public static float ScreenFar
        {
            get { return Settings.Default.ScreenFar; }
        }

        public static bool Use4XMSAA
        {
            get { return Settings.Default.User4XMSAA; }
        }

        public static bool MultiSampleEnabled
        {
            get { return Settings.Default.MultiSampleEnabled; }
        }

        public static bool AntiAliasedEnabled
        {
            get { return Settings.Default.AntiAliasedEnabled; }
        }

        public static int MouseSensitivity
        {
            get { return Settings.Default.MouseSensitivity; }
        }

        public static bool FrustrumCullingEnabled
        {
            get { return Settings.Default.FrustrumCullingEnabled; }
        }

        public static string ResourcePath
        {
            get { return Settings.Default.ResourcePath; }
        }

        public static bool WallClipEnabled
        {
            get { return Settings.Default.WallClip; }
        }

        public static float Acceleration
        {
            get { return Settings.Default.Acceleration; }
        }

        public static float DeAcceleration
        {
            get { return Settings.Default.DeAcceleration; }
        }

        public static float MaxAcceleration
        {
            get { return Settings.Default.MaxAcceleration; }
        }

        public static float SidewaysAcceleration
        {
            get { return Settings.Default.SidewaysAcceleration; }
        }

        public static float MaxSidewaysAcceleration
        {
            get { return Settings.Default.MaxSidewaysAcceleration; }
        }

        public static float UpAcceleration
        {
            get { return Settings.Default.UpAcceleration; }
        }

        public static float MaxUpAcceleration
        {
            get { return Settings.Default.MaxUpAcceleration; }
        }

        public static float DownAcceleration
        {
            get { return Settings.Default.DownAcceleration; }
        }

        public static float MaxDownAcceleration
        {
            get { return Settings.Default.MaxDownAcceleration; }
        }

        public static bool Topdown
        {
            get { return Settings.Default.Topdown; }
        }

        public static float ZoomInAcceleration
        {
            get { return Settings.Default.TopdownInZoomAcceleration; }
        }

        public static float ZoomOutAcceleration
        {
            get { return Settings.Default.TopdownOutZoomAcceleration; }
        }

        public static float MaxZoomAcceleration
        {
            get { return Settings.Default.TopdownMaxZoomAcceleration; }
        }

        public static float SpotLightRange { get; set; }

        public static float SpotLightFactor { get; set; }

        public static float SpotLightAttentuationA { get; set; }

        public static float SpotLightAttentuationB { get; set; }

        public static float SpotLightAttentuationC { get; set; }
    }
}
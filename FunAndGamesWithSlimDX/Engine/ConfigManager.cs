using DungeonHack.Properties;

namespace DungeonHack.Engine
{
    public static class ConfigManager
    {
        public static bool FullScreen => Settings.Default.FullScreen;

        public static int ScreenWidth => Settings.Default.ScreenWidth;

        public static int ScreenHeight => Settings.Default.ScreenHeight;

        public static float ScreenNear => Settings.Default.ScreenNear;

        public static float ScreenFar => Settings.Default.ScreenFar;

        public static bool Use4XMSAA => Settings.Default.User4XMSAA;

        public static bool MultiSampleEnabled => Settings.Default.MultiSampleEnabled;

        public static bool AntiAliasedEnabled => Settings.Default.AntiAliasedEnabled;

        public static int MouseSensitivity => Settings.Default.MouseSensitivity;

        public static bool FrustrumCullingEnabled => Settings.Default.FrustrumCullingEnabled;

        public static string ResourcePath => Settings.Default.ResourcePath;

        public static bool WallClipEnabled => Settings.Default.WallClip;

        public static float Acceleration => Settings.Default.Acceleration;

        public static float DeAcceleration => Settings.Default.DeAcceleration;

        public static float MaxAcceleration => Settings.Default.MaxAcceleration;

        public static float SidewaysAcceleration => Settings.Default.SidewaysAcceleration;

        public static float MaxSidewaysAcceleration => Settings.Default.MaxSidewaysAcceleration;

        public static float UpAcceleration => Settings.Default.UpAcceleration;

        public static float MaxUpAcceleration => Settings.Default.MaxUpAcceleration;

        public static float DownAcceleration => Settings.Default.DownAcceleration;

        public static float MaxDownAcceleration => Settings.Default.MaxDownAcceleration;

        public static bool Topdown => Settings.Default.Topdown;

        public static float ZoomInAcceleration => Settings.Default.TopdownInZoomAcceleration;

        public static float ZoomOutAcceleration => Settings.Default.TopdownOutZoomAcceleration;

        public static float MaxZoomAcceleration => Settings.Default.TopdownMaxZoomAcceleration;

        public static int VSync => Settings.Default.VSync;

        public static float SpotLightRange { get; set; }

        public static float SpotLightFactor { get; set; }

        public static float SpotLightAttentuationA { get; set; }

        public static float SpotLightAttentuationB { get; set; }

        public static float SpotLightAttentuationC { get; set; }

        public static bool SpotlightOn { get; set; }
        public static float FogStart { get; set; }
        public static float FogEnd { get; set; }

        public static int UseNormalMap { get; set; }
    }
}
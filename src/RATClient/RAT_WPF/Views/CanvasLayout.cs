namespace RAT_WPF.Views
{
    //KI start (Claude Opus 4.8, prompt 28): the topology canvas is a large fixed-size area you can pan around with
    // the scroll bars (a true infinite canvas would need virtualised scrolling). Device positions are clamped to
    // this area so a node can never land in unreachable "void" outside the scrollable region.
    public static class CanvasLayout
    {
        public const double Width = 6000;
        public const double Height = 4000;
        public const double NodeWidth = 90;
        public const double NodeHeight = 120;

        /// <summary>Clamp an X so the whole node stays inside the canvas.</summary>
        public static int ClampX(double x)
        {
            double max = Width - NodeWidth;
            if (x < 0) { x = 0; } else if (x > max) { x = max; }
            return (int)x;
        }

        /// <summary>Clamp a Y so the whole node stays inside the canvas.</summary>
        public static int ClampY(double y)
        {
            double max = Height - NodeHeight;
            if (y < 0) { y = 0; } else if (y > max) { y = max; }
            return (int)y;
        }
    }
    //KI end
}

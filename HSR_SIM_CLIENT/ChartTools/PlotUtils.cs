using ScottPlot.Control;

namespace HSR_SIM_CLIENT.ChartTools;

public static class PlotUtils
{
    public static PlotActions NoWheelZoom()
    {
        return new PlotActions()
        {
            ZoomIn = delegate { },
            ZoomOut = delegate { },
            PanUp = StandardActions.PanUp,
            PanDown = StandardActions.PanDown,
            PanLeft = StandardActions.PanLeft,
            PanRight = StandardActions.PanRight,
            DragPan = StandardActions.DragPan,
            DragZoom = StandardActions.DragZoom,
            DragZoomRectangle = StandardActions.DragZoomRectangle,
            ZoomRectangleClear = StandardActions.ZoomRectangleClear,
            ZoomRectangleApply = StandardActions.ZoomRectangleApply,
            ToggleBenchmark = StandardActions.ToggleBenchmark,
            AutoScale = StandardActions.AutoScale,
            ShowContextMenu = StandardActions.ShowContextMenu,
        };
    }

}
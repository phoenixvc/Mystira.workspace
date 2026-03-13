namespace Mystira.App.PWA.Components;

/// <summary>
/// Configurable frame styles for the hero portal visual.
/// Each maps to a separate SVG file in wwwroot/images/frames/.
/// </summary>
public enum PortalFrameStyle
{
    /// <summary>
    /// Original inline SVG frame (backward-compatible default).
    /// </summary>
    Classic,

    /// <summary>
    /// Engraved acanthus leaf fans at corners with double-line border.
    /// Heaviest visual weight — museum artifact feel.
    /// </summary>
    OrnateLeaf,

    /// <summary>
    /// Stylized flower ornaments at all 8 anchor points (4 corners + 4 mid-edges).
    /// Organic/magical feel with moderate weight.
    /// </summary>
    FloralCorner,

    /// <summary>
    /// Scalloped edges with small leaf ornaments at corners.
    /// Lightest weight — storybook-friendly and approachable.
    /// </summary>
    HandDrawn
}

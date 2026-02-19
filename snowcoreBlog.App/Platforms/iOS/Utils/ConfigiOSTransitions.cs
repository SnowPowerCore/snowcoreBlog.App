using CustomShellMaui.Enum;

namespace CustomShellMaui.Platforms.iOS;

public class ConfigiOSTransitions
{
    public ConfigiOS AnimationIn { get; set; } = new();

    public ConfigiOS AnimationOut { get; set; } = new();

    public TransitionPageType AbovePage { get; set; }
}
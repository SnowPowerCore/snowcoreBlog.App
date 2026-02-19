using CustomShellMaui.Enum;

namespace CustomShellMaui.Models;

public class TransitionRoot : Transition
{
    public TransitionPageType AbovePage { get; set; } = TransitionPageType.CurrentPage;
}
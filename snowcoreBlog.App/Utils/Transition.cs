using CustomShellMaui.Enum;

namespace CustomShellMaui.Models;

public class Transition
{
    public TransitionType CurrentPage { get; set; } = TransitionType.None;
    
    public TransitionType NextPage { get; set; } = TransitionType.None;

#if ANDROID
    //ResourcesAndroid xml animation
    public int DurationAndroid { get; set; } = 500;

    public int CurrentPageAndroid { get; set; }

    public int NextPageAndroid { get; set; }

#elif IOS
    public Platforms.iOS.ConfigiOS CurrentPageiOS { get; set; }

    public Platforms.iOS.ConfigiOS NextPageiOS { get; set; }
#endif
}
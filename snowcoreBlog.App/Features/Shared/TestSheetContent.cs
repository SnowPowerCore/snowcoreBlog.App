namespace snowcoreBlog.App.Features.Shared;

public partial class TestSheetContent : Component
{
    protected override void OnMounted()
    {
        base.OnMounted();
    }

    protected override void OnWillUnmount()
    {
        base.OnWillUnmount();
    }

    public override VisualNode Render() =>
        ScrollView(
            VStack(
                Label("Test Content").Center()
            )
        );
}
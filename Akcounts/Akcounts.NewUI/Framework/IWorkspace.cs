namespace Akcounts.NewUI.Framework
{
    public interface IWorkspace
    {
        string Label { get; }
        string Icon { get; }
        string Status { get; }

        void Show();
    }
}

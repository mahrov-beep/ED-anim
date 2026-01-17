namespace Game.ECS.Scripts {
    public interface IOpenableView {
        void Open(bool instant = false);
        void Close(bool instant = false);
    }
}
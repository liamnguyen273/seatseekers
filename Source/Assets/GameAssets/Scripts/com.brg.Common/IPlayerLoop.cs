namespace com.brg.Common
{
    public interface IPlayerLoop
    {
        public void Initialize();
        public void Update(float dt);
        public void OnFinalize();
    }
}

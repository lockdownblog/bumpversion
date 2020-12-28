namespace BumpVersion.Utils
{
    public interface IRepo
    {
        bool IsClean { get; }

        void Commit(string message, params string[] files);

        void Tag(string tag, string tagMessage);

        bool SetUpRepo();
    }
}
namespace BumpVersion.Utils
{
    using System;
    using LibGit2Sharp;

    public class Repo : IRepo
    {
        private Repository repo;
        private Signature author;

        public bool IsClean
        {
            get { return !this.repo.RetrieveStatus().IsDirty; }
        }

        public bool SetUpRepo()
        {
            try
            {
                this.repo = new Repository(".");
                this.author = this.repo.Config.BuildSignature(DateTimeOffset.Now);
            }
            catch (RepositoryNotFoundException)
            {
                return false;
            }

            return true;
        }

        public void Commit(string message, params string[] files)
        {
            foreach (string file in files)
            {
                Commands.Stage(this.repo, file);
            }

            this.repo.Commit(message, this.author, this.author);
        }

        public void Tag(string tag, string tagMessage)
        {
            this.repo.ApplyTag(tag, this.author, tagMessage);
        }

    }
}

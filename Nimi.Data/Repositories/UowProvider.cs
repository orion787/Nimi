using Nimi.Data.Repositories.Helpers;

namespace Nimi.Data.Repositories
{
    public static class UowProvider
    {
        public static UnitOfWork? Instance { get; private set; } = null;

        private static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new UnitOfWork(Helper.DbPath);
            }
        }

        public static UnitOfWork GetInstance()
        {
            if (Instance == null)
                Initialize();
            return Instance;
        }
    }

}

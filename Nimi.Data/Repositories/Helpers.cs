namespace Nimi.Data.Repositories.Helpers
{
    internal static class SolutionPathHelper
    {
        public static string GetSolutionDirectory()
        {
            // Начинаем с директории приложения
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            // Поднимаемся вверх по иерархии папок, пока не найдем файл .sln
            while (directory != null && !DirectoryContainsSolution(directory))
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException("Solution directory not found!");
        }

        private static bool DirectoryContainsSolution(DirectoryInfo directory)
        {
            return directory.GetFiles("*.sln").Length > 0;
        }
    }

    public static class Helper
    {
        public static readonly string _solutionPath = SolutionPathHelper.GetSolutionDirectory();
        public static string DbPath = Path.Combine(_solutionPath, "Resources", "partners.db");
    }
}

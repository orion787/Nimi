using Nimi.Data.Tools;
using Nimi.Data.Models;
using Nimi.Data.Repositories;
using Nimi.Data.Repositories.Helpers;
namespace Nimi.UI;

public static class ImportInitializer
{
    private static readonly string ImportFlagFile = "import.done";

    public static void RunOnce(UnitOfWork uow)
    {
        var _solutionPath = Helper._solutionPath;
        var tablePath = Path.Combine(_solutionPath, "Resources", "Partners_import.xlsx");

        if (File.Exists(ImportFlagFile))
            return;

        var mapping = new Dictionary<string, string>
        {
            ["Тип партнера"] = "Type",
            ["Наименование партнера"] = "Name",
            ["Директор"] = "Director",
            ["Электронная почта партнера"] = "Email",
            ["Телефон партнера"] = "Phone",
            ["Рейтинг"] = "Rating"
        };

        ExcelImporter.Import<Partner>(
            tablePath,
            uow.Partners,
            mapping
        );

        uow.Save();

        File.WriteAllText(ImportFlagFile, DateTime.Now.ToString("u"));
    }
}

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var _solutionPath = Helper._solutionPath;
        var dbPath = Path.Combine(_solutionPath, "Resources", "partners.db");

        var uow = new UnitOfWork(dbPath);
        ImportInitializer.RunOnce(uow);
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new CardForm());
    }    
}

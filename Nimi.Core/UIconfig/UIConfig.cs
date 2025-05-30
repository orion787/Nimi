using Nimi.Core.Entities;

namespace Nimi.Core.UIconfig
{
    public record SpecialWidgetConfig(string WidgetType, List<string>? Options);

    public record FontSettings(string Family = "Arial", int Size = 12);

    public class UIConfig
    {
        public List<string> ExcludedFields { get; } = new List<string> { "Id" };
        public Dictionary<string, string> FieldLabels { get; } = new();
        public Dictionary<string, string> FieldSuffixes { get; } = new();
        public Dictionary<string, SpecialWidgetConfig> SpecialWidgets { get; } = new();
        public Dictionary<int, List<string>> Layout { get; } = new();
        public Type? DisplayedEntityType { get; set; }
        public Dictionary<Type, string> EntityDisplayNames { get; } = new();
        public bool EnableEdit { get; set; } = true;
        public bool EnableHistory { get; set; } = false;

        public void RegisterEntity(Type entityType, string displayName)
        {
            if (!entityType.IsSubclassOf(typeof(EntityBase)))
                throw new ArgumentException("Тип должен наследоваться от EntityBase");

            EntityDisplayNames[entityType] = displayName;
        }

        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string CardBackground { get; set; } = "#FFFFFF";
        public string Alignment { get; set; } = "Left";
        public int ElementSpacing { get; set; } = 5;
        public int CardSpacing { get; set; } = 5;
        public int InterElementSpacing { get; set; } = 40;
        public FontSettings Font { get; set; } = new() { Family = "Arial", Size = 12 };

        public UIConfig(Type modelType)
        {
            var properties = modelType.GetProperties()
                .Where(p => !ExcludedFields.Contains(p.Name))
                .Select(p => p.Name);

            // Default layout: one field per row
            int row = 1;
            foreach (var prop in properties)
            {
                Layout[row] = new List<string> { prop };
                row++;
            }
        }

        public void AddRow(int rowNumber, params string[] fields)
        {
            if (!Layout.ContainsKey(rowNumber))
                Layout[rowNumber] = new List<string>();

            Layout[rowNumber].AddRange(fields);
        }

        public void SetFieldLabel(string field, string label)
            => FieldLabels[field] = label;

        public void SetFieldSuffix(string field, string suffix)
            => FieldSuffixes[field] = suffix;

        public void SetSpecialWidget(string field, string widgetType, List<string>? options = null)
            => SpecialWidgets[field] = new SpecialWidgetConfig(widgetType, options);
    }


    public static class Colors
    {
        public const string White = "#FFFFFF";
        public const string LightGrey = "#F0F0F0";
        public const string LightBlue = "#B9D7F1";
        public const string Seashell = "#FFF5EE";
        public const string Peach = "#FDD9B5";
        public const string Smoke = "#F0F0F0";
        public const string Gray = "#B5B8B1";
    }

}

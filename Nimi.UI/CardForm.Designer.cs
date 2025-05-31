using Nimi.Core.Attributes;
using Nimi.Core.UIconfig;
using Nimi.Data.Repositories;
using Nimi.Data.Repositories.Helpers;

namespace Nimi.UI
{
    public partial class CardForm : Form
    {
        private readonly UnitOfWork _uow;
        private readonly UIConfig _ui;
        private readonly Panel _topPanel;
        private readonly PictureBox _logoBox;
        private readonly Button _btnAdd;
        private readonly Panel _mainContainer;
        private readonly Panel _cardsContainer;
        private readonly string _solutionPath = Helper._solutionPath;

        public CardForm()
        {
           _uow = UowProvider.GetInstance();

            // === Настройка формы ===
            Text = "Учёт партнёров";
            ClientSize = new Size(900, 600);
            BackColor = ColorTranslator.FromHtml(Colors.LightBlue);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;

            // === 1) UoW и UIConfig ===
            _ui = new UIConfig(typeof(Nimi.Data.Models.Partner))
            {
                EnableEdit = true,
                EnableHistory = true
            };

            // === 2) Чистим Layout и задаём только свои строки ===
            _ui.Layout.Clear();
            _ui.EnableHistory = false;
            _ui.AddRow(1, "Type", "Name", "Discount");
            _ui.AddRow(2, "Phone");
            _ui.AddRow(3, "Director", "Email");
            _ui.AddRow(4, "Rating");

            // === 3) Подписи/виджеты/суффиксы ===
            _ui.SetFieldLabel("Type", "Тип");
            _ui.SetFieldLabel("Name", "Партнёр");
            _ui.SetFieldLabel("Discount", "Скидка");
            _ui.SetFieldSuffix("Discount", "%");
            _ui.SetSpecialWidget("Discount", "currency");

            _ui.SetFieldLabel("Phone", "Телефон");
            _ui.SetSpecialWidget("Phone", "phone");

            _ui.SetFieldLabel("Director", "Директор");
            _ui.SetFieldLabel("Rating", "Рейтинг");

            _ui.BackgroundColor = Colors.LightBlue;
            _ui.CardBackground = Colors.Seashell;

            _ui.CardSpacing = 10;

            // === 4) Верхняя панель ===
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = this.BackColor,
                Height = 150
            };

            // — логотип —
            _logoBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(100, 100),
                BackColor = Color.Gray,
                Anchor = AnchorStyles.Top
            };
            LoadLogoImage();
            _topPanel.Controls.Add(_logoBox);

            // — кнопка «Добавить партнёра» —
            _btnAdd = new Button
            {
                Text = "➕ Добавить партнёра",
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            _btnAdd.Click += (s, e) => {
                var modelType = _uow.Partners.GetType().GetGenericArguments()[0];
                var instance = Activator.CreateInstance(modelType)!;
                using var dlg = new EditForm(_uow, instance, _ui);
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadEntities();
            };
            _topPanel.Controls.Add(_btnAdd);

            Controls.Add(_topPanel);

            // === 5) Основной контейнер с прокруткой ===
            _mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColorTranslator.FromHtml(_ui.BackgroundColor)
            };
            Controls.Add(_mainContainer);

            // === 6) Внутренний контейнер для карточек ===
            _cardsContainer = new Panel
            {
                Location = new Point(0, 150),
                Width = ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                AutoSize = true
            };
            _mainContainer.Controls.Add(_cardsContainer);

            // Обработчики событий
            Load += (s, e) =>
            {
                LayoutTopPanel();
                LoadEntities();
            };

            Resize += (s, e) =>
            {
                _cardsContainer.Width = _mainContainer.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                LoadEntities();
            };
        }

        private void LayoutTopPanel()
        {
            _logoBox.Location = new Point(
                (_topPanel.ClientSize.Width - _logoBox.Width) / 2,
                10
            );
            _btnAdd.Location = new Point(
                (_topPanel.ClientSize.Width - _btnAdd.Width) / 2,
                _logoBox.Bottom + 10
            );
        }

        // Отрисовка карточек
        private void LoadEntities()
        {
            _cardsContainer.SuspendLayout();
            _cardsContainer.Controls.Clear();

            int cardWidth = _mainContainer.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 5;
            int yPos = _ui.CardSpacing;
            var layoutRows = _ui.Layout.OrderBy(kv => kv.Key).ToList();

            foreach (var entity in _uow.Partners.GetAll())
            {
                int cardHeight = CalculateCardHeight(layoutRows);

                var card = new Panel
                {
                    Width = cardWidth,
                    Height = cardHeight,
                    Location = new Point(_ui.CardSpacing, yPos),
                    BackColor = ColorTranslator.FromHtml(_ui.CardBackground),
                    Padding = new Padding(_ui.ElementSpacing)
                };

                int y = 0;
                foreach (var row in layoutRows)
                {
                    var fields = row.Value;
                    int cols = fields.Count;
                    int w = (cardWidth - _ui.ElementSpacing * (cols - 1)) / cols;

                    for (int i = 0; i < cols; i++)
                    {
                        var field = fields[i];
                        var prop = entity.GetType().GetProperty(field);
                        if (prop == null || prop.IsDefined(typeof(HiddenAttribute), true))
                            continue;

                        var raw = prop.GetValue(entity);
                        if (_ui.SpecialWidgets.TryGetValue(field, out var cfg))
                            raw = FormatSpecial(cfg.WidgetType, raw);

                        var label = _ui.FieldLabels.GetValueOrDefault(field, field);
                        var suffix = _ui.FieldSuffixes.GetValueOrDefault(field, "");

                        var lbl = new Label
                        {
                            Text = $"{label}: {raw}{suffix}",
                            AutoSize = false,
                            Location = new Point(i * (w + _ui.ElementSpacing), y),
                            Width = w,
                            Height = IncrementHeight(_ui.Font.Size),
                            Font = new Font(_ui.Font.Family, _ui.Font.Size),
                            BackColor = card.BackColor
                        };
                        card.Controls.Add(lbl);
                    }
                    y += IncrementHeight(_ui.Font.Size) + _ui.ElementSpacing;
                }

                if (_ui.EnableEdit)
                {
                    var btn = new Button
                    {
                        Text = "✏️",
                        Size = new Size(30, 30),
                        Location = new Point(cardWidth - 35, y)
                    };
                    btn.Click += (s, e) => {
                        using var dlg = new EditForm(_uow, entity, _ui);
                        if (dlg.ShowDialog() == DialogResult.OK)
                            LoadEntities();
                    };
                    card.Controls.Add(btn);
                }
                if (_ui.EnableHistory)
                {
                    var btn = new Button
                    {
                        Text = "📜",
                        Size = new Size(30, 30),
                        Location = new Point(cardWidth - 70, y)
                    };
                    card.Controls.Add(btn);
                    /*ПРИ НЕОБХОДИМОСТИ МОЖНО ДОБАВИТЬ ДОБАВИТЬ ЕЩЁ ОДНУ ФОРМУ*/
                }

                _cardsContainer.Controls.Add(card);
                yPos += cardHeight + _ui.CardSpacing;
            }

            _cardsContainer.Height = yPos;
            _cardsContainer.Width = cardWidth - 10;

        }

        private int CalculateCardHeight(List<KeyValuePair<int, List<string>>> layoutRows)
        {
            int lineHeight = IncrementHeight(_ui.Font.Size);
            int totalLines = layoutRows.Count;
            int contentHeight = totalLines * (lineHeight + _ui.ElementSpacing);
            int buttonsHeight = (_ui.EnableEdit || _ui.EnableHistory) ? 40 : 0;

            return contentHeight + buttonsHeight + _ui.ElementSpacing * 2;
        }

        private int IncrementHeight(int fontSize) => fontSize + 4;

        private object FormatSpecial(string widgetType, object? value)
        {
            if (widgetType == "phone")
                return FormatPhone(value?.ToString() ?? "");
            if (widgetType == "currency" && decimal.TryParse(value?.ToString(), out var m))
                return m.ToString("N2");
            return value ?? "";
        }

        private string FormatPhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("7") && digits.Length == 11)
                digits = digits[1..];
            return digits.Length == 10
                ? $"+7 {digits[..3]} {digits[3..6]} {digits[6..8]} {digits[8..]}"
                : phone;
        }
        

        private void LoadLogoImage()
        {
            string logoPath = Path.Combine(_solutionPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                try
                {
                    using var img = Image.FromFile(logoPath);
                    _logoBox.Image = new Bitmap(img, _logoBox.Size);
                }
                catch
                {
                }
            }
        }
    }
}

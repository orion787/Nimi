using Microsoft.EntityFrameworkCore;
using Nimi.Core.UIconfig;
using Nimi.Data.Repositories;

namespace Nimi.UI
{
    /// <summary>
    /// Форма добавления/редактирования любой сущности согласно UIConfig.
    /// </summary>
    public class EditForm : Form
    {
        private readonly UnitOfWork _uow;
        private readonly object _entity;
        private readonly UIConfig _ui;
        private readonly TableLayoutPanel _table;

        public EditForm(UnitOfWork uow, object entity, UIConfig uiConfig)
        {
            if (uow is null) throw new ArgumentNullException(nameof(uow));
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (uiConfig is null) throw new ArgumentNullException(nameof(uiConfig));

            _uow = uow;
            _entity = entity;
            _ui = uiConfig;

            Text = (_entity.GetType().Name, GetId()) switch
            {
                (_, null) => "Добавление",
                (_, not null) => "Редактирование"
            };

            // Настраиваем саму форму
            BackColor = ColorTranslator.FromHtml(Colors.White);
            Width = 500;
            Height = 400;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;

            // Таблица для лейаут-формы
            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(10),
                BackColor = this.BackColor
            };
            Controls.Add(_table);

            BuildForm();

            // Кнопка Сохранить
            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnSave.Click += (s, e) => Save();
            Controls.Add(btnSave);

            // Заполнить поля, если редактирование
            if (GetId() != null)
                LoadData();
        }

        private int? GetId()
        {
            var prop = _entity.GetType().GetProperty("Id");
            return (int?)(prop?.GetValue(_entity));
        }

        private void BuildForm()
        {
            // Для каждой строки из UIConfig создаём ряды TableLayout
            int rowIndex = 0;
            foreach (var kv in _ui.Layout.OrderBy(kv => kv.Key))
            {
                foreach (var field in kv.Value)
                {
                    // Надпись
                    var lbl = new Label
                    {
                        Text = _ui.FieldLabels.GetValueOrDefault(field, field),
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = true,
                        BackColor = this.BackColor
                    };
                    _table.Controls.Add(lbl, 0, rowIndex);

                    // Поле ввода
                    Control ctrl;
                    if (_ui.SpecialWidgets.TryGetValue(field, out var cfg)
                        && cfg.WidgetType == "combobox")
                    {
                        var cb = new ComboBox { Dock = DockStyle.Fill };
                        if (cfg.Options != null)
                            cb.Items.AddRange(cfg.Options.ToArray());
                        ctrl = cb;
                    }
                    else
                    {
                        var tb = new TextBox { Dock = DockStyle.Fill };
                        if (_ui.SpecialWidgets.TryGetValue(field, out cfg))
                        {
                            if (cfg.WidgetType == "phone")
                                tb.KeyPress += (_, e) => {
                                    if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                                        e.Handled = true;
                                };
                            if (cfg.WidgetType == "currency")
                                tb.KeyPress += (_, e) => {
                                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
                                        e.Handled = true;
                                };
                        }
                        ctrl = tb;
                    }
                    ctrl.Name = "fld_" + field;
                    _table.Controls.Add(ctrl, 1, rowIndex);

                    rowIndex++;
                }
            }

            // Увеличиваем число строк в таблице
            _table.RowCount = rowIndex;
        }

        private void LoadData()
        {
            foreach (Control c in _table.Controls)
            {
                if (!(c is TextBox or ComboBox)) continue;
                var field = c.Name.Replace("fld_", "");
                var prop = _entity.GetType().GetProperty(field);
                if (prop == null) continue;
                var val = prop.GetValue(_entity)?.ToString() ?? "";
                switch (c)
                {
                    case ComboBox cb: cb.Text = val; break;
                    case TextBox tb: tb.Text = val; break;
                }
            }
        }

        private void Save()
        {
            try
            {
                // 1) Пробегаем по всем полям и записываем в _entity
                foreach (Control c in _table.Controls)
                {
                    if (c is not TextBox and not ComboBox) continue;
                    var field = c.Name.Replace("fld_", "");
                    var prop = _entity.GetType().GetProperty(field);
                    if (prop == null || prop.SetMethod == null) continue;

                    string txt = c is ComboBox cb ? cb.Text : ((TextBox)c).Text;
                    if (string.IsNullOrWhiteSpace(txt))
                        throw new Exception($"Поле '{field}' не может быть пустым");

                    object? value;
                    if (_ui.SpecialWidgets.TryGetValue(field, out var cfg))
                    {
                        value = cfg.WidgetType switch
                        {
                            "currency" => decimal.Parse(txt),
                            "phone" => new string(txt.Where(char.IsDigit).ToArray()),
                            _ => txt
                        };
                    }
                    else
                    {
                        value = Convert.ChangeType(txt, prop.PropertyType);
                    }

                    prop.SetValue(_entity, value);
                }

                // 2) Сохраняем: либо Add для нового, либо Modified для существующего
                var id = GetId();
                if (id == null || id == 0)
                {
                    // Новая сущность — один вызов Add
                    var repoProp = _uow.GetType()
                        .GetProperties()
                        .FirstOrDefault(x =>
                            x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericArguments()[0] == _entity.GetType());

                    if (repoProp != null)
                    {
                        var repo = repoProp.GetValue(_uow);
                        var add = repoProp.PropertyType.GetMethod("Add");
                        add?.Invoke(repo, new[] { _entity });
                    }
                }
                else
                {
                    // Существующая — только одно состояние Modified при необходимости
                    var entry = _uow.Entry(_entity);
                    if (entry.State == EntityState.Detached)
                    {
                        entry.State = EntityState.Modified;
                    }

                }

                // 3) Финальное сохранение
                _uow.Save();
                _uow.Entry(_entity).State = EntityState.Detached;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

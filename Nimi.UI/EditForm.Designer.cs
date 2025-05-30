/*using Nimi.Core;
using Nimi.Core.UIconfig;
using Nimi.Data;
using Nimi.Data.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nimi.UI
{
    public partial class EditForm : Form
    {
        private readonly UnitOfWork _uow;
        private readonly object _entity;
        private readonly UIConfig _ui;
        private readonly Type _type;
        private readonly TableLayoutPanel _table;

        public EditForm(UnitOfWork uow, object entity, UIConfig uiConfig)
        {
            _uow = uow;
            _entity = entity;
            _ui = uiConfig;
            _type = entity.GetType();

            //InitializeComponent();
            Text = (_type.Name, GetId()) switch
            {
                (_, null) => "Добавление",
                (_, not null) => "Редактирование"
            };

            // Создаём TableLayoutPanel динамически
            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };
            Controls.Add(_table);

            BuildForm();
            if (GetId() != null) LoadData();
        }

        private int? GetId()
        {
            var prop = _type.GetProperty("Id");
            return (int?)(prop?.GetValue(_entity));
        }

        private void BuildForm()
        {
            int row = 0;
            foreach (var kv in _ui.Layout.OrderBy(k => k.Key))
            {
                foreach (var field in kv.Value)
                {
                    // Label
                    var lbl = new Label
                    {
                        Text = _ui.FieldLabels.GetValueOrDefault(field, field),
                        AutoSize = true
                    };
                    _table.Controls.Add(lbl, 0, row);

                    // Control: ComboBox or TextBox
                    Control ctrl;
                    if (_ui.SpecialWidgets.TryGetValue(field, out var cfg)
                     && cfg.WidgetType == "combobox")
                    {
                        ctrl = new ComboBox { Dock = DockStyle.Fill };
                        if (cfg.Options != null)
                            ((ComboBox)ctrl).Items.AddRange(cfg.Options.ToArray());
                    }
                    else
                    {
                        ctrl = new TextBox { Dock = DockStyle.Fill };
                        if (cfg.WidgetType == "phone")
                            ctrl.KeyPress += (_, e) => {
                                if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                                    e.Handled = true;
                            };
                        if (cfg.WidgetType == "currency")
                            ctrl.KeyPress += (_, e) => {
                                if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
                                    e.Handled = true;
                            };
                    }
                    ctrl.Name = "fld_" + field;
                    _table.Controls.Add(ctrl, 1, row);
                    row++;
                }
            }

            // Кнопка Сохранить
            var btnSave = new Button { Text = "💾 Сохранить", Dock = DockStyle.Bottom };
            btnSave.Click += (_, __) => Save();
            Controls.Add(btnSave);
        }

        private void LoadData()
        {
            foreach (Control c in _table.Controls)
            {
                if (!(c is TextBox or ComboBox)) continue;
                var field = c.Name.Replace("fld_", "");
                var prop = _type.GetProperty(field);
                if (prop == null) continue;
                var val = prop.GetValue(_entity)?.ToString() ?? "";
                if (c is ComboBox cb) cb.Text = val;
                else if (c is TextBox tb) tb.Text = val;
            }
        }

        private void Save()
        {
            try
            {
                // Собираем данные
                foreach (Control c in _table.Controls)
                {
                    if (!(c is TextBox or ComboBox)) continue;
                    var field = c.Name.Replace("fld_", "");
                    var prop = _type.GetProperty(field);
                    if (prop == null) continue;

                    // Проверка пустых
                    var txt = (c is ComboBox cb) ? cb.Text : ((TextBox)c).Text;
                    if (string.IsNullOrWhiteSpace(txt))
                        throw new Exception($"Поле '{field}' не может быть пустым");

                    // Пропускаем readonly свойства
                    if (prop.GetSetMethod() == null)
                        continue;

                    object? value;
                    if (_ui.SpecialWidgets.TryGetValue(field, out var cfg))
                    {
                        if (cfg.WidgetType == "currency")
                            value = decimal.Parse(txt);
                        else if (cfg.WidgetType == "phone")
                            value = new string(txt.Where(char.IsDigit).ToArray());
                        else
                            value = txt;
                    }
                    else
                    {
                        value = txt;
                    }

                    // Устанавливаем
                    prop.SetValue(_entity, Convert.ChangeType(value, prop.PropertyType));
                }

                // Сохраняем через репозитории
                var idProp = _type.GetProperty("Id")!.GetValue(_entity);
                if ((int?)idProp == null || (int)idProp == 0)
                {
                    // Новая запись
                    var addMethod = typeof(UnitOfWork)
                        .GetProperty(_type.Name + "s")!
                        .GetValue(_uow);
                    var repoType = addMethod.GetType();
                    var add = repoType.GetMethod("Add")!;
                    add.Invoke(addMethod, new[] { _entity });
                }
                _uow.Save();
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
*/
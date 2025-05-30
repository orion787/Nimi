# 🐹 DooBy 
*(FOR EDUCATIONAL PURPOSES)*  

**Database engine for automating work with domain (models in SqLite) and UI generation**

---

### 📋 Requirements
Microsoft.EntityFrameworkCore.Sqlite <br>
Microsoft.EntityFrameworkCore.Design <br>
 
---

📄 *Readme is written for my educational institution (you can translate it using a translator from Russian).*

---

### 📖 **Описание проекта**
Данный проект автоматизует работу с сущностью из какой-либо предметной области. Сущность (в данном проекте) представлят из себя описанный в Nimi.Data.Models объект / несколько объектов, хранящийся в БД SQLite. Библиотека автоматически генерирует GUI (WinForms) для работы с этой модeлью, вы можете предоставить свои настройки для отображения GUI по вашему усмотрению, процесс настройки GUI будет описан ниже.<br>
Основная идея отрисовки заключается в том, что БД хранит список моделей, список котрых (с дополнительной настройкой) отображается в виде карточек, то есть каждая сущность представляет из себя отдельную карточку.

### 🛠️ Инструкция по использованию
Для разработки своего приложения с использованием dooby небходимо:
1. Разарботать свою модель/модели в Nimi.Data.Models и обязаны наследовать от EntityBase.Если вы не хотите, чтобы поле не участвовало в отображении аннотируйте его Hidden.
2. Для создания связей между таблицами можно использовать атрибут EntityRelation(<имя_сущности>).
   <br>Например:
```C#
    public class Partner : EntityBase
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Director { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int Rating { get; set; }


        [Hidden]
        [EntityRelation(typeof(Sale))]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

        [Hidden]
        public decimal TotalSales
            => Sales.Sum(s => s.TotalPrice);

        public int Discount
        {
            get
            {
                if (TotalSales < 10_000) return 0;
                if (TotalSales < 50_000) return 5;
                if (TotalSales < 300_000) return 10;
                return 15;
            }
        }
    }
```


2. В Nimi.Data.DbContexts настройте какие модели вы хотите представлять в виде таблиц в файлу БД SQLite:
   <br>Например:
```C#
public DbSet<Partner> Partners { get; set; }
public DbSet<Product> Products { get; set; }
public DbSet<Sale> Sales { get; set; }
```



2. В Nimi.UI настройте содинение с БД:
   <br>Например:
```C#
string DbPath = Path.Combine(_solutionPath, "Resources", "partners.db");
_uow = new UnitOfWork(DbPath);
```

   
---
Теперь опишу процесс настройки отображения:<br><br>
3. Инициализируейте контекст настроки UI моделью, контекст представлен в dooby.gui.ui_layout_config 
   <br>Например:
```Python
ui_config = UILayoutConfig(Partner)
```
4. Вы можете указать какие поля НЕ хотите отображать
   <br>Например:
```Python
ui_config.excluded_fields.append('hideninfo')
```

5. Опишите общие настроки отображения окна и карточек
   <br>Например:
```Python
ui_config.set_background_color(Colors.smoke)       #Цвет окна
ui_config.set_card_background(Colors.lightblue)    #Цвет карточки
ui_config.set_alignment('left_custom_spacing')     #Выравние в пределах картчоки (доступны: left, center, right, left_custom_spacing)
ui_config.set_inter_element_spacing(40)            #Отступ между полями для left_custom_spacing
ui_config.set_card_spacing(10)                     #Отступ между карточками
ui_config.set_font(('Arial', 12))                  #Выбор шрифта и кегеля
```


6. Построчно настройте отображения, то есть укажите какие поля на каких строках вы хотите видеть:
  <br>Например:
```Python
ui_config.layout = defaultdict(list, {
    1: ['name', 'type', 'discount'],
    2: ['phone'],
    3: ['director'],
    4: ['rating']
})
```
7. Настройте особенности отображения подписей до и после полей
   <br>Например:
```Python
ui_config.set_field_label('name', 'Наименование :')                             #Перед полем 
ui_config.set_field_label('type', 'Тип организации :')                          
ui_config.set_field_label('discount', 'Скидка :')                               
ui_config.set_field_suffix('discount', '%')                                     #После поля
ui_config.set_field_label('phone', 'Телефон :')
ui_config.set_special_widget('type', 'combobox', ['ООО', 'ИП', 'АО'])           #Выбор для режима добавления
ui_config.set_special_widget('phone', 'phone')
ui_config.set_field_label('director', 'Директор :')
ui_config.set_field_label('rating', 'Рейтинг :')
```
8. Для запуска процесса приложения сконструируйте экзампляр класса DooByApp, представленный в dooby.gui.main_page
   <br>Например:
```Python
root = Tk()
DooByApp(root, Partner, title="Учет партнёров", logo_path="resources//logo.png", ui_config=ui_config)
```
9. Запустите процесс отрисовки tkiner:
```Python
root.mainloop()
```


### 📜 Лицензия
Проект залицензирован под MIT License. Свободно используйте, модифицируйте и распространяйте.

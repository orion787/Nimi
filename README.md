### ![nimi-nightmare-baku](https://github.com/user-attachments/assets/831b199e-110d-4646-aec2-3b7f32f220e5)

# Nimi

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
Nimi это DooBy (https://github.com/orion787/DooBy), переписанная C# <br>
Данный проект автоматизует работу с сущностью из какой-либо предметной области. Сущность (в данном проекте) представлят из себя описанный в Nimi.Data.Models объект / несколько объектов, хранящийся в БД SQLite. Библиотека автоматически генерирует GUI (WinForms) для работы с этой модeлью, вы можете предоставить свои настройки для отображения GUI по вашему усмотрению, процесс настройки GUI будет описан ниже.<br>
Основная идея отрисовки заключается в том, что БД хранит список моделей, список котрых (с дополнительной настройкой) отображается в виде карточек, то есть каждая сущность представляет из себя отдельную карточку.

### 🛠️ Инструкция по использованию
Для разработки своего приложения с использованием Nimi небходимо:
1. Разарботать свою модель/модели в Nimi.Data.Models и обязаны наследовать каждую из них от  EntityBase. Если вы не хотите, чтобы поле не участвовало в отображении аннотируйте его Hidden. Если вы хотите сделать поле, которое является автовычисляемым дополнительно аннотируйте его NotMapped, чтобы оно не попало в талицу БД.
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
    }
```

3. Для создания автовычисляемого поля используйте 'UowProvider'
   <br>Например:
```C#
        public int Discount
        {
            get
            {
                UnitOfWork _uow;
                _uow = UowProvider.GetInstance();

                var sales =_uow.Sales
                    .Query()
                    .Where(s => s.PartnerId == Id)
                    .Include(s => s.Product)
                    .AsEnumerable();

                decimal total = sales.Sum(s => (s.Product?.Price ?? 0m) * s.Quantity);

                if (total < 10_000) return 0;
                if (total < 50_000) return 5;
                if (total < 300_000) return 10;
                return 15;
            }
        }
```


4. В Nimi.Data.DbContexts настройте какие модели вы хотите представлять в виде таблиц в файле БД SQLite:
   <br>Например:
```C#
public DbSet<Partner> Partners { get; set; }
public DbSet<Product> Products { get; set; }
public DbSet<Sale> Sales { get; set; }
```



5. В Nimi.Data.Repositories.Helpers настройте содинение с БД:
   <br>Например:
```C#
DbPath = Path.Combine(_solutionPath, "Resources", "partners.db");
```

   
---
Теперь опишу процесс настройки отображения в CardForm.Designer.cs:<br><br>
6. Укажите размеры и заголовок окна с карточками
   <br>Например:
```С#
Text = "Учёт партнёров";
ClientSize = new Size(900, 600);
```


7. Построчно настройте отображения, то есть укажите какие поля на каких строках вы хотите видеть:
  <br>Например:
```C#
_ui.AddRow(1, "Type", "Name", "Discount");      //Строка 1
_ui.AddRow(2, "Phone");                         //Строка 2
_ui.AddRow(3, "Director", "Email");             //Строка 3
_ui.AddRow(4, "Rating");                        //Строка 4
```
8. Настройте особенности отображения подписей до и после полей, а также виджеты (они используются для более удобного отображения. Существуют phone и currency)
   <br>Например:
```C#
// Подписи/виджеты/суффиксы
_ui.SetFieldLabel("Type", "Тип");               //Текст перед полем
_ui.SetFieldLabel("Name", "Партнёр");
_ui.SetFieldLabel("Discount", "Скидка");
_ui.SetFieldSuffix("Discount", "%");            //Текст после поля  
_ui.SetSpecialWidget("Discount", "currency");   //Виджет

_ui.SetFieldLabel("Phone", "Телефон");          //Тест перед полем
_ui.SetSpecialWidget("Phone", "phone");

_ui.SetFieldLabel("Director", "Директор");
_ui.SetFieldLabel("Rating", "Рейтинг");

_ui.BackgroundColor = Colors.LightBlue;         //Цвет окна приложения
_ui.CardBackground = Colors.Seashell;           //Цвет карточки

_ui.CardSpacing = 10;                           //Расстояние между карточками по вертикали
```

### 📜 Лицензия
Проект залицензирован под MIT License. Свободно используйте, модифицируйте и распространяйте.

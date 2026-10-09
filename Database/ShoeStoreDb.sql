/* =====================================================================
   База данных системы оформления заказа обуви
   СУБД: Microsoft SQL Server (LocalDB)
   Нормальная форма: 3НФ, ссылочная целостность обеспечена внешними ключами
   Автор: участник 1 (модель данных и настройка БД)
   ===================================================================== */

IF DB_ID(N'ShoeStoreDb') IS NULL
    CREATE DATABASE ShoeStoreDb;
GO

USE ShoeStoreDb;
GO

/* Удаление таблиц в обратном порядке зависимостей (для повторного запуска) */
DROP TABLE IF EXISTS dbo.OrderItem;
DROP TABLE IF EXISTS dbo.[Order];
DROP TABLE IF EXISTS dbo.ProductSize;
DROP TABLE IF EXISTS dbo.Product;
DROP TABLE IF EXISTS dbo.Size;
DROP TABLE IF EXISTS dbo.Manufacturer;
DROP TABLE IF EXISTS dbo.Category;
DROP TABLE IF EXISTS dbo.[User];
DROP TABLE IF EXISTS dbo.Role;
GO

/* ---------- Справочники ---------- */

CREATE TABLE dbo.Role
(
    RoleId  INT IDENTITY(1,1) NOT NULL,
    Name    NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Role PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_Name UNIQUE (Name)
);

CREATE TABLE dbo.Category
(
    CategoryId  INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Category_Name UNIQUE (Name)
);

CREATE TABLE dbo.Manufacturer
(
    ManufacturerId  INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Manufacturer PRIMARY KEY (ManufacturerId),
    CONSTRAINT UQ_Manufacturer_Name UNIQUE (Name)
);

CREATE TABLE dbo.Size
(
    SizeId  INT IDENTITY(1,1) NOT NULL,
    Value   DECIMAL(4,1)      NOT NULL,
    CONSTRAINT PK_Size PRIMARY KEY (SizeId),
    CONSTRAINT UQ_Size_Value UNIQUE (Value),
    CONSTRAINT CK_Size_Value CHECK (Value BETWEEN 15 AND 50)
);

/* ---------- Пользователи ---------- */

CREATE TABLE dbo.[User]
(
    UserId      INT IDENTITY(1,1) NOT NULL,
    Login       NVARCHAR(50)      NOT NULL,
    LastName    NVARCHAR(50)      NOT NULL,
    FirstName   NVARCHAR(50)      NOT NULL,
    Patronymic  NVARCHAR(50)      NULL,
    RoleId      INT               NOT NULL,
    CONSTRAINT PK_User PRIMARY KEY (UserId),
    CONSTRAINT UQ_User_Login UNIQUE (Login),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId)
        REFERENCES dbo.Role (RoleId)
        ON DELETE NO ACTION ON UPDATE CASCADE
);

/* ---------- Каталог ---------- */

/* Модель обуви */
CREATE TABLE dbo.Product
(
    ProductId       INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(150)     NOT NULL,
    Description     NVARCHAR(MAX)     NULL,
    Composition     NVARCHAR(300)     NULL,      -- состав
    Price           DECIMAL(10,2)     NOT NULL,  -- базовая цена без скидки
    ImagePath       NVARCHAR(255)     NULL,      -- NULL -> выводится picture.png
    CategoryId      INT               NOT NULL,
    ManufacturerId  INT               NOT NULL,
    CONSTRAINT PK_Product PRIMARY KEY (ProductId),
    CONSTRAINT CK_Product_Price CHECK (Price > 0),
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId)
        REFERENCES dbo.Category (CategoryId)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT FK_Product_Manufacturer FOREIGN KEY (ManufacturerId)
        REFERENCES dbo.Manufacturer (ManufacturerId)
        ON DELETE NO ACTION ON UPDATE CASCADE
);

/* Товарная позиция: сочетание модели и размера с доступным количеством */
CREATE TABLE dbo.ProductSize
(
    ProductSizeId  INT IDENTITY(1,1) NOT NULL,
    ProductId      INT               NOT NULL,
    SizeId         INT               NOT NULL,
    Quantity       INT               NOT NULL CONSTRAINT DF_ProductSize_Quantity DEFAULT (0),
    CONSTRAINT PK_ProductSize PRIMARY KEY (ProductSizeId),
    CONSTRAINT UQ_ProductSize UNIQUE (ProductId, SizeId),
    CONSTRAINT CK_ProductSize_Quantity CHECK (Quantity >= 0),
    CONSTRAINT FK_ProductSize_Product FOREIGN KEY (ProductId)
        REFERENCES dbo.Product (ProductId)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_ProductSize_Size FOREIGN KEY (SizeId)
        REFERENCES dbo.Size (SizeId)
        ON DELETE NO ACTION ON UPDATE CASCADE
);

/* ---------- Заказы ---------- */

CREATE TABLE dbo.[Order]
(
    OrderId    INT IDENTITY(1,1) NOT NULL,
    OrderDate  DATETIME2(0)      NOT NULL CONSTRAINT DF_Order_OrderDate DEFAULT (SYSDATETIME()),
    UserId     INT               NOT NULL,   -- клиент, оформивший заказ
    CONSTRAINT PK_Order PRIMARY KEY (OrderId),
    CONSTRAINT FK_Order_User FOREIGN KEY (UserId)
        REFERENCES dbo.[User] (UserId)
        ON DELETE NO ACTION ON UPDATE CASCADE
);

/* Состав заказа. UnitPrice фиксирует цену со скидкой на момент оформления */
CREATE TABLE dbo.OrderItem
(
    OrderItemId    INT IDENTITY(1,1) NOT NULL,
    OrderId        INT               NOT NULL,
    ProductSizeId  INT               NOT NULL,
    Quantity       INT               NOT NULL,
    UnitPrice      DECIMAL(10,2)     NOT NULL,
    CONSTRAINT PK_OrderItem PRIMARY KEY (OrderItemId),
    CONSTRAINT UQ_OrderItem UNIQUE (OrderId, ProductSizeId),
    CONSTRAINT CK_OrderItem_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItem_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderId)
        REFERENCES dbo.[Order] (OrderId)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- товарную позицию, которая есть в заказах, удалить нельзя
    CONSTRAINT FK_OrderItem_ProductSize FOREIGN KEY (ProductSizeId)
        REFERENCES dbo.ProductSize (ProductSizeId)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

CREATE INDEX IX_Product_CategoryId     ON dbo.Product (CategoryId);
CREATE INDEX IX_Product_ManufacturerId ON dbo.Product (ManufacturerId);
CREATE INDEX IX_Order_UserId           ON dbo.[Order] (UserId);
CREATE INDEX IX_OrderItem_ProductSizeId ON dbo.OrderItem (ProductSizeId);
GO

/* =====================================================================
   Тестовые данные (заменить данными из файлов import, Приложение 2)
   ===================================================================== */

INSERT INTO dbo.Role (Name) VALUES
(N'Администратор'), (N'Менеджер'), (N'Клиент');

INSERT INTO dbo.[User] (Login, LastName, FirstName, Patronymic, RoleId) VALUES
(N'admin',   N'Смирнова', N'Ольга',   N'Петровна',    1),
(N'manager', N'Кузнецов', N'Андрей',  N'Игоревич',    2),
(N'client1', N'Иванов',   N'Сергей',  N'Николаевич',  3),
(N'client2', N'Петрова',  N'Мария',   N'Александровна', 3);

INSERT INTO dbo.Category (Name) VALUES
(N'Кроссовки'), (N'Ботинки'), (N'Туфли'), (N'Сапоги');

INSERT INTO dbo.Manufacturer (Name) VALUES
(N'Россия'), (N'Италия'), (N'Китай'), (N'Германия');

INSERT INTO dbo.Size (Value) VALUES
(36), (37), (38), (39), (40), (41), (42), (43), (44);

INSERT INTO dbo.Product (Name, Description, Composition, Price, ImagePath, CategoryId, ManufacturerId) VALUES
(N'Кроссовки беговые «Спринт»', N'Лёгкие кроссовки для бега', N'Текстиль, резина', 5490.00, N'sprint.jpg', 1, 3),
(N'Ботинки зимние «Север»',     N'Утеплённые ботинки на меху', N'Натуральная кожа, мех', 8990.00, N'sever.jpg', 2, 1),
(N'Туфли классические «Милано»', N'Мужские туфли для офиса',   N'Натуральная кожа', 11200.00, NULL, 3, 2),
(N'Сапоги женские «Берлин»',    N'Высокие сапоги на каблуке', N'Замша, кожа', 13500.00, N'berlin.jpg', 4, 4);

/* ProductId, SizeId (1=36 ... 9=44), Quantity */
INSERT INTO dbo.ProductSize (ProductId, SizeId, Quantity) VALUES
(1, 5, 4), (1, 6, 3), (1, 7, 2),          -- Спринт: всего 9 (много)
(2, 6, 1), (2, 7, 1), (2, 8, 0),          -- Север: всего 2 (мало, подсветка)
(3, 7, 2), (3, 8, 2), (3, 9, 1),          -- Милано: всего 5 (мало)
(4, 1, 3), (4, 2, 3), (4, 3, 2);          -- Берлин: всего 8 (много)

INSERT INTO dbo.[Order] (OrderDate, UserId) VALUES
('2026-09-12T14:30:00', 3),
('2026-10-02T11:00:00', 4);

INSERT INTO dbo.OrderItem (OrderId, ProductSizeId, Quantity, UnitPrice) VALUES
(1, 1, 1, 5490.00),
(1, 7, 1, 11200.00),
(2, 10, 1, 10125.00);
GO

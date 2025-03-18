DROP VIEW IF EXISTS vw_Clients, vw_MonthlyIncome, vw_OrdersWithInfo, vw_MasterWorksAmounts
GO

-- используется в форме - в OrderRepository для получения расширенной информации по заказам
CREATE VIEW vw_OrdersWithInfo 
AS
  SELECT o.Код, 
         o.ДатаНачала, 
         ISNULL(o.ДатаОкончания, '1900-01-01') AS ДатаОкончания,
         o.КодКлиента, 
         c.Фамилия AS ФамилияКлиента,
         o.КодАвтомобиля, 
         a.НомернойЗнак AS НомернойЗнакАвтомобиля
    FROM Заказ o
  LEFT JOIN Клиент c 
    ON o.КодКлиента = c.Код
  LEFT JOIN Автомобиль a 
    ON o.КодАвтомобиля = a.Код;
GO

-- используется в диаграмме и отчете - в DBViewsRepository для получения данных "Мастер - КоличествоРабот"
CREATE VIEW vw_MasterWorksAmounts 
AS
  SELECT КодМастера,
         COUNT(*) AS КоличествоРабот
    FROM Работа
    GROUP BY КодМастера;
GO

-- используется в диаграмме и отчете - в DBViewsRepository для получения данных "Месяц - КоличествоРабот - ОбщийДоход"
CREATE VIEW vw_MonthlyIncome 
AS
  SELECT 
    FORMAT(з.ДатаНачала, 'yyyy-MM') AS Месяц, 
    COUNT(DISTINCT з.Код) AS КоличествоЗаказов,
    (SELECT COUNT(Код) 
       FROM Работа 
       WHERE КодЗаказа IN (
         SELECT Код 
           FROM Заказ 
           WHERE FORMAT(ДатаНачала, 'yyyy-MM') = FORMAT(з.ДатаНачала, 'yyyy-MM')
       )
     ) AS КоличествоРабот,
    SUM(р.Стоимость) AS ОбщийДоход
    FROM Заказ з
    INNER JOIN Работа р ON з.Код = р.КодЗаказа
    GROUP BY FORMAT(з.ДатаНачала, 'yyyy-MM');
GO
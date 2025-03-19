DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats
GO
DROP FUNCTION IF EXISTS fn_ComplaintsByWorkId, fn_TotalWorksCostForOrder;
GO

-- используется в параметризованном отчете - получение заказов по датам - в DBProceduresAndFunctionsRepository
CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  SELECT *
    FROM Заказ
    WHERE ДатаНачала BETWEEN @startDate AND @endDate
    ORDER BY ДатаНачала DESC;
END;
GO

-- используется в диаграмме - в DBProceduresAndFunctionsRepository для получения данных "НазваниеВидаРаботы - КоличествоРабот"
CREATE PROCEDURE usp_GetWorkTypesStats
AS
BEGIN
  DECLARE @КодВидаРаботы INT, @Название VARCHAR(100), @КоличествоРабот INT;

  -- Создаем временную таблицу
  CREATE TABLE #temp (
    НазваниеВидаРаботы VARCHAR(100),
    КоличествоРабот INT
  );

  -- Объявляем курсор
  DECLARE cur CURSOR FOR 
  SELECT Код, Название FROM ВидРаботы;

  -- Открываем курсор
  OPEN cur;
  FETCH NEXT FROM cur INTO @КодВидаРаботы, @Название;

  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- Получаем количество работ для данного вида
    SELECT @КоличествоРабот = COUNT(*) 
      FROM Работа 
      WHERE КодВидаРаботы = @КодВидаРаботы;

      -- Вставляем данные в временную таблицу
      INSERT INTO #temp 
        VALUES (@Название, @КоличествоРабот);

      -- Переход к следующей записи
      FETCH NEXT FROM cur INTO @КодВидаРаботы, @Название;
  END;

  CLOSE cur;
  DEALLOCATE cur;

  SELECT * FROM #temp;
  
  DROP TABLE #temp;
END;
GO

-- используется в форме - в ComplaintRepository для получения жалоб по работе 
CREATE FUNCTION fn_ComplaintsByWorkId(@КодРаботы INT)
RETURNS @resultTable TABLE 
  (
    Код INT,
    КодРаботы INT,
    Описание VARCHAR(200),
    Дата DATE
  )
BEGIN
  INSERT INTO @resultTable 
    SELECT *
      FROM Жалоба
      WHERE КодРаботы = @КодРаботы 
  RETURN
END
GO

-- используется в отчете - в DBProceduresAndFunctionsRepository для получения доп информации - общей стоимости всех работ Заказа 
CREATE FUNCTION fn_TotalWorksCostForOrder(@кодЗаказа INT)
RETURNS MONEY
AS
BEGIN
  DECLARE @общаяСтоимость MONEY

  SET @общаяСтоимость = (
    Select SUM(Стоимость) as суммаСтоимостиРаботЗаказа
      From Работа
      Where кодЗаказа = @кодЗаказа  
  )

  RETURN @общаяСтоимость
END
GO
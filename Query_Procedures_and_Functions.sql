DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats;
GO

CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  SELECT ДатаНачала, ДатаОкончания, КодКлиента, КодАвтомобиля
    FROM Заказ
    WHERE ДатаНачала BETWEEN @startDate AND @endDate
    ORDER BY ДатаНачала DESC;
END;
GO

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

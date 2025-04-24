DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats, usp_MergeWorkDetailsByWorkID, usp_MergeWorkDetailsByOrderID
GO

-- используется в параметризованном отчете - получение заказов по датам - в DBProceduresAndFunctionsRepository
CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  CREATE TABLE #temp (
    Код INT,
    ДатаНачала DATE,
    ДатаОкончания DATE,
    КодКлиента INT,
    КодАвтомобиля INT
  );

  INSERT INTO #temp
  SELECT Код, ДатаНачала, ДатаОкончания, КодКлиента, КодАвтомобиля
    FROM Заказ
    WHERE ДатаНачала BETWEEN @startDate AND @endDate;

  SELECT * 
    FROM #temp
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

-- сложная функциональность + модификация данных:
-- для каждой ДеталиРаботы выбранной Работы если есть дубликаты деталей, они объединяются (обновляется ДетальРаботы.Количество)
CREATE PROC usp_MergeWorkDetailsByWorkID
  @кодРаботы INT,
  @counter INT OUTPUT
AS
BEGIN
  SET @counter = 0
  
  -- курсор cur1 - цикл по ДетальРаботы выбранной Работы
  DECLARE @код INT, @кодДетали INT, @количество INT
  
  DECLARE cur1 CURSOR FOR 
    Select код, кодДетали
      From ДетальРаботы
      Where кодРаботы = @кодРаботы
      ORDER BY код, кодДетали
  
  OPEN cur1
  FETCH cur1 INTO @код, @кодДетали
  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- есть ли дубликаты этой ДеталиРаботы?
    DECLARE @кодДубликатаДеталиРаботы INT

    -- курсор cur2 - ищем дубликаты выбранной ДеталиРаботы 
    DECLARE cur2 CURSOR FOR 
      Select код
        From ДетальРаботы
        Where кодРаботы = @кодРаботы AND 
              кодДетали = @кодДетали AND
              код <> @код

    OPEN cur2
    FETCH cur2 INTO @кодДубликатаДеталиРаботы
    WHILE @@FETCH_STATUS = 0
    BEGIN
      IF (@кодДубликатаДеталиРаботы is not null)
      BEGIN
        -- нашлась ДетальРаботы по условию, удалим ее и обновим оригинальную ДетальРаботы
        DECLARE @количествоДеталейВДубликате INT
        Select @количествоДеталейВДубликате = количество
          From ДетальРаботы
          Where код = @кодДубликатаДеталиРаботы

        -- 1. обновляем оригинальную ДетальРаботы:
        UPDATE ДетальРаботы
          SET количество += @количествоДеталейВДубликате
          Where код = @код

        -- 2. удаляем дубликат ДеталиРаботы:
        DELETE FROM ДетальРаботы
          Where код = @кодДубликатаДеталиРаботы

          SET @counter += 1
      END

      FETCH cur2 INTO @кодДубликатаДеталиРаботы
    END
    CLOSE cur2
    DEALLOCATE cur2
  
    FETCH cur1 INTO @код, @кодДетали
  END
  CLOSE cur1
  DEALLOCATE cur1
  
  Select @counter
END

GO 

-- сложная функциональность + модификация данных:
-- для каждой Работы выбранного Заказа вызывается usp_MergeWorkDetails, которая просматривает ДетальРаботы и 
-- при нахождении дубликатов деталей, объединяет их, дополнительно обновляя ДетальРаботы.Количество
CREATE PROC usp_MergeWorkDetailsByOrderID
  @кодЗаказа INT,
  @changesCounter INT OUTPUT
AS
BEGIN
  SET @changesCounter = 0
  
  -- курсор cur1 - цикл по Работам выбранного Заказа
  DECLARE @код INT
  
  DECLARE cur CURSOR FOR 
    Select код
      From Работа
      Where кодЗаказа = @кодЗаказа
      ORDER BY код
  
  OPEN cur
  FETCH cur INTO @код
  WHILE @@FETCH_STATUS = 0
  BEGIN
    DECLARE @tempCounter INT = 0

    -- для каждой работы выбранного заказа вызовем usp_MergeWorkDetailsByWorkID
    EXEC usp_MergeWorkDetailsByWorkID @кодРаботы = @код, @counter = @tempCounter OUTPUT
    
    SET @changesCounter += @tempCounter
  
    FETCH cur INTO @код
  END
  CLOSE cur
  DEALLOCATE cur
END
GO
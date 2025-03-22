DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats, usp_MergeWorkDetailsByWorkID, usp_MergeWorkDetailsByOrderID
GO

-- используетс€ в параметризованном отчете - получение заказов по датам - в DBProceduresAndFunctionsRepository
CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  SELECT *
    FROM «аказ
    WHERE ƒатаЌачала BETWEEN @startDate AND @endDate
    ORDER BY ƒатаЌачала DESC;
END;
GO

-- используетс€ в диаграмме - в DBProceduresAndFunctionsRepository дл€ получени€ данных "Ќазвание¬ида–аботы -  оличество–абот"
CREATE PROCEDURE usp_GetWorkTypesStats
AS
BEGIN
  DECLARE @ од¬ида–аботы INT, @Ќазвание VARCHAR(100), @ оличество–абот INT;

  -- —оздаем временную таблицу
  CREATE TABLE #temp (
    Ќазвание¬ида–аботы VARCHAR(100),
     оличество–абот INT
  );

  -- ќбъ€вл€ем курсор
  DECLARE cur CURSOR FOR 
  SELECT  од, Ќазвание FROM ¬ид–аботы;

  -- ќткрываем курсор
  OPEN cur;
  FETCH NEXT FROM cur INTO @ од¬ида–аботы, @Ќазвание;

  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- ѕолучаем количество работ дл€ данного вида
    SELECT @ оличество–абот = COUNT(*) 
      FROM –абота 
      WHERE  од¬ида–аботы = @ од¬ида–аботы;

      -- ¬ставл€ем данные в временную таблицу
      INSERT INTO #temp 
        VALUES (@Ќазвание, @ оличество–абот);

      -- ѕереход к следующей записи
      FETCH NEXT FROM cur INTO @ од¬ида–аботы, @Ќазвание;
  END;

  CLOSE cur;
  DEALLOCATE cur;

  SELECT * FROM #temp;
  
  DROP TABLE #temp;
END;
GO

-- сложна€ функциональность + модификаци€ данных:
-- дл€ каждой ƒетали–аботы выбранной –аботы если есть дубликаты деталей, они объедин€ютс€ (обновл€етс€ ƒеталь–аботы. оличество)
CREATE PROC usp_MergeWorkDetailsByWorkID
  @код–аботы INT,
  @counter INT OUTPUT
AS
BEGIN
  SET @counter = 0
  
  -- курсор cur1 - цикл по ƒеталь–аботы выбранной –аботы
  DECLARE @код INT, @кодƒетали INT, @количество INT
  
  DECLARE cur1 CURSOR FOR 
    Select код, кодƒетали
      From ƒеталь–аботы
      Where код–аботы = @код–аботы
      ORDER BY код, кодƒетали
  
  OPEN cur1
  FETCH cur1 INTO @код, @кодƒетали
  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- есть ли дубликаты этой ƒетали–аботы?
    DECLARE @кодƒубликатаƒетали–аботы INT

    -- курсор cur2 - ищем дубликаты выбранной ƒетали–аботы 
    DECLARE cur2 CURSOR FOR 
      Select код
        From ƒеталь–аботы
        Where код–аботы = @код–аботы AND 
              кодƒетали = @кодƒетали AND
              код <> @код

    OPEN cur2
    FETCH cur2 INTO @кодƒубликатаƒетали–аботы
    WHILE @@FETCH_STATUS = 0
    BEGIN
      IF (@кодƒубликатаƒетали–аботы is not null)
      BEGIN
        -- нашлась ƒеталь–аботы по условию, удалим ее и обновим оригинальную ƒеталь–аботы
        DECLARE @количествоƒеталей¬ƒубликате INT
        Select @количествоƒеталей¬ƒубликате = количество
          From ƒеталь–аботы
          Where код = @кодƒубликатаƒетали–аботы

        -- 1. обновл€ем оригинальную ƒеталь–аботы:
        UPDATE ƒеталь–аботы
          SET количество += @количествоƒеталей¬ƒубликате
          Where код = @код

        -- 2. удал€ем дубликат ƒетали–аботы:
        DELETE FROM ƒеталь–аботы
          Where код = @кодƒубликатаƒетали–аботы

          SET @counter += 1
      END

      FETCH cur2 INTO @кодƒубликатаƒетали–аботы
    END
    CLOSE cur2
    DEALLOCATE cur2
  
    FETCH cur1 INTO @код, @кодƒетали
  END
  CLOSE cur1
  DEALLOCATE cur1
  
  Select @counter
END

GO 

-- сложна€ функциональность + модификаци€ данных:
-- дл€ каждой –аботы выбранного «аказа вызываетс€ usp_MergeWorkDetails, котора€ просматривает ƒеталь–аботы и 
-- при нахождении дубликатов деталей, объедин€ет их, дополнительно обновл€€ ƒеталь–аботы. оличество
CREATE PROC usp_MergeWorkDetailsByOrderID
  @код«аказа INT,
  @changesCounter INT OUTPUT
AS
BEGIN
  SET @changesCounter = 0
  
  -- курсор cur1 - цикл по –аботам выбранного «аказа
  DECLARE @код INT
  
  DECLARE cur CURSOR FOR 
    Select код
      From –абота
      Where код«аказа = @код«аказа
      ORDER BY код
  
  OPEN cur
  FETCH cur INTO @код
  WHILE @@FETCH_STATUS = 0
  BEGIN
    DECLARE @tempCounter INT = 0

    -- дл€ каждой работы выбранного заказа вызовем usp_MergeWorkDetailsByWorkID
    EXEC usp_MergeWorkDetailsByWorkID @код–аботы = @код, @counter = @tempCounter OUTPUT
    
    SET @changesCounter += @tempCounter
  
    FETCH cur INTO @код
  END
  CLOSE cur
  DEALLOCATE cur
END
GO
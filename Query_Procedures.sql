DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats, usp_MergeWorkDetailsByWorkID, usp_MergeWorkDetailsByOrderID
GO

-- ������������ � ����������������� ������ - ��������� ������� �� ����� - � DBProceduresAndFunctionsRepository
CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  CREATE TABLE #temp (
    ��� INT,
    ���������� DATE,
    ������������� DATE,
    ���������� INT,
    ������������� INT
  );

  INSERT INTO #temp
  SELECT ���, ����������, �������������, ����������, �������������
    FROM �����
    WHERE ���������� BETWEEN @startDate AND @endDate;

  SELECT * 
    FROM #temp
    ORDER BY ���������� DESC;
END;
GO

-- ������������ � ��������� - � DBProceduresAndFunctionsRepository ��� ��������� ������ "������������������ - ���������������"
CREATE PROCEDURE usp_GetWorkTypesStats
AS
BEGIN
  DECLARE @������������� INT, @�������� VARCHAR(100), @��������������� INT;

  -- ������� ��������� �������
  CREATE TABLE #temp (
    ������������������ VARCHAR(100),
    ��������������� INT
  );

  -- ��������� ������
  DECLARE cur CURSOR FOR 
  SELECT ���, �������� FROM ���������;

  -- ��������� ������
  OPEN cur;
  FETCH NEXT FROM cur INTO @�������������, @��������;

  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- �������� ���������� ����� ��� ������� ����
    SELECT @��������������� = COUNT(*) 
      FROM ������ 
      WHERE ������������� = @�������������;

      -- ��������� ������ � ��������� �������
      INSERT INTO #temp 
        VALUES (@��������, @���������������);

      -- ������� � ��������� ������
      FETCH NEXT FROM cur INTO @�������������, @��������;
  END;

  CLOSE cur;
  DEALLOCATE cur;

  SELECT * FROM #temp;
  
  DROP TABLE #temp;
END;
GO

-- ������� ���������������� + ����������� ������:
-- ��� ������ ������������ ��������� ������ ���� ���� ��������� �������, ��� ������������ (����������� ������������.����������)
CREATE PROC usp_MergeWorkDetailsByWorkID
  @��������� INT,
  @counter INT OUTPUT
AS
BEGIN
  SET @counter = 0
  
  -- ������ cur1 - ���� �� ������������ ��������� ������
  DECLARE @��� INT, @��������� INT, @���������� INT
  
  DECLARE cur1 CURSOR FOR 
    Select ���, ���������
      From ������������
      Where ��������� = @���������
      ORDER BY ���, ���������
  
  OPEN cur1
  FETCH cur1 INTO @���, @���������
  WHILE @@FETCH_STATUS = 0
  BEGIN
    -- ���� �� ��������� ���� ������������?
    DECLARE @������������������������ INT

    -- ������ cur2 - ���� ��������� ��������� ������������ 
    DECLARE cur2 CURSOR FOR 
      Select ���
        From ������������
        Where ��������� = @��������� AND 
              ��������� = @��������� AND
              ��� <> @���

    OPEN cur2
    FETCH cur2 INTO @������������������������
    WHILE @@FETCH_STATUS = 0
    BEGIN
      IF (@������������������������ is not null)
      BEGIN
        -- ������� ������������ �� �������, ������ �� � ������� ������������ ������������
        DECLARE @��������������������������� INT
        Select @��������������������������� = ����������
          From ������������
          Where ��� = @������������������������

        -- 1. ��������� ������������ ������������:
        UPDATE ������������
          SET ���������� += @���������������������������
          Where ��� = @���

        -- 2. ������� �������� ������������:
        DELETE FROM ������������
          Where ��� = @������������������������

          SET @counter += 1
      END

      FETCH cur2 INTO @������������������������
    END
    CLOSE cur2
    DEALLOCATE cur2
  
    FETCH cur1 INTO @���, @���������
  END
  CLOSE cur1
  DEALLOCATE cur1
  
  Select @counter
END

GO 

-- ������� ���������������� + ����������� ������:
-- ��� ������ ������ ���������� ������ ���������� usp_MergeWorkDetails, ������� ������������� ������������ � 
-- ��� ���������� ���������� �������, ���������� ��, ������������� �������� ������������.����������
CREATE PROC usp_MergeWorkDetailsByOrderID
  @��������� INT,
  @changesCounter INT OUTPUT
AS
BEGIN
  SET @changesCounter = 0
  
  -- ������ cur1 - ���� �� ������� ���������� ������
  DECLARE @��� INT
  
  DECLARE cur CURSOR FOR 
    Select ���
      From ������
      Where ��������� = @���������
      ORDER BY ���
  
  OPEN cur
  FETCH cur INTO @���
  WHILE @@FETCH_STATUS = 0
  BEGIN
    DECLARE @tempCounter INT = 0

    -- ��� ������ ������ ���������� ������ ������� usp_MergeWorkDetailsByWorkID
    EXEC usp_MergeWorkDetailsByWorkID @��������� = @���, @counter = @tempCounter OUTPUT
    
    SET @changesCounter += @tempCounter
  
    FETCH cur INTO @���
  END
  CLOSE cur
  DEALLOCATE cur
END
GO
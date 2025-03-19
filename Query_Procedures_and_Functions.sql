DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats
GO
DROP FUNCTION IF EXISTS fn_ComplaintsByWorkId, fn_TotalWorksCostForOrder;
GO

-- ������������ � ����������������� ������ - ��������� ������� �� ����� - � DBProceduresAndFunctionsRepository
CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  SELECT *
    FROM �����
    WHERE ���������� BETWEEN @startDate AND @endDate
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

-- ������������ � ����� - � ComplaintRepository ��� ��������� ����� �� ������ 
CREATE FUNCTION fn_ComplaintsByWorkId(@��������� INT)
RETURNS @resultTable TABLE 
  (
    ��� INT,
    ��������� INT,
    �������� VARCHAR(200),
    ���� DATE
  )
BEGIN
  INSERT INTO @resultTable 
    SELECT *
      FROM ������
      WHERE ��������� = @��������� 
  RETURN
END
GO

-- ������������ � ������ - � DBProceduresAndFunctionsRepository ��� ��������� ��� ���������� - ����� ��������� ���� ����� ������ 
CREATE FUNCTION fn_TotalWorksCostForOrder(@��������� INT)
RETURNS MONEY
AS
BEGIN
  DECLARE @�������������� MONEY

  SET @�������������� = (
    Select SUM(���������) as �������������������������
      From ������
      Where ��������� = @���������  
  )

  RETURN @��������������
END
GO
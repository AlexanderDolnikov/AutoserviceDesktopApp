DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod, usp_GetWorkTypesStats;
GO

CREATE PROCEDURE usp_GetOrdersForDatePeriod
  @startDate DATE,
  @endDate DATE
AS
BEGIN
  SELECT ����������, �������������, ����������, �������������
    FROM �����
    WHERE ���������� BETWEEN @startDate AND @endDate
    ORDER BY ���������� DESC;
END;
GO

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

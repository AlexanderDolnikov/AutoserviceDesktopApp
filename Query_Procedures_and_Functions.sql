DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod;
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
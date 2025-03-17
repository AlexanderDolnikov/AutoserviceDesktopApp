DROP PROCEDURE IF EXISTS usp_GetOrdersForDatePeriod;
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
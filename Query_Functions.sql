DROP FUNCTION IF EXISTS fn_ComplaintsByWorkId, fn_TotalWorksCostForOrder;
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
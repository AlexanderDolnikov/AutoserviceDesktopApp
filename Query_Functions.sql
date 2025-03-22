DROP FUNCTION IF EXISTS fn_ComplaintsByWorkId, fn_TotalWorksCostForOrder;
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
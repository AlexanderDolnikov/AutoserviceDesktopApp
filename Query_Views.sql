DROP VIEW IF EXISTS vw_Clients, vw_MonthlyIncome, vw_OrdersWithInfo, vw_MasterWorksAmounts
GO

CREATE VIEW vw_Clients 
AS
  SELECT * FROM ������;
GO

CREATE VIEW vw_OrdersWithInfo 
AS
  SELECT o.���, 
         o.����������, 
         ISNULL(o.�������������, '1900-01-01') AS �������������,
         o.����������, 
         c.������� AS ��������������,
         o.�������������, 
         a.������������ AS ����������������������
    FROM ����� o
  LEFT JOIN ������ c 
    ON o.���������� = c.���
  LEFT JOIN ���������� a 
    ON o.������������� = a.���;
GO

CREATE VIEW vw_MasterWorksAmounts 
AS
  SELECT ����������,
         COUNT(*) AS ���������������
    FROM ������
    GROUP BY ����������;
GO

CREATE VIEW vw_MonthlyIncome 
AS
  SELECT 
    FORMAT(�.����������, 'yyyy-MM') AS �����, 
    COUNT(DISTINCT �.���) AS �����������������,
    (SELECT COUNT(���) 
       FROM ������ 
       WHERE ��������� IN (
         SELECT ��� 
           FROM ����� 
           WHERE FORMAT(����������, 'yyyy-MM') = FORMAT(�.����������, 'yyyy-MM')
       )
     ) AS ���������������,
    SUM(�.���������) AS ����������
    FROM ����� �
    INNER JOIN ������ � ON �.��� = �.���������
    GROUP BY FORMAT(�.����������, 'yyyy-MM');
GO

DROP TABLE Users;
GO

CREATE TABLE Users (
  Id INT IDENTITY PRIMARY KEY,
  Login NVARCHAR(50) UNIQUE NOT NULL,
  PasswordHash NVARCHAR(256) NOT NULL,
	Salt NVARCHAR(50) NOT NULL DEFAULT '',
  Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Сотрудник', 'Администратор'))
);
GO

INSERT INTO Users (Login, PasswordHash, Salt, Role) 
VALUES 
  ('admin', 'mKe8Y5xBOKoZ1o/PalEZih+rVTUDXlXqYuvSjsXneMw=', 'ubvTUHsChNXXMNlzCX4hoQ==', 'Администратор');
GO

SELECT * 
  FROM Users
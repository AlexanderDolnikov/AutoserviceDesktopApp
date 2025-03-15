using System.Data.SqlClient;
using System.Reflection;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess
{
    public static class EntityMapper
    {
        // _mappers 
        private static readonly Dictionary<Type, Func<SqlDataReader, object>> _mappers = new()
        {
            { typeof(Client), reader => new Client
                {
                    Код = (int)reader["Код"],
                    Имя = reader["Имя"].ToString(),
                    Фамилия = reader["Фамилия"].ToString(),
                    Телефон = reader["Телефон"].ToString(),
                    Адрес = reader["Адрес"].ToString()
                }
            },

            { typeof(Order), reader => new Order
                {
                    Код = (int)reader["Код"],
                    ДатаНачала = (DateTime)reader["ДатаНачала"],
                    ДатаОкончания = reader["ДатаОкончания"] as DateTime?,
                    КодКлиента = (int)reader["КодКлиента"],
                    КодАвтомобиля = (int)reader["КодАвтомобиля"]
                }
            },

            { typeof(Car), reader => new Car
                {
                    Код = (int)reader["Код"],
                    КодМодели = (int)reader["КодМодели"],
                    НомернойЗнак = reader["НомернойЗнак"].ToString(),
                    ГодВыпуска = (int)reader["ГодВыпуска"]
                }
            },

            { typeof(Model), reader => new Model
                {
                    Код = (int)reader["Код"],
                    Название = reader["Название"].ToString()
                }
            },

            { typeof(Detail), reader => new Detail
                {
                    Код = (int)reader["Код"],
                    Название = reader["Название"].ToString(),
                    Стоимость = (decimal)reader["Стоимость"],
                    Производитель = reader["Производитель"].ToString()
                }
            },

            { typeof(Work), reader => new Work
                {
                    Код = (int)reader["Код"],
                    КодЗаказа = (int)reader["КодЗаказа"],
                    КодМастера = (int)reader["КодМастера"],
                    КодВидаРаботы = (int)reader["КодВидаРаботы"],
                    Описание = reader["Описание"].ToString(),
                    Стоимость = (decimal)reader["Стоимость"]
                }
            },

            { typeof(WorkType), reader => new WorkType
                {
                    Код = (int)reader["Код"],
                    Название = reader["Название"].ToString()
                }
            },

            { typeof(Master), reader => new Master
                {
                    Код = (int)reader["Код"],
                    Имя = reader["Имя"].ToString(),
                    Фамилия = reader["Фамилия"].ToString(),
                    Телефон = reader["Телефон"].ToString(),
                    Специализация = reader["Специализация"].ToString()
                }
            },

            { typeof(WorkDetail), reader => new WorkDetail
                {
                    Код = (int)reader["Код"],
                    КодРаботы = (int)reader["КодРаботы"],
                    КодДетали = (int)reader["КодДетали"],
                    Количество = (int)reader["Количество"]
                }
            },

            { typeof(Complaint), reader => new Complaint
                {
                    Код = (int)reader["Код"],
                    КодРаботы = (int)reader["КодРаботы"],
                    Описание = reader["Описание"].ToString(),
                    Дата = (DateTime)reader["Дата"]
                }
            },

            { typeof(User), reader => new User
                {
                    Id = (int)reader["Id"],
                    Login = reader["Login"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Salt = reader["Salt"].ToString(),
                    Role = reader["Role"].ToString()
                }
            }
        };

        // _tableNames
        private static readonly Dictionary<Type, string> _tableNames = new()
        {
            { typeof(Client), "Клиент" },
            { typeof(Order), "Заказ" },
            { typeof(Car), "Автомобиль" },
            { typeof(Model), "Модель" },
            { typeof(Detail), "Деталь" },
            { typeof(Work), "Работа" },
            { typeof(WorkType), "ВидРаботы" },
            { typeof(Master), "Мастер" },
            { typeof(WorkDetail), "ДетальРаботы" },
            { typeof(Complaint), "Жалоба" },
            { typeof(User), "Users" }
        };

        // _propertiesCache
        private static readonly Dictionary<Type, PropertyInfo[]> _propertiesCache = new();

        public static Func<SqlDataReader, T> GetMapper<T>() where T : class
        {
            if (_mappers.TryGetValue(typeof(T), out var mapper))
                return reader => (T)mapper(reader);

            throw new InvalidOperationException($"No mapper registered for type {typeof(T).Name}");
        }

        public static string GetTableName(Type type)
        {
            if (_tableNames.TryGetValue(type, out var tableName))
                return tableName;

            throw new InvalidOperationException($"No tableName registered for type {type.Name}");
        }

        public static PropertyInfo[] GetCachedProperties(Type type)
        {
            if (!_propertiesCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties()
                    .Where(p => p.Name != "Код")
                    .ToArray();

                _propertiesCache[type] = properties;
            }

            return properties;
        }
    }
}
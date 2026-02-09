using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SyncJsonClient
{
   public class Item
   {
      public DateTimeOffset Date { get; set; }
      public long Timestamp { get; set; }
      public int Id { get; set; }
      public string Vendor { get; set; }
      public string Name { get; set; }
      public double Price { get; set; }
   }

   // Класс - Событие
   class Event
   {
      public DateTimeOffset Date { get; set; }
      public long Timestamp { get; set; }
   }

   class Program
   {
      private const string BaseUrl = "http://127.0.0.1:8080/api/items";
      private static readonly WebClient Client = new WebClient();

      // Сериализация/десериализация точного времени в Unix‑timestamp в миллисекундах (13‑значное число)
      static void CaseEvent()
      {
         Event log = new Event
         {
            Date = DateTimeOffset.UtcNow,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
         };

         // Настройка формата даты с помощью JsonSerializerSettings
         Console.WriteLine("1. Cериализация. Настройка формата даты с помощью JsonSerializerSettings:");
         JsonSerializerSettings customformat = new JsonSerializerSettings
         {
            DateFormatString = "dd.MM.yyyy HH:mm:ss.fff"
         };

         string jsoncustom = JsonConvert.SerializeObject(log, customformat);
         Console.WriteLine(jsoncustom);

         Event deserializedevent = JsonConvert.DeserializeObject<Event>(jsoncustom, customformat);
         Console.WriteLine("2. Десериализованная дата: {0}", deserializedevent.Date);
         Console.WriteLine("3. Время (в формате строки): {0}", deserializedevent.Date.ToString("dd.MM.yyyy HH:mm:ss.fff"));
         Console.WriteLine("4. Unix timestamp (ms): {0}", deserializedevent.Timestamp);

         // Настройка формата даты с помощью IsoDateTimeConverter
         Console.WriteLine("1. Cериализация. Настройка формата даты с помощью IsoDateTimeConverter:");
         JsonSerializerSettings customsettings = new JsonSerializerSettings
         {
            Converters = { new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff" } }
         };

         string jsonsettings = JsonConvert.SerializeObject(log, customsettings);
         Console.WriteLine(jsonsettings);
         Event deserializedeven = JsonConvert.DeserializeObject<Event>(jsonsettings, customsettings);
         Console.WriteLine("2. Десериализованная дата: {0}", deserializedeven.Date);
         Console.WriteLine("3. Время (в формате строки): {0}", deserializedeven.Date.ToString("dd.MM.yyyy HH:mm:ss.fff"));
         Console.WriteLine("4. Unix timestamp (ms): {0}", deserializedeven.Timestamp);
      }

      static void Main()
      {
         CaseEvent();

         // Получение Timestamp
         Console.WriteLine("========================================================");
         Console.WriteLine("Получение Timestamp через DateTimeOffset (рекомендуется)");
         DateTimeOffset dateTimeOne = DateTimeOffset.UtcNow;
         long timestampOne = dateTimeOne.ToUnixTimeMilliseconds();
         Console.WriteLine("Текущее UTC время: {0}", dateTimeOne);
         Console.WriteLine("Текущее UTC время в милисекундах: {0:dd.MM.yyyy HH:mm:ss.fff}", dateTimeOne);
         Console.WriteLine("Timestamp: {0}", timestampOne);

         Console.ReadKey();

         Console.WriteLine("SyncJsonClient Тест");
         Console.WriteLine("===================");
         // Устанавливаем Content-Type для JSON
         Client.Headers[HttpRequestHeader.ContentType] = "application/json";
         Client.Encoding = System.Text.Encoding.UTF8;
         try
         {
            // 1. Проверяем, что сервер работает
            TestServerConnection();

            // 2. Получаем все элементы (должен быть пустой список)
            Console.WriteLine("\n2. Получение всех элементов (должен быть пустой список):");
            GetAllItems();

            // 3. Создаем первый элемент
            Console.WriteLine("\n3. Создание первого элемента:");
            Item item1 = CreateItem(new Item { Date = DateTimeOffset.UtcNow, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Id = 1, Vendor = "HP", Name = "Ноутбук", Price = 1567.89 });

            // 4. Создаем второй и третий элемент
            Console.WriteLine("\n4. Создание второго и третьего элемента:");
            Item item2 = CreateItem(new Item { Date = DateTimeOffset.UtcNow, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Id = 2, Vendor = "ACER", Name = "Смартфон", Price = 234.56 });
            Item item3 = CreateItem(new Item { Date = DateTimeOffset.UtcNow, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Id = 3, Vendor = "DELL", Name = "Смартфон", Price = 543.21 });

            // 5. Получаем все элементы (должно быть 3 элемента)
            Console.WriteLine("\n5. Получение всех элементов (должно быть 3 элемента):");
            GetAllItems();

            // 6. Получаем элемент по ID
            Console.WriteLine("\n6. Получение элемента по ID {0}:", item2.Id);
            GetItemById(item2.Id);

            // 7. Обновляем элемент
            Console.WriteLine("\n7. Обновление элемента с ID {0}:", item1.Id);
            Item updatedItem = UpdateItem(item1.Id, new Item { Date = DateTimeOffset.UtcNow, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Id = 7, Vendor = "Lenovo", Name = "Игровой ноутбук", Price = 1678.95 });

            // 8. Проверяем обновление
            Console.WriteLine("\n8. Проверка обновленного элемента:");
            GetItemById(updatedItem.Id);

            // 9. Пытаемся получить несуществующий элемент
            Console.WriteLine("\n9. Попытка получить несуществующий элемент (ID=88):");
            GetNonExistentItem(88);

            // 10. Удаляем элемент
            Console.WriteLine("\n10. Удаление элемента с ID {0}:", item3.Id);
            DeleteItem(item3.Id);

            // 11. Проверяем, что элемент удален
            Console.WriteLine("\n11. Проверка, что элемент удален:");
            GetAllItems();

            // 12. Пытаемся удалить несуществующий элемент
            Console.WriteLine("\n12. Попытка удалить несуществующий элемент (ID=77):");
            DeleteNonExistentItem(77);

            // 13. Тестирование некорректных данных
            Console.WriteLine("\n13. Тестирование некорректных данных:");
            TestInvalidData();

            // 14. Тестирование неверного метода
            Console.WriteLine("\n14. Тестирование неверного метода (PATCH):");
            TestInvalidMethod();

            Console.WriteLine("\nВсе тесты завершены!");
         }
         catch (WebException ex)
         {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
               Console.WriteLine("Ошибка HTTP: {0} - {1}", response.StatusCode, response.StatusDescription);
               if (response.ContentLength > 0)
               {
                  using (Stream stream = response.GetResponseStream())
                  {
                     if (stream != null)
                     {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                           string errorBody = reader.ReadToEnd();
                           Console.WriteLine("Тело ошибки: {0}", errorBody);
                        }
                     }
                  }
               }
            }
            else
            {
               Console.WriteLine("Ошибка: {0}", ex.Message);
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("Неожиданная ошибка: {0}", ex.Message);
         }

         Console.ReadKey();
      }

      static void TestServerConnection()
      {
         try
         {
            Client.DownloadString(BaseUrl);
            Console.WriteLine("1. Сервер доступен");
         }
         catch
         {
            Console.WriteLine("1. Сервер недоступен. Убедитесь, что сервер запущен на http://127.0.0.1:8080/");
            throw;
         }
      }

      static void GetAllItems()
      {
         try
         {
            string response = Client.DownloadString(BaseUrl);
            List<Item> items = JsonConvert.DeserializeObject<List<Item>>(response);
            Console.WriteLine("Статус: Успешно");
            Console.WriteLine("Найдено элементов: {0}", items.Count);
            if (items.Count > 0)
            {
               int i = 0;
               while (i < items.Count)
               {
                  Item item = items[i];
                  Console.WriteLine("Date: {0}, Timestamp: {1}, ID: {2}, Производитель: {3}, Название: {4}, Цена: {5:F}",
                     item.Date, item.Timestamp, item.Id, item.Vendor, item.Name, item.Price);
                  i++;
               }
            }
         }
         catch (WebException ex)
         {
            HandleWebException(ex);
         }
      }

      static Item CreateItem(Item item)
      {
         try
         {
            string json = JsonConvert.SerializeObject(item);
            string response = Client.UploadString(BaseUrl, "POST", json);
            Item createdItem = JsonConvert.DeserializeObject<Item>(response);
            Console.WriteLine("Статус: Создано успешно");
            Console.WriteLine("Date: {0}, Timestamp: {1}, ID: {2}, Производитель: {3}, Название: {4}, Цена: {5:F}",
               item.Date, item.Timestamp, item.Id, item.Vendor, item.Name, item.Price);
            return createdItem;
         }
         catch (WebException ex)
         {
            HandleWebException(ex);
            return null;
         }
      }

      static void GetItemById(int id)
      {
         try
         {
            string url = string.Format("{0}/{1}", BaseUrl, id);
            string response = Client.DownloadString(url);
            Item item = JsonConvert.DeserializeObject<Item>(response);
            Console.WriteLine("Статус: Найден");
            Console.WriteLine("Date: {0}, Timestamp: {1}, ID: {2}, Производитель: {3}, Название: {4}, Цена: {5:F}",
               item.Date, item.Timestamp, item.Id, item.Vendor, item.Name, item.Price);
         }
         catch (WebException ex)
         {
            HandleWebException(ex);
         }
      }

      static Item UpdateItem(int id, Item item)
      {
         try
         {
            string url = string.Format("{0}/{1}", BaseUrl, id);
            string json = JsonConvert.SerializeObject(item);
            string response = Client.UploadString(url, "PUT", json);
            Item updatedItem = JsonConvert.DeserializeObject<Item>(response);
            Console.WriteLine("Статус: Обновлено успешно");
            Console.WriteLine("ID: {0}, Производитель: {1}, Название: {2}, Цена: {3:F}", item.Id, item.Vendor, item.Name, item.Price);
            return updatedItem;
         }
         catch (WebException ex)
         {
            HandleWebException(ex);
            return null;
         }
      }

      static void DeleteItem(int id)
      {
         try
         {
            string url = string.Format("{0}/{1}", BaseUrl, id);
            string response = Client.UploadString(url, "DELETE", "");
            JObject result = JObject.Parse(response);
            Console.WriteLine("Статус: Удалено успешно");
            Console.WriteLine("Сообщение: {0}", result["message"]);
         }
         catch (WebException ex)
         {
            HandleWebException(ex);
         }
      }

      static void GetNonExistentItem(int id)
      {
         try
         {
            string url = string.Format("{0}/{1}", BaseUrl, id);
            Client.DownloadString(url);
            Console.WriteLine("Статус: ОШИБКА - элемент найден (не должно было произойти)");
         }
         catch (WebException ex)
         {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
               if (response.StatusCode == HttpStatusCode.NotFound)
               {
                  Console.WriteLine("Статус: Ожидаемая ошибка - элемент не найден");
               }
               else
               {
                  HandleWebException(ex);
               }
            }
            else
            {
               HandleWebException(ex);
            }
         }
      }

      static void DeleteNonExistentItem(int id)
      {
         try
         {
            string url = string.Format("{0}/{1}", BaseUrl, id);
            Client.UploadString(url, "DELETE", "");
            Console.WriteLine("Статус: ОШИБКА - элемент удален (не должно было произойти)");
         }
         catch (WebException ex)
         {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
               if (response.StatusCode == HttpStatusCode.NotFound)
               {
                  Console.WriteLine("Статус: Ожидаемая ошибка - элемент не найден");
               }
               else
               {
                  HandleWebException(ex);
               }
            }
            else
            {
               HandleWebException(ex);
            }
         }
      }

      static void TestInvalidData()
      {
         try
         {
            // Пытаемся отправить невалидный JSON
            string invalidJson = "{invalid json}";
            Client.UploadString(BaseUrl, "POST", invalidJson);
            Console.WriteLine("Статус: ОШИБКА - сервер принял невалидный JSON");
         }
         catch (WebException ex)
         {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
               if (response.StatusCode == HttpStatusCode.BadRequest)
               {
                  Console.WriteLine("Статус: Ожидаемая ошибка - невалидные данные");
                  using (Stream stream = ex.Response.GetResponseStream())
                  {
                     if (stream != null)
                     {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                           string error = reader.ReadToEnd();
                           Console.WriteLine("Сообщение об ошибке: {0}", error);
                        }
                     }
                  }
               }
               else
               {
                  HandleWebException(ex);
               }
            }
            else
            {
               HandleWebException(ex);
            }
         }
      }

      static void TestInvalidMethod()
      {
         try
         {
            // Пытаемся использовать неразрешенный метод
            Client.Headers[HttpRequestHeader.ContentType] = "application/json";
            Client.UploadString(BaseUrl, "PATCH", "{}");
            Console.WriteLine("Статус: ОШИБКА - сервер принял неразрешенный метод");
         }
         catch (WebException ex)
         {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
               if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
               {
                  Console.WriteLine("Статус: Ожидаемая ошибка - метод не разрешен");
               }
               else
               {
                  HandleWebException(ex);
               }
            }
            else
            {
               HandleWebException(ex);
            }
         }
      }

      static void HandleWebException(WebException ex)
      {
         HttpWebResponse response = (HttpWebResponse)ex.Response;
         if (response != null)
         {
            Console.WriteLine("HTTP Ошибка: {0} {1}", (int)response.StatusCode, response.StatusCode);
            using (Stream stream = response.GetResponseStream())
            {
               if (stream != null)
               {
                  using (StreamReader reader = new StreamReader(stream))
                  {
                     string errorBody = reader.ReadToEnd();
                     if (!string.IsNullOrEmpty(errorBody))
                     {
                        Console.WriteLine("Тело ошибки: {0}", errorBody);
                     }
                  }
               }
            }
         }
         else
         {
            Console.WriteLine("Ошибка: {0}", ex.Message);
         }
      }
   }
}
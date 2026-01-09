using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace SyncJsonClient
{
   public class Item
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public double Price { get; set; }
   }

   class Program
   {
      private const string BaseUrl = "http://127.0.0.1:8080/api/items";
      private static readonly WebClient Client = new WebClient();

      static void Main(string[] args)
      {
         Console.WriteLine("SyncJsonServer Тест");
         Console.WriteLine("===================");
         // Устанавливаем Content-Type для JSON
         Client.Headers[HttpRequestHeader.ContentType] = "application/json";
         Client.Encoding = System.Text.Encoding.UTF8;
         try
         {
            // 1. Проверяем, что сервер работает
            TestServerConnection();

            // 2. Получаем все элементы (должен быть пустой список)
            Console.WriteLine("1. Получение всех элементов (должен быть пустой список):");
            GetAllItems();

            // 3. Создаем первый элемент
            Console.WriteLine("\n2. Создание первого элемента:");
            var item1 = CreateItem(new Item { Name = "Ноутбук", Price = 1567.89 });

            // 4. Создаем второй элемент
            Console.WriteLine("\n3. Создание второго элемента:");
            var item2 = CreateItem(new Item { Name = "Смартфон", Price = 234.56 });

            // 5. Получаем все элементы (должно быть 2 элемента)
            Console.WriteLine("\n4. Получение всех элементов (должно быть 2 элемента):");
            GetAllItems();

            // 6. Получаем элемент по ID
            Console.WriteLine("\n5. Получение элемента по ID {0}:", item1.Id);
            GetItemById(item1.Id);

            // 7. Обновляем элемент
            Console.WriteLine("\n6. Обновление элемента с ID {0}:", item1.Id);
            var updatedItem = UpdateItem(item1.Id, new Item { Name = "Игровой ноутбук", Price = 1234.56 });

            // 8. Проверяем обновление
            Console.WriteLine("\n7. Проверка обновленного элемента:");
            GetItemById(updatedItem.Id);

            // 9. Пытаемся получить несуществующий элемент
            Console.WriteLine("\n8. Попытка получить несуществующий элемент (ID=999):");
            GetNonExistentItem(999);

            // 10. Удаляем элемент
            Console.WriteLine("\n9. Удаление элемента с ID {0}:", item2.Id);
            DeleteItem(item2.Id);

            // 11. Проверяем, что элемент удален
            Console.WriteLine("\n10. Проверка, что элемент удален:");
            GetAllItems();

            // 12. Пытаемся удалить несуществующий элемент
            Console.WriteLine("\n11. Попытка удалить несуществующий элемент (ID=999):");
            DeleteNonExistentItem(999);

            // 13. Тестирование некорректных данных
            Console.WriteLine("\n12. Тестирование некорректных данных:");
            TestInvalidData();

            // 14. Тестирование неверного метода
            Console.WriteLine("\n13. Тестирование неверного метода (PATCH):");
            TestInvalidMethod();

            Console.WriteLine("\nВсе тесты завершены!");
         }
         catch (WebException ex)
         {
            if (ex.Response is HttpWebResponse response)
            {
               Console.WriteLine("Ошибка HTTP: {0} - {1}", response.StatusCode, response.StatusDescription);

               if (response.ContentLength > 0)
               {
                  using (var stream = response.GetResponseStream())
                  using (var reader = new System.IO.StreamReader(stream))
                  {
                     var errorBody = reader.ReadToEnd();
                     Console.WriteLine("Тело ошибки: {0}", errorBody);
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

         Console.WriteLine("\nНажмите любую клавишу для выхода...");
         Console.ReadKey();
      }

      static void TestServerConnection()
      {
         try
         {
            var response = Client.DownloadString(BaseUrl);
            Console.WriteLine("Сервер доступен");
         }
         catch
         {
            Console.WriteLine("Сервер недоступен. Убедитесь, что сервер запущен на http://127.0.0.1:8080/");
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
               for (int i = 0; i < items.Count; i++)
               {
                  Item item = items[i];
                  Console.WriteLine("ID: {0}, Название: {1}, Цена: {2:F}", item.Id, item.Name, item.Price);
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

            Console.WriteLine($"Статус: Создано успешно");
            Console.WriteLine("ID: {0}, Название: {1}, Цена: {2:F}", createdItem.Id, createdItem.Name, createdItem.Price);

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
            var url = $"{BaseUrl}/{id}";
            var response = Client.DownloadString(url);
            var item = JsonConvert.DeserializeObject<Item>(response);

            Console.WriteLine($"Статус: Найден");
            Console.WriteLine("ID: {0}, Название: {1}, Цена: {2:F}", item.Id, item.Name, item.Price);
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
            var url = $"{BaseUrl}/{id}";
            var json = JsonConvert.SerializeObject(item);
            var response = Client.UploadString(url, "PUT", json);
            var updatedItem = JsonConvert.DeserializeObject<Item>(response);

            Console.WriteLine($"Статус: Обновлено успешно");
            Console.WriteLine("ID: {0}, Название: {1}, Цена: {2:F}", updatedItem.Id, updatedItem.Name, updatedItem.Price);

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
            var url = $"{BaseUrl}/{id}";
            var response = Client.UploadString(url, "DELETE", "");
            var result = JObject.Parse(response);

            Console.WriteLine($"Статус: Удалено успешно");
            Console.WriteLine($"Сообщение: {result["message"]}");
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
            var url = $"{BaseUrl}/{id}";
            var response = Client.DownloadString(url);
            Console.WriteLine($"Статус: ОШИБКА - элемент найден (не должно было произойти)");
         }
         catch (WebException ex)
         {
            if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.NotFound)
            {
               Console.WriteLine($"Статус: Ожидаемая ошибка - элемент не найден");
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
            var url = $"{BaseUrl}/{id}";
            var response = Client.UploadString(url, "DELETE", "");
            Console.WriteLine($"Статус: ОШИБКА - элемент удален (не должно было произойти)");
         }
         catch (WebException ex)
         {
            if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.NotFound)
            {
               Console.WriteLine($"Статус: Ожидаемая ошибка - элемент не найден");
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
            var invalidJson = "{invalid json}";
            var response = Client.UploadString(BaseUrl, "POST", invalidJson);
            Console.WriteLine($"Статус: ОШИБКА - сервер принял невалидный JSON");
         }
         catch (WebException ex)
         {
            if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.BadRequest)
            {
               Console.WriteLine($"Статус: Ожидаемая ошибка - невалидные данные");
               using (var stream = ex.Response.GetResponseStream())
               using (var reader = new System.IO.StreamReader(stream))
               {
                  var error = reader.ReadToEnd();
                  Console.WriteLine($"Сообщение об ошибке: {error}");
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
            var response = Client.UploadString(BaseUrl, "PATCH", "{}");
            Console.WriteLine($"Статус: ОШИБКА - сервер принял неразрешенный метод");
         }
         catch (WebException ex)
         {
            if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.MethodNotAllowed)
            {
               Console.WriteLine($"Статус: Ожидаемая ошибка - метод не разрешен");
            }
            else
            {
               HandleWebException(ex);
            }
         }
      }

      static void HandleWebException(WebException ex)
      {
         if (ex.Response is HttpWebResponse response)
         {
            Console.WriteLine($"HTTP Ошибка: {(int)response.StatusCode} {response.StatusCode}");

            try
            {
               using (var stream = response.GetResponseStream())
               using (var reader = new System.IO.StreamReader(stream))
               {
                  var errorBody = reader.ReadToEnd();
                  if (!string.IsNullOrEmpty(errorBody))
                  {
                     Console.WriteLine("Тело ошибки: {0}", errorBody);
                  }
               }
            }
            catch
            {
               // Игнорируем ошибки чтения тела ответа
            }
         }
         else
         {
            Console.WriteLine("Ошибка: {0}", ex.Message);
         }
      }
   }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SyncJsonClient
{
   public class Item
   {
      public int Id { get; set; }
      public string Vendor { get; set; }
      public string Name { get; set; }
      public double Price { get; set; }
   }

   class Program
   {
      private const string BaseUrl = "http://127.0.0.1:8080/api/items";
      private static readonly WebClient Client = new WebClient();

      static void Main()
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
            Item item1 = CreateItem(new Item { Vendor = "HP", Name = "Ноутбук", Price = 1567.89 });

            // 4. Создаем второй и третий элемент
            Console.WriteLine("\n3. Создание второго и третьего элемента:");
            Item item2 = CreateItem(new Item { Vendor = "ACER", Name = "Смартфон", Price = 234.56 });
            Item item3 = CreateItem(new Item { Vendor = "DELL", Name = "Смартфон", Price = 543.21 });

            // 5. Получаем все элементы (должно быть 3 элемента)
            Console.WriteLine("\n4. Получение всех элементов (должно быть 3 элемента):");
            GetAllItems();

            // 6. Получаем элемент по ID
            Console.WriteLine("\n5. Получение элемента по ID {0}:", item2.Id);
            GetItemById(item2.Id);

            // 7. Обновляем элемент
            Console.WriteLine("\n6. Обновление элемента с ID {0}:", item1.Id);
            Item updatedItem = UpdateItem(item1.Id, new Item { Vendor = "Lenovo", Name = "Игровой ноутбук", Price = 1678.95 });

            // 8. Проверяем обновление
            Console.WriteLine("\n7. Проверка обновленного элемента:");
            GetItemById(updatedItem.Id);

            // 9. Пытаемся получить несуществующий элемент
            Console.WriteLine("\n8. Попытка получить несуществующий элемент (ID=88):");
            GetNonExistentItem(88);

            // 10. Удаляем элемент
            Console.WriteLine("\n9. Удаление элемента с ID {0}:", item3.Id);
            DeleteItem(item3.Id);

            // 11. Проверяем, что элемент удален
            Console.WriteLine("\n10. Проверка, что элемент удален:");
            GetAllItems();

            // 12. Пытаемся удалить несуществующий элемент
            Console.WriteLine("\n11. Попытка удалить несуществующий элемент (ID=77):");
            DeleteNonExistentItem(77);

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
                  Console.WriteLine("ID: {0}, Производитель: {1}, Название: {2}, Цена: {3:F}", item.Id, item.Vendor, item.Name, item.Price);
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
            Console.WriteLine("ID: {0}, Производитель: {1}, Название: {2}, Цена: {3:F}", item.Id, item.Vendor, item.Name, item.Price);
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
            Console.WriteLine("ID: {0}, Производитель: {1}, Название: {2}, Цена: {3:F}", item.Id, item.Vendor, item.Name, item.Price);
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
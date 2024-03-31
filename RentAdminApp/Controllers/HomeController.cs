using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RentAdminApp.Models;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace RentAdminApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public readonly string IP = "192.168.97.134:7080/";

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

        }


        public static string HashPassword(string password)
        {

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hashedString;
            }
        }

        public async void reftreshtoken()
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];
                        
            if(authToken == null)
            {
                RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync($"http://{IP}api/Token/refreshToken", authToken!.ToString());

            if (response.IsSuccessStatusCode)
            {
                string jwtToken = await response.Content.ReadAsStringAsync();
                // Сохраняем обновленный токен в куки
                HttpContext.Response.Cookies.Append("TokenUser", jwtToken);
            }
            else
            {
                RedirectToAction("Index");
            }
        }

        public async Task SomeAsyncMethod()
        {
            await Task.Delay(100); 
                                    
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        static string[] SplitEmail(string email)
        {
            string[] parts = email.Split('@');
            return parts;
        }

        [HttpPost("/")]
        public async Task<IActionResult> Index(string email, string password)
        {

            var client = _httpClientFactory.CreateClient();

            var hashedPassword = HashPassword(password);

            var userCredentials = new Client
            {
                BirthDate = DateTime.Now,
                FirstName = email,
                IdClient = 0,
                LastName = email,
                MiddleName = email,
                PhoneNumber = email,
                Photo = email,
                RegistrationDate = DateTime.Now,
                Email = email,
                PasswordHash = hashedPassword
            };

            var response = await client.PostAsJsonAsync($"http://{IP}api/Token/signIn", userCredentials);

            if (response.IsSuccessStatusCode)
            {
                string jwtToken = await response.Content.ReadAsStringAsync();
                if (jwtToken != null)
                {
                    string[] emailParts = SplitEmail(email);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var responseClient = await client.GetAsync($"http://{IP}api/Clients/{emailParts[0]}/{emailParts[1]}");
                    var clientContent = await responseClient.Content.ReadAsStringAsync();
                    var clientOne = JsonConvert.DeserializeObject<Client>(clientContent);

                    if (clientOne!.Role! == "admin")
                    {
                        // Сохраняем токен в куки
                        HttpContext.Response.Cookies.Append("TokenUser", jwtToken);
                        HttpContext.Response.Cookies.Append("EmailUser", email);

                        return RedirectToAction("HomePage");

                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return RedirectToAction("Index");
            }
        }

        [HttpGet("/HomePage")]
        public IActionResult HomePage()
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            return View();
        }

        [HttpGet("/ProductParametrInfoPage")]
        public async Task<IActionResult> ProductParametrInfoPage(string search)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var responseProduct = await client.GetAsync($"http://{IP}api/Products");
            var responseCategory = await client.GetAsync($"http://{IP}api/Categories");
            var responseCity = await client.GetAsync($"http://{IP}api/Cities");
            var responseClient = await client.GetAsync($"http://{IP}api/Clients");
            var responseProductParametr = await client.GetAsync($"http://{IP}api/ProductParameters");
            var responseParametr = await client.GetAsync($"http://{IP}api/Parametrs");
            var responseAtributsParam = await client.GetAsync($"http://{IP}api/AtributsParams");
            var responseBookingPrice = await client.GetAsync($"http://{IP}api/BookingPrices");
            var responsePhoto = await client.GetAsync($"http://{IP}api/Photos");

            if (responseProduct.IsSuccessStatusCode && responseCategory.IsSuccessStatusCode
                && responseCity.IsSuccessStatusCode && responseClient.IsSuccessStatusCode
                && responseProductParametr.IsSuccessStatusCode && responseParametr.IsSuccessStatusCode
                && responseAtributsParam.IsSuccessStatusCode && responseBookingPrice.IsSuccessStatusCode
                && responsePhoto.IsSuccessStatusCode)
            {
                var productsInShipment = new List<FullProductParametrInfo>();

                var productsContent = await responseProduct.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(productsContent);

                var clientsContent = await responseClient.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Client>>(clientsContent);

                var citiesContent = await responseCity.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<City>>(citiesContent);

                var categoriesContent = await responseCategory.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesContent);

                var parametrContent = await responseParametr.Content.ReadAsStringAsync();
                var parametrList = JsonConvert.DeserializeObject<List<Parametr>>(parametrContent);

                var productParametersContent = await responseProductParametr.Content.ReadAsStringAsync();
                var productParameters123 = JsonConvert.DeserializeObject<List<ProductParameter>>(productParametersContent);

                var atributsParamsContent = await responseAtributsParam.Content.ReadAsStringAsync();
                var atributsParams = JsonConvert.DeserializeObject<List<AtributsParam>>(atributsParamsContent);

                var bookingPricesContent = await responseBookingPrice.Content.ReadAsStringAsync();
                var bookingPrices = JsonConvert.DeserializeObject<List<BookingPrice>>(bookingPricesContent);

                var photosContent = await responsePhoto.Content.ReadAsStringAsync();
                var photos = JsonConvert.DeserializeObject<List<Photo>>(photosContent);

                foreach (var product in products!)
                {

                    var productParameters = productParameters123!.Where(pp => pp.Product_Id == product.IdProduct).ToList();
                    List<Parametr> parametrs = new List<Parametr>();
                    foreach (var prodparam in productParameters)
                    {
                        parametrs.AddRange(parametrList!.Where(ap => ap.IdParametrs == prodparam.Parameter_Id).ToList());
                    }
                    List<AtributsParam> atributparam = new List<AtributsParam>();
                    foreach (var prodparam in productParameters)
                    {
                        foreach (var param in parametrs)
                        {
                            atributparam.AddRange(atributsParams!.Where(ap => ap.ProductParameterId == prodparam.IdProductParameter).ToList());
                        }
                    }

                    //var atributs = atributsParams.Where(ap => parameters.Contains(ap.ProductParameterId));

                    var prices = bookingPrices?.Where(bp => bp.ProductId == product?.IdProduct);

                    var productPhotos = photos?.Where(p => p.ProductId == product?.IdProduct);

                    Category? category123 = categories?.FirstOrDefault(c => c.IdCategory == product?.CategoryId);

                    City? city123 = cities?.FirstOrDefault(c => c.IdCity == product?.CityId);

                    Client? client123 = clients?.FirstOrDefault(c => c.IdClient == product?.ClientId);

                    List<ProductParameter>? prodparam123 = productParameters?.Where(pp => pp.Product_Id == product?.IdProduct)?.ToList();
                    //List<Parametr> param123 = parametrList.Where(p => parameters.Contains(p.IdParametrs)).ToList();

                    var fullProductInfo = new FullProductParametrInfo
                    {
                        product = product,
                        category = category123,
                        city = city123,
                        сlient = client123,
                        productparametr = prodparam123,
                        parameter = parametrs,
                        attributeParams = atributparam,
                        bookingPrices = prices!.ToList(),
                        photo = productPhotos!.ToList()
                    };

                    // Добавляем экземпляр в список
                    productsInShipment.Add(fullProductInfo);
                }

                List<FullProductParametrInfo> fullproducts = productsInShipment;

                if (!string.IsNullOrEmpty(search))
                {
                    productsInShipment = productsInShipment.Where(e =>
                        (e.product?.NameProduct != null && e.product.NameProduct.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.product?.Descriptions != null && e.product.Descriptions.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.category?.CategoryName != null && e.category.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.city?.Name != null && e.city.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.сlient?.Email != null && e.сlient.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                    if (productsInShipment == null)
                    {
                        return View("ProductParametrInfoPage", fullproducts);
                    }
                    else
                    {
                        return View("ProductParametrInfoPage", productsInShipment);
                    }

                }
                else
                {
                    return View("ProductParametrInfoPage", fullproducts);
                }
            }
            else
            if (responseProduct.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseCategory.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseCity.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseClient.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseProductParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseAtributsParam.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseBookingPrice.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responsePhoto.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("HomePage");


            }
            else
            {
                return RedirectToAction("Index");
            }




        }

        [HttpGet("/ProductParametrInfoPage/EditProduct/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var responseProduct = await client.GetAsync($"http://{IP}api/Products");
            var responseCategory = await client.GetAsync($"http://{IP}api/Categories");
            var responseCity = await client.GetAsync($"http://{IP}api/Cities");
            var responseClient = await client.GetAsync($"http://{IP}api/Clients");
            var responseProductParametr = await client.GetAsync($"http://{IP}api/ProductParameters");
            var responseParametr = await client.GetAsync($"http://{IP}api/Parametrs");
            var responseAtributsParam = await client.GetAsync($"http://{IP}api/AtributsParams");
            var responseBookingPrice = await client.GetAsync($"http://{IP}api/BookingPrices");
            var responsePhoto = await client.GetAsync($"http://{IP}api/Photos");

            if (responseProduct.IsSuccessStatusCode && responseCategory.IsSuccessStatusCode
                && responseCity.IsSuccessStatusCode && responseClient.IsSuccessStatusCode
                && responseProductParametr.IsSuccessStatusCode && responseParametr.IsSuccessStatusCode
                && responseAtributsParam.IsSuccessStatusCode && responseBookingPrice.IsSuccessStatusCode
                && responsePhoto.IsSuccessStatusCode)
            { 
                var productsInShipment = new List<FullProductParametrInfo>();

                var productsContent = await responseProduct.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(productsContent);

                var clientsContent = await responseClient.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Client>>(clientsContent);

                var citiesContent = await responseCity.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<City>>(citiesContent);

                var categoriesContent = await responseCategory.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesContent);

                var parametrContent = await responseParametr.Content.ReadAsStringAsync();
                var parametrList = JsonConvert.DeserializeObject<List<Parametr>>(parametrContent);

                var productParametersContent = await responseProductParametr.Content.ReadAsStringAsync();
                var productParameters123 = JsonConvert.DeserializeObject<List<ProductParameter>>(productParametersContent);

                var atributsParamsContent = await responseAtributsParam.Content.ReadAsStringAsync();
                var atributsParams = JsonConvert.DeserializeObject<List<AtributsParam>>(atributsParamsContent);

                var bookingPricesContent = await responseBookingPrice.Content.ReadAsStringAsync();
                var bookingPrices = JsonConvert.DeserializeObject<List<BookingPrice>>(bookingPricesContent);

                var photosContent = await responsePhoto.Content.ReadAsStringAsync();
                var photos = JsonConvert.DeserializeObject<List<Photo>>(photosContent);

                foreach (var product in products!)
                {

                    var productParameters = productParameters123!.Where(pp => pp.Product_Id == product.IdProduct).ToList();
                    List<Parametr> parametrs = new List<Parametr>();
                    foreach (var prodparam in productParameters)
                    {
                        parametrs.AddRange(parametrList!.Where(ap => ap.IdParametrs == prodparam.Parameter_Id).ToList());
                    }
                    List<AtributsParam> atributparam = new List<AtributsParam>();
                    foreach (var prodparam in productParameters)
                    {
                        foreach (var param in parametrs)
                        {
                            atributparam.AddRange(atributsParams!.Where(ap => ap.ProductParameterId == prodparam.IdProductParameter).ToList());
                        }
                    }
                    atributparam = atributparam.Distinct().ToList();
                    //var atributs = atributsParams.Where(ap => parameters.Contains(ap.ProductParameterId));

                    var prices = bookingPrices!.Where(bp => bp.ProductId == product.IdProduct);

                    var productPhotos = photos!.Where(p => p.ProductId == product.IdProduct);

                    Category category123 = categories!.FirstOrDefault(c => c.IdCategory == product.CategoryId)!;

                    City city123 = cities!.FirstOrDefault(c => c.IdCity == product.CityId)!;

                    Client client123 = clients!.FirstOrDefault(c => c.IdClient == product.ClientId)!;

                    List<ProductParameter> prodparam123 = productParameters.Where(pp => pp.Product_Id == product.IdProduct).ToList();
                    //List<Parametr> param123 = parametrList.Where(p => parameters.Contains(p.IdParametrs)).ToList();

                    var fullProductInfo = new FullProductParametrInfo
                    {
                        product = product,
                        category = category123,
                        city = city123,
                        сlient = client123,
                        productparametr = prodparam123,
                        parameter = parametrs,
                        attributeParams = atributparam,
                        bookingPrices = prices.ToList(),
                        photo = productPhotos.ToList()
                    };

                    // Добавляем экземпляр в список
                    productsInShipment.Add(fullProductInfo);
                }

                ViewBag.City = cities;
                ViewBag.Category = categories;
                //ViewBag.Category = new SelectList(categories!, "CategoryId", "CategoryName");


                FullProductParametrInfo fullprod = productsInShipment.FirstOrDefault(e => e.product!.IdProduct == id)!;

                return View(fullprod);
            }
            else
            if (responseProduct.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseCategory.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseCity.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseClient.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseProductParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseAtributsParam.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseBookingPrice.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responsePhoto.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("ProductParametrInfoPage");


            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("/ProductParametrInfoPage/EditProduct/{id}")]
        public async Task<IActionResult> EditProduct(int id, FullProductParametrInfo updatedProduct)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }



            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Update the product
            updatedProduct!.product!.CityId = updatedProduct!.city!.IdCity;
            var responseProduct = await client.PutAsJsonAsync($"http://{IP}api/Products/{id}", updatedProduct.product);

            if (responseProduct.IsSuccessStatusCode)
            {
                // Update the attribute parameters
                for (int i = 0; i < updatedProduct.attributeParams!.Count; i++)
                {
                    var responseAttributeParam = await client.PutAsJsonAsync($"http://{IP}api/AtributsParams/{updatedProduct.attributeParams[i].IdAtributsParams}", updatedProduct.attributeParams[i]);

                    if (!responseAttributeParam.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }

                // Update the booking prices
                for (int i = 0; i < updatedProduct.bookingPrices!.Count; i++)
                {
                    var responseBookingPrice = await client.PutAsJsonAsync($"http://{IP}api/BookingPrices/{updatedProduct.bookingPrices[i].IdBookingPrice}", updatedProduct.bookingPrices[i]);

                    if (!responseBookingPrice.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }

                return RedirectToAction("ProductParametrInfoPage"); // Перенаправление на страницу с информацией о продукте после успешного редактирования
            }
            else
            if (responseProduct.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("ProductParametrInfoPage");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("/ProductParametrInfoPage/DeleteProduct")] 
        public async Task<IActionResult> DeleteProduct(int ProductParametrInfo)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }



            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var responseProduct = await client.GetAsync($"http://{IP}api/Products");
            var responseCategory = await client.GetAsync($"http://{IP}api/Categories");
            var responseCity = await client.GetAsync($"http://{IP}api/Cities");
            var responseClient = await client.GetAsync($"http://{IP}api/Clients");
            var responseProductParametr = await client.GetAsync($"http://{IP}api/ProductParameters");
            var responseParametr = await client.GetAsync($"http://{IP}api/Parametrs");
            var responseAtributsParam = await client.GetAsync($"http://{IP}api/AtributsParams");
            var responseBookingPrice = await client.GetAsync($"http://{IP}api/BookingPrices");
            var responsePhoto = await client.GetAsync($"http://{IP}api/Photos");

            if (responseProduct.IsSuccessStatusCode && responseCategory.IsSuccessStatusCode
                && responseCity.IsSuccessStatusCode && responseClient.IsSuccessStatusCode
                && responseProductParametr.IsSuccessStatusCode && responseParametr.IsSuccessStatusCode
                && responseAtributsParam.IsSuccessStatusCode && responseBookingPrice.IsSuccessStatusCode
                && responsePhoto.IsSuccessStatusCode)
            {
                var productsInShipment = new List<FullProductParametrInfo>();

                var productsContent = await responseProduct.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(productsContent);

                var clientsContent = await responseClient.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Client>>(clientsContent);

                var citiesContent = await responseCity.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<City>>(citiesContent);

                var categoriesContent = await responseCategory.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesContent);

                var parametrContent = await responseParametr.Content.ReadAsStringAsync();
                var parametrList = JsonConvert.DeserializeObject<List<Parametr>>(parametrContent);

                var productParametersContent = await responseProductParametr.Content.ReadAsStringAsync();
                var productParameters123 = JsonConvert.DeserializeObject<List<ProductParameter>>(productParametersContent);

                var atributsParamsContent = await responseAtributsParam.Content.ReadAsStringAsync();
                var atributsParams = JsonConvert.DeserializeObject<List<AtributsParam>>(atributsParamsContent);

                var bookingPricesContent = await responseBookingPrice.Content.ReadAsStringAsync();
                var bookingPrices = JsonConvert.DeserializeObject<List<BookingPrice>>(bookingPricesContent);

                var photosContent = await responsePhoto.Content.ReadAsStringAsync();
                var photos = JsonConvert.DeserializeObject<List<Photo>>(photosContent);

                foreach (var product in products!)
                {

                    var productParameters = productParameters123!.Where(pp => pp.Product_Id == product.IdProduct).ToList();
                    List<Parametr> parametrs = new List<Parametr>();
                    foreach (var prodparam in productParameters)
                    {
                        parametrs.AddRange(parametrList!.Where(ap => ap.IdParametrs == prodparam.Parameter_Id).ToList());
                    }
                    List<AtributsParam> atributparam = new List<AtributsParam>();
                    foreach (var prodparam in productParameters)
                    {
                        foreach (var param in parametrs)
                        {
                            atributparam.AddRange(atributsParams!.Where(ap => ap.ProductParameterId == prodparam.IdProductParameter).ToList());
                        }
                    }
                    atributparam = atributparam.Distinct().ToList();
                    //var atributs = atributsParams.Where(ap => parameters.Contains(ap.ProductParameterId));

                    var prices = bookingPrices!.Where(bp => bp.ProductId == product.IdProduct);

                    var productPhotos = photos!.Where(p => p.ProductId == product.IdProduct);

                    Category? category123 = categories?.FirstOrDefault(c => c.IdCategory == product.CategoryId);

                    City? city123 = cities?.FirstOrDefault(c => c.IdCity == product.CityId);

                    Client? client123 = clients?.FirstOrDefault(c => c.IdClient == product.ClientId);

                    List<ProductParameter> prodparam123 = productParameters.Where(pp => pp.Product_Id == product.IdProduct).ToList();
                    //List<Parametr> param123 = parametrList.Where(p => parameters.Contains(p.IdParametrs)).ToList();

                    var fullProductInfo = new FullProductParametrInfo
                    {
                        product = product,
                        category = category123,
                        city = city123,
                        сlient = client123,
                        productparametr = prodparam123,
                        parameter = parametrs,
                        attributeParams = atributparam,
                        bookingPrices = prices.ToList(),
                        photo = productPhotos.ToList()
                    };

                    // Добавляем экземпляр в список
                    productsInShipment.Add(fullProductInfo);
                }

                FullProductParametrInfo fullProductParametrInfo = productsInShipment.First(e => e.product!.IdProduct == ProductParametrInfo);

                foreach (var booking in fullProductParametrInfo.bookingPrices!)
                {
                    var responseBookingPriceDelete = await client.DeleteAsync($"http://{IP}api/BookingPrices/{booking.IdBookingPrice}");
                }
                foreach (var atribPar in fullProductParametrInfo.attributeParams!)
                {
                    var responseAtribPar = await client.DeleteAsync($"http://{IP}api/AtributsParams/{atribPar.IdAtributsParams}");
                }
                foreach (var prodparam in fullProductParametrInfo.productparametr!)
                {
                    var responseProdParam = await client.DeleteAsync($"http://{IP}api/ProductParameters/{prodparam.IdProductParameter}");
                }
                var responseProd = await client.DeleteAsync($"http://{IP}api/Products/{fullProductParametrInfo.product!.IdProduct}");

                if (responseProd.IsSuccessStatusCode)
                {
                    return RedirectToAction("ProductParametrInfoPage");
                }
                else
                {
                    return RedirectToAction("ProductParametrInfoPage");
                }
            }
            else
            if (responseProduct.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseCategory.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseCity.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseClient.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseProductParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseParametr.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responseAtributsParam.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseBookingPrice.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                responsePhoto.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("ProductParametrInfoPage");

            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet("/LocationsPage")]
        public async Task<IActionResult> LocationsPage(string search)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }



            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var responseCity = await client.GetAsync($"http://{IP}api/Cities");
            var responseRegion = await client.GetAsync($"http://{IP}api/Regions");
            var responseCountry = await client.GetAsync($"http://{IP}api/Сountry");

            if (responseCity.IsSuccessStatusCode && responseRegion.IsSuccessStatusCode && responseCountry.IsSuccessStatusCode)
            {
                var locationInfos = new List<RentAdminApp.Models.FullLocationInfo>();

                var cityContent = await responseCity.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<City>>(cityContent);

                var regionContent = await responseRegion.Content.ReadAsStringAsync();
                var regions = JsonConvert.DeserializeObject<List<Region>>(regionContent);

                var countryContent = await responseCountry.Content.ReadAsStringAsync();
                List<Сountry> countries = JsonConvert.DeserializeObject<List<Сountry>>(countryContent)!;


                var fullLocationInfo = new FullLocationInfo
                {
                    city = cities,
                    region = regions,
                    country = countries
                };

                locationInfos.Add(fullLocationInfo);

                List<FullLocationInfo> fulllocations = locationInfos;

                if (!string.IsNullOrEmpty(search))
                {
                    //fulllocations = fulllocations.Where(e =>
                    //    (e.country != null && e.country.Any(q => q.NameCountry != null && q.NameCountry.Contains(search, StringComparison.OrdinalIgnoreCase))) ||
                    //    (e.region != null && e.region.Any(w => w.Name != null && w.Name.Contains(search, StringComparison.OrdinalIgnoreCase))) ||
                    //    (e.city != null && e.city.Any(r => r.Name != null && r.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    //).ToList();

                    var filteredCountries = fulllocations.SelectMany(e => e.country!)
                                                    .Where(q => q.NameCountry != null && q.NameCountry.Contains(search, StringComparison.OrdinalIgnoreCase))
                                                    .ToList();

                    // Заменить текущий список стран отфильтрованным списком, если есть совпадения
                    if (filteredCountries.Any())
                    {
                        foreach (var mode in fulllocations)
                        {
                            mode.country = filteredCountries.ToList();
                        }
                    }
                    var filteredregions = fulllocations.SelectMany(e => e.region!)
                                                 .Where(q => q.Name != null && q.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                                                 .ToList();

                    // Заменить текущий список стран отфильтрованным списком, если есть совпадения
                    if (filteredregions.Any())
                    {
                        foreach (var mode in fulllocations)
                        {
                            mode.region = filteredregions.ToList();
                        }
                    }
                    var filteredcity = fulllocations.SelectMany(e => e.city!)
                                                 .Where(q => q.Name != null && q.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                                                 .ToList();

                    // Заменить текущий список стран отфильтрованным списком, если есть совпадения
                    if (filteredcity.Any())
                    {
                        foreach (var mode in fulllocations)
                        {
                            mode.city = filteredcity.ToList();
                        }
                    }


                    if (fulllocations == null)
                    {
                        return View("LocationsPage", locationInfos);
                    }
                    else
                    {
                        return View("LocationsPage", fulllocations);
                    }

                }
                else
                {
                    return View("LocationsPage", locationInfos);
                }

            }
            else
            if (responseCity.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseRegion.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseCountry.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("HomePage");


            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet("/LocationsPage/CreateCountry")]
        public async Task<IActionResult> CreateCountry()
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            await SomeAsyncMethod(); 

            return View();
           
        }

        [HttpPost("/LocationsPage/CreateCountry")]
        public async Task<IActionResult> CreateCountry(Сountry  country)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PostAsJsonAsync($"http://{IP}api/Сountry", country) ;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
        
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        [HttpGet("/LocationsPage/CreateRegion")]
        public async Task<IActionResult> CreateRegion()
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);


            // Получаем список вместимости складов
            var countryResponse = await client.GetAsync($"http://{IP}api/Сountry");
            var countryContent = await countryResponse.Content.ReadAsStringAsync();
            var countryis = JsonConvert.DeserializeObject<List<Сountry>>(countryContent);

            ViewBag.Capacities = countryis;

            return View();

        }

        [HttpPost("/LocationsPage/CreateRegion")]
        public async Task<IActionResult> CreateRegion(Region region)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PostAsJsonAsync($"http://{IP}api/Regions", region);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        
        }

        [HttpGet("/LocationsPage/CreateCity")]
        public async Task<IActionResult> CreateCity()
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var countryResponse = await client.GetAsync($"http://{IP}api/Regions");
            var countryContent = await countryResponse.Content.ReadAsStringAsync();
            var countryis = JsonConvert.DeserializeObject<List<Region>>(countryContent);

            ViewBag.Capacities = countryis;

            return View();

        }

        [HttpPost("/LocationsPage/CreateCity")]
        public async Task<IActionResult> CreateCity(City city)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PostAsJsonAsync($"http://{IP}api/Cities", city);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");

            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        [HttpGet("/LocationsPage/EditCountry/{id}")]
        public async Task<IActionResult> EditCountry(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            
            var counResponse = await client.GetAsync($"http://{IP}api/Сountry/{id}");
            if (counResponse.IsSuccessStatusCode)
            {
                var counContent = await counResponse.Content.ReadAsStringAsync();
                var country = JsonConvert.DeserializeObject<Сountry>(counContent);

                return View(country);
            }
            else
            if (counResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
            
        }

        [HttpPost("/LocationsPage/EditCountry/{id}")]
        public async Task<IActionResult> EditCountry(Сountry country)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{IP}api/Сountry/{country.IdСountry}", country);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }

        }

        [HttpGet("/LocationsPage/EditRegion/{id}")]
        public async Task<IActionResult> EditRegion(int id)
        {

            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var counResponse = await client.GetAsync($"http://{IP}api/Regions/{id}");
            var countryResponse = await client.GetAsync($"http://{IP}api/Сountry");
            if (countryResponse.IsSuccessStatusCode && counResponse.IsSuccessStatusCode)
            {
                var counContent = await counResponse.Content.ReadAsStringAsync();
                var country = JsonConvert.DeserializeObject<Region>(counContent);

                var countryContent = await countryResponse.Content.ReadAsStringAsync();
                var countryis = JsonConvert.DeserializeObject<List<Сountry>>(countryContent);

                ViewBag.Capacities = countryis;

                return View(country);
            }
            else
            if (counResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && countryResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }

        }

        [HttpPost("/LocationsPage/EditRegion/{id}")]
        public async Task<IActionResult> EditRegion(Region region)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{IP}api/Regions/{region.IdRegion}", region);

            // Получаем список вместимости складов
            var countryResponse = await client.GetAsync($"http://{IP}api/Country");
            var countryContent = await countryResponse.Content.ReadAsStringAsync();
            var countryis = JsonConvert.DeserializeObject<List<Сountry>>(countryContent);

            ViewBag.Capacities = countryis; 

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }

        }

        [HttpGet("/LocationsPage/EditCity/{id}")]
        public async Task<IActionResult> EditCity(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var counResponse = await client.GetAsync($"http://{IP}api/Cities/{id}");
            var countryResponse = await client.GetAsync($"http://{IP}api/Regions");
            if (counResponse.IsSuccessStatusCode && countryResponse.IsSuccessStatusCode)
            {
                var counContent = await counResponse.Content.ReadAsStringAsync();
                var country = JsonConvert.DeserializeObject<City>(counContent);

                var countryContent = await countryResponse.Content.ReadAsStringAsync();
                var countryis = JsonConvert.DeserializeObject<List<Region>>(countryContent);

                ViewBag.Capacities = countryis;

                return View(country);

            }
            else
            if (counResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && countryResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }

        }

        [HttpPost("/LocationsPage/EditCity/{id}")]
        public async Task<IActionResult> EditCity(City city)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }


            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{IP}api/Cities/{city.IdCity}", city);


            // Получаем список вместимости складов
            var countryResponse = await client.GetAsync($"http://{IP}api/Regions");
            var countryContent = await countryResponse.Content.ReadAsStringAsync();
            var countryis = JsonConvert.DeserializeObject<List<Region>>(countryContent);

            ViewBag.Capacities = countryis;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }

        }

        [HttpPost("/LocationsPage/DeleteCountry")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.DeleteAsync($"http://{IP}api/Сountry/{id}");
                                                                    
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        [HttpPost("/LocationsPage/DeleteRegion")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.DeleteAsync($"http://{IP}api/Regions/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        [HttpPost("/LocationsPage/DeleteCity")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.DeleteAsync($"http://{IP}api/Cities/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("LocationsPage");
            }
            else
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                reftreshtoken();
                return RedirectToAction("LocationsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        [HttpGet("/ClientsPage")]
        public async Task<IActionResult> ClientsPage(string search)
        {

            var authToken = HttpContext.Request.Cookies["TokenUser"];

            if (string.IsNullOrEmpty(authToken))
            {
                reftreshtoken();
                authToken = HttpContext.Request.Cookies["TokenUser"];
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var responseClients = await client.GetAsync($"http://{IP}api/Clients");
            var responseBookings = await client.GetAsync($"http://{IP}api/Bookings");
            var responseProducts = await client.GetAsync($"http://{IP}api/Products");

            if (responseClients.IsSuccessStatusCode && responseBookings.IsSuccessStatusCode && responseProducts.IsSuccessStatusCode)
            {
                var fullClientInfos = new List<RentAdminApp.Models.FullClientInfo>();

                var clientContent = await responseClients.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Client>>(clientContent);

                var bookingsContent = await responseBookings.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<List<Booking>>(bookingsContent);

                var prosuctsContent = await responseProducts.Content.ReadAsStringAsync();
                List<Product> products = JsonConvert.DeserializeObject<List<Product>>(prosuctsContent)!;

                foreach(var cliented in clients!)
                {
                    List<Booking>? booking = bookings!.Where(e => e.ClientId == cliented.IdClient).ToList();
                    List<Product>? producting = products.Where(e => e.ClientId == cliented.IdClient).ToList();

                    var fullClientInfo = new FullClientInfo
                    {
                        booking = booking,
                        client = cliented,
                        product = producting
                    };

                    fullClientInfos.Add(fullClientInfo);
                }

                List<FullClientInfo> fullclients = fullClientInfos;

                if (!string.IsNullOrEmpty(search))
                {
                    fullclients = fullclients.Where(e =>
                        (e.client?.Email != null && e.client.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.client?.FirstName != null && e.client.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.client?.MiddleName != null && e.client.MiddleName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.client?.LastName != null && e.client.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)) 
                    ).ToList();

                    if (fullclients == null)
                    {
                        return View("ClientsPage", fullClientInfos);
                    }
                    else
                    {
                        return View("ClientsPage", fullclients);
                    }

                }
                else
                {
                    return View("ClientsPage", fullClientInfos);
                }
            }
            else
            if (responseClients.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseBookings.StatusCode == System.Net.HttpStatusCode.Unauthorized && responseProducts.StatusCode == System.Net.HttpStatusCode.Unauthorized )
            {
                reftreshtoken();
                return RedirectToAction("ClientsPage");
            }
            else
            {
                return RedirectToAction("HomePage");
            }
        }

        //[HttpGet("/ClientsPage/CreateClient")]
        //public async Task<IActionResult> CreateClient()
        //{
        //    var authToken = HttpContext.Session.GetString("TokenUser");

        //    if (string.IsNullOrEmpty(authToken))
        //    {
        //        return RedirectToAction("/", "Auth");
        //    }

        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        //    return View();

        //}

        //[HttpPost("/ClientsPage/CreateClient")]
        //public async Task<IActionResult> CreateClient(FullClientInfo fullclientinfo)
        //{
        //    var authToken = HttpContext.Session.GetString("TokenUser");
        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        //    //var response = await client.PostAsJsonAsync($"http://{IP}api/", country);

        //    if (true)//response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("ClientsPage");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Ошибка при создании страны");
        //        return RedirectToAction("ClientsPage");
        //    }
        //}

        //[HttpGet("/ClientsPage/EditClient/{id}")]
        //public async Task<IActionResult> EditClient(int id)
        //{
        //    var authToken = HttpContext.Session.GetString("TokenUser");

        //    if (string.IsNullOrEmpty(authToken))
        //    {

        //    }

        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        //    var counResponse = await client.GetAsync($"http://{IP}api/Сountry/");
        //    var counContent = await counResponse.Content.ReadAsStringAsync();
        //    var country = JsonConvert.DeserializeObject<Сountry>(counContent);

        //    return View();

        //}
        //[HttpPost("/ClientsPage/EditClient/{id}")]
        //public async Task<IActionResult> EditClient(Сountry country)
        //{
        //    var authToken = HttpContext.Session.GetString("TokenUser");
        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        //    var response = await client.PutAsJsonAsync($"http://{IP}api/Сountry/{country.IdСountry}", country);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("ClientsPage");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Ошибка при редактировании страны");
        //        return RedirectToAction("ClientsPage");
        //    }
        //}



        //[HttpPost("/ClientsPage/DeleteClient")]
        //public async Task<IActionResult> DeleteClient(int id)
        //{
        //    var authToken = HttpContext.Session.GetString("TokenUser");
        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        //    var response = await client.DeleteAsync($"http://{IP}api/Сountry/{id}");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("ClientsPage");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Ошибка при удалении страны");
        //        return RedirectToAction("ClientsPage");
        //    }
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AlzaCaseStudy.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using Serilog;

//RestClient implementation
/*
 public async Task<ResponseDto<T>> GetDataAsync<T>(string uri)
{
    string apiRequestAddress = ParseUri(uri);
    var request = new RestRequest($"/api/{apiRequestAddress}", Method.Get);
    request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
    
    _logger.Information($"Requesting data from address: {request.Resource}");
    var response = await _client.ExecuteAsync(request);
    
    if (response.IsSuccessful)
    {
        _logger.Information($"Successfully received data, deserializing...");
        return new ResponseDto<T>
        {
            IsSuccess = true,
            Response = JsonConvert.DeserializeObject<T>(response.Content)
        };
    }
    else
    {
        _logger.Error($"Request failed with status code: {response.StatusCode}");
        _logger.Error($"Response content: {response.Content}");
        return new ResponseDto<T>
        {
            Response = default,
            IsSuccess = false,
            Message = $"Didn't manage to get data from the API: {response.StatusCode}"
        };
    }
}

[OneTimeSetUp]
public void SetUp()
{
    _client = new RestClient("https://webapi.alza.cz");
    var conf = new ConfigurationBuilder().AddJsonFile("Config\\LoggingConfig.json").Build();
    _logger = new LoggerConfiguration().ReadFrom.Configuration(conf).CreateLogger() as ILogger;
    Log.Logger = _logger;
}
 */


namespace AlzaCaseStudy
{
    [TestFixture]
    public class Tests
    {
        private ILogger _logger;
        private HttpClient _client;
        //pozice: java-developer- specialista-prodeje-praha specialista-call-centra-trinec tester-webovych-aplikaci?country=sk
        private const string MAIN_JSON_API_ADDRESS = $"../api/career/v2/positions/tester-webovych-aplikaci?country=sk";
        public string ParseUri(string uri)
        {
            string[] parsedUri = uri.Split("/api/");
            if (parsedUri.Length == 2)                
                return parsedUri[1];
            Log.Error($"Wrong uri format: {uri}");
            return "";
        }

        public async Task<ResponseDto<T>> GetDataAsync<T>(string uri)
        {
            string apiRequestAddress = ParseUri(uri);
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{apiRequestAddress}");
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
            Log.Information($"Requesting data from address: {request.RequestUri}");
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Information($"Succesfully recieved data, deserializing...");
                return new ResponseDto<T>
                {
                    IsSuccess = true,
                    Response = JsonConvert.DeserializeObject<T>(content)
                };
            }
            else
            {
                Log.Error($"Request failed with status code: {response.StatusCode}");
                Log.Error($"Response content: {await response.Content.ReadAsStringAsync()}");
                return new ResponseDto<T>
                {
                    Response = default,
                    IsSuccess = false,
                    Message = $"Didn't manage to get data from the api: {response.StatusCode}"
                };
            }

        }        

        [OneTimeSetUp]
        public void SetUp()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://webapi.alza.cz");
            var conf = new ConfigurationBuilder().AddJsonFile("Config\\LoggingConfig.json").Build();
            _logger = new LoggerConfiguration().ReadFrom.Configuration(conf).CreateLogger() as ILogger;
            Log.Logger = _logger;
        }        

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await Log.CloseAndFlushAsync();
            _client.Dispose();            
        }

        [Test]
        public async Task TestPositionDescription()
        {
            Log.Information("Starting test for description");
            var positionData = await GetDataAsync<PositionResponse>(MAIN_JSON_API_ADDRESS);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(positionData.IsSuccess);
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.PositionItems);
            Assert.IsNotNull(positionData.Response.PositionItems.Meta);
            Assert.IsNotNull(positionData.Response.PositionItems.Meta.Href);
            var positionDescItems = await GetDataAsync<PositionItems>(positionData.Response.PositionItems.Meta.Href);
            Assert.IsTrue(positionDescItems.IsSuccess);
            Assert.IsNotNull(positionDescItems.Response);
            Assert.IsNotNull(positionDescItems.Response.items);
            bool pass = false;
            foreach (var item in positionDescItems.Response.items)
            {
                foreach (var subContent in item.SubContent)
                {
                    if(string.IsNullOrEmpty(subContent))
                        continue;
                    pass = true;
                    break;
                }
                if (pass) break;
            }
            Assert.IsTrue(pass);
            Log.Information("Description test finished");
        }

        [Test]
        public async Task TestPlaceOfEmployment()
        {
            Log.Information("Starting test for place of employment");
            var positionData = await GetDataAsync<PositionResponse>(MAIN_JSON_API_ADDRESS);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(positionData.IsSuccess);
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.PlaceOfEmplyment);
            Assert.IsNotNull(positionData.Response.Department.Name);
            Log.Information($"Finished asserting.");
            Log.Information("Place of employment test finished");
        }

        [Test]
        public async Task TestInterviewers()
        {
            Log.Information("Starting test for interviewers");
            var positionData = await GetDataAsync<PositionResponse>(MAIN_JSON_API_ADDRESS);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(positionData.IsSuccess);
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.GestorUser);
            Assert.IsNotNull(positionData.Response.ExecutiveUser);
            Assert.IsNotEmpty(positionData.Response.GestorUser.Meta.Href);
            Log.Information($"Finished asserting.");
            var gestorUserData = await GetDataAsync<EmployeeResponse>(positionData.Response.GestorUser.Meta.Href);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(gestorUserData.IsSuccess);
            Assert.IsNotNull(gestorUserData.Response);
            Assert.IsNotEmpty(gestorUserData.Response.Name);
            Assert.IsNotEmpty(gestorUserData.Response.Image);
            Assert.IsNotEmpty(gestorUserData.Response.Description);
            Log.Information($"Finished asserting.");
            var executiveUserData = await GetDataAsync<EmployeeResponse>(positionData.Response.ExecutiveUser.Meta.Href);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(executiveUserData.IsSuccess);
            Assert.IsNotNull(executiveUserData.Response);
            Assert.IsNotEmpty(executiveUserData.Response.Name);
            Assert.IsNotEmpty(executiveUserData.Response.Image);
            Assert.IsNotEmpty(executiveUserData.Response.Description);
            Log.Information($"Finished asserting.");
            Log.Information("Interviewers test finished");
        }

        [Test]
        public async Task TestSuitableForStudents()
        {
            Log.Information("Starting test for students");
            var positionData = await GetDataAsync<PositionResponse>(MAIN_JSON_API_ADDRESS);
            Log.Information($"Deserialized, asserting.");
            Assert.IsTrue(positionData.IsSuccess);
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.ForStudents);
            Log.Information($"Finished asserting.");
            Log.Information("Students test finished");
        }        
    }
}
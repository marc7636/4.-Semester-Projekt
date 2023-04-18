﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AssemblyLineManager.AGV
{
    public class AGVClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:8082/v1/status/";

        public AGVClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetStatus()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<string> LoadProgram(string programName)
        {
            var payload = new
            {
                ProgramName = programName,
                State = 1
            };
            return await SendPutRequest(payload);
        }

        public async Task<string> ExecuteProgram()
        {
            var payload = new
            {
                State = 2
            };
            return await SendPutRequest(payload);
        }

        private async Task<string> SendPutRequest(object payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(_baseUrl, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}
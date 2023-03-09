using Microsoft.Identity.Client;
IConfidentialClientApplication app;

app = ConfidentialClientApplicationBuilder.Create("ClientId")
           .WithClientSecret("")
           .WithAuthority(new Uri("https://login.microsoftonline.com/{tenantId}"))
           .Build();

var ResourceId = "ClientId";
var scopes = new[] { ResourceId + "/.default" };


AuthenticationResult result = null;
try
{
    result = await app.AcquireTokenForClient(scopes)
                     .ExecuteAsync();

    Console.WriteLine(result.AccessToken);

    HttpClient httpClient = new();

    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + result.AccessToken);

    var response = await httpClient.GetAsync("http://localhost:5068/WeatherForecast");

    Console.WriteLine(response.Content.ReadAsStringAsync());

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.ReadLine();



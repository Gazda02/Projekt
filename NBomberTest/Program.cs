using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Contracts.Stats;

var scenario = Scenario.Create("load_test", async context =>
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync("http://localhost:5041/api/Customers");
        return response.IsSuccessStatusCode
            ? Response.Ok()
            : Response.Fail();

    })
    .WithWarmUpDuration(TimeSpan.FromSeconds(1))
    .WithLoadSimulations(
        Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(2)) // 50rps x 2s = 100 żądań
    );

NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFormats(ReportFormat.Txt, ReportFormat.Html)
    .WithReportFileName("api_test_report")
    .WithReportFolder("./reports")
    .Run();
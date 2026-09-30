using MedConnect.Client;
using MedConnect.Client.Features.Appointments;
using MedConnect.Client.Features.Analytics;
using MedConnect.Client.Features.Audit;
using MedConnect.Client.Features.Auth;
using MedConnect.Client.Features.Chat;
using MedConnect.Client.Features.Facilities;
using MedConnect.Client.Features.Divisions;
using MedConnect.Client.Features.FieldVisits;
using MedConnect.Client.Features.LabResults;
using MedConnect.Client.Features.Patients;
using MedConnect.Client.Features.Prescriptions;
using MedConnect.Client.Features.Referrals;
using MedConnect.Client.Features.Visits;
using MedConnect.Client.Features.Vitals;
using MedConnect.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The SPA is deployed separately from the Web API (Cloudflare Pages + Windows
// IIS host), so the API origin is configurable. Empty value falls back to
// same-origin.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
ApiEndpoints.Initialize(apiBaseUrl, builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("MedConnectApi", client =>
        client.BaseAddress = new Uri(ApiEndpoints.BaseUrl + "/"))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("MedConnectApi"));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<FacilitiesApiClient>();
builder.Services.AddScoped<DivisionsApiClient>();
builder.Services.AddScoped<PatientsApiClient>();
builder.Services.AddScoped<VisitsApiClient>();
builder.Services.AddScoped<AppointmentsApiClient>();
builder.Services.AddScoped<ReferralsApiClient>();
builder.Services.AddScoped<FieldVisitsApiClient>();
builder.Services.AddScoped<ChatApiClient>();
builder.Services.AddScoped<VitalsApiClient>();
builder.Services.AddScoped<LabResultsApiClient>();
builder.Services.AddScoped<PrescriptionsApiClient>();
builder.Services.AddScoped<AuditApiClient>();
builder.Services.AddScoped<AnalyticsApiClient>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();

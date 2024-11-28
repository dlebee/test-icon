var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGraphQLServer()
    .AddQueryType<QueryType>()
    .AddFiltering(x => x
        .AddDefaults()
    )
    .AddSorting();

var app = builder.Build();



app.MapGet("/", () => TypedResults.Content(content: "<STYLE></STYLE><img class=\"token-icon\" src=\"/icon\" /><br><img class=\"token-icon\" src=\"/icon/not-found\" />",
    contentType: "text/html"));
app.MapGet("/icon", () => Results.Redirect($"https://s3.coinmarketcap.com/static-gravity/image/5a8229787b5e4c809b5914eef709b59a.png"));
app.MapGet("/icon/not-found", () => TypedResults.NotFound());

app.MapGraphQL();

app.Run();



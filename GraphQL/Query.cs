public class Token
{
    public string? Symbol {get; set;}
    public string? ContractAddress {get; set;}
    public string? IconUrl {get;set;}
}

public class ApiTokenMetaResult {
    public string ContractAddress { get; set; }
    public string IconUrl { get; set; }
}

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Field(f => f.Tokens())
            
            .Use(next => async context =>
            {
                await next(context);

                if (context.Result is HotChocolate.Types.Pagination.Connection<Token> paged) {
                    var tokensWithNoIcon = paged.Edges.Where(t => t.Node.IconUrl == null).ToList();
                    if (tokensWithNoIcon.Count > 0) {
                        var client = new HttpClient(); // obvously im being lazy use http factory don't create a new http client
                        var response = await client.PostAsJsonAsync("https://api-test.creditcoin.org/token/v1/evm", new {
                            chainId = "102031",
                            contractAddresses = tokensWithNoIcon.Select(t => t.Node.ContractAddress).ToArray()
                        });
                        if (response.IsSuccessStatusCode) {
                            var tokensMeta = await response.Content.ReadFromJsonAsync<ApiTokenMetaResult[]>();
                            var tokensMetaMap = tokensMeta
                                .Where(t => !string.IsNullOrWhiteSpace(t.IconUrl))
                                .ToDictionary(t => t.ContractAddress, t => t.IconUrl, comparer: StringComparer.InvariantCultureIgnoreCase);
                            foreach(var missing in tokensWithNoIcon) {
                                if (tokensMetaMap.TryGetValue(missing.Node.ContractAddress, out var icon)) {
                                    missing.Node.IconUrl = icon;
                                }
                            }
                        }   
                    }
                }
             })
             .UsePaging()
             .UseFiltering()
             .UseSorting();
    }
}

public class Query()
{
    public static Token[] MockDataSet = [
        new Token {
            Symbol = "WCTC",
            ContractAddress = "0x56072113e08015e1c40a3f3f656b1c1fa78e329e"
        },
        new Token {
            Symbol = "USD-TCoin",
            ContractAddress = "0xa1cc4d7aa040ea903fd00c13e7b43f8e26cbb7f8"
        },
        new Token {
            Symbol = "Tether",
            ContractAddress = "0x64936984808ba2ba09E14c08cC2Ad7FD05b71FFF"
        }
    ];

    public IQueryable<Token> Tokens() => MockDataSet.Select(s => new Token {
        Symbol = s.Symbol,
        ContractAddress = s.ContractAddress,
        IconUrl = null // essentially this fielkd does not exist in the database.
    })
    .ToList()
    .AsQueryable();
}


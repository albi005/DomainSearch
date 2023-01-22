#define A

#if A
using DomainSearch.Data;
using Microsoft.EntityFrameworkCore;
#endif

Directory.SetCurrentDirectory(@"C:\Users\ragan\OneDrive\Programming\Domain");

#if A
Db db = new();

File.Delete("cheapest and shortest.txt");
await using StreamWriter writer = new("cheapest and shortest.txt");

List<Output> list = await db.Offers
    .GroupBy(o => o.DomainId)
    .Select(g => new Output(g.Key, g.Min(o => o.DollarsPerYear)))
    .ToListAsync();

foreach (Output output in list.OrderBy(o => o.DollarsPerYear))
{
    writer.WriteLine($"{output.Domain, -14}{output.DollarsPerYear,3:0}");
}
writer.WriteLine();
foreach (Output output in list.OrderBy(o => o.Domain.Length).ThenBy(o => o.DollarsPerYear))
{
    writer.WriteLine($"{output.Domain, -14}{output.DollarsPerYear,3:0}");
}

internal record Output(string Domain, float DollarsPerYear);
#endif


#if B
using Stream input = File.OpenRead(@"C:\Users\ragan\Desktop\domain2multi-hu00.txt");
StreamReader reader = new(input);

using Stream output = File.OpenWrite("length 3.txt");
StreamWriter writer = new(output);

while (reader.ReadLine() is { } line)
{
    if (line.Length == 6)
        writer.WriteLine(line);
} 
#endif

#if C
char GetChar(byte val) => (char)(val < 10 ? val + 48 : val + 87);

string?[] possible = new string[36 * 36 * 36];
for (byte i = 0; i < 36; i++)
{
    for (byte j = 0; j < 36; j++)
    {
        for (byte l = 0; l < 36; l++)
        {
            possible[i * 36 * 36 + j * 36 + l] = $"{GetChar(i)}{GetChar(j)}{GetChar(l)}";
        }
    }
}

using StreamReader reader = new("length 3.txt");
File.Delete("length 3 available.txt");
await using StreamWriter writer = new("length 3 available.txt");

int k = 0;
while (!reader.EndOfStream)
{
    string line = reader.ReadLine()!;
    while (k < possible.Length - 1 && !line.StartsWith(possible[k]!)) k++;
    if (k == possible.Length)
        break;
    possible[k] = null;
    k++;
}

foreach (string? domain in possible)
{
    if (domain != null)
        writer.WriteLine(domain);
}
#endif
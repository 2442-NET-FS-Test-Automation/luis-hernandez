using System.Text.Json; //Library for working with JSON - written by Microsoft
using LibraryKata.Domain;
using Serilog;

namespace Library.Domain;

public class OpenLibreryClient
{
    //We are going to create and use ONE HTTPClient for the entire process
    //If you use on per call, you are going to leak sockets - and eventually trigger a SocketException

    private static readonly HttpClient client = new();

    //We are going to write an async method. An async method is any method that calls an async code.
    //So, if you use something like .FindAsync() OR you "await a method call the surrounding method MUST be declared as async

    //A Task in C# is like a Promise in JS - it is a placeholder in memory telling th runtime
    //"I expect there to be a LibraryItem (or whatever the Task is "wrapping" with it's brackets)
    //-when this method resolves. I have no idea when that is, so for now - hold that place with a Task.
    //We are also going account for the possibility of a null - because my HTTP call could fail for a number of reasons
    //I could be rate limited, asked fir a bad isbn, OpenLibrary might be down, etc.
    public async Task<LibraryItem> FetchByIsbnAsync(string isbn)
    {
        //Im going to create a string to hold the url Im targeting
        //We will go much more in depth on HTTP, URLSs/URIS, etc.
        string url = $"https://openlibrary.org/search.json?q=isbn:{isbn}&fields=title,author_name&limit=1";

        try
        {
            string jsonResponse = await client.GetStringAsync(url);

            //Wr are going to write our own parsing logic
            //in a method Parse()
            return Parse(jsonResponse);
        } catch(HttpRequestException ex)
        {
            Log.Warning("Network fetch failed for {isbn}: {Message}", isbn, ex.Message);
            return null;
        } catch (Exception ex)
        {
            Log.Warning("FetchByIsbnAsync failed: {Message}", ex.Message);
            return null;
        }
    }

    //We are going to write our own parsing logic
    //Mostly as an excercise to work Json
    public static LibraryItem? Parse(string json)
    {
        //The Search API within OpenLibrary return an JSON object, and outside that object, among other
        Dictionary<string, JsonElement>? resp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (resp is null || !resp.TryGetValue("docs", out JsonElement docs) || docs.GetArrayLength() == 0)
        {
            return null;
        }

        JsonElement foundBook = docs[0];

        string title = foundBook.GetProperty("title").GetString() ?? "Untitled";

        string author = "Unknown";

        if (foundBook.TryGetProperty("author_name", out JsonElement authors) && authors.GetArrayLength() > 0)
        {
            author = authors[0].GetString() ?? "Unknown";
        }

        return LibraryItemFactory.Create(ItemKind.Book, title, author);
    }

    
}